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
using IrksomeIsland.Core.Entities;

namespace IrksomeIsland.Core.Game;

public partial class ChatManager : Node
{
	private const int Max = 100;
	private readonly Array<Dictionary> _log = new();
	private Node? _playersNode;

	public IReadOnlyList<Dictionary> Log => _log;

	public void SetServerMode()
	{ /* nothing for now */
	}

	public override void _Ready()
	{
		_playersNode = GetTree().Root
			.GetNode($"{NodeNames.ApplicationManager}/NetworkGame/{NodeNames.PlayersRoot}");
	}

	public void AnnouncePeerJoined(long pid)
	{
		if (!Multiplayer.IsServer()) return;

		var playerName = LookupName(pid);
		if (playerName == "Player") playerName = $"Player {pid}";
		var msg = new Dictionary
		{
			{ "peer", 0 },
			{ "name", "Server" },
			{ "text", $"{playerName} joined." },
			{ "ts", Time.GetUnixTimeFromSystem() }
		};

		Append(msg);
		Rpc(nameof(RpcDeliver), msg);
	}

	public void AnnouncePeerLeft(long pid)
	{
		if (!Multiplayer.IsServer()) return;

		var playerName = LookupName(pid);
		if (playerName == "Player") playerName = $"Player {pid}";
		var msg = new Dictionary
		{
			{ "peer", 0 },
			{ "name", "Server" },
			{ "text", $"{playerName} left." },
			{ "ts", Time.GetUnixTimeFromSystem() }
		};

		Append(msg);
		Rpc(nameof(RpcDeliver), msg);
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
		var pid = Multiplayer.GetRemoteSenderId();

		// 0 is server, so set it to 1 :shrug
		if (pid == 0) pid = 1;

		var name = LookupName(pid);
		var msg = new Dictionary
		{
			{ "peer", pid },
			{ "name", name },
			{ "text", Sanitize(text) },
			{ "ts", Time.GetUnixTimeFromSystem() }
		};

		IrkLogger.Log($"RpcSay() with {string.Join(", ", msg.Select(kv => $"{kv.Key}={kv.Value}"))}");

		Append(msg);
		Rpc(nameof(RpcDeliver), msg);
	}

	[Rpc]
	private void RpcDeliver(Dictionary msg) => Append(msg);

	public void SendSnapshotTo(long peerId) => RpcId(peerId, nameof(RpcInit), _log);

	[Rpc]
	private void RpcInit(Array<Dictionary> snapshot)
	{
		_log.Clear();
		foreach (var s in snapshot) _log.Add(s);
		MessagesRefreshed?.Invoke();
	}

	private string LookupName(long pid)
	{
		if (_playersNode == null) return $"Player {pid}";

		foreach (var n in _playersNode.GetChildren())
		{
			if (n.GetMultiplayerAuthority() == pid && n is NetworkedCharacter nc)
				return nc.DisplayName;
		}

		return $"Player {pid}";
	}

	private void Append(Dictionary msg)
	{
		_log.Add(msg);
		if (_log.Count > Max) _log.RemoveAt(0);
		MessageAdded?.Invoke(msg);
	}

	public event Action<Dictionary>? MessageAdded;
	public event Action? MessagesRefreshed;

	private static string Sanitize(string s)
	{
		var t = s.Trim();
		return t.Length > 256 ? t[..256] : t;
	}
}
