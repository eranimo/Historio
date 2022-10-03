using System.Collections.Generic;
using RelEcs;
using System;
using Godot;

public class PhysicsDelta {
	public float delta;
}

public static class Defs {
	public static DefStore<DistrictType> District = new DefStore<DistrictType>("District", "districts");
	public static DefStore<ImprovementType> Improvement = new DefStore<ImprovementType>("Improvement", "improvements");
	public static DefStore<UnitType> Unit = new DefStore<UnitType>("Unit", "units");
	public static DefStore<ResourceType> Resource = new DefStore<ResourceType>("Resource", "resources");
	public static DefStore<BuildingType> Building = new DefStore<BuildingType>("Building", "buildings");
	public static DefStore<PopProfessionType> PopProfession = new DefStore<PopProfessionType>("PopProfession", "popProfessions");
	public static DefStore<BiotaType> BiotaType = new DefStore<BiotaType>("BiotaType", "biotaType");
}

public class DebugDaySystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		this.GetElement<GameController>().PrintStrayNodes();
	}
}

public class GameManager {
	public RelEcs.World state;

	// systems that run when the game starts (either initially or when loading a game)
	SystemGroup startSystems = new SystemGroup();

	// runs every day (depending on game speed)
    SystemGroup daySystems = new SystemGroup();

	// runs every tick while game is playing, before day systems, and also at game start
	SystemGroup playSystems = new SystemGroup();

	// runs every tick regardless of play state, and also at game start
	SystemGroup tickSystems = new SystemGroup();

	// runs after game ends or player exists
    SystemGroup stopSystems = new SystemGroup();

	// runs every day and at game start
   	SystemGroup renderSystems = new SystemGroup();

	// runs on every tick when game menu is open
	SystemGroup menuSystems = new SystemGroup();

	public WorldService world;
	private PhysicsDelta physicsDelta;

	public GameManager() {
		setup();

		daySystems
			.Add(new ActionDaySystem())
			.Add(new MovementDaySystem())
			.Add(new DebugDaySystem())
			.Add(new BiotaDaySystem());
		
		playSystems
			.Add(new MovementTweenPlaySystem())
			.Add(new ViewStatePlaySystem());
	
		startSystems
			.Add(new ViewStateStartSystem())
			.Add(new PlayerStartSystem());

		renderSystems
			.Add(new SpriteRenderSystem())
			.Add(new UnitRenderSystem())
			.Add(new BorderRenderSystem())
			.Add(new MinimapRenderSystem());
		
		tickSystems
			.Add(new UnitPanelTickSystem())
			.Add(new ActionTickSystem())
			.Add(new UnitPathTickSystem());

		menuSystems
			.Add(new SaveSystem())
			.Add(new SaveGameModalTickSystem());
	}

	private void setup() {
		world = new WorldService(this);
		state = new RelEcs.World();
		state.AddElement(new Layout(new Point(16.666, 16.165), new Point(16 + .5, 18 + .5)));
		state.AddElement(new ViewStateService(this));
		physicsDelta = new PhysicsDelta();
		state.AddElement(physicsDelta);
		state.AddElement(this);

		// services
		state.AddElement(world);
		state.AddElement(new PathfindingService(this));
		state.AddElement(new Factories(this));
		state.AddElement(new BiotaService(this));
		state.AddElement(new SaveService(this));
	}

	public void Reset() {
		setup();
	}

	// called when game starts
	public void Start(GameDate date) {
		state.AddElement<GameDate>(date);
		Godot.GD.PrintS("(GameManager) start");
		startSystems.Run(state);
		tickSystems.Run(state);
		playSystems.Run(state);
		renderSystems.Run(state);
	}

	// called when game stops
	public void Stop() {
		stopSystems.Run(state);
	}

	public void ProcessDay() {
		try {
			daySystems.Run(state);
			renderSystems.Run(state);
			state.Tick();
		} catch (Exception err) {
			GD.PrintErr("Error processing day: ", err);
		}
	}

	public void ProcessPlaying(float delta) {
		try {
			physicsDelta.delta = delta;
			playSystems.Run(state);
		} catch (Exception err) {
			GD.PrintErr("Error processing playing tick: ", err);
		}
	}

	public void Process(float delta) {
		try {
			tickSystems.Run(state);
		} catch (Exception err) {
			GD.PrintErr("Error processing tick: ", err);
		}
	}

	public void ProcessMenu() {
		try {
			menuSystems.Run(state);
		} catch (Exception err) {
			GD.PrintErr("Error processing menu tick: ", err);
		}
	}

	public T Get<T>(Entity entity) where T : class {
		return state.GetComponent<T>(entity.Identity);
	}

	public T Get<T>(Entity entity, Entity target) where T : class {
		return state.GetComponent<T>(entity.Identity, target.Identity);
	}

	public T Get<T>(Entity entity, Type type) where T : class {
		var typeIdentity = state.GetTypeIdentity(type);
		return state.GetComponent<T>(entity.Identity, typeIdentity);
	}

	public EntityBuilder Spawn() {
		return new EntityBuilder(state, state.Spawn().Identity);
	}

	public EntityBuilder On(Entity entity) {
		return new EntityBuilder(state, entity.Identity);
	}
}
