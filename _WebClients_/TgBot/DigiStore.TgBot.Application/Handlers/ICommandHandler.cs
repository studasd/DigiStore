using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers;

/// <summary>
/// Базовый интерфейс для обработчиков команд
/// </summary>
public interface ICommandHandler
{
	/// <summary>
	/// Обрабатывает команду
	/// </summary>
	Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default);
}

