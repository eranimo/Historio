using Godot;
using System;

public class GameContext : Node {
	public Game Game = null;

	public void OnGameInit(Game game) {
		this.Game = game;
	}

	public void OnGameEnd() {
		this.Game = null;
	}
}