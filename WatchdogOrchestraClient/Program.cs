using System.Text.Json;
using Spectre.Console;
using WatchdogOrchestraClient;

bool endApp = false;

AnsiConsole.WriteLine("[blue] SS14 watchdog orchestrat client [/]");

var configText = File.ReadAllBytes("config.json");

var config = JsonSerializer.Deserialize<Configuration>(configText) ?? new Configuration();

if (string.IsNullOrWhiteSpace(config.Address))
{
	config.Address = AnsiConsole.Ask<string>("Введите адрес сервера");
}

if (string.IsNullOrWhiteSpace(config.Token))
{

}