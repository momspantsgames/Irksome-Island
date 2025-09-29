// Copyright (c) 2025 Momspants Games
//
// MIT License

using Godot;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Entities;

namespace IrksomeIsland.Core.Components;

public partial class PropPusherComponent : Node
{
	private NetworkedCharacter _character = null!;

	public override void _Ready()
	{
		_character = GetParent<NetworkedCharacter>();
	}

	public void PushRigidBodies()
	{
		for (var i = 0; i < _character.GetSlideCollisionCount(); i++)
		{
			var c = _character.GetSlideCollision(i);
			if (c.GetCollider() is not RigidBody3D rb) continue;

			// compute impulse
			var impulse = -c.GetNormal() * Gameplay.CharacterRigidBodyPushForce;

			if (Multiplayer.IsServer())
			{
				rb.ApplyCentralImpulse(impulse);
				rb.Sleeping = false;
			}
			else
			{
				RpcId(1, nameof(ServerPushProp), rb.GetPath(), impulse);
			}
		}
	}

	[Rpc]
	private void ServerPushProp(NodePath propPath, Vector3 impulse)
	{
		var rb = GetTree().Root.GetNodeOrNull<RigidBody3D>(propPath);
		if (rb == null || rb.Freeze) return;

		rb.ApplyCentralImpulse(impulse);
		rb.Sleeping = false;
	}
}
