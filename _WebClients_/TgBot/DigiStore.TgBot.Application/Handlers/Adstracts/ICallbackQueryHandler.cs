using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers.Adstracts;

/// <summary>
/// Базовый интерфейс для обработчиков колбэков
/// </summary>
public interface ICallbackQueryHandler
{
	/// <summary>
	/// Обрабатывает колбэк
	/// </summary>
	Task<UnitResult<Error>> HandleAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken = default);
}
