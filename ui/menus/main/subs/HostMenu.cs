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
using IrksomeIsland.Core.Entities;
using IrksomeIsland.Core.Game;

namespace IrksomeIsland.Ui.Menus.Main.Subs;

public partial class HostMenu : Control, IMenuContent<MainMenu.MainMenuScreen>
{
	private static readonly Dictionary<string, CharacterModelType> ModelNames = new()
	{
		["Kid Nickelback"] = CharacterModelType.CharacterA,
		["Scrivener Rodney"] = CharacterModelType.CharacterB,
		["Archivist Balky"] = CharacterModelType.CharacterC,
		["Blood-Bargainer 5000"] = CharacterModelType.CharacterD,
		["Clemptor"] = CharacterModelType.CharacterE,
		["Diamond Tarjella"] = CharacterModelType.CharacterF,
		["The CHUD"] = CharacterModelType.CharacterG,
		["Big Hank Cramblin"] = CharacterModelType.CharacterH
	};

	private Button? _backButton;
	private Button? _hostButton;
	private LineEdit? _maxPlayers;
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
		_maxPlayers = GetNode<LineEdit>("Inputs/PlayersBox");
		_password = GetNode<LineEdit>("Inputs/PasswordBox");
		_modelList = GetNode<OptionButton>("Inputs/ModelList");

		BuildCharacterModelList();

		_hostButton = GetNode<Button>("HostGame");
		_hostButton.Pressed += OnHostPressed;

		_backButton = GetNode<Button>("Back");
		_backButton.Pressed += () => RequestBack?.Invoke();
	}

	private void BuildCharacterModelList()
	{
		_modelList?.Clear();
		if (_modelList == null) return;

		var i = 0;
		foreach (var kv in ModelNames)
		{
			_modelList.AddItem(kv.Key);
			_modelList.SetItemMetadata(i, (int)kv.Value);
			i++;
		}

		_modelList.Select(0);
	}

	private void OnHostPressed()
	{
		var name = _playerName?.Text?.StripEdges() ?? "Host Player";
		var max = int.TryParse(_maxPlayers?.Text, out var m) ? Mathf.Clamp(m, 1, 64) : 8;
		var port = int.TryParse(_port?.Text, out var p) ? Mathf.Clamp(p, 1024, 65535) : 25565;
		var model = GetSelectedModelOrDefault();

		// var pwd = _password?.Text ?? ""; // not used yet

		var app = GetTree().Root.GetNode<ApplicationManager>(NodeNames.ApplicationManager);
		var net = app.GetNode<NetworkManager>(NodeNames.NetworkManager);

		net.StartServer(port, max);

		var config = new GameConfiguration
		{
			WorldName = NodeNames.WorldMain,
			LocalPlayerName = name,
			MaxPlayers = max,
			LocalPlayerModel = model,
			Port = port,
			GameType = GameType.Network
		};

		app.StartGame(config);
	}

	private CharacterModelType GetSelectedModelOrDefault()
	{
		if (_modelList == null || _modelList.ItemCount == 0)
			return CharacterModelType.CharacterA;

		var meta = _modelList.GetItemMetadata(_modelList.Selected);
		if (meta.VariantType != Variant.Type.Nil)
			return (CharacterModelType)(int)meta;

		// Fallback by text if metadata missing
		var label = _modelList.GetItemText(_modelList.Selected);
		return ModelNames.GetValueOrDefault(label, CharacterModelType.CharacterA);
	}
}
