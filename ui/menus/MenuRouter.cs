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

namespace IrksomeIsland.Ui.Menus;

public abstract partial class MenuRouter<TScreen> : Control where TScreen : struct, Enum
{
	private readonly Stack<TScreen> _history = new();
	private Control? _currentMenu;
	private HBoxContainer? _menuContainer;
	[Export] private NodePath? SlotPath { get; set; }
	protected abstract Dictionary<TScreen, string> ScreenMap { get; }

	public override void _Ready()
	{
		_menuContainer = GetNode<HBoxContainer>(SlotPath);
		ShowScreen(GetDefaultScreen(), false);
	}

	protected abstract TScreen GetDefaultScreen();

	public void ShowScreen(TScreen screen, bool pushHistory = true)
	{
		if (pushHistory && _currentMenu != null)
			_history.Push(screen);

		_currentMenu?.QueueFree();

		if (!ScreenMap.TryGetValue(screen, out var scenePath))
		{
			Logger.Log($"No scene mapped for {screen}", Logger.LogLevel.Error);
			return;
		}

		var instance = GD.Load<PackedScene>(scenePath)?.Instantiate<Control>();
		if (instance == null) return;

		_menuContainer?.AddChild(instance);
		_currentMenu = instance;
		CallDeferred(nameof(GrabFirstFocusable), instance);

		if (instance is IMenuContent<TScreen> content)
		{
			content.RequestScreen = s => ShowScreen(s);
			content.RequestBack = Back;
		}
	}

	public void Back()
	{
		if (_history.Count > 0)
			ShowScreen(_history.Pop(), false);
		else
			ShowScreen(GetDefaultScreen(), false);
	}

	protected static void GrabFirstFocusable(Control root)
	{
		var q = new Queue<Control>();
		q.Enqueue(root);
		while (q.Count > 0)
		{
			var c = q.Dequeue();
			if (c.FocusMode != FocusModeEnum.None)
			{
				c.GrabFocus();
				return;
			}

			foreach (var child in c.GetChildren())
			{
				if (child is Control cc)
					q.Enqueue(cc);
			}
		}
	}
}
