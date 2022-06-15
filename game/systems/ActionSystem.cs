using Godot;

public class ActionTickSystem :ISystem {
	public void Run(Commands commands) {
		commands.Receive((ActionQueueClear e) => {
			if (e.owner.Has<ActionQueue>()) {
				var actionQueue = e.owner.Get<ActionQueue>();
				GD.PrintS("(ActionTickSystem) Action cancelled", actionQueue.currentAction);
				if (actionQueue.currentAction is not null) {
					actionQueue.currentAction.OnCancelled();
				}
				actionQueue.currentAction = null;
				commands.Send(new CurrentActionChanged { entity = e.owner });
				e.owner.Get<ActionQueue>().actions.Clear();
			}
		});

		commands.Receive((ActionQueueAdd e) => {
			if (e.owner.Has<ActionQueue>()) {
				GD.PrintS("(ActionTickSystem) Action added", e.action);
				e.owner.Get<ActionQueue>().actions.Enqueue(e.action);
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
				actionQueue.currentAction.OnStarted();
				// TODO: call CanPerform, use next action if false
				commands.Send(new CurrentActionChanged { entity = entity });
				commands.Send(new ActionQueueChanged { entity = entity });
			}
		}
	}
}