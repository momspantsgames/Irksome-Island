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
using IrksomeIsland.Core.Game;

namespace IrksomeIsland.Ui.Menus.Main.Subs;

public partial class JoinMenu : CharacterChooserMenu, IMenuContent<MainMenu.MainMenuScreen>
{
	private LineEdit? _address;
	private Button? _backButton;
	private Button? _joinButton;
	private OptionButton? _modelList;
	private LineEdit? _password;
	private LineEdit? _playerName;
	private LineEdit? _port;

	public Action<MainMenu.MainMenuScreen>? RequestScreen { get; set; }
	public Action? RequestBack { get; set; }

	public override void _Ready()
	{
		base._Ready();

		_port = GetNode<LineEdit>("Inputs/PortBox");
		_playerName = GetNode<LineEdit>("Inputs/NameBox");
		_password = GetNode<LineEdit>("Inputs/PasswordBox");
		_modelList = GetNode<OptionButton>("Inputs/ModelList");
		_address = GetNode<LineEdit>("Inputs/AddressBox");

		BuildCharacterModelList(_modelList);

		_joinButton = GetNode<Button>("JoinGame");
		_joinButton.Pressed += OnJoinPressed;

		_backButton = GetNode<Button>("Back");
		_backButton.Pressed += () => RequestBack?.Invoke();
	}

	private void OnJoinPressed()
	{
		var address = _address?.Text?.StripEdges() ?? "localhost";
		var name = _playerName?.Text?.StripEdges() ?? "Nameless Jerk";
		var port = int.TryParse(_port?.Text, out var p) ? Mathf.Clamp(p, 1024, 65535) : 25565;
		var model = GetSelectedModelOrDefault(_modelList);

		// var pwd = _password?.Text ?? ""; // not used yet

		var app = GetTree().Root.GetNode<ApplicationManager>(NodeNames.ApplicationManager);
		var net = app.GetNode<NetworkManager>(NodeNames.NetworkManager);

		net.ConnectToServer(address, port);

		var config = new GameConfiguration
		{
			WorldName = NodeNames.WorldMain,
			LocalPlayerName = name,
			LocalPlayerModel = model,
			Port = port,
			GameType = GameType.Network
		};

		app.StartGame(config);
	}
}
