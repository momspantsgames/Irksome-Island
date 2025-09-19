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

namespace IrksomeIsland.Core.Entities.States.Impl;

public class WalkingState(NetworkedCharacter c) : CharacterState(c)
{
	private const float WalkSpeed = 5.0f;
	private const float RunSpeed = 8.5f;
	private const float Accel = 18.0f;
	private const float AirCtrl = 0.35f;
	public override CharacterStateType Id => CharacterStateType.Walking;

	protected override void OnEnter()
	{
		// keep vertical velocity; horizontal continues smoothly
	}

	protected override void OnHandleInput(double delta)
	{
		// no-op; we read input in PhysicsUpdate for tight sync
	}

	protected override void OnPhysicsUpdate(double delta)
	{
		if (!C.IsMultiplayerAuthority()) return;

		var cam = C.GetViewport().GetCamera3D();
		if (cam == null) return;

		var ix = Input.GetActionStrength(Actions.Movement.Right) - Input.GetActionStrength(Actions.Movement.Left);
		var iz = Input.GetActionStrength(Actions.Movement.Backward) - Input.GetActionStrength(Actions.Movement.Forward);
		var wish = new Vector2(ix, iz);
		var hasInput = wish.LengthSquared() > 0.0001f;

		// camera-relative planar basis
		var fwd = -cam.GlobalTransform.Basis.Z;
		fwd.Y = 0f;
		fwd = fwd.Normalized();
		var right = cam.GlobalTransform.Basis.X;
		right.Y = 0f;
		right = right.Normalized();

		// desired world move dir
		var dir = right * wish.X + fwd * wish.Y;
		dir = dir.LengthSquared() > Gameplay.FloatMathEpsilon ? dir.Normalized() : Vector3.Zero;

		// target speed (Shift to run)
		var run = Input.IsActionPressed(Actions.MovementAction.Run);
		var targetSpeed = run ? RunSpeed : WalkSpeed;
		var targetVel = dir * targetSpeed;

		// current vel split
		var v = C.Velocity;
		var horiz = new Vector3(v.X, 0f, v.Z);

		// ground/air control
		var a = C.IsOnFloor() ? Accel : Accel * AirCtrl;
		horiz = horiz.Lerp(targetVel, (float)(1.0 - Math.Exp(-a * delta)));

		// gravity
		var g = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
		v = new Vector3(horiz.X, v.Y - g * (float)delta, horiz.Z);

		C.Velocity = v;
		C.MoveAndSlide();

		// face move direction if any
		if (hasInput && dir.LengthSquared() > Gameplay.FloatMathEpsilon)
		{
			var yaw = Mathf.Atan2(dir.X, dir.Z);
			var rot = C.Rotation;
			rot.Y = Mathf.LerpAngle(rot.Y, yaw, 1f - Mathf.Exp(-12f * (float)delta));
			C.Rotation = rot;
		}

		// state transitions
		if (!hasInput && horiz.LengthSquared() < 0.01f)
			C.RequestState(CharacterStateType.Idle);
	}
}
