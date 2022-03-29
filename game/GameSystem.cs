using System;
using System.Collections.Generic;

public abstract class GameSystem {
	protected GameManager manager;
	public HashSet<Entity> entities = new HashSet<Entity>();

	public GameSystem(GameManager manager) {
		this.manager = manager;

		foreach (Entity entity in manager.Entities) {
			if (Query(entity)) {
				entities.Add(entity);
				OnEntityAdded(entity);
			}
		}
		manager.OnEntityAdded.Subscribe((Entity entity) => {
			if (Query(entity)) {
				entities.Add(entity);
				OnEntityAdded(entity);
			}
		});
		manager.OnEntityRemoved.Subscribe((Entity entity) => {
			if (Query(entity)) {
				entities.Remove(entity);
				OnEntityRemoved(entity);
			}
		});
	}

	public abstract void OnStart();
	public abstract bool Query(Entity entity);
	public abstract void OnEntityAdded(Entity entity);
	public abstract void OnEntityRemoved(Entity entity);
}
