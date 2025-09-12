// Copyright (c) 2025 Momspants Games
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Godot;

namespace IrksomeIsland.Core.Application;

public partial class NetworkManager : Node {

    public enum ConnectionState { Disconnected, Connecting, Connected, Failed }
    public enum NetworkRole { None, Host, Client, DedicatedServer }

    private ENetMultiplayerPeer? _peer;

    public NetworkRole CurrentRole { get; private set; } = NetworkRole.None;
    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

    public bool IsServer => CurrentRole is NetworkRole.Host or NetworkRole.DedicatedServer;
    public bool IsClient => CurrentRole == NetworkRole.Client;

    public static bool IsHeadless => DisplayServer.GetName() == "headless";

	public event Action<long>? PeerJoined;      // server side
    public event Action<long>? PeerLeft;        // server side
    public event Action? Connected;             // client side
    public event Action<string>? ConnectFailed; // client side
    public event Action? ServerWentAway;        // client side

    public override void _Ready()
    {
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.ServerDisconnected += OnServerDisconnected;

        Logger.Log("NetworkManager initialized");
    }

    public override void _ExitTree()
    {
        Disconnect();
    }

    public void StartServer(int port, int maxPlayers)
    {
        if (!EnsureDisconnected("Cannot start server")) return;

        _peer = new ENetMultiplayerPeer();
        var err = _peer.CreateServer(port, maxPlayers);
        if (err != Error.Ok)
        {
            FailConnect($"Failed to create server: {err}");
            return;
        }

        BindPeer(_peer, NetworkRole.Host, ConnectionState.Connected);
        Logger.Log($"Server started on port {port} with max players {maxPlayers}");
    }

    public void StartDedicatedServer(int port, int maxPlayers)
    {
        if (!EnsureDisconnected("Cannot start dedicated server")) return;

        _peer = new ENetMultiplayerPeer();
        var err = _peer.CreateServer(port, maxPlayers);
        if (err != Error.Ok)
        {
            FailConnect($"Failed to create dedicated server: {err}");
            return;
        }

        BindPeer(_peer, NetworkRole.DedicatedServer, ConnectionState.Connected);
        Logger.Log($"Dedicated server started on port {port} with max players {maxPlayers}");
    }

    public void ConnectToServer(string address, int port)
    {
        if (!EnsureDisconnected("Cannot connect")) return;

        _peer = new ENetMultiplayerPeer();
        State = ConnectionState.Connecting;

        var err = _peer.CreateClient(address, port);
        if (err != Error.Ok)
        {
            FailConnect($"Failed to create client: {err}");
            return;
        }

        Multiplayer.MultiplayerPeer = _peer;
        CurrentRole = NetworkRole.Client;

        Logger.Log($"Attempting to connect to {address}:{port}");
    }

    public void Disconnect()
    {
        var peer = _peer;
        _peer = null;
        Multiplayer.MultiplayerPeer = null;
        CurrentRole = NetworkRole.None;
        State = ConnectionState.Disconnected;

        if (peer != null)
        {
	        try { peer.Close(); } catch { /* ignore */ }
        }

        Logger.Log("Disconnected from network");
    }

    public bool IsPlayerConnected(long playerId)
    {
        if (Multiplayer.MultiplayerPeer == null || State != ConnectionState.Connected)
            return false;

        if (playerId == Multiplayer.GetUniqueId())
            return true;

        var peers = Multiplayer.GetPeers();
        if (peers == null) return false;
        foreach (var p in peers)
            if (p == (int)playerId) return true;

        return false;
    }

    public IReadOnlyList<int> CurrentPeers() => Multiplayer.GetPeers();

    private void BindPeer(ENetMultiplayerPeer peer, NetworkRole role, ConnectionState state)
    {
        Multiplayer.MultiplayerPeer = peer;
        CurrentRole = role;
        State = state;
    }

    private bool EnsureDisconnected(string reasonIfBusy)
    {
        if (State == ConnectionState.Disconnected && Multiplayer.MultiplayerPeer == null)
            return true;

        Logger.Log($"{reasonIfBusy}: already connected or connecting", Logger.LogLevel.Warning);
        return false;
    }

    private void FailConnect(string message)
    {
        State = ConnectionState.Failed;
        CurrentRole = NetworkRole.None;
        Logger.Log(message, Logger.LogLevel.Error);
        ConnectFailed?.Invoke(message);
    }

    private void OnPeerConnected(long id)
    {
        Logger.Log($"Peer connected: {id}");
        if (IsServer) PeerJoined?.Invoke(id);
    }

    private void OnPeerDisconnected(long id)
    {
        Logger.Log($"Peer disconnected: {id}");
        if (IsServer) PeerLeft?.Invoke(id);
    }

    private void OnConnectedToServer()
    {
        State = ConnectionState.Connected;
        Logger.Log("Successfully connected to server");
        Connected?.Invoke();
    }

    private void OnConnectionFailed()
    {
        FailConnect("Connection failed");
    }

    private void OnServerDisconnected()
    {
        Logger.Log("Disconnected from server");
        State = ConnectionState.Disconnected;
        CurrentRole = NetworkRole.None;
        ServerWentAway?.Invoke();
    }
}
