using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Updates.Dispatchers;

public interface IUpdateDispatcher
{
	bool CanHandle(Update update);

	Task<UnitResult<Error>> DispatchAsync(Update update, CancellationToken token = default);
}
