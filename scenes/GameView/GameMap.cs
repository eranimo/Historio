using Godot;
using RelEcs;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Linq;

public class GameMap : Node2D {
	public Game game;
	public Layout layout;

	private Camera camera;
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

	// public BehaviorSubject<Entity> selectedHex = new BehaviorSubject<Entity>(null);

	private bool is_placing = false;

	public MapBorders mapBorders;
	public Node2D spriteContainer;
	public MapLabels mapLabels;
	public SettlementLabels settlementLabels;
	public TileMap viewState;
	private GameView gameView;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		GD.PrintS("(GameMap) ready");
		camera = (Camera) GetNode<Camera>("Camera");
		terrain = (TileMap) GetNode<TileMap>("Terrain");
		features = (TileMap) GetNode<TileMap>("Features");
		grid = (TileMap) GetNode<TileMap>("Grid");
		selectionHex = (Sprite) GetNode<Sprite>("SelectionHex");
		hoverHex = (Sprite) GetNode<Sprite>("HoverHex");
		spriteContainer = (Node2D) GetNode<Node2D>("SpriteContainer");
		mapBorders = (MapBorders) GetNode<MapBorders>("MapBorders");
		mapLabels = (MapLabels) GetNode<MapLabels>("MapLabels");
		settlementLabels = (SettlementLabels) GetNode<SettlementLabels>("SettlementLabels");
		viewState = (TileMap) GetNode<TileMap>("ViewState");
	}

	public void RenderMap(Game game) {
		GD.PrintS("(GameMap) render map");
		this.game = game;
		
		var state = game.manager.state;
		layout = state.GetElement<Layout>();

		clickedTile.Subscribe((Entity tile) => {
			var selectedUnit = state.GetElement<SelectedUnit>().unit;
			if (selectedUnit is not null) {
				state.Send(new SelectedUnitUpdate { unit = null });
			}

			// if (selectedHex.Value == tile) {
			// 	selectedHex.OnNext(null);
			// } else {
			// 	selectedHex.OnNext(tile);
			// }
		});

		// selectedHex.Subscribe((Entity tile) => {
		// 	if (tile is null) {
		// 		selectionHex.Hide();
		// 	} else {
		// 		selectionHex.Show();
		// 		var coord = tile.Get<Location>().hex;
		// 		selectionHex.Position = layout.HexToPixel(coord).ToVector();

				
		// 	}
		// });

		hoveredTile.Subscribe((Entity tile) => {
			if (tile is null) {
				hoverHex.Hide();
			} else {
				hoverHex.Show();
				var coord = tile.Get<Location>().hex;
				hoverHex.Position = layout.HexToPixel(coord).ToVector();
			}
		});

		selectionHex.Hide();
		hoverHex.Hide();
		mapBorders.RenderMap(this);

		drawWorld();
		tileUpdates.Subscribe((Entity tile) => this.drawTile(tile));

		gameView.mapInputEnabled.Subscribe((bool isEnabled) => {
			hoveredTile.OnNext(null);
		});
	}

	public void centerCamera(Vector2 vec) {
		camera.Offset = vec;
	}

	public void centerCameraOnTile(Entity tile) {
		var hex = tile.Get<Location>().hex;
		centerCamera(layout.HexToPixel(hex).ToVector());
	}

	private void drawWorld() {
		foreach (Entity tile in game.manager.world.tiles) {
			drawTile(tile);
		}
	}

	private void drawTile(Entity tile) {
		var coord = tile.Get<Location>().hex;
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
		if (!gameView.mapInputEnabled.Value) {
			return;
		}
		base._Input(@event);

		if (@event is InputEventMouseButton) {
			var mouseEventButton = (InputEventMouseButton) @event;
			if (mouseEventButton.IsPressed() && mouseEventButton.ButtonIndex == (int) ButtonList.MaskRight) {
				var clickedHex = getCoordAtCursor();
				var selectedUnit = game.manager.state.GetElement<SelectedUnit>().unit;
				if (game.manager.world.IsValidTile(clickedHex) && selectedUnit is not null) {
					if (mouseEventButton.Shift) {
						GD.PrintS("Queued movement to", clickedHex);
						game.manager.state.Send(new ActionQueueAdd {
							owner = selectedUnit,
							action = new MovementAction(selectedUnit, clickedHex)
						});
					} else {
						GD.PrintS("Set movement to", clickedHex);
						game.manager.state.Send(new ActionQueueClear { owner = selectedUnit });
						game.manager.state.Send(new ActionQueueAdd {
							owner = selectedUnit,
							action = new MovementAction(selectedUnit, clickedHex)
						});
					}
				}
			}
		}

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

		if (!gameView.mapInputEnabled.Value) {
			return;
		}

		if (is_placing) {
			var coord = getCoordAtCursor();
			if (game.manager.world.IsValidTile(coord)) {
				var tile = game.manager.world.GetTile(coord);
				pressedTile.OnNext(tile);
			}
		}
	}
}
