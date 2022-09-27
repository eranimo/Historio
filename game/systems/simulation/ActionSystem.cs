using System.Linq;
using Godot;

public class ActionTickSystem :ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		foreach (var e in this.Receive<ActionQueueClear>()) {
			if (this.HasComponent<ActionQueue>(e.owner)) {
				var actionQueue = this.GetComponent<ActionQueue>(e.owner);
				GD.PrintS("(ActionTickSystem) Action cancelled", actionQueue.currentAction);
				if (actionQueue.currentAction is not null) {
					actionQueue.currentAction.status = ActionStatus.Cancelled;
					actionQueue.currentAction.OnCancelled(this);
				}
				actionQueue.currentAction = null;
				this.Send(new CurrentActionChanged { entity = e.owner });
				this.Send(new ActionQueueChanged { entity = e.owner });
				this.GetComponent<ActionQueue>(e.owner).actions.Clear();
			}
		}

		foreach (var e in this.Receive<ActionQueueAdd>()) {
			if (this.HasComponent<ActionQueue>(e.owner)) {
				GD.PrintS("(ActionTickSystem) Action queued", e.action);
				if (e.action.CanPerform(this)) {
					this.GetComponent<ActionQueue>(e.owner).actions.Enqueue(e.action);
					this.Send(new ActionQueueChanged { entity = e.owner });
					e.action.OnQueued(this);
				} else {
					e.action.status = ActionStatus.Cancelled;
					e.action.OnCancelled(this);
				}
			}
		}

		foreach (var e in this.Receive<ActionQueueRemove>()) {
			if (this.HasComponent<ActionQueue>(e.owner)) {
				var actionQueue = this.GetComponent<ActionQueue>(e.owner);
				e.action.OnCancelled(this);
				actionQueue.actions = new Queue<Action>(actionQueue.actions.Where(a => a != e.action));
				this.Send(new ActionQueueChanged { entity = e.owner });
			}
		}
	}
}

public class ActionDaySystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var entities = this.Query<Entity, ActionQueue>();

		foreach(var (entity, actionQueue) in entities) {
			if (actionQueue.currentAction is not null) {
				if (actionQueue.currentAction.status == ActionStatus.Finished) {
					GD.PrintS("(ActionSystem) Action finished", actionQueue.currentAction);
					actionQueue.currentAction.OnFinished(this);
					actionQueue.currentAction = null;
					this.Send(new CurrentActionChanged { entity = entity });
				} else if (actionQueue.currentAction.status == ActionStatus.Cancelled) {
					GD.PrintS("(ActionSystem) Action cancelled", actionQueue.currentAction);
					actionQueue.currentAction = null;
					this.Send(new CurrentActionChanged { entity = entity });
				}
			}

			if (actionQueue.currentAction is null && actionQueue.actions.Count > 0) {
				actionQueue.currentAction = actionQueue.actions.Dequeue();
				actionQueue.currentAction.OnStarted(this);
				this.Send(new ActionStarted { entity = entity });
			}
		}
	}
}