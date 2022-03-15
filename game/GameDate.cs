using System;

public struct GameDate {
	public int dayTicks;
	private DateTime dateTime;

	public GameDate(int dayTicks) {
		dateTime = new DateTime(1, 1, 1).AddDays(dayTicks);
		this.dayTicks = dayTicks;
	}

	public GameDate NextDay() {
		return new GameDate(dayTicks + 1);
	}

	public bool isFirstOfMonth { get { return dateTime.Day == 1; } }
	public bool isFirstOfWeek { get { return dateTime.DayOfWeek == DayOfWeek.Monday; } }

	public override string ToString() {
		return $"{dateTime.ToString("dddd")}, {dateTime.ToString("MMMM d")}, year {dateTime.Year}";
	}
}
