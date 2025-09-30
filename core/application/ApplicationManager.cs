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
	public IrkGame? ActiveGame { get; private set; }

	public NetworkManager NetworkManager { get; private set; } = null!;
	private static bool IsHeadless => DisplayServer.GetName() == "headless";

	public override void _Ready()
	{
		Name = NodeNames.ApplicationManager;
		NetworkManager = new NetworkManager { Name = NodeNames.NetworkManager };
		AddChild(NetworkManager);

		if (IsHeadless)
		{
			var config = LoadFromFile();
			NetworkManager.StartServer(config.Port, config.MaxPlayers, true);

			RenderingServer.SetRenderLoopEnabled(false);
			Engine.MaxFps = 0;
			for (var i = 0; i < AudioServer.GetBusCount(); i++)
				AudioServer.SetBusMute(i, true);

			StartGame(config);
		}
		else
		{
			StartGame(new GameConfiguration { GameType = GameType.Attract, WorldName = NodeNames.WorldMain });
		}
	}

	public void StartGame(GameConfiguration config)
	{
		StopGame();
		ActiveGame = CreateGame(config);
		AddChild(ActiveGame);
		ActiveGame.StartGame();
	}

	private void StopGame()
	{
		ActiveGame?.StopGame();
		ActiveGame?.QueueFree();
		ActiveGame = null;
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

	private GameConfiguration LoadFromFile()
	{
		var cfg = new ConfigFile();
		cfg.Load(Paths.ServerConfigFilePath);

		var config = new GameConfiguration
		{
			Port = (int)cfg.GetValue(null, "port", Gameplay.Configuration.DefaultServerPort),
			MaxPlayers = (int)cfg.GetValue(null, "max_clients", Gameplay.Configuration.DefaultMaxPlayers),
			Name = (string)cfg.GetValue(null, "server_name", Gameplay.Configuration.DefaultServerName),
			GameType = GameType.Network,
			WorldName = NodeNames.WorldMain
		};

		return config;
	}
}
