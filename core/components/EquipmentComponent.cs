// Copyright (c) 2025 Momspants Games
//
// MIT License

using Godot;
using Godot.Collections;
using IrksomeIsland.Core.Bus;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Entities;
using IrksomeIsland.Core.Props;

namespace IrksomeIsland.Core.Components;

public partial class EquipmentComponent : Node3D
{
	private Marker3D? _backEquipPoint;
	private NetworkedProp? _backItem;
	private string _backItemName = "";

	private CharacterBus _bus = null!;
	private Marker3D? _headEquipPoint;
	private NetworkedProp? _headItem;
	private string _headItemName = "";
	private Marker3D? _leftHandEquipPoint;
	private NetworkedProp? _leftHandItem;
	private string _leftHandItemName = "";
	private Marker3D? _rightHandEquipPoint;
	private NetworkedProp? _rightHandItem;

	private string _rightHandItemName = "";
	private MultiplayerSynchronizer _sync = null!;

	[Export]
	public string RightHandItemName
	{
		get => _rightHandItemName;
		set
		{
			if (_rightHandItemName == value) return;
			var previous = _rightHandItemName;
			_rightHandItemName = value;
			ApplySlot(NodeNames.EquipmentAttachmentPoint.RightHand, previous, _rightHandItemName);
		}
	}

	[Export]
	public string LeftHandItemName
	{
		get => _leftHandItemName;
		set
		{
			if (_leftHandItemName == value) return;
			var previous = _leftHandItemName;
			_leftHandItemName = value;
			ApplySlot(NodeNames.EquipmentAttachmentPoint.LeftHand, previous, _leftHandItemName);
		}
	}

	[Export]
	public string HeadItemName
	{
		get => _headItemName;
		set
		{
			if (_headItemName == value) return;
			var previous = _headItemName;
			_headItemName = value;
			ApplySlot(NodeNames.EquipmentAttachmentPoint.Head, previous, _headItemName);
		}
	}

	[Export]
	public string BackItemName
	{
		get => _backItemName;
		set
		{
			if (_backItemName == value) return;
			var previous = _backItemName;
			_backItemName = value;
			ApplySlot(NodeNames.EquipmentAttachmentPoint.Back, previous, _backItemName);
		}
	}

	private void Attach(NetworkedProp item, string slot) => Equip(item, slot);

	public void BindTo(CharacterBus bus)
	{
		_bus = bus;
		_bus.EquipRequested += Attach;
		_bus.PrimaryUseRequested += OnPrimaryUse;
		_bus.SecondaryUseRequested += OnSecondaryUse;
		_bus.DropRequested += OnDropRequested;
	}

	public void BindTo(Node modelScene)
	{
		_backEquipPoint = (Marker3D)modelScene.FindChild(NodeNames.EquipmentAttachmentPoint.Back);
		_headEquipPoint = (Marker3D)modelScene.FindChild(NodeNames.EquipmentAttachmentPoint.Head);
		_leftHandEquipPoint = (Marker3D)modelScene.FindChild(NodeNames.EquipmentAttachmentPoint.LeftHand);
		_rightHandEquipPoint = (Marker3D)modelScene.FindChild(NodeNames.EquipmentAttachmentPoint.RightHand);
	}

	public override void _EnterTree()
	{
		_sync = new MultiplayerSynchronizer { Name = "EquipmentSync", RootPath = ".." };
		_sync.SetMultiplayerAuthority(GetMultiplayerAuthority());

		var rc = new SceneReplicationConfig();

		// Replicate equipped item names per slot
		rc.AddProperty(new NodePath(":RightHandItemName"));
		rc.PropertySetReplicationMode(new NodePath(":RightHandItemName"),
			SceneReplicationConfig.ReplicationMode.OnChange);

		rc.AddProperty(new NodePath(":LeftHandItemName"));
		rc.PropertySetReplicationMode(new NodePath(":LeftHandItemName"),
			SceneReplicationConfig.ReplicationMode.OnChange);

		rc.AddProperty(new NodePath(":HeadItemName"));
		rc.PropertySetReplicationMode(new NodePath(":HeadItemName"), SceneReplicationConfig.ReplicationMode.OnChange);

		rc.AddProperty(new NodePath(":BackItemName"));
		rc.PropertySetReplicationMode(new NodePath(":BackItemName"), SceneReplicationConfig.ReplicationMode.OnChange);

		_sync.ReplicationConfig = rc;

		AddChild(_sync);
	}

