// Copyright (c) 2025 Momspants Games
//
// MIT License

using Godot;

namespace IrksomeIsland.Core.Components;

public interface IInteractable
{
	// Called on the server to perform the interaction
	void OnInteractServer(Node3D interactor);

	// Optional client hint (prompt text, icon path, etc.)
	string? GetInteractionPrompt();
}

public partial class InteractionComponent : Node
{
	private IInteractable? _current;

	private Node3D _ownerNode = null!;
	private Area3D _queryArea = null!;
	private CollisionShape3D _queryShape = null!;
	[Export] public float Radius { get; set; } = 2.0f;
	[Export] public float Height { get; set; } = 1.6f;

	public override void _Ready()
	{
		_ownerNode = GetParent<Node3D>();

		_queryArea = new Area3D { Name = "InteractionQuery" };
		_queryShape = new CollisionShape3D
		{
			Shape = new CylinderShape3D { Radius = Radius, Height = Height }
		};

		_queryArea.AddChild(_queryShape);
		AddChild(_queryArea);
	}

	public override void _Process(double delta)
	{
		var space = _ownerNode.GetWorld3D().DirectSpaceState;
		if (space == null) return;

		var shape = new CylinderShape3D { Radius = Radius, Height = Height };
		var parms = new PhysicsShapeQueryParameters3D
		{
			Shape = shape,
			Transform = _ownerNode.GlobalTransform,
			CollisionMask = uint.MaxValue,
			CollideWithAreas = true,
			CollideWithBodies = true
		};

		var results = space.IntersectShape(parms, 16);
		_current = null;
		foreach (var hit in results)
		{
			if (hit.TryGetValue("collider", out var colliderVar))
			{
				var obj = colliderVar.AsGodotObject();
				if (obj is Node node && FindInteractable(node) is { } interactable)
				{
					_current = interactable;
					break;
				}
			}
		}

		// if (Input.IsActionJustPressed(ActionName))
		// {
		// 	TryInteract();
		// }
	}

	public void TryInteract()
	{
		if (_current == null) return;
		if (!Multiplayer.IsServer())
		{
			RpcId(1, nameof(RpcRequestInteract));
			return;
		}

		_current.OnInteractServer(_ownerNode!);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcRequestInteract()
	{
		if (!Multiplayer.IsServer()) return;
		var sender = Multiplayer.GetRemoteSenderId();
		if (_ownerNode?.GetMultiplayerAuthority() != sender) return;
		if (_current == null) return;
		_current.OnInteractServer(_ownerNode!);
	}

	private static IInteractable? FindInteractable(Node node)
	{
		// Node implements IInteractable directly
		if (node is IInteractable here) return here;

		// Or a sibling/child implements it
		foreach (var child in node.GetChildren())
		{
			if (child is IInteractable ii) return ii;
		}

		return null;
	}
}
