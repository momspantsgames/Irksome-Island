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

namespace IrksomeIsland.Core.Game;

public partial class ChatManager : Node
{
	private const int Max = 100;
	private readonly Array<string> _log = new();

	public IReadOnlyList<string> Log => _log;

	public void SetServerMode()
	{ /* nothing for now */
	}

	public void SendLocal(string text)
	{
		if (string.IsNullOrWhiteSpace(text)) return;

		if (Multiplayer.IsServer())
		{
			// host/server says something: handle locally, then fan out
			RpcSay(text); // local call, not an RPC packet
		}
		else
		{
			RpcId(1, nameof(RpcSay), text); // client → server
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcSay(string text)
	{
		if (!Multiplayer.IsServer()) return;
		var from = Multiplayer.GetRemoteSenderId();
		var line = $"{(from == 0 ? "Server" : from.ToString())}: {Sanitize(text)}";
		Append(line);
		Rpc(nameof(RpcDeliver), line);
	}

	[Rpc]
	private void RpcDeliver(string line) => Append(line);

	public void SendSnapshotTo(long peerId) => RpcId(peerId, nameof(RpcInit), _log);

	[Rpc]
	private void RpcInit(Array<string> snapshot)
	{
		_log.Clear();
		foreach (var s in snapshot) _log.Add(s);
		MessagesRefreshed?.Invoke();
	}

	private void Append(string line)
	{
		_log.Add(line);
		if (_log.Count > Max) _log.RemoveAt(0);
		MessageAdded?.Invoke(line);
	}

	public event Action<string>? MessageAdded;
	public event Action? MessagesRefreshed;

	private static string Sanitize(string s)
	{
		var t = s.Trim();
		return t.Length > 256 ? t[..256] : t;
	}
}
