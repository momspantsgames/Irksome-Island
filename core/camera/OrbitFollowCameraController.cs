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
	private bool _rotating;

	private Node3D? _target;
	private float _yaw, _pitch;
	public float MaxArm = 8.0f;
	public float MinArm = 1.8f;
	public Vector2 PitchLimitsRad = new(-1.2f, 1.2f);
	public float PivotHeight = 2.0f;
	public bool RequireRmb = true; // MMO-style
	public float Sensitivity = 0.015f;
	public float TurnSpeed = 1.6f;
	public float ZoomStep = 0.7f;

	public void SetTarget(CameraRig rig, Node3D target)
	{
		_target = target;
		rig.SetTarget(target);

		// Init yaw/pitch from current rig↔target
		var to = rig.GlobalTransform.Origin - target.GlobalTransform.Origin;
		if (to.LengthSquared() > 1e-6f)
		{
			_yaw = Mathf.Atan2(to.X, to.Z);
			_pitch = Mathf.Asin(to.Normalized().Y);
		}

		rig.DesiredArmLength = Mathf.Clamp(rig.DesiredArmLength <= 0 ? 5.5f : rig.DesiredArmLength, MinArm, MaxArm);
	}

	public override void HandleInput(CameraRig rig, InputEvent e)
	{
		if (e is InputEventMouseButton mb)
		{
			if (mb.ButtonIndex == MouseButton.Right)
			{
				_rotating = mb.Pressed;
				Input.MouseMode = _rotating ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
			}

			if (mb.Pressed && mb.ButtonIndex == MouseButton.WheelUp)
				rig.DesiredArmLength = Mathf.Clamp(rig.DesiredArmLength - ZoomStep, MinArm, MaxArm);

			if (mb.Pressed && mb.ButtonIndex == MouseButton.WheelDown)
				rig.DesiredArmLength = Mathf.Clamp(rig.DesiredArmLength + ZoomStep, MinArm, MaxArm);
		}

		if (e is InputEventMouseMotion mm && (_rotating || !RequireRmb))
		{
			_yaw -= mm.Relative.X * Sensitivity;
			_pitch -= mm.Relative.Y * Sensitivity;
			_pitch = Mathf.Clamp(_pitch, PitchLimitsRad.X, PitchLimitsRad.Y);
			_yaw = Mathf.Wrap(_yaw, -Mathf.Pi, Mathf.Pi);
		}
	}

	public override void UpdateCamera(CameraRig rig, double dt)
	{
		if (_target == null || !_target.IsInsideTree()) return;

		if (_rotating)
		{
			var kYaw = (Input.GetActionStrength("cam_rotate_right") - Input.GetActionStrength("cam_rotate_left")) *
			           TurnSpeed * (float)dt;

			var kPit = (Input.GetActionStrength("cam_pitch_up") - Input.GetActionStrength("cam_pitch_down")) *
			           TurnSpeed * (float)dt;

			if (kYaw != 0) _yaw += kYaw;
			if (kPit != 0) _pitch = Mathf.Clamp(_pitch + kPit, PitchLimitsRad.X, PitchLimitsRad.Y);
			_yaw = Mathf.Wrap(_yaw, -Mathf.Pi, Mathf.Pi);
		}

		var t = _target.GlobalTransform.Origin + new Vector3(0, PivotHeight, 0);
		var cp = Mathf.Cos(_pitch);
		var dir = new Vector3(cp * Mathf.Sin(_yaw), Mathf.Sin(_pitch), cp * Mathf.Cos(_yaw));

		rig.DesiredPivot = t;
		rig.LookAt(t + dir, Vector3.Up);
	}

	public override void OnDetach(CameraRig rig)
	{
		_rotating = false;
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}
}
