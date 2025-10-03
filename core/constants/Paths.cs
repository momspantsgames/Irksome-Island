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

using IrksomeIsland.Core.Entities;

namespace IrksomeIsland.Core.Constants;

public static class Paths
{
	private const string ResourcesPrefix = "res://";
	private const string Assets = "assets";
	private const string SceneSuffix = ".tscn";
	private const string ConfigSuffix = ".cfg";
	private const string ResourceSuffix = ".tres";
	private const string Prefabs = "prefabs";
	private const string Characters = "characters";
	private const string Worlds = "worlds";
	private const string MainMenu = "ui/menus/main/subs";
	private const string Music = "audio/music";
	private const string Mp3Suffix = ".mp3";
	private const string OggSuffix = ".ogg";
	private const string Sounds = "audio/sounds";
	private const string Ui = "ui";
	private const string Prompts = "prompts";
	public const string MainMenuScene = ResourcesPrefix + "ui/menus/main/MainMenu" + SceneSuffix;
	public const string NetworkedCharacterScene = ResourcesPrefix + "core/entities/NetworkedCharacter" + SceneSuffix;
	public const string ChatPanelScene = ResourcesPrefix + "ui/chat/ChatPanel" + SceneSuffix;
	public const string InteractionHudScene = ResourcesPrefix + Ui + "/InteractionHud" + SceneSuffix;
	public const string ServerConfigFilePath = ResourcesPrefix + "server" + ConfigSuffix;

	public static readonly IReadOnlyDictionary<CharacterModelType, string> CharacterModels =
		new Dictionary<CharacterModelType, string>
		{
			[CharacterModelType.CharacterA] = ForCharacterModel("CharacterA"),
			[CharacterModelType.CharacterB] = ForCharacterModel("CharacterB"),
			[CharacterModelType.CharacterC] = ForCharacterModel("CharacterC"),
			[CharacterModelType.CharacterD] = ForCharacterModel("CharacterD"),
			[CharacterModelType.CharacterE] = ForCharacterModel("CharacterE"),
			[CharacterModelType.CharacterF] = ForCharacterModel("CharacterF"),
			[CharacterModelType.CharacterG] = ForCharacterModel("CharacterG"),
			[CharacterModelType.CharacterH] = ForCharacterModel("CharacterH")
		};

	public static string ForMusic(string fileName) =>
		$"{ResourcesPrefix}{Assets}/{Music}/{fileName}{Mp3Suffix}";

	public static string ForSound(string fileName) =>
		$"{ResourcesPrefix}{Assets}/{Sounds}/{fileName}{OggSuffix}";

	public static string ForCharacterModel(string fileName) =>
		$"{ResourcesPrefix}{Prefabs}/{Characters}/{fileName}{SceneSuffix}";

	public static string ForPrompt(string fileName) =>
		$"{ResourcesPrefix}{Ui}/{Prompts}/{fileName}{SceneSuffix}";

	public static string ForWorld(string worldName) =>
		$"{ResourcesPrefix}{Worlds}/{worldName}/{worldName}{SceneSuffix}";

	public static string ForMainMenu(string fileName) => $"{ResourcesPrefix}{MainMenu}/{fileName}{SceneSuffix}";

	public static class Props
	{
		public const string DartScene = ResourcesPrefix + "/" + Prefabs + "/props/Dart" + SceneSuffix;
		public const string BlasterAScene = ResourcesPrefix + "/" + Prefabs + "/props/BlasterA" + SceneSuffix;
	}

	public static class Animation
	{
		public const string DefaultCharacterAnimStateMachine =
			ResourcesPrefix + "assets/animations/CharacterAnimationStateMachine" + ResourceSuffix;

		public const string PlaybackPath = "parameters/playback";
		public const string LocomotionPlaybackPath = "parameters/Locomotion/playback";
	}
}
