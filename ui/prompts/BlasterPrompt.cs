using Godot;

namespace IrksomeIsland.Ui.Prompts;

public partial class BlasterPrompt : Control
{
	private Label? _title;
	private TextureRect? _glyph;
	private Label? _desc;

	public override void _Ready()
	{
		_title = GetNodeOrNull<Label>("PanelContainer/VBox/Title") ?? GetNodeOrNull<Label>("VBox/Title");
		_glyph = GetNodeOrNull<TextureRect>("PanelContainer/VBox/HBox/Glyph") ?? GetNodeOrNull<TextureRect>("VBox/HBox/Glyph");
		_desc = GetNodeOrNull<Label>("PanelContainer/VBox/Desc") ?? GetNodeOrNull<Label>("VBox/Desc");
	}

	public void Configure(string title, string glyphName, string desc)
	{
		if (_title != null) _title.Text = title;
		if (_desc != null) _desc.Text = desc;
		if (_glyph != null)
		{
			// Placeholder: try to load a texture by name; ok if missing
			var tex = GD.Load<Texture2D>($"res://assets/textures/ui/{glyphName}");
			_glyph.Texture = tex;
		}
	}
}


