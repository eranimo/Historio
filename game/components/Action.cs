using System;
using System.Linq;

public class ActionQueue {
	public Action currentAction;
	public Queue<Action> actions = new Queue<Action>();

	public void removeAction(Action action) {
		actions = new Queue<Action>(actions.Where(a => a != action));
	}
}

// trigger when action queue has changed
public class ActionQueueChanged {
	public Entity entity;
}

// trigger when current action changes
public class CurrentActionChanged {
	public Entity entity;
}

// trigger to add an action to an entity
public class ActionQueueAdd {
	public Entity owner;
	public Action action;
}

// trigger to clear action queue
public class ActionQueueClear {
	public Entity owner;
}

public enum ActionType {
	Movement,
}

public enum ActionStatus {
	Inactive, // in queue and not started
	Active, // action started and is currentAction
	Cancelled, // action started but was cancelled, will be removed 
	Finished, // action finished, will be removed
}

public abstract class Action {
	public static ActionType type;
	public Entity owner;
	public ActionStatus status;

	public Action(Entity owner) {
		this.owner = owner;
		this.status = ActionStatus.Inactive;
	}

	// called before action is added to the queue
	// and before it is started
	public abstract bool CanPerform();

	// called when action has started
	public abstract void OnStarted();

	// called on each day tick after started
	public abstract void OnDayTick(GameDate date);

	public abstract void OnCancelled();

	public abstract string GetLabel();

	public static Dictionary<ActionType, string> actionTypeNames = new Dictionary<ActionType, string>() {
		{ ActionType.Movement, "Movement" },
	};

	public override string ToString() {
		return base.ToString() + string.Format("({0})", status);
	}
}

public class MovementAction : Action {
	public static new ActionType type = ActionType.Movement;
	public Hex target;

	public MovementAction(
		Entity owner,
		Hex target
	) : base(owner) {
		this.target = target;
	}

	public override bool CanPerform() {
		return this.owner.Has<Movement>();
	}

	public override void OnStarted() {
		var movement = owner.Get<Movement>();
		movement.currentTarget = target;
		movement.movementAction = this;
	}

	public override void OnCancelled() {
		var movement = owner.Get<Movement>();
		movement.Reset();
	}

	public override void OnDayTick(GameDate date) {
		
	}

	public override string GetLabel() {
		return $"Moving to {target}";
	}
}
