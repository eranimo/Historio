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
}

public class GameManager {
	public RelEcs.World state;

	// systems that run when the game starts (either initially or when loading a game)
	SystemGroup startSystems = new SystemGroup();

	// runs every day (game)
    SystemGroup daySystems = new SystemGroup();

	// runs every 60 FPS, before day systems
	SystemGroup tickSystems = new SystemGroup();

	// runs after game ends or player exists
    SystemGroup stopSystems = new SystemGroup();

	// runs every day and at game start
   	SystemGroup renderSystems = new SystemGroup();

	// runs every tick and at game start
	SystemGroup frameSystems = new SystemGroup();

	public World world;
	private PhysicsDelta physicsDelta;

	public GameManager() {
		world = new World(this);
		state = new RelEcs.World();
		state.AddElement(new Layout(new Point(16.666, 16.165), new Point(16 + .5, 18 + .5)));
		state.AddElement(new MapViewState(this));
		physicsDelta = new PhysicsDelta();
		state.AddElement(physicsDelta);

		// services
		state.AddElement(world);
		state.AddElement(new Pathfinder(this));
		state.AddElement(new Factories(this));

		daySystems
			.Add(new ActionSystem())
			.Add(new MovementSystem());
		
		tickSystems
			.Add(new MovementTweenSystem())
			.Add(new ViewStateSystem());
	
		startSystems
			.Add(new ViewStateStartupSystem())
			.Add(new ViewStateSystem())
			.Add(new PlayerStartSystem());

		renderSystems
			.Add(new SpriteRenderSystem())
			.Add(new UnitRenderSystem())
			.Add(new BorderRenderSystem())
			.Add(new MinimapRenderSystem());
		
		frameSystems
			.Add(new UnitPanelUISystem())
			.Add(new ActionTickSystem())
			.Add(new UnitPathSystem());
	}

	// called when game starts
	public void Start(GameDate date) {
		state.AddElement<GameDate>(date);
		Godot.GD.PrintS("(GameManager) start");
		startSystems.Run(state);
		renderSystems.Run(state);
		frameSystems.Run(state);
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

	public void Process(float delta) {
		try {
			physicsDelta.delta = delta;
			tickSystems.Run(state);
		} catch (Exception err) {
			GD.PrintErr("Error processing tick: ", err);
		}
	}

	public void UIProcess(float delta) {
		try {
			frameSystems.Run(state);
		} catch (Exception err) {
			GD.PrintErr("Error processing frame: ", err);
		}
	}
}
