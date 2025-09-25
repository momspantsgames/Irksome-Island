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
using IrksomeIsland.Core.Entities.States;

namespace IrksomeIsland.Core.Entities;

public abstract partial class NetworkedCharacter : CharacterBody3D
{
	protected readonly Dictionary<CharacterStateType, CharacterState> States = new();
	private MultiplayerSynchronizer _sync = null!;
	protected CharacterState? CurrentState;

	[Export] public CharacterStateType CurrentStateId { get; private set; } = CharacterStateType.Idle;

	[Export] public CharacterModelType ModelTypeId { get; private set; } = CharacterModelType.CharacterA;

	protected abstract void BuildStates();
	protected abstract void ApplyModel(CharacterModelType id);

	public override void _EnterTree()
	{
		_sync = new MultiplayerSynchronizer { Name = NodeNames.NetworkedCharacterSynchronizer, RootPath = ".." };
		_sync.SetMultiplayerAuthority(GetMultiplayerAuthority());

		var rc = new SceneReplicationConfig();
		rc.AddProperty(new NodePath(":global_transform"));
		rc.AddProperty(new NodePath(":CurrentStateId"));
		_sync.ReplicationConfig = rc;

		AddChild(_sync);
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

		IrkLogger.Log($"NetworkedCharacter ready: State={CurrentStateId}, Model={ModelTypeId}",
			IrkLogger.LogLevel.Trace);
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
		if (Multiplayer.IsServer())
		{
			SetState(desired);
			return;
		}

		if (IsMultiplayerAuthority())
		{
			SetState(desired);
		}

		// Notify server (e.g., for validation/bookkeeping)
		RpcId(1, nameof(RpcRequestState), (byte)desired);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcRequestState(byte desired)
	{
		if (!Multiplayer.IsServer()) return;
		var sender = Multiplayer.GetRemoteSenderId();
		if (sender != GetMultiplayerAuthority()) return;

		// Only mutate on server if the server is the authority for this node
		if (Multiplayer.GetUniqueId() == GetMultiplayerAuthority())
		{
			SetState((CharacterStateType)desired);
		}
	}

	public override void _Process(double delta)
	{
		CurrentState?.HandleInput(delta);
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		IrkLogger.Log($"Player {Name} velocity: {Velocity}", IrkLogger.LogLevel.Trace);
		CurrentState?.PhysicsUpdate(delta);
		base._PhysicsProcess(delta);
	}
}
