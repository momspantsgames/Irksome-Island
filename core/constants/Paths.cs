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

public static class Paths
{
	private const string ResourcesPrefix = "res://";
	private const string SceneSuffix = ".tscn";
	private const string Prefabs = "prefabs";
	private const string Characters = "characters";
	private const string Worlds = "worlds";
	private const string MainMenu = "ui/menus/main/subs";
	public const string MainMenuScene = ResourcesPrefix + "ui/menus/main/MainMenu" + SceneSuffix;

	public static string ForCharacterModel(string fileName) =>
		$"{ResourcesPrefix}{Prefabs}/{Characters}/{fileName}{SceneSuffix}";

	public static string ForWorld(string worldName) =>
		$"{ResourcesPrefix}{Worlds}/{worldName}/{worldName}{SceneSuffix}";

	public static string ForMainMenu(string fileName) => $"{ResourcesPrefix}{MainMenu}/{fileName}{SceneSuffix}";
}
