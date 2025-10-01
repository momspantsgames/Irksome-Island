// Copyright (c) 2025 Momspants Games
//
// MIT License

using Godot;
using IrksomeIsland.Core.Bus;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Props;

namespace IrksomeIsland.Core.Components;

public partial class EquipmentComponent : Node
{
	private Marker3D? _backEquipPoint;

	private CharacterBus _bus = null!;
	private Marker3D? _headEquipPoint;
	private Marker3D? _leftHandEquipPoint;
	private Marker3D? _rightHandEquipPoint;
	private MultiplayerSynchronizer _sync = null!;

	private void Attach(NetworkedProp item, string slot)
	{
		_bus.RaiseEquipped(item, slot);
	}

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

		// todo: replication stuff

		_sync.ReplicationConfig = rc;

		AddChild(_sync);
	}
}
