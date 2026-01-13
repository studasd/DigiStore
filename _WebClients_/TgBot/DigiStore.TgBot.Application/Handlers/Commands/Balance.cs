using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Handlers.Commands;

/// <summary>
/// Обработчик команды /balance
/// </summary>
public class Balance : BaseHandler, ICommandHandler
{
	public const string Command = BotCommands.Balance;

	private readonly IProfileService _profileService;
	private readonly ISessionService _sessionService;
	private readonly ILogger<Balance> _logger;


	public Balance(
		ITelegramBotClient botClient,
		IProfileService profileService,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<Balance> logger)
		: base(botClient, localizationService)
	{
		_profileService = profileService;
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task HandleAsync(Message message, CancellationToken cancellationToken = default)
	{
		// Handle /balance command - show wallet info

		try
		{
			var telegramId = message.From!.Id;
			var chatId = message.Chat.Id;

			// Get session
			var sessionResult = await _sessionService.GetSessionAsync(telegramId, cancellationToken);
			if (sessionResult.IsFailure || sessionResult.Value.UserId == default)
			{
				await SendErrorMessage(chatId, "Session expired", cancellationToken);
				return;
			}

			var session = sessionResult.Value;
			var langCode = session.LangCode;

			var profileResult = await _profileService.GetFullProfileAsync(session.UserId, telegramId, cancellationToken);
			if (!profileResult.IsSuccess)
			{
				await SendErrorMessage(chatId, _localService.GetMessage(LocalKeys.Errors.Occurred, langCode), cancellationToken);
				return;
			}

			var profile = profileResult.Value!;

			var text = $@"
💰 {_localService.GetMessage(LocalKeys.Balances.Info, langCode)}
{_localService.GetMessage(LocalKeys.Balances.CurrentBalance, langCode)}: <b>{profile.Balance:F2} {profile.Currency}</b>
🔗 {_localService.GetMessage(LocalKeys.Balances.LinkedAccounts, langCode)}:
👤 Telegram: @{profile.Username ?? ""}
";

			var keyboard = GetBackToMainMenuKeyboard(langCode);

			await _botClient.SendMessage(
				chatId,
				text,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in BalanceCommandHandler");
			await SendErrorMessage(message.Chat.Id, "An error occurred", cancellationToken);
		}
	}
}
