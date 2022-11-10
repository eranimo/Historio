public partial class UnitPanelTickSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		if (!World.HasElement<UnitPanel>()) {
			return;
		}

		var unitPanel = World.GetElement<UnitPanel>();

		var gamePanel = World.GetElement<GamePanel>();
		if (gamePanel.CurrentPanel.HasValue && gamePanel.CurrentPanel.Value.type == GamePanelType.Unit) {
			var selectedUnit = gamePanel.CurrentPanel.Value.entity;
			
			foreach (var e in World.Receive<CurrentActionChanged>(this)) {
				if (e.entity == selectedUnit) {
					unitPanel.UpdateView(e.entity);
				}
			}

			foreach (var e in World.Receive<UnitMoved>(this)) {
				if (e.unit == selectedUnit) {
					unitPanel.UpdateView(e.unit);
				}
			}

			foreach (var e in World.Receive<ActionQueueChanged>(this)) {
				if (e.entity == selectedUnit) {
					unitPanel.UpdateView(e.entity);
				}
			}
		}
	}
}