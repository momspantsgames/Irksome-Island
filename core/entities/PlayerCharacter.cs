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

namespace IrksomeIsland.Core.Entities;

public partial class PlayerCharacter : CharacterBody3D {

	private Node3D _modelRoot;
	private Node _currentModel;
	private CharacterModel ModelId { get; set; } = CharacterModel.CharacterA;

	public override void _Ready()
	{
		_modelRoot = GetNode<Node3D>(NodeNames.ModelRoot);
		ApplyModel(ModelId);
	}

	private bool SetModel(CharacterModel id)
	{
		if (!ModelMap.ContainsKey(id)) return false;
		if (id == ModelId && _currentModel != null) return true;
		ModelId = id;
		ApplyModel(id);
		return true;
	}

	private void ApplyModel(CharacterModel id)
	{
		_currentModel?.QueueFree();
		var path = ModelMap[id];
		var ps = ResourceLoader.Load<PackedScene>(path);
		if (ps == null) return;
		_currentModel = ps.Instantiate();
		_modelRoot.AddChild(_currentModel);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcRequestModel(short id)
	{
		if (!Multiplayer.IsServer()) return;
		var cid = (CharacterModel)id;
		SetModel(cid);
	}

	private static readonly Dictionary<CharacterModel, string> ModelMap =
		new()
		{
			[CharacterModel.CharacterA] = Paths.ForCharacterModel("CharacterA")
		};
}
