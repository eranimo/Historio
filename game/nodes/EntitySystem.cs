using Godot;
using System;

public abstract class EntitySystem : Node {
	private GameContext gameContext;
	private Type entityType;

	public EntitySystem(Type entityType) {
		this.entityType = entityType;
	}

	public override void _Ready() {
		gameContext = (GameContext) GetTree().Root.GetNode("GameContext");
		var manager = gameContext.game.manager;
		foreach (Entity entity in manager.GetEntitiesByType(entityType)) {
			OnEntityAdded(entity);
		}
		manager.OnEntityAdded.Subscribe((Entity entity) => {
			if (entity.GetType() == entityType) {
				OnEntityAdded(entity);
			}
		});
		manager.OnEntityRemoved.Subscribe((Entity entity) => {
			if (entity.GetType() == entityType) {
				OnEntityRemoved(entity);
			}
		});
	}

	public abstract void OnEntityAdded(Entity entity);
	public abstract void OnEntityRemoved(Entity entity);
}