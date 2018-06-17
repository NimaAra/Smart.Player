<Query Kind="Statements">
  <Namespace>System.Globalization</Namespace>
</Query>

var file = new FileInfo(@"D:\My Code\GitHub\Smart.Player\World Cup Schedule\Schedule - Raw.txt");

var fixtures = new List<KeyValuePair<DateTime, string>>();

bool isDate = false;
DateTime fixtureDate = default;

foreach(var line in File.ReadLines(file.FullName).Where(l => l.Length > 0))
{
	isDate = DateTime.TryParse(line, out var date);
	if (isDate)
	{
		fixtureDate = date;
	} else {
		var channel = line.EndsWith("BBC") ? "BBC" : "ITV";
		var startIdx = line.IndexOf(',');
		var teams = line.Substring(0, startIdx);
		var timeOfDayStr = line.Substring(startIdx, line.Length - startIdx - 4);
		
		if(!DateTime.TryParseExact(timeOfDayStr, ", htt", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var time)){
			throw new InvalidDataException(line);
		}
		
		var fixtureDateTime = fixtureDate.Add(time.TimeOfDay);
		Console.WriteLine("{0}, {1}, {2}", fixtureDateTime, channel, teams);
	}
}