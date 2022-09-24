public class UnitPanelTickSystem : ISystem {
	public void Run(Commands commands) {
		if (!commands.HasElement<UnitPanel>()) {
			return;
		}

		var unitPanel = commands.GetElement<UnitPanel>();

		var gamePanel = commands.GetElement<GamePanel>();
		if (gamePanel.CurrentPanel.HasValue && gamePanel.CurrentPanel.Value.type == GamePanelType.Unit) {
			var selectedUnit = gamePanel.CurrentPanel.Value.entity;
			commands.Receive((CurrentActionChanged e) => {
				if (e.entity == selectedUnit) {
					unitPanel.UpdateView(e.entity);
				}
			});

			commands.Receive((UnitMoved e) => {
				if (e.unit == selectedUnit) {
					unitPanel.UpdateView(e.unit);
				}
			});

			commands.Receive((ActionQueueChanged e) => {
				if (e.entity == selectedUnit) {
					unitPanel.UpdateView(e.entity);
				}
			});
		}
	}
}