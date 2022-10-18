using Godot;
using MessagePack;
using MessagePack.Formatters;

public class ColorFormatter : IMessagePackFormatter<Color> {
	public Color Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
		return new Color(reader.ReadInt32());
	}

	public void Serialize(ref MessagePackWriter writer, Color value, MessagePackSerializerOptions options) {
		writer.WriteInt32(value.ToRgba32());
	}
}