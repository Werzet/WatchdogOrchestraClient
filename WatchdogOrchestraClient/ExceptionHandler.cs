using Spectre.Console;

namespace WatchdogOrchestraClient;

internal static class ExceptionHandler
{
	public static void Init()
	{
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
	}

	private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception exc)
		{
			HandleError("Необработанная ошибка", exc);
		}
		else
		{
			HandleError("Необработанная ошибка", new Exception(e.ExceptionObject.ToString()));
		}
	}

	public static void HandleError(string msg, Exception exc)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.Write(new Rule(msg).LeftJustified());
		AnsiConsole.WriteLine();
		AnsiConsole.WriteException(exc);
	}
}