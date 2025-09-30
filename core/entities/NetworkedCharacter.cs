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
using IrksomeIsland.Core.Components;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Entities.States;

namespace IrksomeIsland.Core.Entities;

public partial class NetworkedCharacter : CharacterBody3D, ICharacterStateContext
{
	private static readonly System.Collections.Generic.Dictionary<CharacterModelType, PackedScene> ModelCache = new();
	private AnimationComponent? _animation;

	private CharacterModelType _characterModelTypeId = CharacterModelType.CharacterA;
	private Node? _currentModel;
	private string _displayName = "Player";

	private EquipmentComponent? _equipment;
	private Node3D? _modelRoot;
	private Label3D? _nameplate;
	private PropPusherComponent? _propPusher;
	private CharacterStateComponent? _state;
	private MultiplayerSynchronizer _sync = null!;

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

	public NetworkedCharacter Character => this;
	public bool IsServer => Multiplayer.IsServer();
	public bool IsOwner => IsMultiplayerAuthority();
	public void AnimTravel(string animName) => _animation?.Travel(animName);


	public void PushRigidBodies()
	{
		_propPusher?.PushRigidBodies();
	}

	private static string? GetModelPath(CharacterModelType id) => Paths.CharacterModels.GetValueOrDefault(id);

	public override void _EnterTree()
	{
		_sync = new MultiplayerSynchronizer { Name = NodeNames.NetworkedCharacterSynchronizer, RootPath = ".." };
		_sync.SetMultiplayerAuthority(GetMultiplayerAuthority());

		var rc = new SceneReplicationConfig();
		rc.AddProperty(new NodePath(":global_transform"));
		rc.AddProperty(new NodePath(":DisplayName"));
		rc.AddProperty(new NodePath(":ModelTypeId"));

		rc.PropertySetReplicationMode(new NodePath(":DisplayName"), SceneReplicationConfig.ReplicationMode.OnChange);
		rc.PropertySetReplicationMode(new NodePath(":ModelTypeId"), SceneReplicationConfig.ReplicationMode.OnChange);
		_sync.ReplicationConfig = rc;

		AddChild(_sync);
	}

	public void Bootstrap(CharacterModelType model)
	{
		ModelTypeId = model;
	}

	public override void _Ready()
	{
		_modelRoot = GetNode<Node3D>(NodeNames.ModelRoot);
		_state = new CharacterStateComponent { Name = NodeNames.CharacterStateComponent };
		_state.SetMultiplayerAuthority(GetMultiplayerAuthority());
		AddChild(_state);
		_state.Initialize(this, CharacterStateFactory.All);

		_animation = new AnimationComponent { Name = NodeNames.AnimationComponent };
		AddChild(_animation);

		_equipment = new EquipmentComponent { Name = NodeNames.EquipmentComponent };
		AddChild(_equipment);

		var interaction = new InteractionComponent { Name = NodeNames.InteractionComponent };
		AddChild(interaction);

		_propPusher = new PropPusherComponent { Name = NodeNames.PropPusherComponent };
		_propPusher.SetMultiplayerAuthority(GetMultiplayerAuthority());
		AddChild(_propPusher);

		_nameplate = GetNode<Label3D>(NodeNames.Nameplate);
		_nameplate.Text = DisplayName;
		_nameplate.Visible = !IsMultiplayerAuthority();

		ApplyModel(ModelTypeId);
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

	public void RequestState(CharacterStateType desired, Dictionary? payload = null)
		=> _state?.Request(desired, payload);
}
