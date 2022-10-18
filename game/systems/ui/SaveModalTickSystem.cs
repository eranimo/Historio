using System;
using System.Linq;

public class SaveModalTickSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		if (!this.HasElement<SaveGameModal>()) {
			return;
		}

		foreach (var e in this.Receive<SaveModalLoadTrigger>()) {
			var saveGameModal = this.GetElement<SaveGameModal>();
			var player = this.GetElement<Player>();
			var gameDate = this.GetElement<GameDate>();
			saveGameModal.CountryName = this.GetComponent<CountryData>(player.playerCountry).name;
			saveGameModal.SaveNameInput = gameDate.ToString();
		}

		foreach (var e in this.Receive<SaveModalSaveTrigger>()) {
			var saveGameModal = this.GetElement<SaveGameModal>();
			var player = this.GetElement<Player>();
			var gameDate = this.GetElement<GameDate>();
			var countryName = this.GetComponent<CountryData>(player.playerCountry).name;
			var worldData = this.GetElement<WorldData>();

			var nameSanitized = System.IO.Path.GetInvalidFileNameChars().Aggregate(e.name, (f, c) => f.Replace(c, '_'));
			var entry = new SavedGameEntryMetadata {
				name = nameSanitized,
				saveName = e.name,
				countryName = countryName,
				dayTicks = gameDate.dayTicks,
				saveDate = DateTime.Now,
			};
			this.Send(new SaveGameTrigger { entry = entry });
		}
	}
}