using Godot;
using RelEcs;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;

public class GameMap : Node2D {
	public Game game;
	public Layout layout;

	private TileMap terrain;
	private TileMap features;
	private TileMap grid;
	private Sprite selectionHex;
	private Sprite hoverHex;

	// subject events
	private Subject<Entity> tileUpdates = new Subject<Entity>();
	private Subject<Entity> pressedTile = new Subject<Entity>();
	private Subject<Entity> clickedTile = new Subject<Entity>();
	private Subject<Entity> hoveredTile = new Subject<Entity>();

	public BehaviorSubject<Entity> selectedHex = new BehaviorSubject<Entity>(null);

	private bool is_placing = false;

	public MapBorders mapBorders;
	public Node2D spriteContainer;

	public override void _Ready() {
		GD.PrintS("(GameMap) ready");
		terrain = (TileMap) GetNode<TileMap>("Terrain");
		features = (TileMap) GetNode<TileMap>("Features");
		grid = (TileMap) GetNode<TileMap>("Grid");
		selectionHex = (Sprite) GetNode<Sprite>("SelectionHex");
		hoverHex = (Sprite) GetNode<Sprite>("HoverHex");
		spriteContainer = (Node2D) GetNode<Node2D>("SpriteContainer");
		mapBorders = (MapBorders) GetNode<MapBorders>("MapBorders");

		layout = new Layout(new Point(16.666, 16.165), new Point(16 + .5, 18 + .5));

		clickedTile.Subscribe((Entity tile) => {
			if (selectedHex.Value == tile) {
				selectedHex.OnNext(null);
			} else {
				selectedHex.OnNext(tile);
			}
		});

		selectedHex.Subscribe((Entity tile) => {
			if (tile is null) {
				selectionHex.Hide();
			} else {
				selectionHex.Show();
				var coord = tile.Get<Hex>();
				selectionHex.Position = layout.HexToPixel(coord).ToVector();
			}
		});

		hoveredTile.Subscribe((Entity tile) => {
			if (tile is null) {
				hoverHex.Hide();
			} else {
				hoverHex.Show();
				var coord = tile.Get<Hex>();
				hoverHex.Position = layout.HexToPixel(coord).ToVector();
			}
		});
	}

	public void RenderMap(Game game) {
		GD.PrintS("(GameMap) render map");
		this.game = game;

		selectionHex.Hide();
		hoverHex.Hide();
		mapBorders.RenderMap(this);

		drawWorld();
		tileUpdates.Subscribe((Entity tile) => this.drawTile(tile));
	}

	private void drawWorld() {
		foreach (Entity tile in game.manager.world.tiles) {
			drawTile(tile);
		}
	}

	private void drawTile(Entity tile) {
		var coord = tile.Get<Hex>();
		var data = tile.Get<TileData>();
		grid.SetCell(coord.col, coord.row, 1);
		terrain.SetCell(coord.col, coord.row, data.GetTerrainTilesetIndex().Value);

		if (data.GetFeatureTilesetIndex().HasValue) {
			features.SetCell(coord.col, coord.row, data.GetFeatureTilesetIndex().Value);
		}
	}

	public override void _Input(InputEvent @event) {
		if (this.game == null) {
			return;
		}
		base._Input(@event);

		if (@event.IsActionPressed("view_select")) {
			is_placing = true;
			var coord = getCoordAtCursor();
			if (game.manager.world.IsValidTile(coord)) {
				var tile = game.manager.world.GetTile(coord);
				clickedTile.OnNext(tile);
			}
		} else if (@event.IsActionReleased("view_select")) {
			is_placing = false;
		}

		if (@event is InputEventMouseMotion) {
			var coord = getCoordAtCursor();
			if (game.manager.world.IsValidTile(coord)) {
				var tile = game.manager.world.GetTile(coord);
				hoveredTile.OnNext(tile);
			}
		}
	}

	private Hex getCoordAtCursor() {
		var cursorPos = GetLocalMousePosition();
		return layout.PixelToHex(new Point(cursorPos.x, cursorPos.y));
	}

	public override void _PhysicsProcess(float delta) {
		base._PhysicsProcess(delta);

		if (is_placing) {
			var coord = getCoordAtCursor();
			if (game.manager.world.IsValidTile(coord)) {
				var tile = game.manager.world.GetTile(coord);
				pressedTile.OnNext(tile);
			}
		}
	}
}
