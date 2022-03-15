using Godot;

public class Entity {
	private GameManager manager;
  public bool deleted = false;

	public void Init(GameManager manager) {
		this.manager = manager;
	}

  public virtual void Update(GameDate date) {}
  public virtual void OnAdded() {}
  public virtual void OnRemoved() {}
}
