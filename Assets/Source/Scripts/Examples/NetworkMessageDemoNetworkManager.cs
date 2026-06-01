using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public sealed class NetworkMessageDemoNetworkManager : NetworkManager
{
    private readonly List<NetworkConnectionToClient> _playerConnections = new();

    public event Action ServerPlayersChanged;

    public IReadOnlyList<NetworkConnectionToClient> PlayerConnections => _playerConnections;

    public override void OnServerAddPlayer(NetworkConnectionToClient connection)
    {
        GameObject player = Instantiate(playerPrefab);

        if (player.TryGetComponent(out NetworkMessageDemoPlayer demoPlayer))
        {
            demoPlayer.SetPlayerId(connection.connectionId);
        }

        NetworkServer.AddPlayerForConnection(connection, player);
        _playerConnections.Add(connection);
        ServerPlayersChanged?.Invoke();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient connection)
    {
        _playerConnections.Remove(connection);
        base.OnServerDisconnect(connection);
        ServerPlayersChanged?.Invoke();
    }

    public override void OnStopServer()
    {
        _playerConnections.Clear();
        ServerPlayersChanged?.Invoke();
        base.OnStopServer();
    }
}
