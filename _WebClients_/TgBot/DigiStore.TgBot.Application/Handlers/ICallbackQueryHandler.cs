using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers;

/// <summary>
/// Базовый интерфейс для обработчиков колбэков
/// </summary>
public interface ICallbackQueryHandler
{
	/// <summary>
	/// Данные колбэка, которые обрабатывает этот хэндлер (точное значение или префикс)
	/// </summary>
	string CallbackData { get; }

	/// <summary>
	/// Если true, то CallbackData используется как префикс для поиска
	/// </summary>
	bool IsPrefix { get; }

	/// <summary>
	/// Обрабатывает колбэк
	/// </summary>
	Task HandleAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken = default);
}
