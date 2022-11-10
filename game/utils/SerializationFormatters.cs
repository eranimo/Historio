using Godot;
using MessagePack;
using MessagePack.Formatters;

public partial class ColorFormatter : IMessagePackFormatter<Color> {
	public Color Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
		return new Color((uint) reader.ReadInt32());
	}

	public void Serialize(ref MessagePackWriter writer, Color value, MessagePackSerializerOptions options) {
		writer.WriteInt32((int) value.ToRgba32());
	}
}