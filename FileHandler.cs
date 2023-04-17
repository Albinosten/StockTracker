using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

namespace StockTrackerApp
{
	public interface IEntity
	{
		string Name {get;}
	}
	
	public class AvanzaExportFileHandler : FileHandler<Avanza>
    {
        protected override string filename => "TransaktionerAvanza2.csv";

        public override Avanza Parse(string s)
        {
			try
			{
				return Avanza.ParseFromExport(s);
			}
			catch
			{
				Console.WriteLine(s);
			}
			return new Avanza();
        }
    }
    public class StockPriceFileHandler : FileHandler<StockPrice>
    {
        protected override string filename => "StocksPrice.csv";

        public override StockPrice Parse(string s)
        {
			return StockPrice.Parse(s);
        }
    }
    public class StockFileHandler : FileHandler<Stock>
    {
        protected override string filename => "Stocks.csv";

        public override Stock Parse(string s)
        {
			return Stock.Parse(s);
        }
    }
    public class TransactionFileHandler : FileHandler<Transaction>
    {
        public override Transaction Parse(string s)
        {
			return Transaction.Parse(s);
        }

        protected override string filename => "Transactions.csv";
    }
    public abstract class FileHandler<T>
		where T : IEntity
	{
		[Obsolete]
		private static string location => "";
#pragma warning disable CS0612 // Type or member is obsolete
		public static string Location => string.IsNullOrEmpty(location) ? Directory.GetCurrentDirectory() + "/Files/" : location;
#pragma warning restore CS0612 // Type or member is obsolete

		string filePath => Location + filename;
		protected abstract string filename {get;}
		
		public bool Create(IList<T> entities)
		{
			if (!File.Exists(filePath))
			{
				File.Create(filePath).Close();
			}

			var file = File.Open(filePath, FileMode.Append);
			var streamWriter = new StreamWriter(file);

			foreach (var line in entities)
			{
				streamWriter.WriteLine(line.ToString());
			}

			streamWriter.Close();
			file.Close();
			return true;
		}
		public bool Create(IEnumerable<T> entities)
		{
			return this.Create(entities.ToList());
		}
		public bool Create(T line)
		{
			return this.Create(new[] { line });
		}
		public void Delete(IList<string> logs)
		{
			if (!File.Exists(filePath))
			{
				File.Create(filePath).Close();
			}
			var a = this.GetAllLines(new CommandRequest(){ExactSearch = true})
				.Where(l => !logs.Contains(l))
				.Select(x => x.ToString())
				.ToList();
			File.WriteAllLines(filePath, a
				);
		}
		public void Delete(IEnumerable<T> log)
		{
			this.Delete(log.Select(s => s.ToString()).ToList());
		}
		public void Delete(T log)
		{
			this.Delete(new T[] { log });
		}
		public IList<T> GetAll()
		{
			return this.GetAll(new CommandRequest());
		}
		public IList<T> GetAll(CommandRequest request)
		{
			return this.GetAllLines(request)
				.Select(Parse)
				.ToList();
		}
		
		protected IList<string> GetAllLines(CommandRequest request)
		{
			if (!File.Exists(filePath))
			{
				return new List<string>();
			}
			return File.ReadAllLines(filePath)
				.Where(x => Filter(request, x))
				.ToList();
		}
		private bool Filter(CommandRequest request, string line)
		{
			if (!string.IsNullOrEmpty(request.Arg))
			{
				if (request.ExactSearch)
				{
					return Parse(line).Name.ToUpper() == request.Arg.ToUpper();
				}
				return Parse(line).Name.ToUpper().Contains(request.Arg.ToUpper());
			}
			return true;
		}
		public void Reset()
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
		public string Backup()
		{
			var newName = Location + filename + $"{DateTimeOffset.Now.LocalDateTime:yyyy-MM-dd HH-mm-ss}" + ".csv";
			if (!File.Exists(newName))
			{
				File.Create(newName).Close();
			}
			var allRows = this.GetAllLines(new CommandRequest());

			var file = File.Open(newName, FileMode.Append);
			var streamWriter = new StreamWriter(file);

			foreach (var line in allRows)
			{
				streamWriter.WriteLine(line);
			}

			streamWriter.Close();
			file.Close();
			return newName;
		}
		private (List<(string, long)>, long) GetAllFiles()
		{
			var files = Directory.GetFiles(Location)
				.Select(x => (x.Remove(0, Location.Length), GetFileSize(x)))
				.ToList();
			return (files, files.Sum(x => x.Item2));
		}
		public long GetFileSize(string fileName)
		{
			if (File.Exists(fileName))
			{
				return new FileInfo(fileName).Length;
			}
			return 0;
		}
		public abstract T Parse(string s);
		public void Restore(string restoreFrom)
		{
			try
			{
				var allLines = File
					.ReadAllLines(Location + restoreFrom)
					.Select(Parse)
					.ToList()
					;
				this.Reset();
				this.Create(allLines);
				PrintWithColor.WriteLine("Restored from: " + restoreFrom);
			}
			catch
			{
				PrintWithColor.WriteLine("Could not restore from: " + restoreFrom);
			}
		}
	}
}