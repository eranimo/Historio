using System.Linq;
using Godot;

public partial class ActionTickSystem :ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		foreach (var e in World.Receive<ActionQueueClear>(this)) {
			if (World.HasComponent<ActionQueue>(e.owner)) {
				var actionQueue = World.GetComponent<ActionQueue>(e.owner);
				GD.PrintS("(ActionTickSystem) Action cancelled", actionQueue.currentAction);
				if (actionQueue.currentAction is not null) {
					actionQueue.currentAction.status = ActionStatus.Cancelled;
					actionQueue.currentAction.OnCancelled(this, e.owner);
				}
				actionQueue.currentAction = null;
				World.Send(new CurrentActionChanged { entity = e.owner });
				World.Send(new ActionQueueChanged { entity = e.owner });
				World.GetComponent<ActionQueue>(e.owner).actions.Clear();
			}
		}

		foreach (var e in World.Receive<ActionQueueAdd>(this)) {
			if (World.HasComponent<ActionQueue>(e.owner)) {
				GD.PrintS("(ActionTickSystem) Action queued", e.action);
				if (e.action.CanPerform(this, e.owner)) {
					World.GetComponent<ActionQueue>(e.owner).actions.Enqueue(e.action);
					World.Send(new ActionQueueChanged { entity = e.owner });
					e.action.OnQueued(this, e.owner);
				} else {
					e.action.status = ActionStatus.Cancelled;
					e.action.OnCancelled(this, e.owner);
				}
			}
		}

		foreach (var e in World.Receive<ActionQueueRemove>(this)) {
			if (World.HasComponent<ActionQueue>(e.owner)) {
				var actionQueue = World.GetComponent<ActionQueue>(e.owner);
				e.action.OnCancelled(this, e.owner);
				actionQueue.actions = new Queue<Action>(actionQueue.actions.Where(a => a != e.action));
				World.Send(new ActionQueueChanged { entity = e.owner });
			}
		}
	}
}

public partial class ActionDaySystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var entities = World.Query<Entity, ActionQueue>().Build();

		foreach(var (entity, actionQueue) in entities) {
			if (actionQueue.currentAction is not null) {
				if (actionQueue.currentAction.status == ActionStatus.Finished) {
					GD.PrintS("(ActionSystem) Action finished", actionQueue.currentAction);
					actionQueue.currentAction.OnFinished(this, entity);
					actionQueue.currentAction = null;
					World.Send(new CurrentActionChanged { entity = entity });
				} else if (actionQueue.currentAction.status == ActionStatus.Cancelled) {
					GD.PrintS("(ActionSystem) Action cancelled", actionQueue.currentAction);
					actionQueue.currentAction = null;
					World.Send(new CurrentActionChanged { entity = entity });
				}
			}

			if (actionQueue.currentAction is null && actionQueue.actions.Count > 0) {
				actionQueue.currentAction = actionQueue.actions.Dequeue();
				actionQueue.currentAction.OnStarted(this, entity);
				World.Send(new ActionStarted { entity = entity });
			}
		}
	}
}