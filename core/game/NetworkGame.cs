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
using IrksomeIsland.Core.Constants;

namespace IrksomeIsland.Core.Game;

public partial class NetworkGame(GameConfiguration config) : IrkGame(config)
{
	private MultiplayerSpawner _playerSpawner = null!;
	private MultiplayerSpawner _propSpawner = null!;

	public override void _Ready()
	{
		base._Ready();

		_playerSpawner = GetOrCreate<MultiplayerSpawner>(NodeNames.PlayerSpawner);
		_playerSpawner.SpawnPath = $"../{NodeNames.PlayersRoot}";
		_playerSpawner.SpawnFunction = new Callable(this, nameof(SpawnPlayerFromData));

		_propSpawner = GetOrCreate<MultiplayerSpawner>(NodeNames.PropSpawner);
		_propSpawner.SpawnPath = $"../{NodeNames.PropsRoot}";
		_propSpawner.SpawnFunction = new Callable(this, nameof(SpawnPropFromData));
	}

	public Node3D ServerAddPlayer(int peerId, string scenePath)
	{
		if (!Multiplayer.IsServer()) throw new InvalidOperationException("Server only");
		var data = new Dictionary { { "path", scenePath }, { "peer", peerId } };
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

		var ps = ResourceLoader.Load<PackedScene>(path);
		var node = ps.Instantiate<Node3D>();
		node.Name = $"Player_{peer}";
		node.SetMultiplayerAuthority(peer);

		Players[peer] = node;
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
