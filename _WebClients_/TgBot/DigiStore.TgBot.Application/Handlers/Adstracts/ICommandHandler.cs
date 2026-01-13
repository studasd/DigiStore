using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers.Adstracts;

/// <summary>
/// Базовый интерфейс для обработчиков команд
/// </summary>
public interface ICommandHandler
{
	/// <summary>
	/// Обрабатывает команду
	/// </summary>
	Task<UnitResult<Error>> HandleAsync(Message message, CancellationToken cancellationToken = default);
}
