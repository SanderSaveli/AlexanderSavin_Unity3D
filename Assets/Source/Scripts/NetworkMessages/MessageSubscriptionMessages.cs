using Mirror;

public struct MessageSubscribeRequest : NetworkMessage
{
    public string TypeName;
}

public struct MessageUnsubscribeRequest : NetworkMessage
{
    public string TypeName;
}

public struct MessageEnvelope : NetworkMessage
{
    public string TypeName;
    public string JsonPayload;
}
