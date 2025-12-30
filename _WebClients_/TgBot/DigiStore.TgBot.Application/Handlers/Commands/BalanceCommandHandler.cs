using DigiStore.TgBot.Application.Constants;
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
public class BalanceCommandHandler : ICommandHandler
{
	public const string Command = BotCommands.Balance;
	
	private readonly ITelegramBotClient _botClient;
	private readonly ITelegramProfileService _profileService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<BalanceCommandHandler> _logger;

	string ICommandHandler.Command => Command;

	public BalanceCommandHandler(
		ITelegramBotClient botClient,
		ITelegramProfileService profileService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<BalanceCommandHandler> logger)
	{
		_botClient = botClient;
		_profileService = profileService;
		_sessionService = sessionService;
		_localizationService = localizationService;
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
			var loc = _localizationService;

			var profileResult = await _profileService.GetFullProfileAsync(session.UserId.Value, telegramId, cancellationToken);
			if (!profileResult.IsSuccess)
			{
				await SendErrorMessage(chatId, loc.GetMessage("error_occurred", languageCode), cancellationToken);
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

	private async Task SendErrorMessage(
		long chatId,
		string error,
		CancellationToken ct)
	{
		await _botClient.SendMessage(chatId, $"❌ {error}", cancellationToken: ct);
	}
}
