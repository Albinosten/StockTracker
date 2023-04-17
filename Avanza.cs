
namespace StockTrackerApp
{
	public class Avanza : IEntity
	{
		public Avanza()
		{
		}
		public string Name {get; set;}
		public decimal PriceEach {get;set;}
        public Action Action {get;set;}

		private long timestamp;
		public long Timestamp => timestamp;
		public string DateAndTimeStamp => Date + " " + TimeStamp;
		public string Date => $"{DateTimeOffset.FromUnixTimeSeconds(this.timestamp).LocalDateTime:MMM-dd}";
		public string TimeStamp => $"{DateTimeOffset.FromUnixTimeSeconds(this.timestamp).LocalDateTime:HH:mm:ss}";
		
        public override string ToString()
		{
			return string.Join(", ", this.Name, this.Action, this.Count, this.PriceEach, this.timestamp);
		}
        /*
Datum;		Konto	;Typ av transaktion	;Värdepapper	;Antal	;Kurs	;Belopp;Courtage;Valuta;ISIN;Resultat
2023-04-12;	7966503	;Sälj				;BE Group		;-125	;143	;17875;	-;SEK;SE0008321921;-
2022-04-27;	7966503	;Utdelning;			BE Group		;13		;12		;156;	-;SEK;SE0008321921;-
2022-06-02;	7966503	;Övrigt;			Hexatronic Group;-27;-;-;-;-;SE0018040677;-
        */
        public decimal Count {get;set;}

        public static Avanza Parse(string s)
		{
			var values = s.Split(", ");
			return new Avanza
			{
                Name = values[3],
                Action = GetAction(values[2]),
                Count = Math.Abs(int.Parse(values[4])),
				PriceEach = decimal.Parse(values[5]),
                timestamp = DateTimeOffset.Parse(values[0]).ToUnixTimeSeconds(),
			};
		}
		public static Avanza ParseFromExport(string s)
		{
			var values = s.Split(";");
			var action = GetAction(values[2]);
			if(action == Action.Deposit)
			{
				return new Avanza
				{
					timestamp = DateTimeOffset.Parse(values[0]).ToUnixTimeSeconds(),
					Action = action,
					Name = values[3],
					Count = 0,
					PriceEach = 0,
				};	
			}
			if(action == Action.Intrest)
			{
			 	return new Avanza()
				{
					Name ="ränta",
					Action = action,
				};
			}

			if(action == Action.Other)
			{
			 	return new Avanza()
				{
					Name = values[3],
					Action = action,
					Count = decimal.Parse(values[4]),
				};
			}
			return new Avanza
				{
					timestamp = DateTimeOffset.Parse(values[0]).ToUnixTimeSeconds(),
					Action = action,
					Name = values[3],
					Count = Math.Abs(decimal.Parse(values[4])),
					PriceEach = decimal.Parse(values[5].Replace(',','.')),
				};
		}
		private static Action GetAction(string s) => s switch
		{
			"Sälj" => Action.Sell,
			"Köp" => Action.Buy,
			"Utdelning" => Action.Payout,
			"Räntor" => Action.Intrest,
			"Insättning" => Action.Deposit,
			_ => Action.Other,
		};
		public override bool Equals(object obj)
		{
			var other = (StockPrice)obj;
			return this.ToString() == other.ToString()
				;
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}