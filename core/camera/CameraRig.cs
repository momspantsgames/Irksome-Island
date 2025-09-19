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

	private SpringArm3D _arm = null!;
	private CameraController? _controller;
	[Export] public bool EnableSmoothing { get; set; } = true;
	[Export] public float FollowSpeed { get; set; } = 12f;
	[Export(PropertyHint.Layers3DPhysics)] public uint CollisionMask { get; set; } = 1;
	[Export] public float ArmLength { get; set; } = 1f;

	public Node3D? Target { get; private set; }
	public Vector3 DesiredPivot { get; set; }
	public float DesiredArmLength { get; set; }
	public Camera3D Camera { get; private set; } = null!;

	public override void _Ready()
	{
		_arm = GetNodeOrNull<SpringArm3D>("SpringArm3D") ?? CreateArm();
		Camera = _arm.GetNodeOrNull<Camera3D>("Camera3D") ?? CreateCamera();

		_arm.CollisionMask = CollisionMask;
		_arm.SpringLength = ArmLength;
		DesiredArmLength = ArmLength;
	}

	private SpringArm3D CreateArm()
	{
		var arm = new SpringArm3D { Name = "SpringArm3D", SpringLength = ArmLength, CollisionMask = CollisionMask };
		AddChild(arm);
		return arm;
	}

	private Camera3D CreateCamera()
	{
		var c = new Camera3D { Name = "Camera3D", Current = true };
		_arm.AddChild(c);
		return c;
	}

	public void SetTarget(Node3D? t)
	{
		Target = t;
		_arm.ClearExcludedObjects();

		// ignore the target and the camera rigging
		if (t != null)
		{
			foreach (var co in EnumerateColliders(t))
				_arm.AddExcludedObject(co.GetRid());
		}

		foreach (var co in EnumerateColliders(this))
			_arm.AddExcludedObject(co.GetRid());
	}

	private static IEnumerable<CollisionObject3D> EnumerateColliders(Node n)
	{
		if (n is CollisionObject3D co) yield return co;
		foreach (var c in n.GetChildren())
		{
			foreach (var found in EnumerateColliders(c))
			{
				yield return found;
			}
		}
	}

	public void SetController(CameraController? controller)
	{
		_controller?.OnDetach(this);
		_controller = controller;
		_controller?.OnAttach(this);
		if (_controller != null) // sync shared values
		{
			FollowSpeed = _controller.FollowSpeed;
			DesiredArmLength = Gameplay.Camera.ThirdPersonSpringArmLength;
		}
	}

	public override void _UnhandledInput(InputEvent @event) => _controller?.HandleInput(this, @event);

	public override void _Process(double delta)
	{
		_controller?.UpdateCamera(this, delta);

		// apply smoothing and arm settings
		GlobalPosition = EnableSmoothing
			? CameraController.Smooth(GlobalPosition, DesiredPivot, delta, FollowSpeed)
			: DesiredPivot;

		ArmLength = DesiredArmLength;
		_arm.SpringLength = ArmLength;
		_arm.CollisionMask = CollisionMask;
	}
}
