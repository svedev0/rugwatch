using Spectre.Console;

namespace rugwatch;

internal class Logger
{
	public static void LogInfo(string message)
	{
		AnsiConsole.MarkupLine($"[grey54]{message}[/]");
	}

	public static void LogWarning(string message)
	{
		AnsiConsole.MarkupLine($"[yellow]{message}[/]");
	}

	public static void LogError(string message)
	{
		AnsiConsole.MarkupLine($"[red]{message}[/]");
	}
}
