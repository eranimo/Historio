using System;
using System.Linq;
using Godot;

[Serializable]
public class ActionQueue {
	public Action currentAction;
	public Queue<Action> actions = new Queue<Action>();
}

// trigger when action queue has changed
public class ActionQueueChanged {
	public Entity entity;
}

// trigger when current action changes
public class CurrentActionChanged {
	public Entity entity;
}

// trigger when action is started
public class ActionStarted {
	public Entity entity;
}

// trigger to add an action to an entity
public class ActionQueueAdd {
	public Entity owner;
	public Action action;
}

// trigger when action is removed from queue
public class ActionQueueRemove {
	public Entity owner;
	public Action action;
}

// trigger to clear action queue
public class ActionQueueClear {
	public Entity owner;
}

public enum ActionStatus {
	Inactive, // in queue and not started
	Active, // action started and is currentAction
	Cancelled, // action started but was cancelled, will be removed 
	Finished, // action finished, will be removed
}

[Serializable]
public abstract class Action {
	public Entity owner;
	public ActionStatus status;

	public Action(Entity owner) {
		this.owner = owner;
		this.status = ActionStatus.Inactive;
	}

	// called before action is added to the queue
	// and before it is started
	public abstract bool CanPerform(ISystem system);

	// called when action has started
	public abstract void OnStarted(ISystem system);

	// called on each day tick after started
	public abstract void OnDayTick(GameDate date);

	public abstract void OnCancelled(ISystem system);

	public abstract void OnQueued(ISystem system);

	public abstract void OnFinished(ISystem system);

	public abstract string GetLabel();

	public override string ToString() {
		return base.ToString() + string.Format("({0})", status);
	}
}

public class MovementAction : Action {
	public Hex target;

	public MovementAction(
		Entity owner,
		Hex target
	) : base(owner) {
		this.target = target;
	}

	public override bool CanPerform(ISystem system) {
		return system.HasComponent<Movement>(this.owner)
			&& system.HasComponent<Location>(this.owner);
	}

	public override void OnStarted(ISystem system) {
		var world = system.GetElement<WorldService>();
		var pathfinder = system.GetElement<PathfindingService>();

		var movement = system.GetComponent<Movement>(owner);
		var location = system.GetComponent<Location>(owner);

		movement.currentTarget = target;
		movement.movementAction = this;

		var fromTile = world.GetTile(location.hex);
		var toTile = world.GetTile(movement.currentTarget);

		GD.PrintS("(MovementAction) From:", location.hex, "To:", movement.currentTarget);
		var path = pathfinder.getPath(fromTile, toTile);
		if (path == null) {
			GD.PrintS("(MovementAction) No path to target, removing target");
			movement.currentTarget = null;
			status = ActionStatus.Cancelled;
		} else {
			GD.PrintS("(MovementAction) Found path:", String.Join(", ", path));
			movement.path = new List<Hex>(path);
			movement.currentPathIndex = 0;
		}			
	}

	public override void OnQueued(ISystem system) {
		system.Send(new UnitMovementPathUpdated { unit = owner });
	}

	public override void OnFinished(ISystem system) {
		system.Send(new UnitMovementPathUpdated { unit = owner });
	}

	public override void OnCancelled(ISystem system) {
		var movement = system.GetComponent<Movement>(owner);
		system.Send(new UnitMovementPathUpdated { unit = owner });
		movement.Reset();
	}

	public override void OnDayTick(GameDate date) {
		
	}

	public override string GetLabel() {
		return $"Moving to {target}";
	}
}
