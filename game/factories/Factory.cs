public class Factory {
	protected readonly GameManager manager;

	public Factory(GameManager manager) {
		this.manager = manager;
	}

	public EntityBuilder Spawn() {
		return new EntityBuilder(manager.state, manager.state.Spawn().Identity);
	}

	public EntityBuilder On(Entity entity) {
		return new EntityBuilder(manager.state, entity.Identity);
	}
}

public class Factories {
	public UnitFactory unitFactory;
	public DistrictFactory districtFactory;
	public ImprovementFactory improvementFactory;
	public PopFactory popFactory;
	public BiotaFactory biotaFactory;

	public Factories(GameManager manager) {
		unitFactory = new UnitFactory(manager);
		districtFactory = new DistrictFactory(manager);
		improvementFactory = new ImprovementFactory(manager);
		popFactory = new PopFactory(manager);
		biotaFactory = new BiotaFactory(manager);
	}
}