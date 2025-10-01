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

public partial class Blaster : NetworkedProp, IInteractable
{
	private PackedScene? _dartScene;
	private Marker3D? _muzzle;
	private MultiplayerSynchronizer _sync = null!;

	public void OnInteractServer(Node3D interactor)
	{
		throw new NotImplementedException();
	}

	public string GetInteractionPrompt() => throw new NotImplementedException();

	public override void _Ready()
	{
		base._Ready();

		CollisionLayer = CollisionLayers.Props.ToMask();
		CollisionMask = (CollisionLayers.World | CollisionLayers.Characters | CollisionLayers.Projectiles |
		                 CollisionLayers.Dynamic | CollisionLayers.Props).ToMask();

		_dartScene = GD.Load<PackedScene>(Paths.Props.DartScene);
		_muzzle = GetNode<Marker3D>("Muzzle");
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
