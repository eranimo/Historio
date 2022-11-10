using Godot;
using System;

public partial class LoadSaveEntryListItem : PanelContainer {
	private Label saveEntryName;
	private Label saveDate;

	public delegate void Load();
	public delegate void Delete();

	public event Load OnLoad;
	public event Delete OnDelete;

	public string SaveEntryName {
		get { return saveEntryName.Text; }
		set { saveEntryName.Text = value; }
	}

	public string SaveDate {
		get { return saveDate.Text; }
		set { saveDate.Text = value; }
	}

	public override void _Ready() {
		saveEntryName = (Label) GetNode("%SaveEntryName");
		saveDate = (Label) GetNode("%SaveDate");

		GetNode("%LoadEntryButton").Connect("pressed",new Callable(this,nameof(handleLoad)));
		GetNode("%DeleteEntryButton").Connect("pressed",new Callable(this,nameof(handleDelete)));
	}

	private void handleLoad() {
		OnLoad.Invoke();
	}

	private void handleDelete() {
		OnDelete.Invoke();
	}
}
