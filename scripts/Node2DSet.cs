using Godot;
using System;
using System.Collections.Generic;

public class Node2DSet<T> : Node2D where T : Node {
	private HashSet<T> items = new HashSet<T>();

	public void Add(T label) {
		if (!items.Contains(label)) {
			AddChild(label);
		}
		items.Add(label);
	}

	public void Remove(T label) {
		T value;
		if (items.TryGetValue(label, out value)) {
			RemoveChild(value);
		}
	}
}
