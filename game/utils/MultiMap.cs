using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class MultiMap<K, V> : IEnumerable<(K, List<V>)> {
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

	public IEnumerator<(K, List<V>)> GetEnumerator() {
		foreach (var key in Keys) {
			yield return (key, dict[key]);
		}
	}

	IEnumerator IEnumerable.GetEnumerator() {
		foreach (var key in Keys) {
			yield return (key, dict[key]);
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
