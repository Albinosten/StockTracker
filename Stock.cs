
namespace StockTrackerApp
{
	public class Stock : IEntity
	{
		public Stock()
		{
			this.Name = string.Empty;
		}
		public string Name {get; set;}
		public bool Include{get;set;}
        public override string ToString()
		{
			return string.Join(", ", this.Name, this.Include);
		}
		public static Stock Parse(string s)
		{
			var values = s.Split(", ");
			return new Stock
			{
				Name = values[0],
				Include = bool.Parse(values[1]),
			};
		}
		public override bool Equals(object obj)
		{
			var other = (Stock)obj;
			return this.Name == other.Name
				;
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}