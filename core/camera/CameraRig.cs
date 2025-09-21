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

namespace IrksomeIsland.Core.Camera;

public partial class CameraRig : Node3D
{
	private CameraController? _controller;
	public uint ArmCollisionMask = 1u << 0 | 1u << 1 | 1u << 2;
	public float ArmLength = 5.5f;
	public float DesiredArmLength;       // boom length
	public Vector3 DesiredPivot;         // where the rig should sit
	public bool EnableSmoothing = false; // start OFF to remove “animate back”
	public float FollowSpeed = 12f;

	public Node3D? Target;

	public SpringArm3D Arm { get; private set; } = null!;
	public Camera3D Cam { get; private set; } = null!;

	public override void _Ready()
	{
		Arm = GetNodeOrNull<SpringArm3D>("SpringArm3D") ?? CreateArm();
		Cam = Arm.GetNodeOrNull<Camera3D>("Camera3D") ?? CreateCamera();

		// SpringArm extends along local −Z. Flip it so −Z points away from target.
		Arm.RotationDegrees = new Vector3(0, 180, 0);

		Arm.SpringLength = ArmLength;
		Arm.CollisionMask = ArmCollisionMask;
		DesiredArmLength = ArmLength;
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
		Target = t;
		Arm.ClearExcludedObjects();
		// Exclude player and rig colliders so camera doesn’t retract on itself
		foreach (var co in EnumerateColliders(t)) Arm.AddExcludedObject(co.GetRid());
		foreach (var co in EnumerateColliders(this)) Arm.AddExcludedObject(co.GetRid());
	}

	private static IEnumerable<CollisionObject3D> EnumerateColliders(Node n)
	{
		if (n is CollisionObject3D co) yield return co;
		foreach (var c in n.GetChildren())
		foreach (var x in EnumerateColliders(c))
			yield return x;
	}

	public void SetController(CameraController? c)
	{
		_controller?.OnDetach(this);
		_controller = c;
		_controller?.OnAttach(this);
		if (c != null) FollowSpeed = c.FollowSpeed;
	}

	public override void _UnhandledInput(InputEvent @event) => _controller?.HandleInput(this, @event);

	public override void _Process(double delta)
	{
		_controller?.UpdateCamera(this, delta);

		GlobalPosition = EnableSmoothing
			? CameraController.Smooth(GlobalPosition, DesiredPivot, delta, FollowSpeed)
			: DesiredPivot;

		ArmLength = DesiredArmLength;
		Arm.SpringLength = ArmLength;
		Arm.CollisionMask = ArmCollisionMask;
	}
}
