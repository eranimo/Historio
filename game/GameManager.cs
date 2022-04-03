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

public class EntityTypeSubscription {
	public Subject<Entity> OnEntityAdded = new Subject<Entity>();
	public Subject<Entity> OnEntityRemoved = new Subject<Entity>();
}

public class GameManager {
	private HashSet<Entity> entities = new HashSet<Entity>();
	private Dictionary<Type, List<Entity>> entitiesByType = new Dictionary<Type, List<Entity>>();
	private Dictionary<Type, EntityTypeSubscription> entityTypeSubscriptions = new Dictionary<Type, EntityTypeSubscription>();
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

		var b1 = new Building(Building.BuildingType.Village, world.GetTile(new Hex(1, 1)));
		AddEntity(b1);
	}

	public IEnumerable<Entity> GetEntitiesByType(Type type) {
		if (!entitiesByType.ContainsKey(type)) {
			yield break;
		}
		foreach (Entity entity in entitiesByType[type]) {
			yield return entity;
		}
	}

	public IEnumerable<Entity> Entities => entities;
	public EntityQuery<T> Query<T>() where T : Entity => new EntityQuery<T>(this, typeof(T));

	public void RegisterEntityType(Type entityType) {
		if (!entitiesByType.ContainsKey(entityType)) {
			entitiesByType[entityType] = new List<Entity>();
			entityTypeSubscriptions[entityType] = new EntityTypeSubscription();
		}
	}


	public void AddEntity(Entity entity) {
		if (entities.Contains(entity)) {
			throw new Exception("Cannot add Entity that is already added");
		}
		entities.Add(entity);

		var entityType = entity.GetType();

		RegisterEntityType(entityType);

		entitiesByType[entityType].Add(entity);
		entity.Init(this);
		entity.OnAdded();
		OnEntityAdded.OnNext(entity);
		entityTypeSubscriptions[entityType].OnEntityAdded.OnNext(entity);
	}

	public void RemoveEntity(Entity entity) {
		if (entities.Contains(entity)) {
			throw new Exception("Cannot remove Entity that is not added");
		}
		var entityType = entity.GetType();
		deleteQueue.Enqueue(entity);
		entity.deleted = true;
		OnEntityRemoved.OnNext(entity);
		entityTypeSubscriptions[entityType].OnEntityRemoved.OnNext(entity);
	}

	public EntityTypeSubscription EntityType(Type entityType) => entityTypeSubscriptions[entityType];

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
