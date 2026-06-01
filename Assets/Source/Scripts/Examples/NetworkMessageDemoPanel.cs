using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public sealed class NetworkMessageDemoPanel : MonoBehaviour
{
    [SerializeField] private NetworkMessageDemoNetworkManager _networkManager;
    [SerializeField] private Button _sendAllButton;
    [SerializeField] private Button _playerButtonPrefab;
    [SerializeField] private Transform _playerButtonsRoot;
    [SerializeField] private string _defaultMessage = "Hello Client!";
    [SerializeField] private string _sendToPlayerLabelFormat = "Send to {0}";

    private readonly List<Button> _playerButtons = new();
    private INetworkMessageService _messageService;

    [Inject]
    public void Construct(INetworkMessageService messageService)
    {
        _messageService = messageService;
    }

    private void OnEnable()
    {
        _sendAllButton.onClick.AddListener(SendToAllPlayers);

        _networkManager.ServerPlayersChanged += RebuildPlayerButtons;

        RebuildPlayerButtons();
    }

    private void OnDisable()
    {
        _sendAllButton.onClick.RemoveListener(SendToAllPlayers);

        _networkManager.ServerPlayersChanged -= RebuildPlayerButtons;

        ClearPlayerButtons();
    }

    private void SendToAllPlayers()
    {
        _messageService.SendToAllSubscribed(CreateMessage(-1));
    }

    private void SendToPlayer(NetworkConnectionToClient connection, int playerId)
    {
        _messageService.SendToSubscribed(connection, CreateMessage(playerId));
    }

    private HelloMessage CreateMessage(int targetPlayerId)
    {
        return new HelloMessage(_defaultMessage);
    }

    private void RebuildPlayerButtons()
    {
        ClearPlayerButtons();

        if (_networkManager == null || _playerButtonPrefab == null || _playerButtonsRoot == null)
        {
            return;
        }

        foreach (NetworkConnectionToClient connection in _networkManager.PlayerConnections)
        {
            if (connection == null || connection.identity == null)
            {
                continue;
            }

            if (!connection.identity.TryGetComponent(out NetworkMessageDemoPlayer player))
            {
                continue;
            }

            Button button = Instantiate(_playerButtonPrefab, _playerButtonsRoot);
            int playerId = player.PlayerId;
            SetButtonLabel(button, string.Format(_sendToPlayerLabelFormat, playerId));
            button.onClick.AddListener(() => SendToPlayer(connection, playerId));
            _playerButtons.Add(button);
        }
    }

    private void ClearPlayerButtons()
    {
        foreach (Button button in _playerButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }

        _playerButtons.Clear();
    }

    private static void SetButtonLabel(Button button, string text)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();

        if (label != null)
        {
            label.text = text;
        }
    }
}
