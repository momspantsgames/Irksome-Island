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
using IrksomeIsland.Core.Components;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Entities.States;

namespace IrksomeIsland.Core.Entities;

public partial class NetworkedCharacter : CharacterBody3D
{
	private static readonly Dictionary<CharacterModelType, PackedScene> ModelCache = new();
	private readonly Dictionary<CharacterStateType, CharacterState> _states = new();
	private AnimationComponent? _animation;

	private CharacterModelType _characterModelTypeId = CharacterModelType.CharacterA;
	private Node? _currentModel;
	private CharacterState? _currentState;
	private CharacterStateType _currentStateId = CharacterStateType.Idle;
	private string _displayName = "Player";
	private Node3D? _modelRoot;
	private Label3D? _nameplate;
	private MultiplayerSynchronizer _sync = null!;

	[Export]
	public CharacterStateType CurrentStateId
	{
		get => _currentStateId;
		set
		{
			if (_currentStateId == value) return;
			_currentStateId = value;
			ChangeState(value);
		}
	}

	[Export]
	public CharacterModelType ModelTypeId
	{
		get => _characterModelTypeId;
		set
		{
			if (_characterModelTypeId == value) return;
			_characterModelTypeId = value;
			ApplyModel(value);
		}
	}


	[Export]
	public string DisplayName
	{
		get => _displayName;
		set
		{
			if (_displayName == value) return;
			_displayName = value;
			if (_nameplate != null) _nameplate.Text = value;
		}
	}

	public EquipmentComponent? Equipment { get; private set; }

	private void BuildStates()
	{
		foreach (var kvp in CharacterStateFactory.All)
			_states[kvp.Key] = kvp.Value(this);
	}

	private static string? GetModelPath(CharacterModelType id) => Paths.CharacterModels.GetValueOrDefault(id);

	public override void _EnterTree()
	{
		_sync = new MultiplayerSynchronizer { Name = NodeNames.NetworkedCharacterSynchronizer, RootPath = ".." };
		_sync.SetMultiplayerAuthority(GetMultiplayerAuthority());

		var rc = new SceneReplicationConfig();
		rc.AddProperty(new NodePath(":global_transform"));
		rc.AddProperty(new NodePath(":CurrentStateId"));
		rc.AddProperty(new NodePath(":DisplayName"));
		rc.AddProperty(new NodePath(":ModelTypeId"));

		rc.PropertySetReplicationMode(new NodePath(":CurrentStateId"), SceneReplicationConfig.ReplicationMode.OnChange);
		rc.PropertySetReplicationMode(new NodePath(":DisplayName"), SceneReplicationConfig.ReplicationMode.OnChange);
		rc.PropertySetReplicationMode(new NodePath(":ModelTypeId"), SceneReplicationConfig.ReplicationMode.OnChange);
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
		_modelRoot = GetNode<Node3D>(NodeNames.ModelRoot);
		_animation = new AnimationComponent { Name = NodeNames.AnimationComponent };
		AddChild(_animation);

		Equipment = new EquipmentComponent { Name = NodeNames.EquipmentComponent };
		AddChild(Equipment);

		_nameplate = GetNode<Label3D>(NodeNames.Nameplate);
		_nameplate.Text = DisplayName;
		_nameplate.Visible = !IsMultiplayerAuthority();

		BuildStates();
		ChangeState(CurrentStateId);
		ApplyModel(ModelTypeId);
	}

	public void ServerEquip(Guid propId, string attachNodePath, Transform3D localOffset)
	{
		Equipment?.ServerEquip(propId, attachNodePath, localOffset);
	}

	public void ServerUnequip()
	{
		Equipment?.ServerUnequip();
	}

	public void AnimTravel(string s) => _animation?.Travel(s);

	private void ChangeState(CharacterStateType next)
	{
		if (!_states.TryGetValue(next, out var newState)) return;

		_currentState?.Exit();
		_currentState = newState;
		_currentState.Enter();
	}

	private void SetState(CharacterStateType s)
	{
		if (CurrentStateId == s) return;
		CurrentStateId = s;
		ChangeState(s);
	}

	private void ApplyModel(CharacterModelType id)
	{
		var path = GetModelPath(id);
		if (string.IsNullOrEmpty(path)) return;

		_currentModel?.QueueFree();

		if (!ModelCache.TryGetValue(id, out var ps))
		{
			ps = ResourceLoader.Load<PackedScene>(path);
			if (ps != null) ModelCache[id] = ps;
		}

		if (ps == null) return;

		_currentModel = ps.Instantiate();
		_modelRoot?.AddChild(_currentModel);
		_animation?.BindTo(_currentModel);
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
		_currentState?.HandleInput(delta);
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		_currentState?.PhysicsUpdate(delta);
		base._PhysicsProcess(delta);
	}
}
