using RelEcs;

public class GameManager {
	public RelEcs.World state;

	// systems that run when the game starts (either initially or when loading a game)
	SystemGroup startSystems = new SystemGroup();

	// runs every day (game)
    SystemGroup runSystems = new SystemGroup();

	// runs every day (UI)
    SystemGroup uiSystems = new SystemGroup();

	// runs after game ends or player exists
    SystemGroup stopSystems = new SystemGroup();

	public WorldService world;

	public GameManager() {
		world = new WorldService(this);
		state = new RelEcs.World();
	}

	// called when game starts
	public void Start() {
		Godot.GD.PrintS("GameManager start");
		startSystems.Run(state);
	}

	// called when game stops
	public void Stop() {
		stopSystems.Run(state);
	}

	public void Process(GameDate date) {
		runSystems.Run(state);
		state.Tick();
	}
}
