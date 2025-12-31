using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Interfaces;
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
	
	private readonly ITelegramProfileService _profileService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILogger<Balance> _logger;

	string ICommandHandler.Command => Command;

	public Balance(
		ITelegramBotClient botClient,
		ITelegramProfileService profileService,
		ITelegramSessionService sessionService,
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
			var session = await _sessionService.GetSessionAsync(telegramId, cancellationToken);
			if (session?.UserId == null)
			{
				await SendErrorMessage(chatId, "Session expired", cancellationToken);
				return;
			}

			var languageCode = session.LanguageCode ?? "en";

			var profileResult = await _profileService.GetFullProfileAsync(session.UserId.Value, telegramId, cancellationToken);
			if (!profileResult.IsSuccess)
			{
				await SendErrorMessage(chatId, _localizationService.GetMessage("error_occurred", languageCode), cancellationToken);
				return;
			}

			var profile = profileResult.Value!;

			var text = $@"
💰 {_localizationService.GetMessage("balance_info", languageCode)}
{_localizationService.GetMessage("current_balance", languageCode)}: <b>{profile.Balance:F2} {profile.Currency}</b>
🔗 {_localizationService.GetMessage("linked_accounts", languageCode)}:
👤 Telegram: @{profile.TelegramUsername ?? "Not set"}
";

			var keyboard = GetBackToMainMenuKeyboard(languageCode);

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
