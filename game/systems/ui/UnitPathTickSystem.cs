public class UnitPathTickSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var selectedUnitPath = this.GetElement<SelectedUnitPath>();
		var gamePanel = this.GetElement<GamePanel>();
		if (gamePanel.CurrentPanel.HasValue && gamePanel.CurrentPanel.Value.type == GamePanelType.Unit) {
			var selectedUnit = gamePanel.CurrentPanel.Value.entity;
			foreach (var e in this.Receive<UnitMoved>()) {
				if (e.unit == selectedUnit) {
					selectedUnitPath.RenderPath(e.unit);
				}
			}

			foreach (var e in this.Receive<UnitMovementPathUpdated>()) {
				if (e.unit == selectedUnit) {
					selectedUnitPath.RenderPath(e.unit);
				}
			}
		}
	}
}