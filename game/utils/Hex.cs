// FROM https://www.redblobgames.com/grids/hexagons/codegen/output/lib.cs

using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

public enum HexDirection {
	SouthEast = 0,
	NorthEast = 1,
	North = 2,
	NorthWest = 3,
	SouthWest = 4,
	South = 5,
}

public enum HexCorner {
	SouthWest = 0,
	West = 1,
	NorthWest = 2,
	NorthEast = 3,
	East = 4,
	SouthEast = 5,
}

public class HexSide {
	private readonly Hex hex;
	private readonly HexDirection direction;

	private Dictionary<HexDirection, HexCorner[]> directionToCorners = new Dictionary<HexDirection, HexCorner[]> {
		{ HexDirection.SouthWest, new HexCorner[] { HexCorner.West, HexCorner.SouthWest } },
		{ HexDirection.NorthWest, new HexCorner[] { HexCorner.NorthWest, HexCorner.West } },
		{ HexDirection.North, new HexCorner[] { HexCorner.NorthEast, HexCorner.NorthWest } },
		{ HexDirection.NorthEast, new HexCorner[] { HexCorner.East, HexCorner.NorthEast } },
		{ HexDirection.SouthEast, new HexCorner[] { HexCorner.SouthEast, HexCorner.East } },
		{ HexDirection.South, new HexCorner[] { HexCorner.SouthWest, HexCorner.SouthEast } },
	};

	public HexSide(Hex hex, HexDirection direction) {
		this.hex = hex;
		this.direction = direction;
	}

	public HexCorner[] Corners {
		get {
			return directionToCorners[direction];
		}
	}

	public HexCorner LeftCorner {
		get {
			return directionToCorners[direction][0];
		}
	}

	public HexCorner RightCorner {
		get {
			return directionToCorners[direction][1];
		}
	}
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
	public static Point operator /(Point a, float b) => new Point(a.x / b, a.y / b);
	public static Point operator *(Point a, Point b) => new Point(a.x * b.x, a.y * b.y);
	public static Point operator *(Point a, float b) => new Point(a.x * b, a.y * b);

	public Vector2 ToVector() {
		return new Vector2((float) x, (float) y);
	}

	public static Point FromVector(Vector2 vec) {
		return new Point(vec.x, vec.y);
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

	static public CubeCoord Direction(HexDirection direction) {
		return CubeCoord.directions[(int) direction];
	}

	public CubeCoord Neighbor(HexDirection direction) {
		return Add(CubeCoord.Direction(direction));
	}

	public CubeCoord Neighbor(HexDirection direction, int distance) {
		return Add(CubeCoord.Direction(direction).Scale(distance));
	}

	static public List<CubeCoord> diagonals = new List<CubeCoord> {
		new CubeCoord(2, -1, -1),
		new CubeCoord(1, -2, 1),
		new CubeCoord(-1, -1, 2),
		new CubeCoord(-2, 1, 1),
		new CubeCoord(-1, 2, -1),
		new CubeCoord(1, 1, -2)
	};

	public CubeCoord DiagonalNeighbor(HexDirection direction) {
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
		CubeCoord hex = Add(CubeCoord.Direction((HexDirection) 4).Scale(radius));
		for (int i = 0; i < 6; i++) {
			for (int j = 0; j < radius; j++) {
				results.Add(hex);
				hex = hex.Neighbor((HexDirection) i);
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

[Serializable]
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

	public static bool operator == (Hex h1, Hex h2) {
		if (((object) h1) == null || ((object) h2) == null) {
			return System.Object.Equals(h1, h2);
		}
		return h1.Equals(h2);
	}

	public static bool operator != (Hex h1, Hex h2) {
		if (((object) h1) == null || ((object) h2) == null) {
			return !System.Object.Equals(h1, h2);
		}
		return !h1.Equals(h2);
	}

	public HexSide Side(HexDirection direction) {
		return new HexSide(this, direction);
	}

	public Hex Neighbor(HexDirection direction) {
		return Hex.FromCube(ToCube(this).Neighbor(direction));
	}

	public Hex Neighbor(HexDirection direction, int distance) {
		return Hex.FromCube(ToCube(this).Neighbor(direction, distance));
	}

	public Hex[] Neighbors() {
		return new Hex[] {
			Neighbor(HexDirection.SouthEast),
			Neighbor(HexDirection.NorthEast),
			Neighbor(HexDirection.North),
			Neighbor(HexDirection.NorthWest),
			Neighbor(HexDirection.SouthWest),
			Neighbor(HexDirection.South),
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

	public List<Hex> Ring(int radius = 1) {
		return Hex.ToCube(this).Ring(radius).Select(cube => Hex.FromCube(cube)).ToList();
	}

	public List<Hex> Bubble(int radius = 1) {
		var hexes = new List<Hex>();
		for (int i = radius; i > 0; i--) {
			hexes.AddRange(Ring(i));
		}
		return hexes;
	}

	public List<Hex> Spiral(int radius = 1) {
		return Hex.ToCube(this).Spiral(radius).Select(cube => Hex.FromCube(cube)).ToList();
	}

	public int Distance(Hex hex) {
		return Hex.ToCube(this).Distance(Hex.ToCube(hex));
	}

	public Vector2 ToVector() {
		return new Vector2((float) col, (float) row);
	}

	public static Hex FromVector(Vector2 vector) {
		return new Hex((int) vector.x, (int) vector.y);
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
