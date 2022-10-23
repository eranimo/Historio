using System;
using System.Linq;
using Godot;
using MessagePack;

[MessagePackObject]
public class ActionQueue {
	[Key(0)]
	public Action currentAction;
	[Key(1)]
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

[Union(0, typeof(MovementAction))]
public abstract class Action {
	[Key(0)]
	public ActionStatus status;

	[SerializationConstructor]
	public Action() {
		this.status = ActionStatus.Inactive;
	}

	// called before action is added to the queue
	// and before it is started
	public abstract bool CanPerform(ISystem system, Entity owner);

	// called when action has started
	public abstract void OnStarted(ISystem system, Entity owner);

	// called on each day tick after started
	public abstract void OnDayTick(GameDate date);

	public abstract void OnCancelled(ISystem system, Entity owner);

	public abstract void OnQueued(ISystem system, Entity owner);

	public abstract void OnFinished(ISystem system, Entity owner);

	public abstract string GetLabel();

	public override string ToString() {
		return base.ToString() + string.Format("({0})", status);
	}
}

[MessagePackObject]
public class MovementAction : Action {
	[Key(1)]
	public Hex target;

	public override bool CanPerform(ISystem system, Entity owner) {
		return system.HasComponent<Movement>(owner)
			&& system.HasComponent<Location>(owner);
	}

	public override void OnStarted(ISystem system, Entity owner) {
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

	public override void OnQueued(ISystem system, Entity owner) {
		system.Send(new UnitMovementPathUpdated { unit = owner });
	}

	public override void OnFinished(ISystem system, Entity owner) {
		system.Send(new UnitMovementPathUpdated { unit = owner });
	}

	public override void OnCancelled(ISystem system, Entity owner) {
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
