namespace StockTrackerApp
{
    public enum CommandNumbers
    {
        Meny = 0,
        Buy = 1,
        Sell = 2,
        List = 3,
        Result = 5,
        Payouts = 6,
        Holding = 7,
        AddStock = 8,
        RemoveStock = 9,
        ToggleInclude = 97,
        SetStockPrice = 98,
        Exit = 99,
        ParseFromExport = 999,

        Invalid = -1,

    }
}