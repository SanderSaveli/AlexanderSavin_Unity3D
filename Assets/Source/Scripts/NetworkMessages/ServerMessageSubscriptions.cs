using System;
using System.Collections.Generic;
using Mirror;

public sealed class ServerMessageSubscriptions
{
    private readonly Dictionary<int, HashSet<string>> _subscriptions = new();

    public event Action<NetworkConnectionToClient, Type> Added;
    public event Action<NetworkConnectionToClient, Type> Removed;

    public bool IsSubscribed(NetworkConnectionToClient connection, Type messageType)
    {
        if (connection == null || messageType == null)
        {
            return false;
        }

        return _subscriptions.TryGetValue(connection.connectionId, out HashSet<string> types)
               && types.Contains(messageType.AssemblyQualifiedName);
    }

    public void Add(NetworkConnectionToClient connection, string typeName)
    {
        if (connection == null || string.IsNullOrWhiteSpace(typeName))
        {
            return;
        }

        if (!_subscriptions.TryGetValue(connection.connectionId, out HashSet<string> types))
        {
            types = new HashSet<string>();
            _subscriptions.Add(connection.connectionId, types);
        }

        if (types.Add(typeName))
        {
            Added?.Invoke(connection, Type.GetType(typeName));
        }
    }

    public void Remove(NetworkConnectionToClient connection, string typeName)
    {
        if (connection == null || string.IsNullOrWhiteSpace(typeName))
        {
            return;
        }

        if (!_subscriptions.TryGetValue(connection.connectionId, out HashSet<string> types))
        {
            return;
        }

        if (types.Remove(typeName))
        {
            Removed?.Invoke(connection, Type.GetType(typeName));
        }

        if (types.Count == 0)
        {
            _subscriptions.Remove(connection.connectionId);
        }
    }

    public void RemoveConnection(NetworkConnectionToClient connection)
    {
        if (connection != null)
        {
            _subscriptions.Remove(connection.connectionId);
        }
    }

    public void Clear()
    {
        _subscriptions.Clear();
    }
}
