using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Attributes;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Commands;

/// <summary>
/// Обработчик команды /balance
/// </summary>
[Command(BotCommands.Balance)]
public class BalanceCommandHandler : ICommandHandler
{
	private readonly ITelegramProfileService _profileService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<BalanceCommandHandler> _logger;

	public BalanceCommandHandler(
		ITelegramProfileService profileService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<BalanceCommandHandler> logger)
	{
		_profileService = profileService;
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
	{
		try
		{
			var telegramId = message.From!.Id;
			var chatId = message.Chat.Id;

			// Get session
			var session = await _sessionService.GetSessionAsync(telegramId, cancellationToken);
			if (session?.UserId == null)
			{
				await SendErrorMessage(botClient, chatId, "Session expired", cancellationToken);
				return;
			}

			var languageCode = session.LanguageCode ?? "en";
			var loc = _localizationService;

			var profileResult = await _profileService.GetFullProfileAsync(session.UserId.Value, telegramId, cancellationToken);
			if (!profileResult.IsSuccess)
			{
				await SendErrorMessage(botClient, chatId, loc.GetMessage("error_occurred", languageCode), cancellationToken);
				return;
			}

			var profile = profileResult.Value!;

			var text = $@"
💰 {loc.GetMessage("balance_info", languageCode)}
{loc.GetMessage("current_balance", languageCode)}: <b>{profile.Balance:F2} {profile.Currency}</b>
🔗 {loc.GetMessage("linked_accounts", languageCode)}:
👤 Telegram: @{profile.TelegramUsername ?? "Not set"}
";

			var keyboard = new InlineKeyboardMarkup(new[]
			{
				new[]
				{
					InlineKeyboardButton.WithCallbackData(
						loc.GetMessage("back", languageCode),
						CallbackData.MenuMain)
				},
			});

			await botClient.SendMessage(
				chatId,
				text,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in BalanceCommandHandler");
			await SendErrorMessage(botClient, message.Chat.Id, "An error occurred", cancellationToken);
		}
	}

	private async Task SendErrorMessage(
		ITelegramBotClient botClient,
		long chatId,
		string error,
		CancellationToken ct)
	{
		await botClient.SendMessage(chatId, $"❌ {error}", cancellationToken: ct);
	}
}

