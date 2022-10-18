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

	private GameCamera camera;
	private TileMap terrain;
	private TileMap features;
	private TileMap grid;
	private Rivers rivers;
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
		camera = (GameCamera) GetNode<GameCamera>("GameCamera");
		terrain = (TileMap) GetNode<TileMap>("Terrain");
		features = (TileMap) GetNode<TileMap>("Features");
		grid = (TileMap) GetNode<TileMap>("Grid");
		rivers = (Rivers) GetNode<Rivers>("Rivers");
		selectionHex = (Sprite) GetNode<Sprite>("SelectionHex");
		hoverHex = (Sprite) GetNode<Sprite>("HoverHex");
		spriteContainer = (Node2D) GetNode<Node2D>("SpriteContainer");
		mapBorders = (MapBorders) GetNode<MapBorders>("MapBorders");
		mapLabels = (MapLabels) GetNode<MapLabels>("MapLabels");
		settlementLabels = (SettlementLabels) GetNode<SettlementLabels>("SettlementLabels");
		viewState = (TileMap) GetNode<TileMap>("ViewState");

		gameView.GameController.gameMapInputSubject.Subscribe((GameMapInput mapInput) => {
			if (mapInput.type == GameMapInputType.LeftClick) {
				if (game.manager.world.IsValidTile(mapInput.hex)) {
					var tile = game.manager.world.GetTile(mapInput.hex);
					clickedTile.OnNext(tile);
				}
			} else if (mapInput.type == GameMapInputType.Hovered) {
				if (game.manager.world.IsValidTile(mapInput.hex)) {
					var tile = game.manager.world.GetTile(mapInput.hex);
					hoveredTile.OnNext(tile);
				}
			} else if (mapInput.type == GameMapInputType.RightClick) {
				var selectedUnit = gameView.GameController.currentUnit;
				if (selectedUnit is not null) {
					if (mapInput.isShiftModifier) {
						GD.PrintS("Queued movement to", mapInput.hex);
						game.manager.state.Send(new ActionQueueAdd {
							owner = selectedUnit,
							action = new MovementAction(selectedUnit, mapInput.hex)
						});
					} else {
						GD.PrintS("Set movement to", mapInput.hex);
						game.manager.state.Send(new ActionQueueClear { owner = selectedUnit });
						game.manager.state.Send(new ActionQueueAdd {
							owner = selectedUnit,
							action = new MovementAction(selectedUnit, mapInput.hex)
						});
					}
				}
			}
		});
	}

	public void RenderMap(Game game) {
		GD.PrintS("(GameMap) render map");
		this.game = game;
		
		var state = game.manager.state;
		layout = state.GetElement<Layout>();

		clickedTile.Subscribe((Entity tile) => {
			gameView.GameController.GamePanel.PanelSet(new GamePanelState {
				type = GamePanelType.Tile,
				entity = tile
			});
		});

		hoveredTile.Subscribe((Entity tile) => {
			if (tile is null) {
				hoverHex.Hide();
			} else {
				hoverHex.Show();
				var coord = gameView.game.manager.Get<Location>(tile).hex;
				hoverHex.Position = layout.HexToPixel(coord).ToVector();
			}
		});

		selectionHex.Hide();
		hoverHex.Hide();
		mapBorders.RenderMap(this);

		foreach (Entity tile in game.manager.world.tiles) {
			drawTile(tile);
		}
		rivers.RenderRivers();

		tileUpdates.Subscribe((Entity tile) => this.drawTile(tile));
	}

	public void centerCamera(Vector2 vec) {
		camera.SetCameraCenter(vec);
	}

	public void centerCameraOnTile(Entity tile) {
		var hex = gameView.game.manager.Get<Location>(tile).hex;
		centerCamera(layout.HexToPixel(hex).ToVector());
	}

	public void SetSelectedTile(Entity tile) {
		if (tile is null) {
			selectionHex.Hide();
		} else {
			selectionHex.Show();
			var hex = gameView.game.manager.Get<Location>(tile).hex;
			selectionHex.Position = layout.HexToPixel(hex).ToVector();
		}
	}

	private void drawTile(Entity tile) {
		var coord = gameView.game.manager.Get<Location>(tile).hex;
		var data = gameView.game.manager.Get<TileData>(tile);
		grid.SetCell(coord.col, coord.row, 1);
		terrain.SetCell(coord.col, coord.row, data.GetTerrainTilesetIndex().Value);

		if (data.GetFeatureTilesetIndex().HasValue) {
			features.SetCell(coord.col, coord.row, data.GetFeatureTilesetIndex().Value);
		}
	}
}
