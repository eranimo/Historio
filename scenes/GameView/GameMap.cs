using Godot;
using System;
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
	private MapBorders mapBorders;

	// subject events
	private Subject<Tile> tileUpdates = new Subject<Tile>();
	private Subject<Tile> pressedTile = new Subject<Tile>();
	private Subject<Tile> hoveredTile = new Subject<Tile>();

	private BehaviorSubject<Tile> selectedHex = new BehaviorSubject<Tile>(null);

	public async void RenderMap(Game game) {
		GD.PrintS("(GameMap) render map");
		this.game = game;

		terrain = (TileMap) GetNode<TileMap>("Terrain");
		features = (TileMap) GetNode<TileMap>("Features");
		grid = (TileMap) GetNode<TileMap>("Grid");
		selectionHex = (Sprite) GetNode<Sprite>("SelectionHex");
		selectionHex.Hide();
		layout = new Layout(Layout.flat, new Point(16.666, 16.165), new Point(16 + .5, 18 + .5));

		mapBorders = (MapBorders) GetNode<MapBorders>("MapBorders");
		mapBorders.RenderMap(this);

		mapBuildings = (MapBuildings) GetNode<MapBuildings>("MapBuildings");
		mapBuildings.RenderMap(this);

		var tiles = game.manager.world.GetTiles();
		var i = 0;
		foreach (Tile tile in tiles) {
			drawTile(tile);
			i++;
		}
		GD.PrintS("Draw", i, "tiles");

		tileUpdates.Subscribe((Tile tile) => this.drawTile(tile));

		Observable.DistinctUntilChanged(pressedTile).Subscribe((Tile tile) => {
			if (selectedHex.Value == tile) {
				selectedHex.OnNext(null);
			} else {
				selectedHex.OnNext(tile);
			}
		});

		selectedHex.Subscribe((Tile tile) => {
			if (tile != null) {
				selectionHex.Show();
				Hex hex = OffsetCoord.QoffsetToCube(OffsetCoord.ODD, tile.coord);
				selectionHex.Position = layout.HexToPixel(hex).ToVector() - layout.origin.ToVector();
			} else {
				selectionHex.Hide();
			}
		});
	}

	private bool is_placing = false;
	private MapBuildings mapBuildings;

	private void drawTile(Tile tile) {
		grid.SetCell(tile.coord.col, tile.coord.row, 1);
		terrain.SetCell(tile.coord.col, tile.coord.row, tile.GetTerrainTilesetIndex().Value);

		if (tile.GetFeatureTilesetIndex().HasValue) {
			features.SetCell(tile.coord.col, tile.coord.row, tile.GetFeatureTilesetIndex().Value);
		}
	}

	public override void _Input(InputEvent @event) {
		if (this.game == null) {
			return;
		}
		base._Input(@event);

		if (@event.IsActionPressed("view_select")) {
			is_placing = true;
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

	private OffsetCoord getCoordAtCursor() {
		var cursorPos = GetLocalMousePosition();
		var clickedCoord = layout.PixelToHex(new Point(cursorPos.x, cursorPos.y)).HexRound();
		var clickedCoordOffset = OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, clickedCoord);
		return clickedCoordOffset;
	}

	public override void _PhysicsProcess(float delta) {
		base._PhysicsProcess(delta);

		if (is_placing) {
			var coord = getCoordAtCursor();
			if (game.manager.world.IsValidTile(coord)) {
				Tile tile = game.manager.world.GetTile(coord);
				pressedTile.OnNext(tile);
			}
		}
	}
}
