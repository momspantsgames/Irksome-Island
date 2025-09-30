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

namespace IrksomeIsland.Core.Camera;

public partial class CameraRig : Node3D
{
	private const bool EnableSmoothing = false;

	private static readonly uint ArmCollisionMask =
		(CollisionLayers.World | CollisionLayers.Props | CollisionLayers.Dynamic).ToMask();

	private float _armLength = Gameplay.Camera.MinZoom;
	private CameraController? _controller;

	private float _followSpeed = Gameplay.Camera.FollowSpeed;
	private Node3D? _target;
	public Vector3 DesiredPivot { get; set; }
	public float DesiredArmLength { get; set; }

	private SpringArm3D Arm { get; set; } = null!;
	public Camera3D Cam { get; private set; } = null!;

	public override void _Ready()
	{
		Arm = GetNodeOrNull<SpringArm3D>("SpringArm3D") ?? CreateArm();
		Cam = Arm.GetNodeOrNull<Camera3D>("Camera3D") ?? CreateCamera();

		// SpringArm extends along local −Z. Flip it so −Z points away from target.
		Arm.RotationDegrees = new Vector3(0, 180, 0);

		Arm.SpringLength = _armLength;
		Arm.CollisionMask = ArmCollisionMask;
		DesiredArmLength = _armLength;
		DesiredPivot = GlobalPosition; // seed to avoid snapping from origin
	}

	private SpringArm3D CreateArm()
	{
		var arm = new SpringArm3D { Name = "SpringArm3D" };
		arm.Shape = new SphereShape3D { Radius = 0.3f }; // REQUIRED for collisions
		AddChild(arm);
		return arm;
	}

	private Camera3D CreateCamera()
	{
		var c = new Camera3D { Name = "Camera3D" };
		Arm.AddChild(c);
		return c;
	}

	public void SetTarget(Node3D t)
	{
		_target = t;
		Arm.ClearExcludedObjects();
		// Exclude player and rig colliders so camera doesn’t retract on itself
		foreach (var co in EnumerateColliders(t)) Arm.AddExcludedObject(co.GetRid());
		foreach (var co in EnumerateColliders(this)) Arm.AddExcludedObject(co.GetRid());
	}

	private static IEnumerable<CollisionObject3D> EnumerateColliders(Node n)
	{
		if (n is CollisionObject3D co) yield return co;
		var cs = n.GetChildren();
		foreach (var c in cs)
		{
			foreach (var x in EnumerateColliders(c))
				yield return x;
		}
	}

	public void SetController(CameraController? c)
	{
		_controller?.OnDetach(this);
		_controller = c;
		_controller?.OnAttach(this);
		if (c != null) _followSpeed = c.FollowSpeed;
	}

	public override void _UnhandledInput(InputEvent @event) => _controller?.HandleInput(this, @event);

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		_controller?.UpdateCamera(this, delta);

		GlobalPosition = EnableSmoothing
			? CameraController.Smooth(GlobalPosition, DesiredPivot, delta, _followSpeed)
			: DesiredPivot;

		_armLength = DesiredArmLength;
		Arm.SpringLength = _armLength;
		Arm.CollisionMask = ArmCollisionMask;
	}
}
