using Godot;
using System;

public class LoadSaveListItem : PanelContainer {
	private Label countryName;
	private Label lastSaveDate;

	public delegate void Select();
	public delegate void LoadLatest();
	public delegate void Delete();

	public event Select OnSelect;	
	public event LoadLatest OnLoadLatest;	
	public event Delete OnDelete;

	public string CountryName {
		get { return countryName.Text; }
		set { countryName.Text = value; }
	}

	public string LastSaveDate {
		get { return lastSaveDate.Text; }
		set { lastSaveDate.Text = value; }
	}

	public override void _Ready() {
		countryName = (Label) GetNode("%CountryName");
		lastSaveDate =  (Label) GetNode("%LastSaveDate");

		GetNode("%SelectButton").Connect("pressed", this, nameof(handleSelect));
		GetNode("%LoadLatestButton").Connect("pressed", this, nameof(handleLoadLatest));
		GetNode("%DeleteButton").Connect("pressed", this, nameof(handleDelete));
	}

	private void handleSelect() {
		OnSelect.Invoke();
	}

	private void handleLoadLatest() {
		OnLoadLatest.Invoke();
	}

	private void handleDelete() {
		OnDelete.Invoke();
	}
}
