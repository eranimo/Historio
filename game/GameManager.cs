using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

// container for global game state
public class GameState {
	public WorldOptions worldOptions;
	public Hex worldSize;
}

public abstract class GameSystem {
	protected GameManager manager;

	public GameSystem(GameManager manager) {
		this.manager = manager;

		foreach (Entity entity in manager.Entities) {
			if (Query(entity)) {
				OnEntityAdded(entity);
			}
		}
		manager.OnEntityAdded.Subscribe((Entity entity) => {
			if (Query(entity)) {
				OnEntityAdded(entity);
			}
		});
		manager.OnEntityRemoved.Subscribe((Entity entity) => {
			if (Query(entity)) {
				OnEntityRemoved(entity);
			}
		});
	}

	public abstract void OnStart();
	public abstract bool Query(Entity entity);
	public abstract void OnEntityAdded(Entity entity);
	public abstract void OnEntityRemoved(Entity entity);
}

public class GameManager {
	private HashSet<Entity> entities = new HashSet<Entity>();
	private Dictionary<Type, List<Entity>> entitiesByType = new Dictionary<Type, List<Entity>>();
	private Queue<Entity> deleteQueue = new Queue<Entity>();
	public Subject<Entity> OnEntityAdded = new Subject<Entity>();
	public Subject<Entity> OnEntityRemoved = new Subject<Entity>();

	public GameState state = new GameState();
	public GameWorld world;

	public GameManager() {
		// setup GameSystems
		world = new GameWorld(this);
	}

	public void Start() {
		world.OnStart();

		var b1 = new Building(Building.BuildingType.Village, world.GetTile(new Hex(1, 1)));

		var namegen = new NameGenerator("greek");
		for (int i = 0; i < 10; i++)
			GD.PrintS(namegen.GetName(3, 7));
		AddEntity(b1);
	}

	public IEnumerable<Entity> GetEntitiesByType(Type type) {
		foreach (Entity entity in entitiesByType[type]) {
			yield return entity;
		}
	}

	public IEnumerable<Entity> Entities => entities;

	public void AddEntity(Entity entity) {
		if (entities.Contains(entity)) {
			throw new Exception("Cannot add Entity that is already added");
		}
		entities.Add(entity);

		var entityType = entity.GetType();

		if (!entitiesByType.ContainsKey(entity.GetType())) {
			entitiesByType[entityType] = new List<Entity>();
		}
		entitiesByType[entityType].Add(entity);
		entity.Init(this);
		entity.OnAdded();
		OnEntityAdded.OnNext(entity);
	}

	public void RemoveEntity(Entity entity) {
		if (entities.Contains(entity)) {
			throw new Exception("Cannot remove Entity that is not added");
		}
		deleteQueue.Enqueue(entity);
		entity.deleted = true;
		OnEntityRemoved.OnNext(entity);
	}

	public void Process(GameDate date) {
		foreach (Entity entity in entities) {
			if (entity.deleted == false) {
				entity.Update(date);
			}
		}

		// remove entities marked as deleted
		while (deleteQueue.Count > 0) {
			Entity entity = deleteQueue.Dequeue();
			deleteEntity(entity);
		}
	}

	private void deleteEntity(Entity entity) {
		entities.Remove(entity);
		entitiesByType[entity.GetType()].Remove(entity);
		entity.OnRemoved();
	}

	public void Export(File file) {
		// TODO: implement
	}

	public void Import(File file) {
		// TODO: implement
	}
}
