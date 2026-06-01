using Mirror;
using TMPro;
using UnityEngine;

public sealed class NetworkMessageLocalIdentityLable : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private string _serverText = "Server";
    [SerializeField] private string _clientFormat = "Player {0}";
    [SerializeField] private string _disconnectedText = "Disconnected";

    private void OnEnable()
    {
        NetworkClient.OnConnectedEvent += UpdateLabel;
        NetworkClient.OnDisconnectedEvent += UpdateLabel;
        NetworkServer.OnConnectedEvent += OnServerConnectionChanged;
        NetworkServer.OnDisconnectedEvent += OnServerConnectionChanged;

        UpdateLabel();
    }

    private void OnDisable()
    {
        NetworkClient.OnConnectedEvent -= UpdateLabel;
        NetworkClient.OnDisconnectedEvent -= UpdateLabel;
        NetworkServer.OnConnectedEvent -= OnServerConnectionChanged;
        NetworkServer.OnDisconnectedEvent -= OnServerConnectionChanged;
    }

    private void Update()
    {
        UpdateLabel();
    }

    private void OnServerConnectionChanged(NetworkConnectionToClient connection)
    {
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (_label == null)
        {
            return;
        }

        if (NetworkServer.active)
        {
            _label.text = _serverText;
            return;
        }

        NetworkIdentity localPlayer = NetworkClient.localPlayer;

        if (localPlayer != null && localPlayer.TryGetComponent(out NetworkMessageDemoPlayer player))
        {
            _label.text = string.Format(_clientFormat, player.PlayerId);
            return;
        }

        _label.text = _disconnectedText;
    }
}
