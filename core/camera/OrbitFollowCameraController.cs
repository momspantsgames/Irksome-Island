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
using IrksomeIsland.Core.Constants;

namespace IrksomeIsland.Core.Camera;

public partial class OrbitFollowCameraController : CameraController
{
	private const float PivotHeight = 2.0f;
	private const bool RequireRmb = true;
	private bool _rotating;
	private Node3D? _target;
	private float _yaw, _pitch;
	private bool InvertY { get; set; } = true;

	public void SetTarget(CameraRig rig, Node3D target)
	{
		_target = target;
		rig.SetTarget(target);

		var to = rig.GlobalTransform.Origin - target.GlobalTransform.Origin;
		if (to.LengthSquared() > 1e-6f)
		{
			_yaw = Mathf.Atan2(to.X, to.Z);
			_pitch = Mathf.Asin(to.Normalized().Y);
		}

		var defaultZoom = Gameplay.Camera.MinZoom + (Gameplay.Camera.MaxZoom - Gameplay.Camera.MinZoom) / 2f;
		rig.DesiredArmLength = Mathf.Clamp(rig.DesiredArmLength <= 0 ? defaultZoom : rig.DesiredArmLength,
			Gameplay.Camera.MinZoom, Gameplay.Camera.MaxZoom);
	}

	public override void HandleInput(CameraRig rig, InputEvent e)
	{
		if (e is InputEventMouseButton { ButtonIndex: MouseButton.Right } mb)
		{
			_rotating = mb.Pressed;
			Input.MouseMode = _rotating ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
		}
		else if (e is InputEventMouseMotion mm && (_rotating || !RequireRmb))
		{
			var reducer = .01f;
			_yaw -= mm.Relative.X * Gameplay.Camera.MouseSensitivity * reducer;
			_pitch += (InvertY ? 1f : -1f) * mm.Relative.Y * Gameplay.Camera.MouseSensitivity * reducer;
			_pitch = Mathf.Clamp(_pitch, Gameplay.Camera.PitchLimitsRad.X, Gameplay.Camera.PitchLimitsRad.Y);
			_yaw = Mathf.Wrap(_yaw, -Mathf.Pi, Mathf.Pi);
		}
	}

	public override void UpdateCamera(CameraRig rig, double delta)
	{
		if (_target == null || !_target.IsInsideTree()) return;

		var app = rig.GetTree().Root.GetNode<ApplicationManager>(NodeNames.ApplicationManager);
		var inputBlocked = app.IsGameplayInputBlocked;

		// keyboard & mouse
		if (_rotating)
		{
			if (!inputBlocked)
			{
				var kYaw = (Input.GetActionStrength(Actions.Camera.RotateRight) -
				            Input.GetActionStrength(Actions.Camera.RotateLeft)) *
				           Gameplay.Camera.TurnSpeed * (float)delta;

				var kPit = (Input.GetActionStrength(Actions.Camera.PitchUp) -
				            Input.GetActionStrength(Actions.Camera.PitchDown)) *
				           Gameplay.Camera.TurnSpeed * (float)delta;

				if (Math.Abs(kYaw) > Gameplay.FloatMathEpsilon) _yaw += kYaw;
				if (Math.Abs(kPit) > Gameplay.FloatMathEpsilon)
					_pitch = Mathf.Clamp(_pitch + kPit, Gameplay.Camera.PitchLimitsRad.X, Gameplay.Camera.PitchLimitsRad.Y);

				_yaw = Mathf.Wrap(_yaw, -Mathf.Pi, Mathf.Pi);
			}
		}

		var step = 0;
		if (!inputBlocked)
		{
			if (Input.IsActionJustPressed(Actions.Camera.ZoomIn)) step += 1;
			if (Input.IsActionJustPressed(Actions.Camera.ZoomOut)) step -= 1;
		}

		if (step != 0)
		{
			rig.DesiredArmLength = Mathf.Clamp(
				rig.DesiredArmLength - step * Gameplay.Camera.ZoomStep,
				Gameplay.Camera.MinZoom,
				Gameplay.Camera.MaxZoom
			);
		}

		// joystick
		var stick = inputBlocked
			? Vector2.Zero
			: Input.GetVector(Actions.Camera.RotateLeft, Actions.Camera.RotateRight, Actions.Camera.PitchDown,
				Actions.Camera.PitchUp);

		if (stick.LengthSquared() > Gameplay.FloatMathEpsilon)
		{
			_yaw -= stick.X * Gameplay.Camera.ThumbstickSensitivity * (float)delta;
			_pitch += (InvertY ? -1f : 1f) * stick.Y * Gameplay.Camera.ThumbstickSensitivity * (float)delta;
			_pitch = Mathf.Clamp(_pitch, Gameplay.Camera.PitchLimitsRad.X, Gameplay.Camera.PitchLimitsRad.Y);
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
