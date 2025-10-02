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

public class WalkingState(ICharacterStateContext ctx) : CharacterState(ctx)
{
	private bool _animIsSprint;
	public override CharacterStateType Id => CharacterStateType.Walking;

	protected override void OnEnter()
	{
		_animIsSprint = false;
		Ctx.AnimTravel(Animations.Walk);
	}

	protected override void OnPhysicsUpdate(double delta)
	{
		if (!IsOwner) return;

		if (Input.IsActionJustPressed(Actions.MovementAction.Jump) && Ctx.Body.IsOnFloor())
			Ctx.RequestState(CharacterStateType.Jumping);

		var cam = Ctx.Body.GetViewport().GetCamera3D();

		var wish = Input.GetVector(
			Actions.Movement.Left,
			Actions.Movement.Right,
			Actions.Movement.Backward,
			Actions.Movement.Forward
		);

		var hasInput = wish.LengthSquared() > Gameplay.FloatMathEpsilon;

		// camera-relative planar basis (fallback to character basis if camera not ready)
		var basisSource = cam != null ? cam.GlobalTransform.Basis : Ctx.Body.GlobalTransform.Basis;
		var fwd = -basisSource.Z;
		fwd.Y = 0f;
		fwd = fwd.Normalized();
		var right = basisSource.X;
		right.Y = 0f;
		right = right.Normalized();

		// desired world move dir
		var dir = right * wish.X + fwd * wish.Y;
		dir = dir.LengthSquared() > Gameplay.FloatMathEpsilon ? dir.Normalized() : Vector3.Zero;

		// target speed (Shift to run)
		var run = Input.IsActionPressed(Actions.MovementAction.Sprint);
		if (run != _animIsSprint)
		{
			Ctx.AnimTravel(run ? Animations.Sprint : Animations.Walk);
			_animIsSprint = run;
		}

		var targetSpeed = run ? Gameplay.Character.RunSpeed : Gameplay.Character.WalkSpeed;
		var targetVel = dir * targetSpeed;

		// current vel split
		var v = Ctx.Body.Velocity;
		var horiz = new Vector3(v.X, 0f, v.Z);

		// ground/air control
		var a = Ctx.Body.IsOnFloor()
			? Gameplay.Character.Acceleration
			: Gameplay.Character.Acceleration * Gameplay.Character.AirControlFactor;

		horiz = horiz.Lerp(targetVel, (float)(1.0 - Math.Exp(-a * delta)));

		// gravity
		var g = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
		v = new Vector3(horiz.X, v.Y - g * (float)delta, horiz.Z);

		Ctx.Body.Velocity = v;
		Ctx.Body.MoveAndSlide();
		Ctx.PushRigidBodies();

		// face move direction if any
		if (hasInput && dir.LengthSquared() > Gameplay.FloatMathEpsilon)
		{
			var yaw = Mathf.Atan2(dir.X, dir.Z);
			var rot = Ctx.Body.Rotation;
			rot.Y = Mathf.LerpAngle(rot.Y, yaw, 1f - Mathf.Exp(-Gameplay.Character.RotationSpeed * (float)delta));
			Ctx.Body.Rotation = rot;
		}

		// state transitions
		if (!hasInput && horiz.LengthSquared() < Gameplay.FloatMathEpsilon)
			Ctx.RequestState(CharacterStateType.Idle);
	}
}
