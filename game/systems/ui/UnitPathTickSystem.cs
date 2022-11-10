public partial class UnitPathTickSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var selectedUnitPath = World.GetElement<SelectedUnitPath>();
		var gamePanel = World.GetElement<GamePanel>();
		if (gamePanel.CurrentPanel.HasValue && gamePanel.CurrentPanel.Value.type == GamePanelType.Unit) {
			var selectedUnit = gamePanel.CurrentPanel.Value.entity;
			foreach (var e in World.Receive<UnitMoved>(this)) {
				if (e.unit == selectedUnit) {
					selectedUnitPath.RenderPath(e.unit);
				}
			}

			foreach (var e in World.Receive<UnitMovementPathUpdated>(this)) {
				if (e.unit == selectedUnit) {
					selectedUnitPath.RenderPath(e.unit);
				}
			}
		}
	}
}