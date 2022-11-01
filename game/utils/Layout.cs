// FROM https://www.redblobgames.com/grids/hexagons/codegen/output/lib.cs

using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class Layout {
	public Layout(Point size, Point origin) {
		this.size = size;
		this.origin = origin;
	}
	public readonly Point size;
	public readonly Point origin;
	static public Orientation flat = new Orientation(3.0 / 2.0, 0.0, Math.Sqrt(3.0) / 2.0, Math.Sqrt(3.0), 2.0 / 3.0, 0.0, -1.0 / 3.0, Math.Sqrt(3.0) / 3.0, 0.0);

	// returns hex origin
	public Point HexToPixel(CubeCoord h) {
		Orientation M = flat;
		double x = (M.f0 * h.q + M.f1 * h.r) * size.x;
		double y = (M.f2 * h.q + M.f3 * h.r) * size.y;
		return new Point(x + origin.x, y + origin.y) - origin;
	}

	public Point HexToPixel(Hex hex) {
		return HexToPixel(Hex.ToCube(hex));
	}

	public Hex PixelToHex(Point p) {
		Orientation M = flat;
		Point pt = new Point((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
		double q = M.b0 * pt.x + M.b1 * pt.y;
		double r = M.b2 * pt.x + M.b3 * pt.y;
		var frac = new FractionalCubeCoord(q, r, -q - r);
		return Hex.FromCube(frac.HexRound());
	}

	public Point HexCornerOffset(HexCorner corner) {
		Orientation M = flat;
		double angle = 2.0 * Math.PI * (M.start_angle - ((int) corner)) / 6.0;
		return new Point(size.x * Math.Cos(angle), size.y * Math.Sin(angle));
	}

	public Point HexEdgeMidpoint(HexDirection dir) {
		var p1 = HexCornerOffset(dir.LeftCorner());
		var p2 = HexCornerOffset(dir.RightCorner());
		return Point.FromVector(p1.ToVector().LinearInterpolate(p2.ToVector(), 0.5f));
	}

	public List<Point> PolygonCorners(CubeCoord h) {
		List<Point> corners = new List<Point> {};
		Point center = HexToPixel(h);
		for (int i = 0; i < 6; i++) {
			Point offset = HexCornerOffset((HexCorner) i);
			corners.Add(new Point(center.x + offset.x, center.y + offset.y));
		}
		return corners;
	}

	public List<Point> PolygonCorners(Hex hex) {
		return PolygonCorners(Hex.ToCube(hex));
	}

    public Point HexSize => new Point(
		2 * size.x,
		Math.Sqrt(3) * size.y
	);

    public Point GridDimensions(int cols, int rows) {
		CubeCoord lastHex = Hex.ToCube(new Hex(cols - 1, rows - 1));
		var lastHexPoint = HexToPixel(lastHex);
		double gridWidth = lastHexPoint.x + HexSize.x;
		double gridHeight = lastHexPoint.y + HexSize.y + 4f;
		return new Point(gridWidth, gridHeight);
	}

	public Point Centroid(IEnumerable<Hex> hexes) {
		double x = 0;
		double y = 0;
		foreach (var hex in hexes) {
			var point = HexToPixel(hex);
			x += point.x;
			y += point.y;
		}
		return new Point(x / hexes.Count(), y / hexes.Count());
	}
}