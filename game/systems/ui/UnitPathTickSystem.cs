public class UnitPathTickSystem : ISystem {
	public void Run(Commands commands) {
		var selectedUnitPath = commands.GetElement<SelectedUnitPath>();
		var gamePanel = commands.GetElement<GamePanel>();
		if (gamePanel.CurrentPanel.HasValue && gamePanel.CurrentPanel.Value.type == GamePanelType.Unit) {
			var selectedUnit = gamePanel.CurrentPanel.Value.entity;
			commands.Receive((UnitMoved e) => {
				if (e.unit == selectedUnit) {
					selectedUnitPath.RenderPath(e.unit);
				}
			});

			commands.Receive((UnitMovementPathUpdated e) => {
				if (e.unit == selectedUnit) {
					selectedUnitPath.RenderPath(e.unit);
				}
			});
		}
	}
}