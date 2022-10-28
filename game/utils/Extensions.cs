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