using System.ComponentModel;
using System.Net.Http.Headers;
using System.Reflection;
using Spectre.Console;
using WatchdogOrchestra;
using WatchdogOrchestraClient;

ExceptionHandler.Init();

AnsiConsole.MarkupLine("[blue]SS14 watchdog orchestrat client[/]");

var config = ConfigFileManager.GetConfig();

if (string.IsNullOrWhiteSpace(config.Address))
{
	AnsiConsole.MarkupLine("[red]Адрес сервера не задан.[/]");

	return;
}

var client = GetHttpClient(config.Address);

try
{
	if (!await CheckTokenExsisting(config, client))
	{
		return;
	}

	client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Token);

	bool endApp = false;

	AnsiConsole.Clear();

	AnsiConsole.MarkupLine("[blue]SS14 watchdog orchestrat client[/]");

	var serverList = await GetInstances(client);

	if (serverList == null)
	{
		return;
	}

	while (!endApp)
	{
		var instance = AskInstance(serverList);

		var action = AskAction();

		if (!AnsiConsole.Confirm($"Произвести [red]{GetEnumDescription(action)}[/] сервера [green]{instance.Name}[/]?"))
		{
			continue;
		}

		try
		{
			var instanceApi = new ServerInstanceClient(client);

			switch (action)
			{
				case ServerAction.Restart:
					await instanceApi.RestartAsync(instance.Name);

					AnsiConsole.MarkupLine($"[green]{instance.Name}[/] успешно отправлена команда на перезапуск");
					break;

				case ServerAction.Update:
					await instanceApi.UpdateAsync(instance.Name);

					AnsiConsole.MarkupLine($"[green]{instance.Name}[/] успешно отправлена команда на обновление");
					break;
			}
		}
		catch (Exception exc)
		{
			ExceptionHandler.HandleError("Ошибка при выполнении действия над сервером", exc);
		}
	}
}
finally
{
	client.Dispose();
}

static InstanceConfiguration AskInstance(ICollection<InstanceConfiguration> instances)
{
	var panel = new Panel(InctanceListToString(instances))
			.Header("Список серверов")
			.RoundedBorder()
			.Expand();

	AnsiConsole.Write(panel);

	bool askInstance = true;

	var inctanse = new InstanceConfiguration();

	while (askInstance)
	{
		var serverId = AnsiConsole.Ask<int>("Введите [green]номер сервера[/]:");

		var chosedInstance = instances.ElementAtOrDefault(serverId);

		if (chosedInstance != null)
		{
			inctanse = chosedInstance;

			break;
		}
	}

	return inctanse;
}

static ServerAction AskAction()
{
	var panel = new Panel(GetActionsDescription())
			.Header("Список действий")
			.RoundedBorder()
			.Expand();

	AnsiConsole.Write(panel);

	bool askAction = true;

	var action = ServerAction.Restart;

	while (askAction)
	{
		var chosedAction = AnsiConsole.Ask<ServerAction>("Введите [green]действие[/]:");

		if (Enum.IsDefined(chosedAction))
		{
			action = chosedAction;

			break;
		}
	}

	return action;
}

static Task<LoginResponse> RequestToken(HttpClient client)
{
	var api = new LoginClient(client);

	(var login, var password) = GetLoginParameters();

	return api.LoginAsync(new LoginRequestParameters
	{
		UserName = login,
		Password = password
	});
}

static HttpClient GetHttpClient(string address)
{
	var client = new HttpClient
	{
		BaseAddress = new Uri(address)
	};

	return client;
}

static (string login, string password) GetLoginParameters()
{
	var login = AnsiConsole.Ask<string>("Введите [green]имя пользователя[/] orchestrat:");

	var password = AnsiConsole.Prompt(
		new TextPrompt<string>("Введите [green]пароль[/] orchestrat:")
			.PromptStyle("red")
			.Secret(null));

	return (login, password);
}

static async Task<bool> CheckTokenExsisting(Configuration config, HttpClient client)
{
	if (string.IsNullOrWhiteSpace(config.Token))
	{
		try
		{
			var rsp = await RequestToken(client);

			config.Token = rsp.Token;
		}
		catch (Exception exc)
		{
			ExceptionHandler.HandleError("Ошибка при получении токена авторизации.", exc);

			return false;
		}

		ConfigFileManager.SaveConfig(config);
	}

	return true;
}

static async Task<ICollection<InstanceConfiguration>?> GetInstances(HttpClient client)
{
	ICollection<InstanceConfiguration>? list = null;

	try
	{
		AnsiConsole.MarkupLine("Получение [blue]списка серверов[/] от оркестрата");

		var api = new ServerInstanceClient(client);

		list = await api.GetListAsync();

		if (list.Count == 0)
		{
			AnsiConsole.MarkupLine("Получен [red]пустой список[/] от оркестрата.");

			return null;
		}
	}
	catch (Exception exc)
	{
		ExceptionHandler.HandleError("Ошибка при получении списка серверов от оркестрата.", exc);
	}

	return list;
}

static string InctanceListToString(ICollection<InstanceConfiguration> instances)
{
	return string.Join(Environment.NewLine, instances.Select((value, index) => $"{index}: {value.Name}"));
}

static string GetActionsDescription()
{
	return string.Join(Environment.NewLine, Enum.GetValues<ServerAction>().Select(x => $"{(int)x}: {GetEnumDescription(x)}"));
}

static string GetEnumDescription(Enum value)
{
	// Get the Description attribute value for the enum value
	FieldInfo? fi = value.GetType().GetField(value.ToString());

	if (fi == null)
	{
		return string.Empty;
	}

	DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

	if (attributes.Length > 0)
		return attributes[0].Description;
	else
		return value.ToString();
}