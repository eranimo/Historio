// FROM https://www.redblobgames.com/grids/hexagons/codegen/output/lib.cs

using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

public enum Direction {
	SouthEast = 0,
	NorthEast = 1,
	North = 2,
	NorthWest = 3,
	SouthWest = 4,
	South = 5,
}

public struct Point {
	public Point(double x, double y) {
		this.x = x;
		this.y = y;
	}
	public readonly double x;
	public readonly double y;

	public override int GetHashCode() {
		return (x, y).GetHashCode();
	}

	public static Point operator -(Point a, Point b) => new Point(a.x - b.x, a.y - b.y);
	public static Point operator +(Point a, Point b) => new Point(a.x + b.x, a.y + b.y);
	public static Point operator /(Point a, Point b) => new Point(a.x / b.x, a.y / b.y);
	public static Point operator *(Point a, Point b) => new Point(a.x * b.x, a.y * b.y);

	public Vector2 ToVector() {
		return new Vector2((float) x, (float) y);
	}

	public override string ToString() {
		return base.ToString() + string.Format("({0}, {1})", this.x, this.y);
	}
}

public struct CubeCoord {
	public CubeCoord(int q, int r, int s) {
		this.q = q;
		this.r = r;
		this.s = s;
		if (q + r + s != 0) throw new ArgumentException("q + r + s must be 0");
	}
	public readonly int q;
	public readonly int r;
	public readonly int s;

	public override int GetHashCode() {
		return (q, r, s).GetHashCode();
	}

	public CubeCoord Add(CubeCoord b) {
		return new CubeCoord(q + b.q, r + b.r, s + b.s);
	}

	public CubeCoord Subtract(CubeCoord b) {
		return new CubeCoord(q - b.q, r - b.r, s - b.s);
	}

	public CubeCoord Scale(int k) {
		return new CubeCoord(q * k, r * k, s * k);
	}

	public CubeCoord RotateLeft() {
		return new CubeCoord(-s, -q, -r);
	}

	public CubeCoord RotateRight() {
		return new CubeCoord(-r, -s, -q);
	}

	static public List<CubeCoord> directions = new List<CubeCoord> {
		new CubeCoord(1, 0, -1),
		new CubeCoord(1, -1, 0),
		new CubeCoord(0, -1, 1),
		new CubeCoord(-1, 0, 1),
		new CubeCoord(-1, 1, 0),
		new CubeCoord(0, 1, -1)
	};

	static public CubeCoord Direction(Direction direction) {
		return CubeCoord.directions[(int) direction];
	}

	public CubeCoord Neighbor(Direction direction) {
		return Add(CubeCoord.Direction(direction));
	}

	static public List<CubeCoord> diagonals = new List<CubeCoord> {
		new CubeCoord(2, -1, -1),
		new CubeCoord(1, -2, 1),
		new CubeCoord(-1, -1, 2),
		new CubeCoord(-2, 1, 1),
		new CubeCoord(-1, 2, -1),
		new CubeCoord(1, 1, -2)
	};

	public CubeCoord DiagonalNeighbor(Direction direction) {
		return Add(CubeCoord.diagonals[(int) direction]);
	}

	public int Length() {
		return (int)((Math.Abs(q) + Math.Abs(r) + Math.Abs(s)) / 2);
	}

	public int Distance(CubeCoord b) {
		return Subtract(b).Length();
	}

	public override string ToString() {
		return base.ToString() + string.Format("({0}, {1}, {2})", this.q, this.r, this.s);
	}

	public List<CubeCoord> Ring(int radius = 1) {
		List<CubeCoord> results = new List<CubeCoord>();
		CubeCoord hex = Add(CubeCoord.Direction((Direction) 4).Scale(radius));
		for (int i = 0; i < 6; i++) {
			for (int j = 0; j < radius; j++) {
				results.Add(hex);
				hex = hex.Neighbor((Direction) i);
			}
		}
		return results;
	}

	public List<CubeCoord> Spiral(int radius = 1) {
		List<CubeCoord> results = new List<CubeCoord>();
		results.Add(this);
		for (int k = 1; k <= radius; k++) {
			results.AddRange(Ring(k));
		}
		return results;
	}
}

public struct FractionalCubeCoord {
	public FractionalCubeCoord(double q, double r, double s) {
		this.q = q;
		this.r = r;
		this.s = s;
		if (Math.Round(q + r + s) != 0) throw new ArgumentException("q + r + s must be 0");
	}
	public readonly double q;
	public readonly double r;
	public readonly double s;

	public CubeCoord HexRound() {
		int qi = (int)(Math.Round(q));
		int ri = (int)(Math.Round(r));
		int si = (int)(Math.Round(s));
		double q_diff = Math.Abs(qi - q);
		double r_diff = Math.Abs(ri - r);
		double s_diff = Math.Abs(si - s);
		if (q_diff > r_diff && q_diff > s_diff) {
			qi = -ri - si;
		} else
		if (r_diff > s_diff) {
			ri = -qi - si;
		} else {
			si = -qi - ri;
		}
		return new CubeCoord(qi, ri, si);
	}

