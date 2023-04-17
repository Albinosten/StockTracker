
namespace StockTrackerApp
{
	public class StockPrice : IEntity
	{
		public StockPrice()
		{
			this.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
		}
		public string Name {get; set;}
		public decimal PriceEach {get;set;}

		private long timestamp;
		public long Timestamp => timestamp;
		public string DateAndTimeStamp => Date + " " + TimeStamp;
		public string Date => $"{DateTimeOffset.FromUnixTimeSeconds(this.timestamp).LocalDateTime:MMM-dd}";
		public string TimeStamp => $"{DateTimeOffset.FromUnixTimeSeconds(this.timestamp).LocalDateTime:HH:mm:ss}";
		
        public override string ToString()
		{
			return string.Join(", ", this.Name, this.PriceEach, this.timestamp);
		}
		public static StockPrice Parse(string s)
		{
			var values = s.Split(", ");
			return new StockPrice
			{
				Name = values[0],
				PriceEach = decimal.Parse(values[1]),
				timestamp = long.Parse(values[2]),
			};
		}
		public override bool Equals(object obj)
		{
			var other = (StockPrice)obj;
			return this.Name == other.Name
				&& this.PriceEach == other.PriceEach
				;
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}