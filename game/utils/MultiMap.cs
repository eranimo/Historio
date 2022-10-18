using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MultiMap<K, V> {
	private Dictionary<K, List<V>> dict = new Dictionary<K, List<V>>();

	public void Add(K key, V value) {
		List<V> list;
		if (this.dict.TryGetValue(key, out list)) {
			list.Add(value);
		} else {
			list = new List<V>();
			list.Add(value);
			this.dict[key] = list;
		}
	}

	public IEnumerable<K> Keys {
		get {
			return this.dict.Keys;
		}
	}

	public List<V> this[K key] {
		get {
			List<V> list;
			if (!this.dict.TryGetValue(key, out list)) {
				list = new List<V>();
				this.dict[key] = list;
			}
			return list;
		}
	}
}