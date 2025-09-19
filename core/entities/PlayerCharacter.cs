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
using IrksomeIsland.Core.Entities.States.Impl;

namespace IrksomeIsland.Core.Entities;

public partial class PlayerCharacter : NetworkedCharacter
{

	private static readonly Dictionary<CharacterModelType, string> ModelMap = new()
	{
		[CharacterModelType.CharacterA] = Paths.ForCharacterModel("CharacterA")
	};

	private static readonly Dictionary<CharacterModelType, PackedScene> Cache = new();
	private Node? _currentModel;

	private Node3D? _modelRoot;

	protected override void BuildStates()
	{
		States[CharacterStateType.Idle] = new IdleState(this);
		States[CharacterStateType.Walking] = new WalkingState(this);
	}

	protected override void ApplyModel(CharacterModelType id)
	{
		if (!ModelMap.TryGetValue(id, out var path)) return;

		_currentModel?.QueueFree();
		if (!Cache.TryGetValue(id, out var ps))
		{
			ps = ResourceLoader.Load<PackedScene>(path);
			if (ps != null) Cache[id] = ps;
		}

		if (ps == null) return;

		_currentModel = ps.Instantiate();
		_modelRoot?.AddChild(_currentModel);
	}

	public override void _Ready()
	{
		base._Ready();

		_modelRoot = GetNode<Node3D>(NodeNames.ModelRoot);
	}
}
