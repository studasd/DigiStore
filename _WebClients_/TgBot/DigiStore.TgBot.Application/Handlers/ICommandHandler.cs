using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers;

/// <summary>
/// Базовый интерфейс для обработчиков команд
/// </summary>
public interface ICommandHandler
{
	/// <summary>
	/// Команда, которую обрабатывает этот хэндлер
	/// </summary>
	string Command { get; }

	/// <summary>
	/// Обрабатывает команду
	/// </summary>
	Task HandleAsync(Message message, CancellationToken cancellationToken = default);
}
