using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers;

/// <summary>
/// Базовый интерфейс для обработчиков колбэков
/// </summary>
public interface ICallbackQueryHandler
{
	/// <summary>
	/// Обрабатывает колбэк
	/// </summary>
	Task HandleAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken = default);
}

