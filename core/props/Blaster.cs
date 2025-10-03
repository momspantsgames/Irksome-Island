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
using IrksomeIsland.Core.Application;
using IrksomeIsland.Core.Components;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Entities;
using IrksomeIsland.Core.Game;
using IrksomeIsland.Ui.Prompts;

namespace IrksomeIsland.Core.Props;

public partial class Blaster : NetworkedProp, IInteractable, IUsableProp
{
	private PackedScene? _dartScene;
	private Marker3D? _grip;
	private Marker3D? _muzzle;
	private MultiplayerSynchronizer _sync = null!;

	public void OnInteractServer(Node3D interactor)
	{
		if (!Multiplayer.IsServer()) return;
		if (interactor is not NetworkedCharacter character) return;

		var equip = character.GetNode<EquipmentComponent>(NodeNames.EquipmentComponent);
		equip.Equip(this, NodeNames.EquipmentAttachmentPoint.RightHand);
	}

	public string GetInteractionPrompt() => "Pick up blaster";

	public PackedScene? GetInteractionPromptScene() => GD.Load<PackedScene>(Paths.ForPrompt("BlasterPrompt"));

	public void ConfigureInteractionPrompt(Control ui, InteractionPromptContext ctx)
	{
		if (ui is BlasterPrompt bp)
		{
			var desc = "Press to equip";
			bp.Configure("Blaster", ctx.InteractActionGlyph, desc);
		}
	}

	public void OnPrimaryUseServer(Node userContext)
	{
		if (!Multiplayer.IsServer()) return;
		if (_muzzle == null || _dartScene == null) return;

		var app = GetTree().Root.GetNode<ApplicationManager>(NodeNames.ApplicationManager);
		if (app.ActiveGame is NetworkGame game)
		{
			var xf = _muzzle.GlobalTransform;
			var lin = -_muzzle.GlobalTransform.Basis.Z * Gameplay.DartShootVelocity;
			var ang = Vector3.Zero;
			game.ServerSpawnProjectile(Paths.Props.DartScene, xf, lin, ang);

			// Play fire SFX at muzzle on all peers including shooter
			game.Rpc(nameof(NetworkGame.RpcPlaySfx), Paths.ForSound("impact"), xf.Origin);
		}
	}

	public void OnSecondaryUseServer(Node userContext)
	{
		// alt-fire placeholder
	}


	public override void _Ready()
	{
		base._Ready();

		CollisionLayer = CollisionLayers.Props.ToMask();
		CollisionMask = (CollisionLayers.World | CollisionLayers.Characters | CollisionLayers.Projectiles |
		                 CollisionLayers.Dynamic | CollisionLayers.Props).ToMask();

		_dartScene = GD.Load<PackedScene>(Paths.Props.DartScene);
		_muzzle = GetNode<Marker3D>("Muzzle");
		_grip = GetNode<Marker3D>("Grip");
	}

	public override void _EnterTree()
	{
		SetMultiplayerAuthority(1);

		_sync = new MultiplayerSynchronizer { Name = "PropSync", RootPath = ".." };
		var rc = new SceneReplicationConfig();

		rc.AddProperty(new NodePath(":global_transform"));
		rc.AddProperty(new NodePath(":linear_velocity"));
		rc.AddProperty(new NodePath(":angular_velocity"));

		rc.PropertySetReplicationMode(new NodePath(":global_transform"),
			SceneReplicationConfig.ReplicationMode.OnChange);

		rc.PropertySetReplicationMode(new NodePath(":linear_velocity"),
			SceneReplicationConfig.ReplicationMode.OnChange);

		rc.PropertySetReplicationMode(new NodePath(":angular_velocity"),
			SceneReplicationConfig.ReplicationMode.OnChange);

		_sync.ReplicationConfig = rc;

		AddChild(_sync);
	}
}
