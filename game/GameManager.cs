using System.Collections.Generic;
using RelEcs;


public class ViewSystem : ISystem {
	void ISystem.Run(Commands commands) {
		Process(commands);
	}
	public virtual void Process(Commands commands) {}
}

public class GameManager {
	public RelEcs.World state;

	// systems that run when the game starts (either initially or when loading a game)
	SystemGroup startSystems = new SystemGroup();

	// runs every day (game)
    SystemGroup runSystems = new SystemGroup();

	// runs after game ends or player exists
    SystemGroup stopSystems = new SystemGroup();

	// runs every day and at game start
   	SystemGroup renderSystems = new SystemGroup();

	public World world;

	public GameManager() {
		world = new World(this);
		state = new RelEcs.World();
		state.AddElement(world);

		renderSystems
			.Add(new SpriteRenderSystem())
			.Add(new TerritoryRenderSystem());
	}

	// called when game starts
	public void Start() {
		Godot.GD.PrintS("(GameManager) start");
		startSystems.Run(state);
		renderSystems.Run(state);
	}

	// called when game stops
	public void Stop() {
		stopSystems.Run(state);
	}

	public void Process(GameDate date) {
		runSystems.Run(state);
		renderSystems.Run(state);
		Godot.GD.PrintS("Process", date.dayTicks);
		state.Tick();
	}
}
