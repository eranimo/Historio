using Godot;
using System;

public class GameMap : Node2D {
	private GameWorld world;
	private TileMap terrain;
	private TileMap features;
	private TileMap grid;
	private Layout layout;

	private Color LINE_COLOR = new Color(1, 1, 1);

	public void RenderMap(GameWorld world) {
		this.world = world;
		terrain = (TileMap) GetNode<TileMap>("Terrain");
		features = (TileMap) GetNode<TileMap>("Features");
		grid = (TileMap) GetNode<TileMap>("Grid");
		layout = new Layout(Layout.flat, new Point(16.666, 16.165), new Point(16 + .5, 18 + .5));
		
		foreach(Tile tile in world.tiles) {
			grid.SetCell(tile.coord.col, tile.coord.row, 1);
			terrain.SetCell(tile.coord.col, tile.coord.row, 1);
		}
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (@event.IsActionPressed("view_select")) {
			var cursorPos = GetLocalMousePosition();
			var clickedCoord = layout.PixelToHex(new Point(cursorPos.x, cursorPos.y)).HexRound();

			GD.PrintS("Clicked pos:", cursorPos);
			var clickedCoordOffset = OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, clickedCoord);
			GD.PrintS("Clicked coord:", clickedCoordOffset);


			terrain.SetCell(clickedCoordOffset.col, clickedCoordOffset.row, 2);
		}
	}
}
