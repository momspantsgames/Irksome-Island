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

namespace IrksomeIsland.Ui.Menus.Main.Subs;

public partial class HomeMenu : Control, IMenuContent<MainMenu.MainMenuScreen>
{
	private Button? _hostButton;
	private Button? _joinButton;
	private Button? _optionsButton;
	private Button? _quitButton;

	public Action<MainMenu.MainMenuScreen>? RequestScreen { get; set; }
	public Action? RequestBack { get; set; }

	public override void _Ready()
	{
		base._Ready();

		_quitButton = GetNode<Button>("Quit");
		_quitButton.Pressed += OnQuitPressed;

		_hostButton = GetNode<Button>("Host");
		_hostButton.Pressed += OnHostPressed;

		_joinButton = GetNode<Button>("Join");
		_joinButton.Pressed += OnJoinPressed;

		_optionsButton = GetNode<Button>("Options");
		_optionsButton.Pressed += OnOptionsPressed;
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}

	private void OnHostPressed()
	{
		RequestScreen?.Invoke(MainMenu.MainMenuScreen.Host);
	}

	private void OnJoinPressed()
	{
		RequestScreen?.Invoke(MainMenu.MainMenuScreen.Join);
	}

	private void OnOptionsPressed()
	{
		RequestScreen?.Invoke(MainMenu.MainMenuScreen.Options);
	}
}
