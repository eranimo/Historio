using Godot;
using System;
using System.Reactive.Subjects;

public delegate void SaveEntryDelete();
public delegate void SaveEntryOverwrite();

public class SaveEntryListItem : PanelContainer {
	private Label saveEntryName;
	private Label saveDate;
	private Button deleteButton;
	private Button overwriteButton;

	public event SaveEntryDelete SaveEntryDelete;
	public event SaveEntryOverwrite SaveEntryOverwrite;

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

		deleteButton = (Button) GetNode("%DeleteButton");
		overwriteButton = (Button) GetNode("%OverwriteButton");

		deleteButton.Connect("pressed", this, nameof(handleDelete));
		overwriteButton.Connect("pressed", this, nameof(handleOverwrite));
	}

	private void handleDelete() {
		SaveEntryDelete.Invoke();
	}

	private void handleOverwrite() {
		SaveEntryOverwrite.Invoke();
	}
}
