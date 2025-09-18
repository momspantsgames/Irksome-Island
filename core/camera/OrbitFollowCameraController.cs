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

public partial class OrbitFollowCameraController : CameraController
{
	private float _pitch; // radians
	private float _yaw;   // radians
	public Node3D? Target { get; set; }
	public Vector3 Offset { get; set; } = new(0, 2.0f, -5.0f);
	public float FollowSpeed { get; set; } = 12f;
	public bool LookAtTarget { get; set; } = true;

	public bool RequireRightMouse { get; set; } = true;
	public float Sensitivity { get; set; } = 0.01f;
	public Vector2 PitchLimitsRad { get; set; } = new(-1.2f, 1.2f);
	public float ZoomStep { get; set; } = 0.6f;
	public float MinRadius { get; set; } = 1.5f;
	public float MaxRadius { get; set; } = 8.0f;
	public void SetTarget(Node3D target) => Target = target;

	public override void OnAttach(CameraRig rig)
	{
		if (Target == null || !IsInstanceValid(Target)) return;
		// Initialize yaw/pitch from current rig->target vector
		var to = rig.GlobalTransform.Origin - Target.GlobalTransform.Origin;
		var r = to.Length();
		if (r > 1e-4f)
		{
			_yaw = Mathf.Atan2(to.X, to.Z);
			_pitch = Mathf.Asin(to.Normalized().Y);
			Offset = new Vector3(0, 0, -r); // store radius in Z
		}
	}

	public override void OnDetach(CameraRig rig)
	{
		// nothing
	}

	public override void HandleInput(CameraRig rig, InputEvent e)
	{
		if (e is InputEventMouseMotion mm)
		{
			if (!RequireRightMouse || Input.IsMouseButtonPressed(MouseButton.Right))
			{
				_yaw -= mm.Relative.X * Sensitivity;
				_pitch -= mm.Relative.Y * Sensitivity;
				_pitch = Mathf.Clamp(_pitch, PitchLimitsRad.X, PitchLimitsRad.Y);
			}
		}
		else if (e is InputEventMouseButton mb && mb.Pressed)
		{
			var radius = Mathf.Abs(Offset.Z);
			if (mb.ButtonIndex == MouseButton.WheelUp) radius -= ZoomStep;
			if (mb.ButtonIndex == MouseButton.WheelDown) radius += ZoomStep;
			radius = Mathf.Clamp(radius, MinRadius, MaxRadius);
			Offset = new Vector3(Offset.X, Offset.Y, -radius);
		}
	}

	public override void UpdateCamera(CameraRig rig, double delta)
	{
		if (Target == null || !IsInstanceValid(Target)) return;

		var t = Target.GlobalTransform.Origin;

		var radius = Mathf.Abs(Offset.Z);
		var cp = Mathf.Cos(_pitch);
		var x = radius * cp * Mathf.Sin(_yaw);
		var y = radius * Mathf.Sin(_pitch) + Offset.Y;
		var z = radius * cp * Mathf.Cos(_yaw);

		var desired = t + new Vector3(x, y, z);

		var dt = Mathf.Min((float)delta, 0.05f);
		var a = 1f - Mathf.Exp(-FollowSpeed * dt);
		rig.GlobalPosition = rig.GlobalPosition.Lerp(desired, a);

		if (LookAtTarget)
		{
			var to = t - rig.GlobalTransform.Origin;
			if (to.LengthSquared() > 1e-6f) rig.LookAt(t, Vector3.Up);
		}
	}
}
