using System.Globalization;
using DotNetEnv;
using rugwatch.Database;
using Spectre.Console;

namespace rugwatch;

internal class Program
{
	public static void Main(string[] _)
	{
		Env.Load(".env");
		Logger.LogInfo("Initialising rugwatch...");

		DatabaseContext dbContext = new(Env.GetString("DATABASE_PATH"));
		dbContext.Seed();

		CultureInfo cultureInfo = new("en-US");
		cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
		cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
		cultureInfo.NumberFormat.NumberGroupSeparator = " ";
		cultureInfo.NumberFormat.CurrencyGroupSeparator = " ";
		Thread.CurrentThread.CurrentCulture = cultureInfo;

		Table table = new Table()
			.Collapse()
			.ShowRowSeparators()
			.AddColumns([
				"Timestamp",
				"Trade type",
				"Coin symbol",
				"Coin name",
				"Coin amount",
				"Coin price",
				"Trade value",
				"Username",
				"User ID"]);

		RugplayClient client = new(Env.GetString("WEBSOCKET_URL"));
		client.Connect();

		Logger.LogInfo("Listening... Press any key to exit.");

		AnsiConsole.Live(table).Start(ctx =>
		{
			while (!Console.KeyAvailable)
			{
				while (client.TradesQueue.Count > 0)
				{
					if (!client.TradesQueue.TryDequeue(out Trade? trade))
					{
						continue;
					}

					ConsoleLogTrade(table, trade);

					dbContext.Trades.Add(trade);
					dbContext.SaveChanges();
				}

				ctx.Refresh();
				Task.Delay(50).Wait();
			}
		});

		Logger.LogInfo("Exiting...");
		client.Disconnect();
	}

	private static void ConsoleLogTrade(Table table, Trade trade)
	{
		string tradeType = trade.TradeType == "BUY" ?
			"[green]BUY[/] " : "[red]SELL[/]";

		table.AddRow(
			CreateMarkup($"{trade.Timestamp:yyyy-MM-dd HH:mm:ss}", Justify.Left),
			CreateMarkup(tradeType, Justify.Left),
			CreateMarkup($"*{ToAscii(trade.CoinSymbol, '?')}", Justify.Right),
			CreateMarkup(ToAscii(trade.CoinName, '?'), Justify.Right),
			CreateMarkup($"{trade.CoinAmount:N0}", Justify.Right),
			CreateMarkup($"{trade.CoinPrice:C6}", Justify.Right),
			CreateMarkup($"{trade.TradeValue:C0}", Justify.Right),
			CreateMarkup(trade.Username, Justify.Left),
			CreateMarkup($"{trade.UserId}", Justify.Right));
	}

	private static Markup CreateMarkup(string input, Justify justify)
	{
		return new Markup(input).Justify(justify);
	}

	private static string ToAscii(string input, char rep)
	{
		return string.Concat(input.Select(c => char.IsAscii(c) ? c : rep));
	}
}
