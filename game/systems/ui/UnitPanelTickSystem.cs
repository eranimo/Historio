public class UnitPanelTickSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		if (!this.HasElement<UnitPanel>()) {
			return;
		}

		var unitPanel = this.GetElement<UnitPanel>();

		var gamePanel = this.GetElement<GamePanel>();
		if (gamePanel.CurrentPanel.HasValue && gamePanel.CurrentPanel.Value.type == GamePanelType.Unit) {
			var selectedUnit = gamePanel.CurrentPanel.Value.entity;
			
			foreach (var e in this.Receive<CurrentActionChanged>()) {
				if (e.entity == selectedUnit) {
					unitPanel.UpdateView(e.entity);
				}
			}

			foreach (var e in this.Receive<UnitMoved>()) {
				if (e.unit == selectedUnit) {
					unitPanel.UpdateView(e.unit);
				}
			}

			foreach (var e in this.Receive<ActionQueueChanged>()) {
				if (e.entity == selectedUnit) {
					unitPanel.UpdateView(e.entity);
				}
			}
		}
	}
}