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

using IrksomeIsland.Core.Constants;
using Timer = Godot.Timer;

namespace IrksomeIsland.Core.Props;

public partial class Dart : NetworkedProp
{
	public override void _Ready()
	{
		base._Ready();

		CollisionLayer = CollisionLayers.Projectiles.ToMask();
		CollisionMask = (CollisionLayers.World | CollisionLayers.Characters | CollisionLayers.Projectiles |
		                 CollisionLayers.Dynamic | CollisionLayers.Props).ToMask();

		Mass = Gameplay.DartMass;

		// Server owns and despawns projectiles
		if (Multiplayer.IsServer())
		{
			var timer = new Timer
			{
				OneShot = true,
				WaitTime = Gameplay.DartTimeToLive
			};

			AddChild(timer);
			timer.Timeout += QueueFree;
			timer.Start();
		}
	}
}
