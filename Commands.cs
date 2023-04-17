using System.Linq;
using System.Collections.Generic;

namespace StockTrackerApp
{
    public class Meny : ICommand
    {
		public bool Execute()
        {
            var commands = new []
			{
                typeof(Buy),
                typeof(Sell),
                typeof(List),
                typeof(Result),
                typeof(AddStock),
                typeof(SetStockPrice),
			};

			PrintWithColor.WriteLine("*************************************");
			// foreach (var command in commands.Select(CommandCreator.Create))
			foreach (var command in CommandCreator.CreateAll())
			{
				PrintWithColor.WriteLine((int)command.CommandNumber + " : " + command.GetType().Name);
			}
			PrintWithColor.WriteLine("X" + " : " + typeof(Exit).Name);

			var nextCommand = CommandCreator.GetCommand(Console.ReadLine());
			Console.Clear();

			return nextCommand.Execute();
		}
        public IList<string> Aliases => new string[]{};
		public CommandNumbers CommandNumber => CommandNumbers.Meny;
    
    }
    public class Buy : BuySellBase, ICommand
    {
        public CommandNumbers CommandNumber => CommandNumbers.Buy;
        
        protected override Action action => Action.Buy;

        public bool Execute()
        {
            PrintWithColor.WriteLine("Buy - Stock Name:");
            base.Invoke();

            return true;
        }
    }
     public class Sell : BuySellBase, ICommand
    {
        public CommandNumbers CommandNumber => CommandNumbers.Sell;
        
        protected override Action action => Action.Sell;

        public bool Execute()
        {
            PrintWithColor.WriteLine("Sell - Stock Name:");
            base.Invoke();

            return true;
        }
    }
    public abstract class BuySellBase
    {
        protected abstract Action action {get;}

        public IList<string> Aliases => new string[]{};
        protected bool Invoke()
        {
            var stocks = new StockFileHandler()
				.GetAll()
				.ToList();

			for (int i = stocks.Count - 1; i >= 0; i--)
			{
				PrintWithColor.WriteLine(i + " : " + stocks[i].Name);
			}
			var result = int.TryParse(Console.ReadLine(), out int index)
				&& index < stocks.Count
				&& index >= 0
                ;
				

            PrintWithColor.WriteLine("Price each");
            result = decimal.TryParse(Console.ReadLine(), out var priceEach);
            PrintWithColor.WriteLine("Count");
            result = int.TryParse(Console.ReadLine(), out var count);
            
            if(!result)
            {
                PrintWithColor.WriteLine("Error in formation");
                return true;
            }
            
            var transaction = new Transaction()
            {
                Action = this.action,
                Name = stocks[index].Name,
                PriceEach = priceEach,
                Count = count,
            };
            var fileHandler = new TransactionFileHandler();
            fileHandler.Create(transaction);

            return true;
        }
    }


    public class AddStock : ICommand
    {
        public CommandNumbers CommandNumber => CommandNumbers.AddStock;

        public IList<string> Aliases => new string[]{};

        public bool Execute()
        {
            PrintWithColor.WriteLine("New stock name:");
            var name = Console.ReadLine();
            if(string.IsNullOrEmpty(name))
            {
                PrintWithColor.WriteLine("No name set");
                return true;
            }
            var StockFileHandler = new StockFileHandler();
            StockFileHandler.Create(new Stock(){Name = name});

            return true;
        }
    }

    public class SetStockPrice : ICommand
    {
        public CommandNumbers CommandNumber => CommandNumbers.SetStockPrice;

        public IList<string> Aliases => new string[]{};

        public bool Execute()
        {
        
            var stocks = new StockFileHandler()
				.GetAll()
				.ToList();

			for (int i = stocks.Count - 1; i >= 0; i--)
			{
				PrintWithColor.WriteLine(i + " : " + stocks[i].Name);
			}
			var result = int.TryParse(Console.ReadLine(), out int index)
				&& index < stocks.Count
				&& index >= 0
                ;
            PrintWithColor.WriteLine("Price:");
            result = decimal.TryParse(Console.ReadLine(), out var priceEach);
            
            if(!result)
            {
				PrintWithColor.WriteLine("Error");
                return true;
            }
            var StockPriceFileHandler = new StockPriceFileHandler();
            StockPriceFileHandler.Create(new StockPrice(){Name = stocks[index].Name, PriceEach = priceEach});
				

            return true;
        }
    }

