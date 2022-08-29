public class Factory {
	protected readonly GameManager manager;

	public Factory(GameManager manager) {
		this.manager = manager;
	}
}

public class Factories {
	public UnitFactory unitFactory;
	public DistrictFactory districtFactory;
	public ImprovementFactory improvementFactory;

	public Factories(GameManager manager) {
		unitFactory = new UnitFactory(manager);
		districtFactory = new DistrictFactory(manager);
		improvementFactory = new ImprovementFactory(manager);
	}
}