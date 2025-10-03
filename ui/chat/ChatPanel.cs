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
using Godot.Collections;
using IrksomeIsland.Core.Application;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Game;

namespace IrksomeIsland.Ui.Chat;

public partial class ChatPanel : Control
{
	private ChatManager _chat = null!;
	private LineEdit _input = null!;
	private RichTextLabel _list = null!;

	public override void _Ready()
	{
		_list = GetNode<RichTextLabel>("PanelContainer/VBoxContainer/ChatList");
		_input = GetNode<LineEdit>("PanelContainer/VBoxContainer/ChatInput");
		_chat = GetTree().Root
			.GetNode<ChatManager>($"{NodeNames.ApplicationManager}/NetworkGame/{NodeNames.ChatManager}");

		_chat.MessageAdded += OnMessageAdded;
		_chat.MessagesRefreshed += RedrawAll;

		RedrawAll();
		_input.TextSubmitted += OnSubmit;

		// Block gameplay input while typing
		_input.FocusEntered += OnFocusEntered;
		_input.FocusExited += OnFocusExited;
	}

	private void OnFocusEntered()
	{
		var app = GetTree().Root.GetNode<ApplicationManager>(NodeNames.ApplicationManager);
		app.IsGameplayInputBlocked = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	private void OnFocusExited()
	{
		var app = GetTree().Root.GetNode<ApplicationManager>(NodeNames.ApplicationManager);
		app.IsGameplayInputBlocked = false;
	}

	private void OnSubmit(string text)
	{
		_chat.SendLocal(text);
		_input.Text = string.Empty;
		_input.GrabFocus();
	}

	private void OnMessageAdded(Dictionary msg)
	{
		_list.AppendText($"{msg["name"]}: {msg["text"]}\n");
		_list.ScrollToLine(_list.GetLineCount());
	}

	private void RedrawAll()
	{
		_list.Clear();
		foreach (var msg in _chat.Log)
			_list.AppendText($"{msg["name"]}: {msg["text"]}\n");

		_list.ScrollToLine(_list.GetLineCount());
	}
}
