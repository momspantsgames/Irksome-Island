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

namespace IrksomeIsland.Core.Props;

public partial class PropPlaceholder : Marker3D
{
	[Export] public PackedScene ItemScene { get; set; } = null!;
	[Export] public Transform3D LocalOffset { get; set; } = Transform3D.Identity;

	public override void _Ready()
	{
		if (!Multiplayer.IsServer())
		{
			QueueFree();
			return;
		}

		CallDeferred(nameof(SpawnNow));
	}

	private void SpawnNow()
	{
		if (!Multiplayer.IsServer())
		{
			QueueFree();
			return;
		}

		var appManager = GetTree().Root.GetNode<ApplicationManager>($"{NodeNames.ApplicationManager}");
		var propsRoot = appManager?.ActiveGame?.GetNode<Node3D>($"{NodeNames.PropsRoot}");

		if (propsRoot != null)
		{
			var item = ItemScene.Instantiate<Node3D>();
			propsRoot.AddChild(item, true);
			item.GlobalTransform = GlobalTransform;

			QueueFree();
		}
	}
}
