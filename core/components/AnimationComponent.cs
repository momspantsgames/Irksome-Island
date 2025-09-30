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

namespace IrksomeIsland.Core.Components;

public partial class AnimationComponent : Node
{
	private AnimationNodeStateMachinePlayback? _animPlayback;
	private Node3D _modelRoot = null!;
	private NodePath _modelRootPath = "..";
	private AnimationTree? _tree;

	public override void _Ready()
	{
		_modelRoot = GetNode<Node3D>(_modelRootPath);
	}

	// Call after you instance/swap a model
	public void BindTo(Node modelInstance)
	{
		_tree = modelInstance.GetNodeOrNull<AnimationTree>(NodeNames.AnimationTree);
		if (_tree == null) return;
		_tree.Active = true;
		_animPlayback = (AnimationNodeStateMachinePlayback)_tree.Get(Paths.Animation.PlaybackPath);
	}

	private static void SetLoop(AnimationPlayer player, string animName)
	{
		var anim = player.GetAnimation(animName);
		if (anim != null)
			anim.LoopMode = Animation.LoopModeEnum.Linear;
	}

	public void Travel(string stateName) => _animPlayback?.Travel(stateName);
}
