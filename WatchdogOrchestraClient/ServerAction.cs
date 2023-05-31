using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchdogOrchestraClient;

internal enum ServerAction
{
	[Description("Перезапустить")]
	Restart = 0,

	[Description("Обновить")]
	Update = 1
}
