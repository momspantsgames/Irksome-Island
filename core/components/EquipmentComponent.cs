// Copyright (c) 2025 Momspants Games
//
// MIT License

using Godot;
using IrksomeIsland.Core.Application;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Game;

namespace IrksomeIsland.Core.Components;

public partial class EquipmentComponent : Node
{
	private string? _attachNodePath;

	private bool _clientSetupApplied;

	private Guid? _equippedPropId;
	private bool _hadCollision;
	private bool _hadRigid;
	private Transform3D _localOffset = Transform3D.Identity;
	private uint _prevCollisionLayer;
	private uint _prevCollisionMask;
	private bool _prevRigidFreeze;
	private Node3D? _propNode;
	private Node3D? _setupTarget;
	private Node3D? _socket;
	private MultiplayerSynchronizer _sync = null!;

	[Export] public bool Active { get; set; }

	[Export]
	public string? PropGuid
	{
		get => _equippedPropId?.ToString();
		set
		{
			Guid? parsed = null;
			if (!string.IsNullOrEmpty(value) && Guid.TryParse(value, out var g)) parsed = g;
			if (_equippedPropId == parsed) return;
			_equippedPropId = parsed;
			ResolveProp();
		}
	}

	[Export]
	public string? AttachNodePath
	{
		get => _attachNodePath;
		set
		{
			if (_attachNodePath == value) return;
			_attachNodePath = value;
			ResolveSocket();
		}
	}

	[Export] public Transform3D LocalOffset { get; set; }

	public override void _EnterTree()
	{
		_sync = new MultiplayerSynchronizer { Name = "EquipmentSync", RootPath = "." };
		_sync.SetMultiplayerAuthority(GetMultiplayerAuthority());

		var rc = new SceneReplicationConfig();
		rc.AddProperty(new NodePath(":Active"));
		rc.AddProperty(new NodePath(":PropGuid"));
		rc.AddProperty(new NodePath(":AttachNodePath"));
		rc.AddProperty(new NodePath(":LocalOffset"));

		rc.PropertySetReplicationMode(new NodePath(":Active"), SceneReplicationConfig.ReplicationMode.OnChange);
		rc.PropertySetReplicationMode(new NodePath(":PropGuid"), SceneReplicationConfig.ReplicationMode.OnChange);
		rc.PropertySetReplicationMode(new NodePath(":AttachNodePath"), SceneReplicationConfig.ReplicationMode.OnChange);
		rc.PropertySetReplicationMode(new NodePath(":LocalOffset"), SceneReplicationConfig.ReplicationMode.OnChange);
		_sync.ReplicationConfig = rc;

		AddChild(_sync);
	}

	public override void _Ready()
	{
		// no op yet
	}

	public void ServerEquip(Guid propId, string attachNodePath, Transform3D localOffset)
	{
		if (!Multiplayer.IsServer()) return;
		PropGuid = propId.ToString();
		AttachNodePath = attachNodePath;
		LocalOffset = localOffset;
		Active = true;
	}

	public void ServerUnequip()
	{
		if (!Multiplayer.IsServer()) return;
		Active = false;
		PropGuid = null;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Active && _socket != null)
		{
			if (_propNode == null && _equippedPropId != null)
				ResolveProp();

			if (_propNode != null)
			{
				if (!_clientSetupApplied)
					ApplyClientSetup(_propNode);

				_propNode.GlobalTransform = _socket.GlobalTransform * _localOffset;
			}
		}
		else if (_clientSetupApplied && _setupTarget != null)
		{
			RestoreClientState(_setupTarget);
		}
	}

	private IrkGame? GetActiveGame()
	{
		var app = GetTree().Root.GetNodeOrNull<ApplicationManager>(NodeNames.ApplicationManager);
		return app?.ActiveGame;
	}

	private void ResolveSocket()
	{
		_socket = null;
		if (string.IsNullOrEmpty(_attachNodePath)) return;
		try
		{
			_socket = GetParent().GetNodeOrNull<Node3D>(_attachNodePath!);
		} catch
		{
			_socket = null;
		}
	}

	private void ResolveProp()
	{
		if (_clientSetupApplied && _setupTarget != null && _setupTarget != _propNode)
			RestoreClientState(_setupTarget);

		_propNode = null;
		if (_equippedPropId == null) return;
		var game = GetActiveGame();
		if (game != null && game.TryGetProp(_equippedPropId.Value, out var prop))
		{
			_propNode = prop;
		}
	}

	private void ApplyClientSetup(Node3D target)
	{
		_setupTarget = target;
		_hadRigid = false;
		_hadCollision = false;

		if (target is RigidBody3D rigid)
		{
			_hadRigid = true;
			_prevRigidFreeze = rigid.Freeze;
			rigid.Freeze = true;
		}

		if (target is CollisionObject3D coll)
		{
			_hadCollision = true;
			_prevCollisionLayer = coll.CollisionLayer;
			_prevCollisionMask = coll.CollisionMask;
			coll.CollisionLayer = 0;
			coll.CollisionMask = 0;
		}

		_clientSetupApplied = true;
	}

	private void RestoreClientState(Node3D target)
	{
		if (target is RigidBody3D rigid && _hadRigid)
		{
			rigid.Freeze = _prevRigidFreeze;
		}

		if (target is CollisionObject3D coll && _hadCollision)
		{
			coll.CollisionLayer = _prevCollisionLayer;
			coll.CollisionMask = _prevCollisionMask;
		}

		_clientSetupApplied = false;
		_setupTarget = null;
		_hadRigid = false;
		_hadCollision = false;
	}
}
