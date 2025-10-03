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
using Godot.Collections;
using IrksomeIsland.Core.Entities.States;

namespace IrksomeIsland.Core.Components;

public partial class CharacterStateComponent : Node
{
	private readonly System.Collections.Generic.Dictionary<CharacterStateType, CharacterState> _states = new();
	private ICharacterStateContext _ctx = null!;
	private CharacterState? _current;
	private CharacterStateType _currentStateId = CharacterStateType.Idle;
	private MultiplayerSynchronizer? _sync;
	[Export] public Dictionary LastPayload { get; private set; } = new();

	[Export]
	public CharacterStateType CurrentStateId
	{
		get => _currentStateId;
		set
		{
			if (_currentStateId == value) return;
			_currentStateId = value;
			ChangeState(value, LastPayload);
		}
	}

	public void Initialize(
		ICharacterStateContext ctx,
		IReadOnlyDictionary<CharacterStateType, Func<ICharacterStateContext, CharacterState>> factory)
	{
		_ctx = ctx;

		foreach (var kvp in factory)
			_states[kvp.Key] = kvp.Value(_ctx);

		// Replicate CurrentStateId OnChange. Use a deterministic name so paths match across peers.
		_sync = new MultiplayerSynchronizer { Name = "StateSync", RootPath = ".." };
		_sync.SetMultiplayerAuthority(GetMultiplayerAuthority());
		var rc = new SceneReplicationConfig();
		rc.AddProperty(new NodePath(":CurrentStateId"));
		rc.PropertySetReplicationMode(new NodePath(":CurrentStateId"),
			SceneReplicationConfig.ReplicationMode.OnChange);

		rc.AddProperty(new NodePath(":LastPayload"));
		rc.PropertySetReplicationMode(new NodePath(":LastPayload"),
			SceneReplicationConfig.ReplicationMode.OnChange);

		_sync.ReplicationConfig = rc;
		AddChild(_sync);

		ChangeState(CurrentStateId, null);
	}

	public override void _Process(double delta) => _current?.HandleInput(delta);
	public override void _PhysicsProcess(double delta) => _current?.PhysicsUpdate(delta);

	// Optional payload carried with transition
	public void Request(CharacterStateType desired, Dictionary? payload = null)
	{
		if (_ctx.IsServer || _ctx.IsOwner)
			Set(desired, payload);

		if (!_ctx.IsServer)
			RpcId(1, nameof(RpcRequest), (byte)desired, payload ?? new Dictionary());
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcRequest(byte desired, Dictionary payload)
	{
		if (!_ctx.IsServer) return; // only the server authorizes transitions
		Set((CharacterStateType)desired, payload);
	}

	private void Set(CharacterStateType s, Dictionary? payload)
	{
		LastPayload = payload ?? new Dictionary();
		if (_currentStateId == s && payload is null)
		{
			ChangeState(s, null);
			return;
		}

		CurrentStateId = s;
	}

	private void ChangeState(CharacterStateType next, Dictionary? payload)
	{
		if (!_states.TryGetValue(next, out var st)) return;

		_current?.Exit();

		// Configure new state with payload before Enter
		st.Configure(payload);

		_current = st;
		_current.Enter();
	}
}
