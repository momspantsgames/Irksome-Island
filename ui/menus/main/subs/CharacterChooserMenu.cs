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
using IrksomeIsland.Core.Entities;

namespace IrksomeIsland.Ui.Menus.Main.Subs;

public abstract partial class CharacterChooserMenu : Control
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

	protected static void BuildCharacterModelList(OptionButton modelList)
	{
		modelList.Clear();
		var i = 0;
		foreach (var kv in ModelNames)
		{
			modelList.AddItem(kv.Key);
			modelList.SetItemMetadata(i, (int)kv.Value);
			i++;
		}

		modelList.Select(0);
	}

	protected static CharacterModelType GetSelectedModelOrDefault(OptionButton? modelList)
	{
		if (modelList == null || modelList.ItemCount == 0)
			return CharacterModelType.CharacterA;

		var meta = modelList.GetItemMetadata(modelList.Selected);
		if (meta.VariantType != Variant.Type.Nil)
			return (CharacterModelType)(int)meta;

		// Fallback by text if metadata missing
		var label = modelList.GetItemText(modelList.Selected);
		return ModelNames.GetValueOrDefault(label, CharacterModelType.CharacterA);
	}
}