    public class Result : ListBase, ICommand
    {
		public bool Execute()
        {
            this.Invoke();
                
            return true;
        }

        protected override bool Filter(Action action)
        {
            return true;
        }

        public IList<string> Aliases => new string[]{};
        public CommandNumbers CommandNumber => CommandNumbers.Result;
    }
    public abstract class ListBase
    {
        protected abstract bool Filter(Action action);
        protected IEnumerable<IGrouping<string,Transaction>> TransactionPerStock()
        {
            var fileHandler = new TransactionFileHandler();
            var request = new CommandRequest(){Arg = Console.ReadLine()};
            var stocks = new StockFileHandler()
                .GetAll()
                .Where(x => x.Include)
                .Select(x => x.Name.ToUpper())
                .ToList();
            var transactionsPerStock = fileHandler
                .GetAll(request)
                .Where(x => Filter(x.Action))
                .GroupBy(x => x.Name.ToUpper())
                .Where(x => stocks.Contains(x.Key))
                ;
                return transactionsPerStock;
        }
        protected void Invoke()
        {
            var notFinishedTransactions = new List<Transaction>();
            var transactionsPerStock = this.TransactionPerStock();
            foreach(IList<Transaction> stockTransactions in transactionsPerStock)
            {   
                PrintWithColor.WriteLine(stockTransactions[0].Name);
                foreach(var transaction in stockTransactions.OrderBy(x => x.Timestamp))
                {
                    PrintWithColor.WriteLine(transaction.AsReadably());
                }
                var balanceLeft = stockTransactions
                    .Where(x => x.Action == Action.Buy || x.Action == Action.Sell || x.Action == Action.Other)
                    .Sum(x => x.GetBalanceChange());
                var notFinishedTransaction =  new Transaction(){Name = stockTransactions[0].Name};

                if(balanceLeft != decimal.Zero)
                {
                    var lastStockPrice = new StockPriceFileHandler()
                        .GetAll(new CommandRequest(){Arg = stockTransactions[0].Name})
                        .OrderByDescending(x => x.Timestamp)
                        .FirstOrDefault()
                        ?.PriceEach ?? 0;
                        
                    notFinishedTransaction.Count = balanceLeft;
                    notFinishedTransaction.Action = Action.Holding;
                    notFinishedTransaction.PriceEach = balanceLeft > decimal.Zero ? lastStockPrice : 0;
                    PrintWithColor.WriteLine(notFinishedTransaction);
                    notFinishedTransactions.Add(notFinishedTransaction);
                }
                PrintWithColor.WriteLine("Sum: " + (stockTransactions.Sum(x => x.GetTotalValue()) + notFinishedTransaction.GetTotalValue()));
                PrintWithColor.WriteLine("");
            }
            var total = transactionsPerStock.Sum(x => x.Sum(x => x.GetTotalValue())) 
                + notFinishedTransactions.Sum(x => x.GetTotalValue());
            PrintWithColor.WriteLine("Total: " + total);
         }
        
    }
    public class Payouts : ListBase, ICommand
    {
        public IList<string> Aliases => new string[]{};
        public CommandNumbers CommandNumber => CommandNumbers.Payouts;

        public bool Execute()
        {
            this.Invoke();
            return true;
        }
        protected override bool Filter(Action action)
        {
            return action == Action.Payout;
        }
    }
     public class Holding : ListBase, ICommand
    {
        public IList<string> Aliases => new string[]{};
        public CommandNumbers CommandNumber => CommandNumbers.Holding;

