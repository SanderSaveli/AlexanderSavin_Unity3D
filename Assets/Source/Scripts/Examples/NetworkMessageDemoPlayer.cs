using Mirror;
using UnityEngine;
using Zenject;

public sealed class NetworkMessageDemoPlayer : NetworkBehaviour
{
    [SyncVar]
    private int _playerId = -1;

    public int PlayerId => _playerId;

    private IChatView _chatView;
    private INetworkMessageService _messageService;

    [Inject]
    private void Construct(INetworkMessageService messageService, IChatView chatView)
    {
        _messageService = messageService;
        _chatView = chatView;
    }

    public override void OnStartLocalPlayer()
    {
        if (_messageService == null || _chatView == null)
        {
            Debug.LogError($"{nameof(NetworkMessageDemoPlayer)} was not injected. Add ZenAutoInjecter to the player prefab and bind {nameof(IChatView)} in the scene installer.");
            return;
        }

        _chatView.Clear();
        _messageService.Subscribe<HelloMessage>(OnHelloMessage);
    }

    public override void OnStopLocalPlayer()
    {
        _messageService?.Unsubscribe<HelloMessage>(OnHelloMessage);
    }

    [Server]
    public void SetPlayerId(int playerId)
    {
        _playerId = playerId;
    }

    private void OnHelloMessage(HelloMessage message)
    {
        _chatView.AddLine(message.Text);
    }
}
