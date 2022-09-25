using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MultiSet<K, V> {
	private Dictionary<K, HashSet<V>> dict = new Dictionary<K, HashSet<V>>();

	public void Add(K key, V value) {
		if (dict.ContainsKey(key)) {
			dict[key].Add(value);
		} else {
			dict[key] = new HashSet<V> { value };
		}
	}

	public IEnumerable<K> Keys {
		get {
			return this.dict.Keys;
		}
	}

	public bool Contains(K key) {
		return dict.ContainsKey(key);
	}

	public bool Contains(K key, V value) {
		if (dict.ContainsKey(key)) {
			return dict[key].Contains(value);
		}
		return false;
	}

	public HashSet<V> this[K key] {
		get {
			if (!dict.ContainsKey(key)) {
				dict[key] = new HashSet<V>();
			}
			return dict[key];
		}
	}
}
