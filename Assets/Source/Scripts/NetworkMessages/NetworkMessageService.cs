using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Zenject;

public sealed class NetworkMessageService : INetworkMessageService, IInitializable, IDisposable
{
    private readonly ServerMessageSubscriptions _serverSubscriptions = new();
    private readonly Dictionary<string, Type> _knownTypes = new();
    private readonly Dictionary<string, Delegate> _clientHandlers = new();

    public event Action<NetworkConnectionToClient, Type> ServerSubscriptionAdded
    {
        add => _serverSubscriptions.Added += value;
        remove => _serverSubscriptions.Added -= value;
    }

    public event Action<NetworkConnectionToClient, Type> ServerSubscriptionRemoved
    {
        add => _serverSubscriptions.Removed += value;
        remove => _serverSubscriptions.Removed -= value;
    }

    public void Initialize()
    {
        NetworkServer.ReplaceHandler<MessageSubscribeRequest>(OnServerSubscribe, false);
        NetworkServer.ReplaceHandler<MessageUnsubscribeRequest>(OnServerUnsubscribe, false);
        NetworkClient.ReplaceHandler<MessageEnvelope>(OnClientEnvelope, false);

        NetworkClient.OnConnectedEvent += SendCurrentSubscriptionsToServer;
        NetworkServer.OnDisconnectedEvent += OnServerDisconnected;
    }

    public void Dispose()
    {
        NetworkServer.UnregisterHandler<MessageSubscribeRequest>();
        NetworkServer.UnregisterHandler<MessageUnsubscribeRequest>();
        NetworkClient.UnregisterHandler<MessageEnvelope>();
        NetworkClient.OnConnectedEvent -= SendCurrentSubscriptionsToServer;
        NetworkServer.OnDisconnectedEvent -= OnServerDisconnected;

        _serverSubscriptions.Clear();
        _knownTypes.Clear();
        _clientHandlers.Clear();
    }

    public void Subscribe<TMessage>(Action<TMessage> handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        string typeName = GetTypeName<TMessage>();
        RememberType<TMessage>();

        if (_clientHandlers.TryGetValue(typeName, out Delegate current))
        {
            _clientHandlers[typeName] = Delegate.Combine(current, handler);
        }
        else
        {
            _clientHandlers.Add(typeName, handler);
        }

        if (NetworkClient.isConnected)
        {
            NetworkClient.Send(new MessageSubscribeRequest { TypeName = typeName });
        }
    }

    public void Unsubscribe<TMessage>(Action<TMessage> handler = null)
    {
        string typeName = GetTypeName<TMessage>();

        if (handler == null)
        {
            _clientHandlers.Remove(typeName);
        }
        else if (_clientHandlers.TryGetValue(typeName, out Delegate current))
        {
            Delegate next = Delegate.Remove(current, handler);

            if (next == null)
            {
                _clientHandlers.Remove(typeName);
            }
            else
            {
                _clientHandlers[typeName] = next;
            }
        }

        if (NetworkClient.isConnected)
        {
            NetworkClient.Send(new MessageUnsubscribeRequest { TypeName = typeName });
        }
    }

    public bool IsSubscribed(NetworkConnectionToClient connection, Type messageType)
    {
        if (connection == null || messageType == null)
        {
            return false;
        }

        return _serverSubscriptions.IsSubscribed(connection, messageType);
    }

    public bool SendToSubscribed<TMessage>(NetworkConnectionToClient connection, TMessage message)
    {
        if (!IsSubscribed(connection, typeof(TMessage)))
        {
            return false;
        }

        connection.Send(CreateEnvelope(message));
        return true;
    }

    public int SendToAllSubscribed<TMessage>(TMessage message)
    {
        int sentCount = 0;

        foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
        {
            if (SendToSubscribed(connection, message))
            {
                sentCount++;
            }
        }

        return sentCount;
    }

    private void OnServerSubscribe(NetworkConnectionToClient connection, MessageSubscribeRequest request)
    {
        _serverSubscriptions.Add(connection, request.TypeName);
    }

    private void OnServerUnsubscribe(NetworkConnectionToClient connection, MessageUnsubscribeRequest request)
    {
        _serverSubscriptions.Remove(connection, request.TypeName);
    }

    private void OnServerDisconnected(NetworkConnectionToClient connection)
    {
        _serverSubscriptions.RemoveConnection(connection);
    }

    private void SendCurrentSubscriptionsToServer()
    {
        foreach (string typeName in _clientHandlers.Keys)
        {
            NetworkClient.Send(new MessageSubscribeRequest { TypeName = typeName });
        }
    }

    private void OnClientEnvelope(MessageEnvelope envelope)
    {
        if (!_clientHandlers.TryGetValue(envelope.TypeName, out Delegate handler))
        {
            return;
        }

        Type messageType = ResolveType(envelope.TypeName);

        if (messageType == null)
        {
            Debug.LogWarning($"Cannot resolve network message type '{envelope.TypeName}'.");
            return;
        }

        object message = JsonUtility.FromJson(envelope.JsonPayload, messageType);
        handler.DynamicInvoke(message);
    }

    private MessageEnvelope CreateEnvelope<TMessage>(TMessage message)
    {
        RememberType<TMessage>();

        return new MessageEnvelope
        {
            TypeName = GetTypeName<TMessage>(),
            JsonPayload = JsonUtility.ToJson(message)
        };
    }

    private Type ResolveType(string typeName)
    {
        if (_knownTypes.TryGetValue(typeName, out Type type))
        {
            return type;
        }

        type = Type.GetType(typeName);

        if (type != null)
        {
            _knownTypes[typeName] = type;
        }

        return type;
    }

    private static string GetTypeName<TMessage>()
    {
        return GetTypeName(typeof(TMessage));
    }

    private static string GetTypeName(Type type)
    {
        return type.AssemblyQualifiedName;
    }

    private void RememberType<TMessage>()
    {
        Type type = typeof(TMessage);
        _knownTypes[GetTypeName(type)] = type;
    }
}
