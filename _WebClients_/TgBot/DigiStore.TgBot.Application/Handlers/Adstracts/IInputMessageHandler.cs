using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers.Adstracts;

/// <summary>
/// Интерфейс обработчика обычных текстовых сообщений (не команд).
/// </summary>
public interface IInputMessageHandler
{
	///// <summary>
	///// Состояние бота, для которого предназначен обработчик.
	///// Используется для диспетчеризации произвольного текста.
	///// </summary>
	//string State { get; }

	Task<UnitResult<Error>> HandleAsync(Message message, CancellationToken token = default);
}
