using System.Text;
using System.Text.Json;

namespace WatchdogOrchestraClient;

internal class ConfigFileManager
{
	private static string ConfigName => "config.json";

	public static Configuration GetConfig()
	{
		var configText = File.ReadAllBytes(ConfigName);

		return JsonSerializer.Deserialize<Configuration>(Encoding.UTF8.GetString(configText), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? new Configuration();
	}

	internal static void SaveConfig(Configuration config)
	{
		var jConfig = JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true });

		File.WriteAllBytes(ConfigName, Encoding.UTF8.GetBytes(jConfig));
	}
}