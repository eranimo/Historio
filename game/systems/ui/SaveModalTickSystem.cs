using System;
using System.Linq;
using System.IO;

public partial class SaveModalTickSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		if (!World.HasElement<SaveGameModal>()) {
			return;
		}

		foreach (var e in World.Receive<SaveModalLoadTrigger>(this)) {
			var saveGameModal = World.GetElement<SaveGameModal>();
			var player = World.GetElement<Player>();
			var gameDate = World.GetElement<GameDate>();
			saveGameModal.CountryName = World.GetComponent<CountryData>(player.playerCountry).name;
			saveGameModal.SaveNameInput = gameDate.ToString();
		}

		foreach (var e in World.Receive<SaveModalSaveTrigger>(this)) {
			var saveGameModal = World.GetElement<SaveGameModal>();
			var player = World.GetElement<Player>();
			var gameDate = World.GetElement<GameDate>();
			var countryName = World.GetComponent<CountryData>(player.playerCountry).name;
			var worldData = World.GetElement<WorldData>();

			var nameSanitized = Path.GetInvalidFileNameChars().Aggregate(e.name, (f, c) => f.Replace(c, '_'));
			var entry = new SavedGameEntryMetadata {
				name = nameSanitized,
				saveName = e.name,
				countryName = countryName,
				dayTicks = gameDate.dayTicks,
				saveDate = DateTime.Now,
			};
			World.Send(new SaveGameTrigger { entry = entry });
		}
	}
}