	public void Equip(NetworkedProp item, string slot)
	{
		if (Multiplayer.IsServer())
		{
			SetSlot(item, slot);
		}
		else
		{
			RpcId(1, nameof(RpcRequestEquip), item.Name.ToString(), slot);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcRequestEquip(string itemName, string slot)
	{
		if (!Multiplayer.IsServer()) return;
		var item = ResolveItemByName(itemName);
		if (item == null) return;
		SetSlot(item, slot);
	}

	private void SetSlot(NetworkedProp item, string slot)
	{
		switch (slot)
		{
			case NodeNames.EquipmentAttachmentPoint.RightHand:
				RightHandItemName = item.Name.ToString();
				break;
			case NodeNames.EquipmentAttachmentPoint.LeftHand:
				LeftHandItemName = item.Name.ToString();
				break;
			case NodeNames.EquipmentAttachmentPoint.Head:
				HeadItemName = item.Name.ToString();
				break;
			case NodeNames.EquipmentAttachmentPoint.Back:
				BackItemName = item.Name.ToString();
				break;
			default:
				return;
		}
	}

	private void OnPrimaryUse()
	{
		if (Multiplayer.IsServer())
			PerformPrimaryUse();
		else
			RpcId(1, nameof(RpcRequestPrimaryUse));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcRequestPrimaryUse()
	{
		if (!Multiplayer.IsServer()) return;
		PerformPrimaryUse();
	}

	private void PerformPrimaryUse()
	{
		var item = _rightHandItem ?? _leftHandItem;
		(item as IUsableProp)?.OnPrimaryUseServer(this);
	}

	private void OnSecondaryUse()
	{
		if (Multiplayer.IsServer())
			PerformSecondaryUse();
		else
			RpcId(1, nameof(RpcRequestSecondaryUse));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcRequestSecondaryUse()
	{
		if (!Multiplayer.IsServer()) return;
		PerformSecondaryUse();
	}

	private void PerformSecondaryUse()
	{
		var item = _rightHandItem ?? _leftHandItem;
		(item as IUsableProp)?.OnSecondaryUseServer(this);
	}

	private void OnDropRequested()
	{
		if (Multiplayer.IsServer())
			PerformDrop();
		else
			RpcId(1, nameof(RpcRequestDrop));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcRequestDrop()
	{
		if (!Multiplayer.IsServer()) return;
		PerformDrop();
	}

	private void PerformDrop()
	{
		var item = _rightHandItem ?? _leftHandItem;
		if (item == null) return;

		// Clear slot names so ApplySlot unfreezes and stops following
		if (_rightHandItem == item) RightHandItemName = string.Empty;
		else if (_leftHandItem == item) LeftHandItemName = string.Empty;

		// Place on the floor in front of the character
		var character = GetParent<NetworkedCharacter>();
		if (character == null) return;

		var forward = -character.GlobalTransform.Basis.Z;
		forward.Y = 0f;
		if (forward.LengthSquared() < 0.0001f) forward = new Vector3(0, 0, -1);
		forward = forward.Normalized();

		var start = character.GlobalTransform.Origin + forward * 1.0f + new Vector3(0, 1.0f, 0);
		var end = start + new Vector3(0, -3.0f, 0);

		var space = GetWorld3D().DirectSpaceState;
		var rayQuery = PhysicsRayQueryParameters3D.Create(start, end);
		rayQuery.CollisionMask = (uint)(CollisionLayers.World | CollisionLayers.Props | CollisionLayers.Dynamic);
		rayQuery.Exclude = new Array<Rid> { character.GetRid(), item.GetRid() };
		var hit = space.IntersectRay(rayQuery);

		var dropPos = hit.Count > 0 ? (Vector3)hit["position"] : end;
		var up = Vector3.Up;
		var basis = new Basis(new Quaternion(up, Mathf.Atan2(forward.X, -forward.Z)));
		var xf = new Transform3D(basis, dropPos + up * 0.05f);

		item.SetMultiplayerAuthority(1);
		item.Freeze = false;
		item.LinearVelocity = Vector3.Zero;
		item.AngularVelocity = Vector3.Zero;
		item.GlobalTransform = xf;
	}

	private void ApplySlot(string slot, string previousItemName, string nextItemName)
	{
		// Clear previous mapping and unfreeze if we are switching/removing
		if (!string.IsNullOrEmpty(previousItemName) &&
		    (string.IsNullOrEmpty(nextItemName) || previousItemName != nextItemName))
		{
			var prev = ResolveItemByName(previousItemName);
			if (prev != null)
			{
				prev.Freeze = false;
				// Notify unequipped when we clear a previous item from a slot
				_bus.RaiseUnequipped(prev, slot);
			}

			switch (slot)
			{
				case NodeNames.EquipmentAttachmentPoint.RightHand: _rightHandItem = null; break;
				case NodeNames.EquipmentAttachmentPoint.LeftHand: _leftHandItem = null; break;
				case NodeNames.EquipmentAttachmentPoint.Head: _headItem = null; break;
				case NodeNames.EquipmentAttachmentPoint.Back: _backItem = null; break;
			}
		}

		if (string.IsNullOrEmpty(nextItemName)) return;
		var item = ResolveItemByName(nextItemName);
		if (item == null) return;

		// Freeze physics; server will drive transform to follow equip point
		item.Freeze = true;
		item.LinearVelocity = Vector3.Zero;
		item.AngularVelocity = Vector3.Zero;

		switch (slot)
		{
			case NodeNames.EquipmentAttachmentPoint.RightHand: _rightHandItem = item; break;
			case NodeNames.EquipmentAttachmentPoint.LeftHand: _leftHandItem = item; break;
			case NodeNames.EquipmentAttachmentPoint.Head: _headItem = item; break;
			case NodeNames.EquipmentAttachmentPoint.Back: _backItem = item; break;
		}

		// Notify listeners (animation, etc.) on both server and clients when slot is applied
		_bus.RaiseEquipped(item, slot);
	}

	private NetworkedProp? ResolveItemByName(string name)
	{
		var root = GetTree().Root;
		var node = root.FindChild(name, true, false);
		return node as NetworkedProp;
	}

	private Marker3D? GetEquipPoint(string slot)
	{
		return slot switch
		{
			NodeNames.EquipmentAttachmentPoint.RightHand => _rightHandEquipPoint,
			NodeNames.EquipmentAttachmentPoint.LeftHand => _leftHandEquipPoint,
			NodeNames.EquipmentAttachmentPoint.Head => _headEquipPoint,
			NodeNames.EquipmentAttachmentPoint.Back => _backEquipPoint,
			_ => null
		};
	}

	public override void _Process(double delta)
	{
		// Server and owning client drive equipped items to follow attachment points by setting global transforms
		var parent = GetParent();
		var isOwnerClient = parent != null && parent.IsMultiplayerAuthority() && !Multiplayer.IsServer();
		if (!Multiplayer.IsServer() && !isOwnerClient) return;

		if (_rightHandItem != null && _rightHandEquipPoint != null)
		{
			_rightHandItem.SetDeferred("global_transform",
				ComputeAlignedTransform(_rightHandItem, _rightHandEquipPoint));
		}

		if (_leftHandItem != null && _leftHandEquipPoint != null)
			_leftHandItem.SetDeferred("global_transform", ComputeAlignedTransform(_leftHandItem, _leftHandEquipPoint));

		if (_headItem != null && _headEquipPoint != null)
			_headItem.SetDeferred("global_transform", ComputeAlignedTransform(_headItem, _headEquipPoint));

		if (_backItem != null && _backEquipPoint != null)
			_backItem.SetDeferred("global_transform", ComputeAlignedTransform(_backItem, _backEquipPoint));
	}

	private static Transform3D ComputeAlignedTransform(NetworkedProp item, Marker3D attachPoint)
	{
		var nodePath = item.AlignmentNodePath;
		Node3D? grip = null;
		if (!nodePath.IsEmpty)
			grip = item.GetNodeOrNull<Node3D>(nodePath);

		if (grip == null)
			return attachPoint.GlobalTransform * item.AlignmentOffset;

		// We want: (item.Global * grip.Local) == (attach.Global * offset)
		// => item.Global = attach.Global * offset * inverse(grip.Local)
		var inverseLocalGrip = grip.Transform.AffineInverse();
		return attachPoint.GlobalTransform * item.AlignmentOffset * inverseLocalGrip;
	}

	public bool HasUsableEquippedItem()
	{
		var item = _rightHandItem ?? _leftHandItem;
		return item is IUsableProp;
	}
}
