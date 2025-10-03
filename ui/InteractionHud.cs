// Copyright (c) 2025 Momspants Games
//
// MIT License

using Godot;
using IrksomeIsland.Core.Components;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Entities;
using IrksomeIsland.Core.Props;

namespace IrksomeIsland.Ui;

public partial class InteractionHud : Control
{
	private Control? _activePrompt;
	private Node3D? _activeTarget;
	private InteractionComponent? _interaction;
	private IInteractable? _last;
	private NetworkedCharacter? _localPlayer;
	private Node3D? _playersRoot;
	private Control? _slot;

	public override void _Ready()
	{
		_slot = GetNodeOrNull<Control>("PromptSlot");
		_playersRoot = GetTree().Root
			.GetNodeOrNull<Node3D>($"{NodeNames.ApplicationManager}/NetworkGame/{NodeNames.PlayersRoot}");
	}

	public override void _Process(double delta)
	{
		if (_playersRoot == null) return;
		if (_localPlayer == null) FindLocalPlayer();
		if (_localPlayer == null) return;
		if (_interaction == null)
		{
			_interaction = _localPlayer.GetNodeOrNull<InteractionComponent>(NodeNames.InteractionComponent);
		}

		if (_interaction == null) return;

		var current = _interaction.Current;
		if (ReferenceEquals(current, _last)) return;
		_last = current;
		SwapPrompt(current);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		UpdatePromptPosition();
	}

	private void SwapPrompt(IInteractable? target)
	{
		if (_slot == null) return;
		if (_activePrompt != null)
		{
			_activePrompt.QueueFree();
			_activePrompt = null;
		}

		_activeTarget = null;

		if (target == null)
		{
			return;
		}

		var scene = target.GetInteractionPromptScene();
		if (scene != null)
		{
			var ui = scene.Instantiate<Control>();
			ui.MouseFilter = MouseFilterEnum.Ignore;
			ui.SetAnchorsPreset(LayoutPreset.TopLeft);
			ui.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
			ui.SizeFlagsVertical = SizeFlags.ShrinkCenter;
			_slot.AddChild(ui);
			_activePrompt = ui;
			var ctx = new InteractionPromptContext
			{
				InteractActionName = "Interact",
				Interactor = _localPlayer!,
				Target = (Node3D)target
			};

			target.ConfigureInteractionPrompt(ui, ctx);
			_activeTarget = (Node3D)target;
			return;
		}

		var fallback = target.GetInteractionPrompt();
		if (!string.IsNullOrWhiteSpace(fallback))
		{
			var panel = new PanelContainer();
			var style = new StyleBoxFlat
			{
				BgColor = new Color(0, 0, 0, 0.6f),
				BorderColor = new Color(1, 1, 1, 0.8f),
				BorderWidthLeft = 2,
				BorderWidthTop = 2,
				BorderWidthRight = 2,
				BorderWidthBottom = 2,
				CornerRadiusTopLeft = 6,
				CornerRadiusTopRight = 6,
				CornerRadiusBottomRight = 6,
				CornerRadiusBottomLeft = 6,
				ContentMarginLeft = 8,
				ContentMarginTop = 6,
				ContentMarginRight = 8,
				ContentMarginBottom = 6
			};
			panel.AddThemeStyleboxOverride("panel", style);
			panel.MouseFilter = MouseFilterEnum.Ignore;
			panel.SetAnchorsPreset(LayoutPreset.TopLeft);
			panel.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
			panel.SizeFlagsVertical = SizeFlags.ShrinkCenter;

			var label = new Label { Text = fallback };
			label.MouseFilter = MouseFilterEnum.Ignore;
			panel.AddChild(label);

			_slot.AddChild(panel);
			_activePrompt = panel;
			_activeTarget = (Node3D)target;
		}
	}

	private void UpdatePromptPosition()
	{
		if (_activePrompt == null || _activeTarget == null) return;
		var cam = GetViewport().GetCamera3D();
		if (cam == null) return;

		var camPos = cam.GlobalTransform.Origin;
		var camFwd = -cam.GlobalTransform.Basis.Z;
		var world = _activeTarget.GlobalTransform.Origin + new Vector3(0, 1.0f, 0);
		var to = world - camPos;
		var facing = to.Normalized().Dot(camFwd);
		if (facing <= 0)
		{
			_activePrompt.Visible = false;
			return;
		}

		var screen = cam.UnprojectPosition(world);
		var vp = GetViewportRect().Size;
		var pos = new Vector2(Mathf.Clamp(screen.X, 0, vp.X), Mathf.Clamp(screen.Y, 0, vp.Y));
		pos += new Vector2(0, -32);

		_activePrompt.Visible = true;
		_activePrompt.Position = pos;
	}

	private void FindLocalPlayer()
	{
		if (_playersRoot == null) return;
		foreach (var n in _playersRoot.GetChildren())
		{
			if (n is NetworkedCharacter nc && nc.IsMultiplayerAuthority())
			{
				_localPlayer = nc;
				break;
			}
		}
	}
}
