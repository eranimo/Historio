using Godot;

public class ActionSystem : ISystem {
	public void Run(Commands commands) {
		var entities = commands.Query<Entity, ActionQueue>();

		commands.Receive((ActionQueueAdd e) => {
			if (e.owner.Has<ActionQueue>()) {
				e.owner.Get<ActionQueue>().actions.Enqueue(e.action);
			}
		});

		foreach(var (entity, actionQueue) in entities) {
			if (actionQueue.currentAction is not null) {
				if (actionQueue.currentAction.status == ActionStatus.Finished) {
					GD.PrintS("Action finished", actionQueue.currentAction);
					actionQueue.currentAction = null;
					commands.Send(new CurrentActionChanged { entity = entity });
				} else if (actionQueue.currentAction.status == ActionStatus.Cancelled) {
					GD.PrintS("Action cancelled", actionQueue.currentAction);
					actionQueue.currentAction = null;
					commands.Send(new CurrentActionChanged { entity = entity });
				}
			}

			if (actionQueue.currentAction is null && actionQueue.actions.Count > 0) {
				actionQueue.currentAction = actionQueue.actions.Dequeue();
				actionQueue.currentAction.OnStarted();
				// TODO: call CanPerform, use next action if false
				commands.Send(new CurrentActionChanged { entity = entity });
				commands.Send(new ActionQueueChanged { entity = entity });
			}
		}
	}
}