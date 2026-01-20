using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.TgBot.Infrastructure.BotAPI;

public class TelegramOptions
{
	public string BotToken { get; set; }
	public string WebhookUrl { get; set; }
	public bool IsWebhook { get; set; }
	public bool IsDebugShortResponse { get; set; }
	
	public string Proxy { get; set; }
}
