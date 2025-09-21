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

namespace IrksomeIsland.Core.Constants;

public enum CollisionLayers : uint
{
	None = 0,
	World = 1u << 0,       // layer 1
	Props = 1u << 1,       // layer 2
	Dynamic = 1u << 2,     // layer 3
	Characters = 1u << 3,  // layer 4
	Projectiles = 1u << 4, // layer 5
	Triggers = 1u << 5,    // layer 6
	CameraOnly = 1u << 6,  // layer 7
	Reserved = 1u << 7     // layer 8
}

public static class CollisionLayerExtensions
{
	public static uint ToMask(this CollisionLayers layers) => (uint)layers;
}
