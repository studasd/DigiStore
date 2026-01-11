using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.TgBot.Application.Options;

public class TelegramOptions
{
	public string BotToken { get; set; }
	public string WebhookUrl { get; set; }
	public bool IsWebhook { get; set; }
}
