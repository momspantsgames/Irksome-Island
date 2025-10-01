// Copyright (c) 2025 Momspants Games
//
// MIT License

using Godot;
using IrksomeIsland.Core.Bus;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Props;

namespace IrksomeIsland.Core.Components;

public partial class EquipmentComponent : Node3D
{
	private Marker3D? _backEquipPoint;
	private string _backItemName = "";

	private CharacterBus _bus = null!;
	private Marker3D? _headEquipPoint;
	private string _headItemName = "";
	private Marker3D? _leftHandEquipPoint;
	private string _leftHandItemName = "";
	private Marker3D? _rightHandEquipPoint;

	private string _rightHandItemName = "";
	private NetworkedProp? _rightHandItem;
	private NetworkedProp? _leftHandItem;
	private NetworkedProp? _headItem;
	private NetworkedProp? _backItem;
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
			case var s when s == NodeNames.EquipmentAttachmentPoint.RightHand:
				RightHandItemName = item.Name.ToString();
				break;
			case var s when s == NodeNames.EquipmentAttachmentPoint.LeftHand:
				LeftHandItemName = item.Name.ToString();
				break;
			case var s when s == NodeNames.EquipmentAttachmentPoint.Head:
				HeadItemName = item.Name.ToString();
				break;
			case var s when s == NodeNames.EquipmentAttachmentPoint.Back:
				BackItemName = item.Name.ToString();
				break;
			default:
				return;
		}

		_bus.RaiseEquipped(item, slot);
	}

	private void ApplySlot(string slot, string previousItemName, string nextItemName)
	{
		// Clear previous mapping and unfreeze if we are switching/removing
		if (!string.IsNullOrEmpty(previousItemName) && (string.IsNullOrEmpty(nextItemName) || previousItemName != nextItemName))
		{
			var prev = ResolveItemByName(previousItemName);
			if (prev != null)
			{
				prev.Freeze = false;
			}

			switch (slot)
			{
				case var s when s == NodeNames.EquipmentAttachmentPoint.RightHand: _rightHandItem = null; break;
				case var s when s == NodeNames.EquipmentAttachmentPoint.LeftHand: _leftHandItem = null; break;
				case var s when s == NodeNames.EquipmentAttachmentPoint.Head: _headItem = null; break;
				case var s when s == NodeNames.EquipmentAttachmentPoint.Back: _backItem = null; break;
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
			case var s when s == NodeNames.EquipmentAttachmentPoint.RightHand: _rightHandItem = item; break;
			case var s when s == NodeNames.EquipmentAttachmentPoint.LeftHand: _leftHandItem = item; break;
			case var s when s == NodeNames.EquipmentAttachmentPoint.Head: _headItem = item; break;
			case var s when s == NodeNames.EquipmentAttachmentPoint.Back: _backItem = item; break;
		}
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
			var s when s == NodeNames.EquipmentAttachmentPoint.RightHand => _rightHandEquipPoint,
			var s when s == NodeNames.EquipmentAttachmentPoint.LeftHand => _leftHandEquipPoint,
			var s when s == NodeNames.EquipmentAttachmentPoint.Head => _headEquipPoint,
			var s when s == NodeNames.EquipmentAttachmentPoint.Back => _backEquipPoint,
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
		_rightHandItem.SetDeferred("global_transform", _rightHandEquipPoint.GlobalTransform);
	if (_leftHandItem != null && _leftHandEquipPoint != null)
		_leftHandItem.SetDeferred("global_transform", _leftHandEquipPoint.GlobalTransform);
	if (_headItem != null && _headEquipPoint != null)
		_headItem.SetDeferred("global_transform", _headEquipPoint.GlobalTransform);
	if (_backItem != null && _backEquipPoint != null)
		_backItem.SetDeferred("global_transform", _backEquipPoint.GlobalTransform);
	}
}
