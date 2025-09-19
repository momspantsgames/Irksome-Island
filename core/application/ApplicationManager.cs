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
using IrksomeIsland.Core.Game;

namespace IrksomeIsland.Core.Application;

public partial class ApplicationManager : Node
{
	private IrkGame? _activeGame;
	public NetworkManager NetworkManager { get; private set; } = null!;

	public override void _Ready()
	{
		Name = NodeNames.ApplicationManager;
		NetworkManager = new NetworkManager { Name = NodeNames.NetworkManager };
		AddChild(NetworkManager);

		StartGame(new GameConfiguration { GameType = GameType.Attract, WorldName = NodeNames.WorldMain });
	}

	public void StartGame(GameConfiguration config)
	{
		StopGame();
		_activeGame = CreateGame(config);
		AddChild(_activeGame);
		_activeGame.StartGame();
	}

	private void StopGame()
	{
		_activeGame?.StopGame();
		_activeGame?.QueueFree();
		_activeGame = null;
	}

	private static IrkGame CreateGame(GameConfiguration config)
	{

		IrkGame game = config.GameType switch
		{
			GameType.Attract => new AttractGame(config),
			GameType.Network => new NetworkGame(config),
			_ => throw new InvalidOperationException($"Unknown game type: {config.GameType}")
		};

		return game;
	}
}
