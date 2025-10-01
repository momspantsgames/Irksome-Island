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

public static class NodeNames
{
	private const string AppRoot = "AppRoot";
	public const string NetworkManager = "NetworkManager";
	public const string ChatManager = "ChatManager";
	public const string ApplicationManager = "ApplicationManager";
	public const string AnimationComponent = "AnimationComponent";
	public const string CharacterStateComponent = "CharacterStateComponent";
	public const string EquipmentComponent = "EquipmentComponent";
	public const string InteractionComponent = "InteractionComponent";
	public const string PropPusherComponent = "PropPusherComponent";
	public const string AnimationTree = "AnimationTree";
	public const string ModelRoot = "ModelRoot";
	public const string Nameplate = "Nameplate";
	public const string NetworkedCharacterSynchronizer = "NetCharSynchronizer";
	public const string PlayersRoot = "Players";
	public const string PropsRoot = "Props";
	public const string PlayerSpawner = "PlayerSpawner";
	public const string PropSpawner = "PropSpawner";
	public const string CameraRig = "CameraRig";
	public const string WorldMain = "MainWorld";

	public static string ApplicationManagerPath() => $"{AppRoot}/{ApplicationManager}";

	public static class EquipmentAttachmentPoint
	{
		public const string Head = "HeadEquip";
		public const string Back = "BackEquip";
		public const string LeftHand = "LeftEquip";
		public const string RightHand = "RightEquip";
	}
}
