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

using Godot.Collections;

namespace IrksomeIsland.Core.Entities.States;

public abstract class CharacterState(ICharacterStateContext ctx)
{
	protected readonly ICharacterStateContext Ctx = ctx;

	public abstract CharacterStateType Id { get; }

	protected bool IsOwner => Ctx.IsOwner;
	protected bool IsServer => Ctx.IsServer;

	// Called by the machine before Enter when a payload exists
	public virtual void Configure(Dictionary? payload)
	{
	}

	public void Enter()
	{
		OnEnter();
	}

	public void Exit()
	{
		OnExit();
	}

	public void HandleInput(double delta)
	{
		OnHandleInput(delta);
	}

	public void PhysicsUpdate(double delta)
	{
		OnPhysicsUpdate(delta);
	}

	protected virtual void OnEnter()
	{
	}

	protected virtual void OnExit()
	{
	}

	protected virtual void OnHandleInput(double delta)
	{
	}

	protected virtual void OnPhysicsUpdate(double delta)
	{
	}
}
