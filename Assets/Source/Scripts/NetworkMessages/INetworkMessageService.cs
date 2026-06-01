using System;
using Mirror;

public interface INetworkMessageService
{
    public event Action<NetworkConnectionToClient, Type> ServerSubscriptionAdded;
    public event Action<NetworkConnectionToClient, Type> ServerSubscriptionRemoved;

    public void Subscribe<TMessage>(Action<TMessage> handler);
    public void Unsubscribe<TMessage>(Action<TMessage> handler = null);

    public bool IsSubscribed(NetworkConnectionToClient connection, Type messageType);
    public bool SendToSubscribed<TMessage>(NetworkConnectionToClient connection, TMessage message);
    public int SendToAllSubscribed<TMessage>(TMessage message);
}
