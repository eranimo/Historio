using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

public class EntityQuery<T> where T : Entity {
	private Type entityType;

	private Func<Entity, bool> whereFunc = null;
	private EntityQueryDispose disposer;
	private readonly GameManager manager;

	public EntityQuery(GameManager manager, Type entityType) {
		this.manager = manager;
		this.entityType = entityType;
	}

	public class EntityQueryDispose : IDisposable {
		private readonly GameManager manager;
		private readonly EntityQuery<T> query;
		private readonly IDisposable onEntityAddedSubscriber;
		private readonly IDisposable onEntityRemovedSubscriber;

		public EntityQueryDispose(
			GameManager manager,
			EntityQuery<T> query,
			IDisposable onEntityAddedSubscriber,
			IDisposable onEntityRemovedSubscriber
		) {
			this.manager = manager;
			this.query = query;
			this.onEntityAddedSubscriber = onEntityAddedSubscriber;
			this.onEntityRemovedSubscriber = onEntityRemovedSubscriber;
		}

		public void Dispose() {
			onEntityAddedSubscriber.Dispose();
			onEntityRemovedSubscriber.Dispose();
		}
	}

	public EntityQuery<T> Where(Func<Entity, bool> func) {
		this.whereFunc = func;
		return this;
	}

	private bool shouldAddEntity(Entity entity) {
		return whereFunc == null || whereFunc(entity);
	}

	public EntityQuery<T> Execute(Action<Entity> onEntityAdded, Action<Entity> onEntityRemoved) {
		manager.RegisterEntityType(entityType);
		foreach (Entity entity in manager.GetEntitiesByType(entityType).Where(shouldAddEntity)) {
			onEntityAdded(entity);
		}

		var onEntityAddedSubscriber = manager.EntityType(entityType).OnEntityAdded.Where(shouldAddEntity).Subscribe(onEntityAdded);
		var onEntityRemovedSubscriber = manager.EntityType(entityType).OnEntityRemoved.Where(shouldAddEntity).Subscribe(onEntityRemoved);
		this.disposer = new EntityQueryDispose(manager, this, onEntityAddedSubscriber, onEntityRemovedSubscriber);
		return this;
	}

	public void Dispose() {
		disposer.Dispose();
	}
}
