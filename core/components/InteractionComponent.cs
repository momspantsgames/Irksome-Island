// Copyright (c) 2025 Momspants Games
//
// MIT License

using Godot;
using IrksomeIsland.Core.Application;
using IrksomeIsland.Core.Bus;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Core.Entities;
using IrksomeIsland.Core.Props;

namespace IrksomeIsland.Core.Components;

public partial class InteractionComponent : Area3D, ICharacterBusAware
{
	private readonly HashSet<IInteractable> _candidates = new();
	private readonly List<IInteractable> _ordered = new();
	private CharacterBus _bus = null!;
	private IInteractable? _current;

	public IInteractable? Current => _current;

	public void BindTo(CharacterBus bus)
	{
		_bus = bus;
		_bus.InteractionRequested += TryInteract;
		_bus.InteractionCycleRequested += OnCycle;
	}

	private void TryInteract()
	{
		if (_current is not NetworkedProp prop)
		{
			IrkLogger.Log($"TryInteract() called but no current target");
			return;
		}

		var interactor = GetParent<NetworkedCharacter>();

		if (Multiplayer.IsServer())
		{
			PerformInteractionServer(prop, interactor);
		}
		else
		{
			RpcId(1, nameof(RpcPerformInteraction), prop.GetPath(), interactor.GetPath());
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcPerformInteraction(NodePath propPath, NodePath interactorPath)
	{
		if (!Multiplayer.IsServer()) return;
		var prop = GetNodeOrNull<NetworkedProp>(propPath);
		var interactor = GetNodeOrNull<NetworkedCharacter>(interactorPath);
		if (prop == null || interactor == null) return;
		PerformInteractionServer(prop, interactor);
	}

	private static void PerformInteractionServer(NetworkedProp prop, NetworkedCharacter interactor)
	{
		if (prop is IInteractable interactable)
		{
			try
			{
				interactable.OnInteractServer(interactor);
			}
			catch (Exception ex)
			{
				IrkLogger.Log($"Interaction failed: {ex.Message}");
			}
		}
	}

	public override void _Ready()
	{
		Name = NodeNames.InteractionComponent;

		var detectionShape = new CollisionShape3D
		{
			// could be a cone for more forward-facing interactions
			Shape = new SphereShape3D { Radius = Gameplay.Character.InteractDetectRadius }
		};

		AddChild(detectionShape);

		Monitoring = true;
		Monitorable = false;
		CollisionMask = CollisionLayers.Props.ToMask();

		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
		AreaEntered += OnAreaEntered;
		AreaExited += OnAreaExited;
	}

	private void OnBodyEntered(Node3D body)
	{
		TryAdd(body);
	}

	private void OnBodyExited(Node3D body)
	{
		TryRemove(body);
	}

	private void OnAreaEntered(Area3D area)
	{
		TryAdd(area);
	}

	private void OnAreaExited(Area3D area)
	{
		TryRemove(area);
	}

	private void TryAdd(Node n)
	{
		if (TryFindInteractable(n, out var it) && _candidates.Add(it))
			RebuildOrdered();
	}

	private void OnCycle(int dir)
	{
		if (_ordered.Count == 0)
		{
			_current = null;
			return;
		}

		var idx = _current != null ? _ordered.IndexOf(_current) : -1;
		if (idx < 0) idx = 0;
		idx = Mod(idx + (dir >= 0 ? +1 : -1), _ordered.Count);
		_current = _ordered[idx];
	}

	private static int Mod(int x, int m) => (x % m + m) % m;

	private void TryRemove(Node n)
	{
		if (TryFindInteractable(n, out var it) && _candidates.Remove(it))
		{
			if (ReferenceEquals(_current, it)) _current = null;
			RebuildOrdered();
		}
	}

	private void RebuildOrdered()
	{
		_ordered.Clear();
		_ordered.AddRange(_candidates);

		var parent = GetParent<NetworkedCharacter>();
		var origin = parent.GlobalTransform.Origin;
		var fwd = -parent.GlobalTransform.Basis.Z;

		_ordered.Sort((a, b) =>
		{
			var na = (NetworkedProp)a;
			var nb = (NetworkedProp)b;
			var da = na.GlobalTransform.Origin - origin;
			var db = nb.GlobalTransform.Origin - origin;

			var dotA = fwd.Dot(da.Normalized());
			var dotB = fwd.Dot(db.Normalized());

			// prioritize in front, then nearer, then name for stability
			var cmp = -dotA.CompareTo(dotB);
			if (cmp != 0) return cmp;
			cmp = da.Length().CompareTo(db.Length());
			return cmp != 0 ? cmp : string.Compare(na.Name.ToString(), nb.Name.ToString(), StringComparison.Ordinal);
		});

		_current ??= _ordered.FirstOrDefault();
	}

	private static bool TryFindInteractable(Node n, out IInteractable it)
	{
		while (n != null)
		{
			if (n is IInteractable cast)
			{
				it = cast;
				return true;
			}

			n = n.GetParent();
		}

		it = null!;
		return false;
	}
}
