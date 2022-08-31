using System.Linq;
using Godot;

public class ActionTickSystem :ISystem {
	public void Run(Commands commands) {
		commands.Receive((ActionQueueClear e) => {
			if (e.owner.Has<ActionQueue>()) {
				var actionQueue = e.owner.Get<ActionQueue>();
				GD.PrintS("(ActionTickSystem) Action cancelled", actionQueue.currentAction);
				if (actionQueue.currentAction is not null) {
					actionQueue.currentAction.status = ActionStatus.Cancelled;
					actionQueue.currentAction.OnCancelled(commands);
				}
				actionQueue.currentAction = null;
				commands.Send(new CurrentActionChanged { entity = e.owner });
				commands.Send(new ActionQueueChanged { entity = e.owner });
				e.owner.Get<ActionQueue>().actions.Clear();
			}
		});

		commands.Receive((ActionQueueAdd e) => {
			if (e.owner.Has<ActionQueue>()) {
				GD.PrintS("(ActionTickSystem) Action queued", e.action);
				if (e.action.CanPerform()) {
					e.owner.Get<ActionQueue>().actions.Enqueue(e.action);
					commands.Send(new ActionQueueChanged { entity = e.owner });
					e.action.OnQueued(commands);
				} else {
					e.action.status = ActionStatus.Cancelled;
					e.action.OnCancelled(commands);
				}
			}
		});

		commands.Receive((ActionQueueRemove e) => {
			if (e.owner.Has<ActionQueue>()) {
				var actionQueue = e.owner.Get<ActionQueue>();
				e.action.OnCancelled(commands);
				actionQueue.actions = new Queue<Action>(actionQueue.actions.Where(a => a != e.action));
				commands.Send(new ActionQueueChanged { entity = e.owner });
			}
		});
	}
}

public class ActionSystem : ISystem {
	public void Run(Commands commands) {
		var entities = commands.Query<Entity, ActionQueue>();

		foreach(var (entity, actionQueue) in entities) {
			if (actionQueue.currentAction is not null) {
				if (actionQueue.currentAction.status == ActionStatus.Finished) {
					GD.PrintS("(ActionSystem) Action finished", actionQueue.currentAction);
					actionQueue.currentAction.OnFinished(commands);
					actionQueue.currentAction = null;
					commands.Send(new CurrentActionChanged { entity = entity });
				} else if (actionQueue.currentAction.status == ActionStatus.Cancelled) {
					GD.PrintS("(ActionSystem) Action cancelled", actionQueue.currentAction);
					actionQueue.currentAction = null;
					commands.Send(new CurrentActionChanged { entity = entity });
				}
			}

			if (actionQueue.currentAction is null && actionQueue.actions.Count > 0) {
				actionQueue.currentAction = actionQueue.actions.Dequeue();
				actionQueue.currentAction.OnStarted(commands);
				commands.Send(new ActionStarted { entity = entity });
			}
		}
	}
}