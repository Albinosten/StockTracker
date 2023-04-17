namespace StockTrackerApp
{
    public interface ICommandRequest 
	{
		string Arg { get; set; }
		bool ExactSearch { get; set; }
		//bool Clear { get; set; }
	}
}