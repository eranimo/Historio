using Godot;
using Newtonsoft.Json;

public class Def : Resource {
	public string type { get; set; }
	public string id { get; set; }
}

public class DefStore<T> where T : Def {
	private Dictionary<string, T> items;
	private readonly string type;
	private readonly string filePath;

	public DefStore(string type, string filePath) {
		this.type = type;
		this.filePath = filePath;
		loadData();
	}

	private void loadData() {
		var file = new File();
		var path = $"res://game/defs/{filePath}.json";
		if (!file.FileExists(path)) {
			throw new System.Exception($"No Def found named {type}");
		}
		file.Open(path, File.ModeFlags.Read);
		var json = file.GetAsText();
		file.Close();
		var data = JsonConvert.DeserializeObject<List<T>>(json);
		items = new Dictionary<string, T>();
		foreach (var item in data) {
			item.type = type;
			items[item.id] = item;
		}
		Godot.GD.PrintS($"Loaded def \"{type}\" with {data.Count} items");
	}

	public bool Has(string id) {
		return items.ContainsKey(id);
	}

	public T Get(string id) {
		return items[id];
	}
}
