
namespace StockTrackerApp
{
	public enum Action
	{
		Buy = 1,
		Sell = 2,
		Holding = 3,
		Payout = 4,
		Intrest = 5,
		Deposit = 6,
		Other = 0,
	}
	public class Transaction : IEntity
	{
		public Action Action;
		public string Name {get; set;}
        public decimal PriceEach {get;set;}
        public int Count {get;set;}
		
		public void SetTimeStamp(long l)
		{
			this.timestamp = l;
		}
		public long Timestamp => timestamp;
		private long timestamp;
		public string DateAndTimeStamp => Date + " " + TimeStamp;
		public string Date => $"{DateTimeOffset.FromUnixTimeSeconds(this.timestamp).LocalDateTime:yy-MMM-dd}";
		public string TimeStamp => $"{DateTimeOffset.FromUnixTimeSeconds(this.timestamp).LocalDateTime:HH:mm:ss}";
		
		public Transaction()
		{
			this.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
		}
		
		private string DisplayReadable(params Func<Transaction, object>[] properties)
		{
			return string.Join(", ", properties.Select(x => x.Invoke(this)));
		}

		public string AsReadably()
		{
			return this.DisplayReadable(x => x.Name
				, x => x.Action
				, x => "Count: " + x.Count
				, x => "Price each: " + x.PriceEach
				, x => "Value: " + x.GetTotalValue()
				, x => x.DateAndTimeStamp
				);
		}
		public decimal GetTotalValue()
		{
			var sign = this.Action == Action.Buy ? -1 : 1;
			return this.Count * this.PriceEach * sign;
		}
		public int GetBalanceChange()
		{
			var sign = (this.Action == Action.Buy || this.Action == Action.Other)
			? 1 
			: -1;
			return this.Count * sign;
		}
        public override string ToString()
		{
			return string.Join(", ", this.Name, this.Action, this.Count, this.PriceEach, this.timestamp);
		}
		public static Transaction Parse(string s)
		{
			var values = s.Split(", ");
			
			return new Transaction
			{
				Name = values[0],
				Action = Enum.Parse<Action>(values[1]),
				Count = int.Parse(values[2]),
				PriceEach = decimal.Parse(values[3]),
				timestamp = long.Parse(values[4]),
			};
		}
		public override bool Equals(object obj)
		{
			var other = (Transaction)obj;
			return this.Name == other.Name
				&& this.Action == other.Action
				&& this.TimeStamp == other.TimeStamp
				;
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}