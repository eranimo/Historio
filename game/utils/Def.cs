using System;
using System.Reflection;
using Godot;
using MessagePack;
using Newtonsoft.Json;

[MessagePackObject]
public partial class Def {
	[Key(0)]
	public string type { get; set; }

	[Key(1)]
	public string id { get; set; }
}

public partial class DefRef<T> where T : Def {
	public string def { get; set; }
	public string id { get; set; }

	public T Get() {
		var storeInfo = typeof(Defs).GetField(def, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
		if (storeInfo == null) {
			throw new Exception($"DefRef '{def}' not found");
		}
		var store = (DefStore<T>) storeInfo.GetValue(null);
		if (!store.Has(id)) {
			throw new Exception($"DefRef '{def}' of ID '{id}' not found");
		}
		return store.Get(id);
	}
}

public partial class DefStore<T> where T : Def {
	private Dictionary<string, T> items;
	private readonly string type;
	private readonly string filePath;

	public DefStore(string type, string filePath) {
		this.type = type;
		this.filePath = filePath;
		loadData();
	}

	private void loadData() {
		var path = $"res://game/defs/{filePath}.json";
		var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (file is null) {
			throw new System.Exception($"No Def found named {type}");
		}

		var json = file.GetAsText();

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
