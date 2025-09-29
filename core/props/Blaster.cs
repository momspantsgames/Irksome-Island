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
using IrksomeIsland.Core.Constants;

namespace IrksomeIsland.Core.Props;

public partial class Blaster : RigidBody3D
{
	private PackedScene? _dartScene;
	private bool _equipped;
	private Node3D? _hand;

	private NodePath _handPath = "";
	private Transform3D _localOffset = Transform3D.Identity;
	private Marker3D? _muzzle;
	private Area3D _pickup = null!;
	private uint _savedLayer, _savedMask;

	public override void _Ready()
	{
		_dartScene = GD.Load<PackedScene>(Paths.Props.DartScene);
		_muzzle = GetNode<Marker3D>("Muzzle");
		_pickup = GetNode<Area3D>("PickupArea");

		_savedLayer = CollisionLayer;
		_savedMask = CollisionMask;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_equipped)
		{
			if (_hand == null && _handPath != "")
				_hand = GetTree().Root.GetNodeOrNull<Node3D>(_handPath);

			if (_hand != null)
				GlobalTransform = _hand.GlobalTransform * _localOffset;

			LinearVelocity = Vector3.Zero;
			AngularVelocity = Vector3.Zero;
		}
	}

	// Call from character (server or single-player). Clients will RPC to server.
	public void RequestEquip(NodePath handSocketPath, Transform3D localOffset)
	{
		if (!Multiplayer.IsServer())
		{
			RpcId(1, nameof(ServerEquip), handSocketPath, localOffset);
			return;
		}

		ServerEquip(handSocketPath, localOffset);
	}

	public void RequestDrop()
	{
		if (!Multiplayer.IsServer())
		{
			RpcId(1, nameof(ServerDrop));
			return;
		}

		ServerDrop();
	}

	public void Fire()
	{
		if (!_equipped) return;
		if (_dartScene == null || _muzzle == null) return;

		var dart = _dartScene.Instantiate<Dart>();
		GetTree().CurrentScene.AddChild(dart);
		dart.GlobalTransform = _muzzle.GlobalTransform;
		dart.LinearVelocity = -_muzzle.GlobalBasis.Z * Gameplay.DartShootVelocity;
	}

	[Rpc]
	private void ServerEquip(NodePath handSocketPath, Transform3D localOffset)
	{
		Rpc(nameof(ClEquip), handSocketPath, localOffset);
		ClEquip(handSocketPath, localOffset);
	}

	[Rpc]
	private void ServerDrop()
	{
		Rpc(nameof(ClientDrop));
		ClientDrop();
	}

	[Rpc]
	private void ClEquip(NodePath handSocketPath, Transform3D localOffset)
	{
		_equipped = true;

		_handPath = handSocketPath;
		_localOffset = localOffset;

		_hand = GetTree().Root.GetNodeOrNull<Node3D>(_handPath);

		Freeze = true;
		_pickup.Monitoring = false;
		CollisionLayer = 0;
		CollisionMask = 0;
	}

	[Rpc]
	private void ClientDrop()
	{
		_equipped = false;

		Freeze = false;
		_pickup.Monitoring = true;
		CollisionLayer = _savedLayer;
		CollisionMask = _savedMask;

		_hand = null;
		_handPath = "";
		_localOffset = Transform3D.Identity;
	}
}
