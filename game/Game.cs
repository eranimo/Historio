using Godot;
using System;
using System.Reactive.Subjects;

public class Polity {
	public string Name;
	public bool IsPlayer;

	public Polity(string name) {
		Name = name;
	}
}

public class Building {
	public enum BuildingType {
		Village,
		Fort,
		Farm,
		Woodcutter,
		Mine,
	}
}

public class Game {
	private BehaviorSubject<bool> playState = new BehaviorSubject<bool>(false);

	public GameWorld world;

	public Game() {

	}

	public IObservable<bool> PlayState { get => playState; }
	public bool IsPlaying { get => playState.Value; }
	public void Play() {
		playState.OnNext(true);
	}
	public void Pause() {
		playState.OnNext(false);
	}
}
