using Godot;
using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;

public class GameMap : Node2D {
	public GameWorld world;
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

	public void RenderMap(GameWorld world) {
		this.world = world;

		terrain = (TileMap) GetNode<TileMap>("Terrain");
		features = (TileMap) GetNode<TileMap>("Features");
		grid = (TileMap) GetNode<TileMap>("Grid");
		selectionHex = (Sprite) GetNode<Sprite>("SelectionHex");
		selectionHex.Hide();
		layout = new Layout(Layout.flat, new Point(16.666, 16.165), new Point(16 + .5, 18 + .5));

		mapBorders = (MapBorders) GetNode<MapBorders>("MapBorders");
		mapBorders.DrawBorders(this, world);
		
		foreach(Tile tile in world.tiles) {
			drawTile(tile);
		}

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

	private void drawTile(Tile tile) {
		grid.SetCell(tile.coord.col, tile.coord.row, 1);
		terrain.SetCell(tile.coord.col, tile.coord.row, tile.GetTerrainTilesetIndex().Value);

		if (tile.GetFeatureTilesetIndex().HasValue) {
			features.SetCell(tile.coord.col, tile.coord.row, tile.GetFeatureTilesetIndex().Value);
		}
	}

	public override void _Input(InputEvent @event) {
		if (this.world == null) {
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
			if (world.IsValidTile(coord)) {
				var tile = world.GetTile(coord);
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
			if (world.IsValidTile(coord)) {
				Tile tile = world.GetTile(coord);
				pressedTile.OnNext(tile);
			}
		}
	}
}