        public bool Execute()
        {
            var holdings = new List<Transaction>();
            var transactionsPerStock = this.TransactionPerStock();
            foreach(IList<Transaction> stockTransactions in transactionsPerStock)
            {   
                var balanceLeft = stockTransactions
                    .Where(x => x.Action == Action.Buy || x.Action == Action.Sell || x.Action == Action.Other)
                    .Sum(x => x.GetBalanceChange());

                if(balanceLeft > decimal.Zero)
                {
                    PrintWithColor.WriteLine(stockTransactions[0].Name);
                    var lastStockPrice = new StockPriceFileHandler()
                        .GetAll(new CommandRequest(){Arg = stockTransactions[0].Name})
                        .OrderByDescending(x => x.Timestamp)
                        .FirstOrDefault()
                        ?.PriceEach ?? 0;
                        
                    var holding =  new Transaction()
                    {
                        Name = stockTransactions[0].Name,
                        Count = balanceLeft,
                        Action = Action.Holding,
                        PriceEach = balanceLeft > decimal.Zero ? lastStockPrice : 0,
                    };
                    holdings.Add(holding);
                    PrintWithColor.WriteLine(holding);

                    PrintWithColor.WriteLine("Sum: " + holding.GetTotalValue());
                    PrintWithColor.WriteLine("");
                }
            }
            var total = holdings.Sum(x => x.GetTotalValue());
            PrintWithColor.WriteLine("Total: " + total);
            return true;
        }
        protected override bool Filter(Action action)
        {
            return action == Action.Buy
                || action == Action.Sell
                || action == Action.Other
                ;
        }
    }

    public class List : ICommand
    {
		public bool Execute()
        {
            var fileHandler = new TransactionFileHandler();
            var transactions = fileHandler.GetAll(new CommandRequest(){});
            foreach(var transaction in transactions)
            {   
                PrintWithColor.WriteLine(transaction.AsReadably());;
            }
            return true;
        }
        public IList<string> Aliases => new string[]{"L", "LS"};
        public CommandNumbers CommandNumber => CommandNumbers.List;
    }
    public class ParseFromExport : ICommand
    {
        public CommandNumbers CommandNumber => CommandNumbers.ParseFromExport;

        public IList<string> Aliases => new string[]{};

        public bool Execute()
        {
            var StockFileHandler = new StockFileHandler();
            var TransactionFileHandler = new TransactionFileHandler();
            var filehandler = new AvanzaExportFileHandler();
            var stockGroup = filehandler
                .GetAll()
                .GroupBy(x => x.Name)
                .ToList();   
            foreach(IList<Avanza> transactions in stockGroup)
            {
                StockFileHandler.Create(new Stock(){Name = transactions[0].Name, Include = true});
                foreach(var transaction in transactions)
                {
                    var newtransaction = new Transaction
                    {
                        Name = transaction.Name,
                        Count = (int)transaction.Count,
                        PriceEach = transaction.PriceEach,
                        Action = transaction.Action,
                    };
                    newtransaction.SetTimeStamp(transaction.Timestamp);
                    TransactionFileHandler.Create(newtransaction);
                }
            }
            return true;
        }
    }
    public class ToggleInclude: ICommand
    {
        public CommandNumbers CommandNumber => CommandNumbers.ToggleInclude;

        public IList<string> Aliases => new string[]{"toggle"};

        public bool Execute()
        {
            Console.Clear();
            var fileHandler = new StockFileHandler();
            var stocks = fileHandler
				.GetAll()
				.ToList();

			for (int i = stocks.Count - 1; i >= 0; i--)
			{
				PrintWithColor.WriteLine(i + " : " + stocks[i].Name + " : "  + stocks[i].Include);
			}
			var result = int.TryParse(Console.ReadLine(), out int index)
				&& index < stocks.Count
				&& index >= 0
                ;
                if(!result){return true;}

            var stock = stocks[index];
            fileHandler.Delete(stock);
            stock.Include = !stock.Include;
            fileHandler.Create(stock);


            return true;
        }
    }
    public class Exit : ICommand
    {
		public bool Execute()
        {
            return false;
        }
        public IList<string> Aliases => new string[]{"X"};
        public CommandNumbers CommandNumber => CommandNumbers.Exit;
    }
    public class Invalid : ICommand
    {
		public bool Execute()
        {
			PrintWithColor.WriteLine("Invalid operation");
            return true;
        }
        public IList<string> Aliases => new string[]{};
		public CommandNumbers CommandNumber => CommandNumbers.Invalid;
    }
}