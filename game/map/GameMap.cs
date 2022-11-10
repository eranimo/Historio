using Godot;
using RelEcs;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Linq;


public partial class GameMap : Node2D {
	public Game game;
	public Layout layout;

	private GameCamera camera;
	private TileMap terrain;
	private TileMap features;
	private TileMap grid;
	private Rivers rivers;
	private Sprite2D selectionHex;
	private Sprite2D hoverHex;
	public MapBorders mapBorders;
	private MapOverlay mapOverlay;
	public Node2D spriteContainer;
	public MapLabels mapLabels;
	public SettlementLabels settlementLabels;
	public TileMap viewState;
	private GameView gameView;

	// subject events
	private Subject<Entity> tileUpdates = new Subject<Entity>();
	private Subject<Entity> pressedTile = new Subject<Entity>();
	private Subject<Entity> clickedTile = new Subject<Entity>();
	private Subject<Entity> hoveredTile = new Subject<Entity>();

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		GD.PrintS("(GameMap) ready");
		camera = GetNode<GameCamera>("%GameCamera");
		terrain = GetNode<TileMap>("%Terrain");
		features = GetNode<TileMap>("%Features");
		grid = GetNode<TileMap>("%Grid");
		rivers = GetNode<Rivers>("%Rivers");
		selectionHex = GetNode<Sprite2D>("%SelectionHex");
		hoverHex = GetNode<Sprite2D>("%HoverHex");
		spriteContainer = GetNode<Node2D>("%SpriteContainer");
		mapBorders = GetNode<MapBorders>("%MapBorders");
		mapOverlay = GetNode<MapOverlay>("%MapOverlay");
		mapLabels = GetNode<MapLabels>("%MapLabels");
		settlementLabels = GetNode<SettlementLabels>("%SettlementLabels");
		viewState = GetNode<TileMap>("%ViewState");

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
					var action = new MovementAction { target = mapInput.hex };
					if (mapInput.isShiftModifier) {
						GD.PrintS("Queued movement to", mapInput.hex);
					} else {
						GD.PrintS("Set movement to", mapInput.hex);
						game.manager.state.Send(new ActionQueueClear { owner = selectedUnit });
					}

					game.manager.state.Send(new ActionQueueAdd {
						owner = selectedUnit,
						action = action
					});
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
		mapOverlay.SetupMap(this);

		MapModes.CurrentMapMode.Subscribe((MapMode mapMode) => {
			mapOverlay.UpdateMap();
		});


		foreach (Entity tile in game.manager.world.tiles) {
			drawTile(tile);
		}
		rivers.RenderRivers();

		tileUpdates.Subscribe((Entity tile) => this.drawTile(tile));

		gameView.zoom.Subscribe((float zoom) => {
			calculateMapMode();
		});

		MapModes.CurrentMapMode.Subscribe((MapMode mapMode) => {
			calculateMapMode();
		});
		calculateMapMode();
	}

	private void calculateMapMode() {
		var zoom = gameView.zoom.Value;
		var isClose = zoom < 1f;
		settlementLabels.Visible = isClose;
		mapLabels.Visible = isClose;
		mapBorders.Visible = isClose;
		// terrain.Visible = isClose;
		features.Visible = isClose;
		spriteContainer.Visible = isClose;
		grid.Visible = isClose;
		rivers.Visible = MapModes.CurrentMapMode.Value.ShowRivers && zoom < 2f;
		// mapOverlay.SelfModulate = Color.ColorN("white", zoom > 1.0 ? 1f : 0.5f);
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
		grid.SetCell(0, new Vector2i(coord.col, coord.row), 1);
		terrain.SetCell(0, new Vector2i(coord.col, coord.row), data.GetTerrainTilesetIndex().Value);

		if (data.GetFeatureTilesetIndex().HasValue) {
			features.SetCell(0, new Vector2i(coord.col, coord.row), data.GetFeatureTilesetIndex().Value);
		}
	}
}
