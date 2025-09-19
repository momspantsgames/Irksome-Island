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
using IrksomeIsland.Core.Entities.States;

namespace IrksomeIsland.Core.Entities;

public abstract partial class NetworkedCharacter : CharacterBody3D
{
	protected readonly Dictionary<CharacterStateType, CharacterState> States = new();
	private MultiplayerSynchronizer _sync = null!;
	protected CharacterState? CurrentState;

	public CharacterStateType CurrentStateId { get; private set; } = CharacterStateType.Idle;
	public CharacterModelType ModelTypeId { get; private set; } = CharacterModelType.CharacterA;

	[ExportCategory("Debug")]
	[Export]
	public CharacterStateType DebugCurrentState
	{
		get => CurrentStateId;
		set => CurrentStateId = value;
	}

	protected abstract void BuildStates();
	protected abstract void ApplyModel(CharacterModelType id);

	public override void _EnterTree()
	{
		_sync = new MultiplayerSynchronizer { Name = NodeNames.NetworkedCharacterSynchronizer };
		_sync.RootPath = "..";
		AddChild(_sync);

		var rc = new SceneReplicationConfig();
		rc.AddProperty(new NodePath(":CurrentStateId"));
		rc.AddProperty(new NodePath(":ModelTypeId"));
		_sync.ReplicationConfig = rc;
	}

	public void Bootstrap(CharacterStateType state, CharacterModelType model)
	{
		CurrentStateId = state;
		ModelTypeId = model;
	}

	public override void _Ready()
	{
		BuildStates();
		ChangeState(CurrentStateId);
		ApplyModel(ModelTypeId);
	}

	protected void ChangeState(CharacterStateType next)
	{
		if (!States.TryGetValue(next, out var newState)) return;

		CurrentState?.Exit();
		CurrentState = newState;
		CurrentState.Enter();
	}

	protected void SetState(CharacterStateType s)
	{
		if (CurrentStateId == s) return;
		CurrentStateId = s;
		ChangeState(s);
	}

	protected void SetModel(CharacterModelType id)
	{
		if (ModelTypeId == id) return;
		ModelTypeId = id;
		ApplyModel(id);
	}

	public void RequestState(CharacterStateType desired)
	{
		if (Multiplayer.IsServer()) SetState(desired);
		else RpcId(1, nameof(RpcRequestState), (byte)desired);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcRequestState(byte desired)
	{
		if (Multiplayer.IsServer()) SetState((CharacterStateType)desired);
	}

	public void RequestModel(CharacterModelType id)
	{
		if (Multiplayer.IsServer()) SetModel(id);
		else RpcId(1, nameof(RpcRequestModel), (short)id);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcRequestModel(short id)
	{
		if (Multiplayer.IsServer()) SetModel((CharacterModelType)id);
	}

	public override void _Process(double delta) => CurrentState.HandleInput(delta);
	public override void _PhysicsProcess(double delta) => CurrentState.PhysicsUpdate(delta);
}
