using Godot;
using System;
using System.Reactive.Subjects;

public class MapContext : Node {
	public BehaviorSubject<float> zoom = new BehaviorSubject<float>(1);
	public IObservable<float> OnZoom { get => zoom; }
}