	public FractionalCubeCoord HexLerp(FractionalCubeCoord b, double t) {
		return new FractionalCubeCoord(q * (1.0 - t) + b.q * t, r * (1.0 - t) + b.r * t, s * (1.0 - t) + b.s * t);
	}

	static public List < CubeCoord > HexLinedraw(CubeCoord a, CubeCoord b) {
		int N = a.Distance(b);
		FractionalCubeCoord a_nudge = new FractionalCubeCoord(a.q + 1e-06, a.r + 1e-06, a.s - 2e-06);
		FractionalCubeCoord b_nudge = new FractionalCubeCoord(b.q + 1e-06, b.r + 1e-06, b.s - 2e-06);
		List < CubeCoord > results = new List < CubeCoord > {};
		double step = 1.0 / Math.Max(N, 1);
		for (int i = 0; i <= N; i++) {
			results.Add(a_nudge.HexLerp(b_nudge, step * i).HexRound());
		}
		return results;
	}

}

public class Hex {
	public Hex() {}
	public Hex(int col, int row) {
		this.col = col;
		this.row = row;
	}
	public readonly int col;
	public readonly int row;
	static public int EVEN = 1;
	static public int ODD = -1;

	public override int GetHashCode() {
		return (col, row).GetHashCode();
	}

	public override bool Equals(object obj)  { 
        var hex = obj as Hex;
		return hex.col == col && hex.row == row; 
    }

	public Hex Neighbor(Direction direction) {
		return Hex.FromCube(ToCube(this).Neighbor(direction));
	}

	public Hex[] Neighbors() {
		return new Hex[] {
			Neighbor(Direction.SouthEast),
			Neighbor(Direction.NorthEast),
			Neighbor(Direction.North),
			Neighbor(Direction.NorthWest),
			Neighbor(Direction.SouthWest),
			Neighbor(Direction.South),
		};
	}

	static public Hex FromCube(CubeCoord h) {
		int offset = Hex.ODD;
		int col = h.q;
		int row = h.r + (int)((h.q + offset * (h.q & 1)) / 2);
		return new Hex(col, row);
	}

	static public CubeCoord ToCube(Hex h) {
		int offset = Hex.ODD;
		int q = h.col;
		int r = h.row - (int)((h.col + offset * (h.col & 1)) / 2);
		int s = -q - r;
		return new CubeCoord(q, r, s);
	}

	public override string ToString() {
		return base.ToString() + string.Format("({0}, {1})", this.row, this.col);
	}

	public Vector2 ToVector() {
		return new Vector2((float) col, (float) row);
	}
}

public struct DoubledCoord {
	public DoubledCoord(int col, int row) {
		this.col = col;
		this.row = row;
	}
	public readonly int col;
	public readonly int row;

	public override int GetHashCode() {
		return (col, row).GetHashCode();
	}

	static public DoubledCoord QdoubledFromCube(CubeCoord h) {
		int col = h.q;
		int row = 2 * h.r + h.q;
		return new DoubledCoord(col, row);
	}

	public CubeCoord QdoubledToCube() {
		int q = col;
		int r = (int)((row - col) / 2);
		int s = -q - r;
		return new CubeCoord(q, r, s);
	}

	static public DoubledCoord RdoubledFromCube(CubeCoord h) {
		int col = 2 * h.q + h.r;
		int row = h.r;
		return new DoubledCoord(col, row);
	}

	public CubeCoord RdoubledToCube() {
		int q = (int)((col - row) / 2);
		int r = row;
		int s = -q - r;
		return new CubeCoord(q, r, s);
	}
}

public struct Orientation {
	public Orientation(double f0, double f1, double f2, double f3, double b0, double b1, double b2, double b3, double start_angle) {
		this.f0 = f0;
		this.f1 = f1;
		this.f2 = f2;
		this.f3 = f3;
		this.b0 = b0;
		this.b1 = b1;
		this.b2 = b2;
		this.b3 = b3;
		this.start_angle = start_angle;
	}
	public readonly double f0;
	public readonly double f1;
	public readonly double f2;
	public readonly double f3;
	public readonly double b0;
	public readonly double b1;
	public readonly double b2;
	public readonly double b3;
	public readonly double start_angle;
}

public struct Layout {
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

	public Point HexCornerOffset(int corner) {
		Orientation M = flat;
		double angle = 2.0 * Math.PI * (M.start_angle - corner) / 6.0;
		return new Point(size.x * Math.Cos(angle), size.y * Math.Sin(angle));
	}

	public List<Point> PolygonCorners(CubeCoord h) {
		List<Point> corners = new List<Point> {};
		Point center = HexToPixel(h);
		for (int i = 0; i < 6; i++) {
			Point offset = HexCornerOffset(i);
			corners.Add(new Point(center.x + offset.x, center.y + offset.y));
		}
		return corners;
	}

    public Point HexSize => new Point(
		2 * size.x,
		Math.Sqrt(3) * size.y
	);

    public Point GridDimensions(int cols, int rows) {
		CubeCoord lastHex = Hex.ToCube(new Hex(cols - 1, rows - 1));
		var lastHexPoint = HexToPixel(lastHex);
		double gridWidth = lastHexPoint.x + (HexSize.x / 2);
		double gridHeight = gridHeight = lastHexPoint.y + (HexSize.y / 2);
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