using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumExtension {
	public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
	   => self.Select((item, index) => (item, index));
}

public static class ColorExtension {
	public static Godot.Color ToGodot(this System.Drawing.Color color) {
		return new Godot.Color(color.GetHashCode());
	}

	public static Godot.Color ToGodot(this byte[] rgb) {
		return Godot.Color.Color8(rgb[0], rgb[1], rgb[2]);
	}
}

public static class LinqExtensions {
	public static T MinBy<T>(this IEnumerable<T> source, Func<T, IComparable> selector) {
		if (source == null) {
			throw new ArgumentNullException(nameof(source));
		}
		if (selector == null) {
			throw new ArgumentNullException(nameof(selector));
		}

		return source.Aggregate((min, cur) => {
			if (min == null) {
				return cur;
			}

			var minComparer = selector(min);

			if (minComparer == null) {
				return cur;
			}

			var curComparer = selector(cur);

			if (curComparer == null) {
				return min;
			}

			return minComparer.CompareTo(curComparer) > 0 ? cur : min;
		});
	}
}