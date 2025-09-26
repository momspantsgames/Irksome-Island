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

public class IdleState(NetworkedCharacter c) : CharacterState(c)
{
	public override CharacterStateType Id => CharacterStateType.Idle;

	protected override void OnEnter()
	{
		C.Velocity = new Vector3(0.0f, C.Velocity.Y, 0.0f);
		C.AnimTravel(Animations.Static);
	}

	protected override void OnHandleInput(double delta)
	{
		if (!IsOwner) return;

		var wish = Input.GetVector(
			Actions.Movement.Left,
			Actions.Movement.Right,
			Actions.Movement.Backward,
			Actions.Movement.Forward
		);

		if (wish.LengthSquared() > Gameplay.FloatMathEpsilon)
			C.RequestState(CharacterStateType.Walking);

		if (Input.IsActionJustPressed(Actions.MovementAction.Jump) && C.IsOnFloor())
			C.RequestState(CharacterStateType.Jumping);
	}

	protected override void OnPhysicsUpdate(double delta)
	{
		if (!IsOwner) return;
		// bleed horizontal speed to zero; keep gravity
		var v = C.Velocity;
		v.X = Mathf.Lerp(v.X, 0f, 1f - Mathf.Exp(-(float)delta * Gameplay.Character.InertiaBleedFactor));
		v.Z = Mathf.Lerp(v.Z, 0f, 1f - Mathf.Exp(-(float)delta * Gameplay.Character.InertiaBleedFactor));

		var g = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
		v.Y -= g * (float)delta;

		C.Velocity = v;
		C.MoveAndSlide();
	}
}
