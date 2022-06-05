using System;

public class GameDate {
	public int dayTicks;
	private DateTime dateTime;

	public GameDate(int dayTicks) {
		dateTime = new DateTime(1, 1, 1).AddDays(dayTicks);
		this.dayTicks = dayTicks;
	}

	public void NextDay() {
		dayTicks += 1;
		dateTime = dateTime.Add(TimeSpan.FromDays(1));
	}

	public bool isFirstOfMonth { get { return dateTime.Day == 1; } }
	public bool isFirstOfWeek { get { return dateTime.DayOfWeek == DayOfWeek.Monday; } }

	public override string ToString() {
		// return $"{dateTime.ToString("d")}, {dateTime.ToString("MMMM d")}, year {dateTime.Year}";
		return dateTime.ToString("dd / MM / yy");
	}
}
