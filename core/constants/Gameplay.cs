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

namespace IrksomeIsland.Core.Constants;

public static class Gameplay
{
	public static float FloatMathEpsilon => 1e-4f;
	public static float DartShootVelocity => 50f;

	public static class Camera
	{
		public static float FollowSpeed => 12f;
		public static float TurnSpeed => 1.6f;
		public static float MouseSensitivity => 0.4f;
		public static float ThumbstickSensitivity => 4f;
		public static float MaxZoom => 12.0f;
		public static float MinZoom => 1.8f;
		public static float ZoomStep => .5f;
		public static Vector2 PitchLimitsRad => new(-1.2f, 1.2f);
	}

	public static class Character
	{
		public static float InertiaBleedFactor => 16.0f; // how fast a character slows down after moving
		public static float WalkSpeed => 5.0f;
		public static float RunSpeed => 8.5f;
		public static float Acceleration => 18.0f;
		public static float AirControlFactor => 0.35f;
		public static float RotationSpeed => 12f;
		public static float JumpSpeed => 6.5f;
	}

	public static class Configuration
	{
		public static string DefaultServerAddress => "127.0.0.1";
		public static int DefaultServerPort => 24565;
		public static int DefaultMaxPlayers => 8;
		public static string DefaultServerName => "Irksome Island Server";
	}
}
