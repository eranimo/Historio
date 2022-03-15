using Godot;
using System;

public class GameContext : Node {
	public Game game = null;

	public void OnGameInit(Game game) {
		this.game = game;
	}

	public void OnGameEnd() {
		this.game = null;
	}
}
