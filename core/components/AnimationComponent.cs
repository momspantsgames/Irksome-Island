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
using IrksomeIsland.Core.Bus;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Props;

namespace IrksomeIsland.Core.Components;

public partial class AnimationComponent : Node, ICharacterBusAware
{
	private AnimationNodeStateMachinePlayback? _animPlayback;
	private Node3D _modelRoot = null!;
	private NodePath _modelRootPath = "..";
	private AnimationTree? _tree;
    private CharacterBus? _bus;

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
		// Support both flat state machine (parameters/playback) and nested Locomotion state machine
		var v = _tree.Get(Paths.Animation.PlaybackPath);
		if (v.VariantType == Variant.Type.Nil)
			v = _tree.Get(Paths.Animation.LocomotionPlaybackPath);
		_animPlayback = v.As<AnimationNodeStateMachinePlayback>();

		// Ensure critical locomotion animations loop even if import metadata differs
		var player = FindAnimationPlayer(modelInstance);
		if (player != null)
		{
			TrySetLoop(player, Animations.Walk);
			TrySetLoop(player, Animations.Sprint);
		}
	}

	public void BindTo(CharacterBus bus)
	{
		_bus = bus;
		_bus.Equipped += OnEquipped;
		_bus.PrimaryUseRequested += OnPrimaryUseRequested;
		_bus.Unequipped += OnUnequipped;
	}

	private void OnEquipped(NetworkedProp item, string slot)
	{
		if (_tree == null) return;
		if (slot == NodeNames.EquipmentAttachmentPoint.RightHand)
		{
			// Arm upper body overlay
			_tree.Set("parameters/UpperHold/blend_amount", 1.0f);
		}
	}

	private void OnUnequipped(NetworkedProp item, string slot)
	{
		if (_tree == null) return;
		if (slot == NodeNames.EquipmentAttachmentPoint.RightHand)
		{
			_tree.Set("parameters/UpperHold/blend_amount", 0.0f);
		}
	}

	private void OnPrimaryUseRequested()
	{
		if (_tree == null) return;
		// Trigger the OneShot; prefer request if available, else fallback to active
		var hasRequest = _tree.Get("parameters/UpperShoot/request").VariantType != Variant.Type.Nil;
		if (hasRequest)
		{
			// OneShotRequest.Fire == 1
			_tree.Set("parameters/UpperShoot/request", 1);
		}
		else
		{
			_tree.Set("parameters/UpperShoot/active", true);
		}
	}

	private static void SetLoop(AnimationPlayer player, string animName)
	{
		var anim = player.GetAnimation(animName);
		if (anim != null)
			anim.LoopMode = Animation.LoopModeEnum.Linear;
	}

	private static AnimationPlayer? FindAnimationPlayer(Node root)
	{
		if (root is AnimationPlayer ap) return ap;
		foreach (var child in root.GetChildren())
		{
			if (child is AnimationPlayer cAp) return cAp;
			var nested = FindAnimationPlayer(child);
			if (nested != null) return nested;
		}
		return null;
	}

	private static void TrySetLoop(AnimationPlayer player, string animName)
	{
		if (player.HasAnimation(animName)) SetLoop(player, animName);
	}

	public void Travel(string stateName) => _animPlayback?.Travel(stateName);
}
