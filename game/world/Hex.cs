using Godot;
using System;
using LibNoise;
using System.Collections.Generic;

namespace Hex {
	public enum Direction {
		SE = 0,
		NE = 1,
		N = 2,
		NW = 3,
		SW = 4,
		S = 5,
	}

	/// <summary>Offset coordinates in odd-q style</summary>
	public struct OffsetCoord {
		public int Col;
		public int Row;

		public OffsetCoord(int Col, int Row) {
			this.Col = Col;
			this.Row = Row;
		}
		
		public AxialCoord ToAxial() {
			var q = this.Col;
			var r = this.Row - (this.Col - (this.Col & 1)) / 2;
			return new AxialCoord(q, r);
		}

		public CubeCoord ToCube() {
			return this.ToAxial().ToCube();
		}

		public override string ToString() {
			return base.ToString() + string.Format("({0}, {1})", this.Col, this.Row);
		}

		public Vector2 AsVector() {
			return new Vector2(this.Col, this.Row);
		}

		public override bool Equals(object obj) {
			if ((obj == null) || ! this.GetType().Equals(obj.GetType())) {
				return false;
			}
			OffsetCoord other = (OffsetCoord) obj;
			return this.Row == other.Row && this.Col == other.Col;
		}

		public override int GetHashCode() {
			return (Col, Row).GetHashCode();
		}
	}

	public struct AxialCoord {
		public double q;
		public double r;

		public AxialCoord(double q, double r) {
			this.q = q;
			this.r = r;
		}

		public CubeCoord ToCube() {
			var q = this.q;
			var r = this.r;
			var s = -q - r;
			return new CubeCoord(q, r, s);
		}

		public OffsetCoord ToOffset() {
			var col = this.q;
			var row = this.r + (this.q - ((int) this.q & 1)) / 2;
			return new OffsetCoord((int) col, (int) row);
		}

		public AxialCoord Round() {
			return this.ToCube().Round().ToAxial();
		}

		public override string ToString() {
			return base.ToString() + string.Format("({0}, {1})", this.q, this.r);
		}

		public Vector2 AsVector() {
			return new Vector2((float) this.q, (float) this.r);
		}

		public override bool Equals(object obj) {
			if ((obj == null) || ! this.GetType().Equals(obj.GetType())) {
				return false;
			}
			AxialCoord other = (AxialCoord) obj;
			return this.q == other.q && this.r == other.r;
		}

		public override int GetHashCode() {
			return (q, r).GetHashCode();
		}
	}

	public struct CubeCoord {
		public double q;
		public double r;
		public double s;

		public CubeCoord(double q, double r, double s) {
			this.q = q;
			this.r = r;
			this.s = s;
		}

		public AxialCoord ToAxial() {
			var q = this.q;
			var r = this.r;
			return new AxialCoord(q, r);
		}

		public OffsetCoord ToOffset() {
			return this.ToAxial().ToOffset();
		}

		public CubeCoord Round() {
			var q = (int) Math.Round(this.q);
			var r = (int) Math.Round(this.r);
			var s = (int) Math.Round(this.s);

			var q_diff = Math.Abs(q - this.q);
			var r_diff = Math.Abs(r - this.r);
			var s_diff = Math.Abs(s - this.s);

			if (q_diff > r_diff && q_diff > s_diff) {
				q = -r - s;
			} else if(r_diff > s_diff) {
				r = -q - s;
			} else {
				s = -q - r;
			}

			return new CubeCoord(q, r, s);
		}

		public override string ToString() {
			return base.ToString() + string.Format("({0}, {1}, {2})", this.q, this.r, this.s);
		}

		public override bool Equals(object obj) {
			if ((obj == null) || ! this.GetType().Equals(obj.GetType())) {
				return false;
			}
			CubeCoord other = (CubeCoord) obj;
			return this.q == other.q && this.r == other.r && this.s == other.s;
		}

		public override int GetHashCode() {
			return (q, r, s).GetHashCode();
		}
	}

	public static class HexConstants {
		public const int HEX_SIZE = 24;

		public static OffsetCoord[,] oddq_directions = new OffsetCoord[2, 6] {
			{
				new OffsetCoord(1,  0), new OffsetCoord(1, -1), new OffsetCoord(0, -1),
				new OffsetCoord(-1, -1), new OffsetCoord(-1,  0), new OffsetCoord(0, 1)
			},
			{
				new OffsetCoord(1, 1), new OffsetCoord(1, 0), new OffsetCoord( 0, -1),
				new OffsetCoord(-1, 0), new OffsetCoord(-1, 1), new OffsetCoord(0, 1),
			}
		};
	}

	public class HexUtils {
		public static float HexWidth {
			get {
				return 2 * HexConstants.HEX_SIZE;
			}
		}

		public static float HexHeight {
			get {
				return (float) Math.Sqrt(3) * HexConstants.HEX_SIZE;
			}
		}

		public static Vector2 GetGridDimensions(int cols, int rows) {
			var lastHexPoint = Hex.HexUtils.HexToPixel(new OffsetCoord(cols - 1, rows - 1));
			var gridWidth = lastHexPoint.x + (HexUtils.HexWidth);
			float gridHeight;
			if ((rows & 2) == 0) {
				gridHeight = lastHexPoint.y + (HexUtils.HexHeight) + (HexUtils.HexHeight / 2);
			} else {
				gridHeight = lastHexPoint.y + (HexUtils.HexHeight);
			}
			return new Vector2(gridWidth, gridHeight);
		}

		public static OffsetCoord oddq_offset_neighbor(OffsetCoord hex, Direction direction) {
			var parity = hex.Col & 1;
			var dir = HexConstants.oddq_directions[parity, (int) direction];
			return new OffsetCoord(hex.Col + dir.Col, hex.Row + dir.Row);
		}

		///<summary>Converts between pixels to offset coordinates</summary>
		public static OffsetCoord PixelToHexOffset(Vector2 point) {
			return HexUtils.PixelToHexAxial(point).ToOffset();
		}

		public static AxialCoord PixelToHexAxial(Vector2 point) {
			var q = (2.0 / 3 * point.x) / HexConstants.HEX_SIZE;
			var r = (-1.0 / 3 * point.x + Math.Sqrt(3) / 3 * point.y) / HexConstants.HEX_SIZE;
			return new AxialCoord(q, r).Round();
		}

		public static Vector2 HexToPixel(OffsetCoord hex) {
			var x = HexConstants.HEX_SIZE * 3/2 * hex.Col;
			var y = HexConstants.HEX_SIZE * Math.Sqrt(3) * (hex.Row + 0.5 * (hex.Col&1));
			return new Vector2((int) x, (int) y);
		}

		public static Vector2 HexToPixel(AxialCoord hex) {
			var x = HexConstants.HEX_SIZE * (3.0/2 * hex.q);
			var y = HexConstants.HEX_SIZE * (Math.Sqrt(3) / 2 * hex.q + Math.Sqrt(3) * hex.r);
			return new Vector2((int) x, (int) y);
		}

		public static Vector2 HexToPixelCenter(OffsetCoord hex) {
			return HexToPixel(hex) + new Vector2(HexWidth / 2, HexHeight / 2);
		}

		public static Vector2 HexToPixelCenter(AxialCoord hex) {
			return HexToPixel(hex) + new Vector2(HexWidth / 2, HexHeight / 2);
		}
	}
}
