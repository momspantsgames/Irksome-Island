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
using static IrksomeIsland.Core.Constants.Gameplay;

namespace IrksomeIsland.Core.Camera;

public partial class OrbitFollowCameraController : CameraController
{
	private float _pitch;
	private Node3D? _target;

	private float _yaw;
	public bool LookAtTarget { get; set; } = true;
	public float Sensitivity { get; set; } = 0.015f; // mouse
	public float TurnSpeed { get; set; } = 1.6f;     // keyboard rad/s
	public bool RequireRmb { get; set; } = true;

	public void SetTarget(Node3D t, CameraRig rig)
	{
		_target = t;
		rig.SetTarget(t);

		// init yaw/pitch from current rig-to-target
		var to = rig.GlobalTransform.Origin - t.GlobalTransform.Origin;
		if (to.LengthSquared() > FloatMathEpsilon)
		{
			_yaw = Mathf.Atan2(to.X, to.Z);
			_pitch = Mathf.Asin(to.Normalized().Y);
		}

		rig.DesiredArmLength = Mathf.Clamp(Gameplay.Camera.ThirdPersonSpringArmLength,
			Gameplay.Camera.MinimumThirdPersonZoom,
			Gameplay.Camera.MaxThirdPersonZoom);
	}

	public override void HandleInput(CameraRig rig, InputEvent e)
	{
		if (e is InputEventMouseMotion mm && (!RequireRmb || Input.IsMouseButtonPressed(MouseButton.Right)))
		{
			_yaw -= mm.Relative.X * Sensitivity;
			_pitch -= mm.Relative.Y * Sensitivity;
			_pitch = Mathf.Clamp(_pitch, Gameplay.Camera.ThirdPersonPitchLimitsRad.X,
				Gameplay.Camera.ThirdPersonPitchLimitsRad.Y);
		}
		else if (e is InputEventMouseButton { Pressed: true } mb)
		{
			switch (mb.ButtonIndex)
			{
				case MouseButton.WheelUp:
					rig.DesiredArmLength -= Gameplay.Camera.ThirdPersonZoomStep;
					break;
				case MouseButton.WheelDown:
					rig.DesiredArmLength += Gameplay.Camera.ThirdPersonZoomStep;
					break;
			}

			rig.DesiredArmLength = Mathf.Clamp(rig.DesiredArmLength, Gameplay.Camera.MinimumThirdPersonZoom,
				Gameplay.Camera.MaxThirdPersonZoom);
		}
	}

	public override void UpdateCamera(CameraRig rig, double delta)
	{
		if (_target == null) return;

		var kYaw = (Input.GetActionStrength(Actions.Camera.RotateRight) -
		            Input.GetActionStrength(Actions.Camera.RotateLeft)) * TurnSpeed * (float)delta;

		var kPitch = (Input.GetActionStrength(Actions.Camera.PitchUp) -
		              Input.GetActionStrength(Actions.Camera.PitchDown)) * TurnSpeed * (float)delta;

		if (kYaw > FloatMathEpsilon)
			_yaw += kYaw;

		if (kPitch > FloatMathEpsilon)
		{
			_pitch = Mathf.Clamp(_pitch + kPitch, Gameplay.Camera.ThirdPersonPitchLimitsRad.X,
				Gameplay.Camera.ThirdPersonPitchLimitsRad.Y);
		}

		// orbit pivot
		var t = _target.GlobalTransform.Origin + new Vector3(0, Gameplay.Camera.ThirdPersonPivotHeight, 0);
		var cp = Mathf.Cos(_pitch);
		var dir = new Vector3(cp * Mathf.Sin(_yaw), Mathf.Sin(_pitch), cp * Mathf.Cos(_yaw));

		// place rig at pivot; spring arm on the rig handles collision/retraction
		rig.DesiredPivot = t + dir * 0.001f;

		if (LookAtTarget) rig.LookAt(t, Vector3.Up);
	}
}
