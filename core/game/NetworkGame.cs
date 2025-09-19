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
using Godot.Collections;
using IrksomeIsland.Core.Application;
using IrksomeIsland.Core.Camera;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Entities;
using IrksomeIsland.Core.Entities.States;

namespace IrksomeIsland.Core.Game;

public partial class NetworkGame(GameConfiguration config) : IrkGame(config)
{
	private MultiplayerSpawner _playerSpawner = null!;
	private MultiplayerSpawner _propSpawner = null!;
	protected NetworkManager Network => App.NetworkManager;

	public override void _Ready()
	{
		base._Ready();

		Name = "NetworkGame";

		_playerSpawner = GetOrCreate<MultiplayerSpawner>(NodeNames.PlayerSpawner);
		_playerSpawner.SpawnPath = $"../{NodeNames.PlayersRoot}";
		_playerSpawner.SpawnFunction = new Callable(this, nameof(SpawnPlayerFromData));

		_propSpawner = GetOrCreate<MultiplayerSpawner>(NodeNames.PropSpawner);
		_propSpawner.SpawnPath = $"../{NodeNames.PropsRoot}";
		_propSpawner.SpawnFunction = new Callable(this, nameof(SpawnPropFromData));
	}

	public override void StartGame()
	{
		base.StartGame();

		// server spawns itself
		if (Network.IsServer)
		{
			ServerAddPlayer(Multiplayer.GetUniqueId(), Paths.PlayerCharacterScene, Configuration.LocalPlayerModel);
		}

		Network.PeerJoined += OnPeerJoined;
		Network.PeerLeft += OnPeerLeft;
		Network.Connected += OnClientConnected;
		Network.ServerWentAway += OnServerWentAway;
	}

	public override void StopGame()
	{
		Network.PeerJoined -= OnPeerJoined;
		Network.PeerLeft -= OnPeerLeft;
		Network.Connected -= OnClientConnected;
		Network.ServerWentAway -= OnServerWentAway;

		base.StopGame();
	}

	private void OnPeerJoined(long id)
	{
		if (!Network.IsServer) return;

		ServerAddPlayer((int)id, Paths.PlayerCharacterScene, Configuration.LocalPlayerModel);
	}

	private void OnPeerLeft(long id)
	{
		if (!Network.IsServer) return;
		ServerRemovePlayer((int)id);
	}

	private void OnClientConnected()
	{
		// client waits; server will spawn and MultiplayerSpawner will sync
	}

	private void OnServerWentAway()
	{
		var newConfig = new GameConfiguration
		{
			WorldName = NodeNames.WorldMain,
			GameType = GameType.Attract
		};

		App.StartGame(newConfig);
	}

	public Node3D ServerAddPlayer(int peerId, string scenePath, CharacterModelType? configurationLocalPlayerModel)
	{
		if (!Multiplayer.IsServer()) throw new InvalidOperationException("Server only");
		var xf = new Transform3D(Basis.Identity, GetPlayerSpawnPoint());
		var state = CharacterStateType.Idle;
		var model = configurationLocalPlayerModel ?? CharacterModelType.CharacterA;
		var data = new Dictionary
		{
			{ "path", scenePath },
			{ "peer", peerId },
			{ "spawn", xf },
			{ "state", (int)state },
			{ "model", (int)model }
		};

		var n = _playerSpawner.Spawn(data) as Node3D
		        ?? throw new InvalidOperationException("Spawn failed");

		Players[peerId] = n;
		return n;
	}

	public void ServerRemovePlayer(int peerId)
	{
		if (!Multiplayer.IsServer()) return;
		if (Players.Remove(peerId, out var n)) n.QueueFree();
	}

	public Node3D ServerAddProp(Guid id, string scenePath)
	{
		if (!Multiplayer.IsServer()) throw new InvalidOperationException("Server only");
		var data = new Dictionary { { "path", scenePath }, { "id", id.ToString() } };
		var n = _propSpawner.Spawn(data) as Node3D
		        ?? throw new InvalidOperationException("Spawn failed");

		Props[id] = n;
		return n;
	}

	public void ServerRemoveProp(Guid id)
	{
		if (!Multiplayer.IsServer()) return;
		if (Props.Remove(id, out var n)) n.QueueFree();
	}

	// Spawn callbacks run on ALL peers.
	// Must RETURN a node NOT yet in the tree. Spawner adds it under SpawnPath.
	private Node SpawnPlayerFromData(Variant dv)
	{
		var d = (Dictionary)dv;
		var path = (string)d["path"];
		var peer = (int)d["peer"];
		var spawn = (Transform3D)d["spawn"];
		var state = (CharacterStateType)(int)d["state"];
		var model = (CharacterModelType)(int)d["model"];

		var ps = ResourceLoader.Load<PackedScene>(path);
		var node = ps.Instantiate<Node3D>();
		node.Name = $"Player_{peer}";
		node.SetMultiplayerAuthority(peer);
		node.SetDeferred("global_transform", spawn);

		if (node is NetworkedCharacter nc)
			nc.Bootstrap(state, model);

		Players[peer] = node;

		// if this is a local player, they need a camera controller
		if (peer == Multiplayer.GetUniqueId() && CameraRig != null)
		{
			var follow = new OrbitFollowCameraController();
			CameraRig.SetController(follow);

			// defer this until the player is in the tree
			node.TreeEntered += () =>
			{
				follow.SetTarget(node, CameraRig);
				CameraRig.Camera.MakeCurrent();
			};
		}

		return node;
	}

	private Node SpawnPropFromData(Variant dv)
	{
		var d = (Dictionary)dv;
		var path = (string)d["path"];
		var id = Guid.Parse((string)d["id"]);

		var ps = ResourceLoader.Load<PackedScene>(path);
		var node = ps.Instantiate<Node3D>();
		node.Name = $"Prop_{id.ToString()[..8]}";

		Props[id] = node;
		return node;
	}
}
