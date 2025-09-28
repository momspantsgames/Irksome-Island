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

public class JumpingState(NetworkedCharacter c) : CharacterState(c)
{
	public override CharacterStateType Id => CharacterStateType.Jumping;

	protected override void OnEnter()
	{
		var v = C.Velocity;
		v.Y = Gameplay.Character.JumpSpeed;
		C.Velocity = v;

		C.AnimTravel(Animations.Idle);
	}

	protected override void OnPhysicsUpdate(double delta)
	{
		if (!IsOwner) return;

		var ix = Input.GetActionStrength(Actions.Movement.Right) - Input.GetActionStrength(Actions.Movement.Left);
		var iz = Input.GetActionStrength(Actions.Movement.Forward) -
		         Input.GetActionStrength(Actions.Movement.Backward);

		var wish = new Vector2(ix, iz);

		var dir = Vector3.Zero;
		if (wish.LengthSquared() > Gameplay.FloatMathEpsilon)
		{
			var cam = C.GetViewport().GetCamera3D();
			var basis = cam != null ? cam.GlobalTransform.Basis : C.GlobalTransform.Basis;
			var fwd = -basis.Z;
			fwd.Y = 0;
			fwd = fwd.Normalized();
			var right = basis.X;
			right.Y = 0;
			right = right.Normalized();
			dir = (right * wish.X + fwd * wish.Y).Normalized();
		}

		// air acceleration
		var v = C.Velocity;
		var horiz = new Vector3(v.X, 0, v.Z);
		var target = dir * (Input.IsActionPressed(Actions.MovementAction.Sprint)
			? Gameplay.Character.RunSpeed
			: Gameplay.Character.WalkSpeed);

		var a = Gameplay.Character.Acceleration * Gameplay.Character.AirControlFactor;
		horiz = horiz.Lerp(target, (float)(1.0 - Math.Exp(-a * delta)));

		// gravity
		var g = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
		v = new Vector3(horiz.X, v.Y - g * (float)delta, horiz.Z);
		C.Velocity = v;

		C.MoveAndSlide();

		// landed -> decide Idle vs Walking
		if (C.IsOnFloor())
		{
			var hasInput = wish.LengthSquared() > Gameplay.FloatMathEpsilon;
			C.AnimTravel(hasInput ? "walk" : "idle");
			C.RequestState(hasInput ? CharacterStateType.Walking : CharacterStateType.Idle);
			return;
		}

		// face move direction in air
		if (dir != Vector3.Zero)
		{
			var yaw = Mathf.Atan2(dir.X, dir.Z);
			var rot = C.Rotation;
			rot.Y = Mathf.LerpAngle(rot.Y, yaw, 1f - Mathf.Exp(-Gameplay.Character.RotationSpeed * (float)delta));
			C.Rotation = rot;
		}
	}
}
