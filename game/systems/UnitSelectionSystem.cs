using RelEcs;

public class UnitSelectionSystem : ISystem {
	public void Run(Commands commands) {
		var selectedUnit = commands.GetElement<SelectedUnit>();

		commands.Receive((SelectedUnitUpdate e) => {
			Godot.GD.PrintS(e.unit, selectedUnit.unit);
			if (selectedUnit.unit is not null) {
				selectedUnit.unit.Get<UnitIcon>().Selected = false;
			}
			if (e.unit is not null) {
				e.unit.Get<UnitIcon>().Selected = true;
			}
			selectedUnit.unit = e.unit;
		});
	}
}