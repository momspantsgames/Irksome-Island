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

namespace IrksomeIsland.Core.Game;

public abstract partial class IrkGame(GameConfiguration config) : Node
{
	public PackedScene? WorldScene { get; set; }
	protected Node? World;

	// todo: props and players nodes - managers?

	// just stores the configuration it was created with
	protected GameConfiguration Configuration { get; init; } = config;

	public virtual void StartGame()
	{
		if (WorldScene == null)
			WorldScene = ResourceLoader.Load<PackedScene>(Paths.ForWorld(Configuration.WorldName));
		if (WorldScene == null)
			throw new InvalidOperationException($"World not found: {Configuration.WorldName}");

		World = WorldScene.Instantiate();

		AddChild(World);
	}

	public virtual void StopGame()
	{
		World?.QueueFree();
		World = null;
		WorldScene = null;
	}

}

