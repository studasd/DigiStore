using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Domain;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Infrastructure.Handlers;


/// <summary>
/// Handles inline button callbacks
/// </summary>
public class CallbackQueryHandlerOLD
{
	private readonly ITelegramUserService _userService;
	private readonly ITelegramWalletService _walletService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<CallbackQueryHandlerOLD> _logger;

	public CallbackQueryHandlerOLD(
		ITelegramUserService userService,
		ITelegramWalletService walletService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<CallbackQueryHandlerOLD> logger)
	{
		_userService = userService;
		_walletService = walletService;
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task Handle(
		ITelegramBotClient botClient,
		Update update,
		CancellationToken ct)
	{
		var query = update.CallbackQuery;
		if (query?.Data == null)
			return;

		try
		{
			var telegramId = query.From.Id;
			var session = await _sessionService.GetSessionAsync(telegramId, ct)
				?? throw new InvalidOperationException("Session not found");

			var data = query.Data;
			var loc = _localizationService;

			if (data.StartsWith(CallbackData.LanguagePrefix))
			{
				await HandleLanguageSelection(botClient, query, session, data, ct);
			}
			else if (data == CallbackData.ProfileView)
			{
				await HandleProfileView(botClient, query, session, ct);
			}
			else if (data == CallbackData.BalanceView)
			{
				await HandleBalanceView(botClient, query, session, ct);
			}
			else if (data == CallbackData.MenuMain)
			{
				await HandleMainMenu(botClient, query, session, ct);
			}

			// Answer callback query
			await botClient.AnswerCallbackQuery(query.Id, cancellationToken: ct);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in CallbackQueryHandler");
			await botClient.AnswerCallbackQuery(
				query!.Id,
				_localizationService.GetMessage("error_occurred", "en"),
				showAlert: true,
				cancellationToken: ct);
		}
	}

	private async Task HandleLanguageSelection(
		ITelegramBotClient botClient,
		CallbackQuery query,
		TelegramUserSession session,
		string data,
		CancellationToken ct)
	{
		var languageCode = data.Replace(CallbackData.LanguagePrefix, "");

		// Update user language in UserService
		if (session.UserId.HasValue)
		{
			await _userService.UpdateLanguageAsync(session.UserId.Value, languageCode, ct);
		}

		// Update session
		session.LanguageCode = languageCode;
		session.SetState(BotState.MainMenu);
		await _sessionService.UpdateSessionAsync(session, ct);

		var loc = _localizationService;
		var message = loc.GetMessage("language_changed", languageCode);

		// Edit message with main menu
		await botClient.EditMessageText(
			query.Message!.Chat.Id,
			query.Message.MessageId,
			message,
			replyMarkup: GetMainMenuKeyboard(languageCode),
			cancellationToken: ct);
	}

	private async Task HandleProfileView(
		ITelegramBotClient botClient,
		CallbackQuery query,
		TelegramUserSession session,
		CancellationToken ct)
	{
		if (!session.UserId.HasValue)
			return;

		var userResult = await _userService.GetUserProfileAsync(session.UserId.Value, ct);
		if (!userResult.IsSuccess)
		{
			await botClient.AnswerCallbackQuery(
				query.Id,
				_localizationService.GetMessage("error_occurred", session.LanguageCode),
				showAlert: true,
				cancellationToken: ct);
			return;
		}

		var user = userResult.Value!;
		var loc = _localizationService;
		var lang = session.LanguageCode ?? "en";

		var text = $@"
{loc.GetMessage("profile_info", lang)}

{loc.GetMessage("email", lang).Replace("{0}", user.Email)}
{loc.GetMessage("full_name", lang).Replace("{0}", user.FullName)}
{loc.GetMessage("telegram_username", lang).Replace("{0}", user.TelegramUsername ?? "Not set")}
{loc.GetMessage("user_roles", lang).Replace("{0}", string.Join(", ", user.Roles))}
";

		await botClient.EditMessageText(
			query.Message!.Chat.Id,
			query.Message.MessageId,
			text,
			replyMarkup: GetBackKeyboard(lang),
			cancellationToken: ct);
	}

	private async Task HandleBalanceView(
		ITelegramBotClient botClient,
		CallbackQuery query,
		TelegramUserSession session,
		CancellationToken ct)
	{
		if (!session.UserId.HasValue)
			return;

		var walletResult = await _walletService.GetBalanceAsync(session.UserId.Value, ct);
		if (!walletResult.IsSuccess)
		{
			await botClient.AnswerCallbackQuery(
				query.Id,
				_localizationService.GetMessage("error_occurred", session.LanguageCode),
				showAlert: true,
				cancellationToken: ct);
			return;
		}

		var wallet = walletResult.Value!;
		var loc = _localizationService;
		var lang = session.LanguageCode ?? "en";

		var text = $@"
{loc.GetMessage("balance_info", lang)}

{loc.GetMessage("current_balance", lang).Replace("{0}", wallet.Balance.ToString("F2")).Replace("{1}", wallet.Currency)}
{loc.GetMessage("total_deposited", lang).Replace("{0}", wallet.TotalDeposited.ToString("F2")).Replace("{1}", wallet.Currency)}
{loc.GetMessage("total_withdrawn", lang).Replace("{0}", wallet.TotalWithdrawn.ToString("F2")).Replace("{1}", wallet.Currency)}
";

		await botClient.EditMessageText(
			query.Message!.Chat.Id,
			query.Message.MessageId,
			text,
			replyMarkup: GetBackKeyboard(lang),
			cancellationToken: ct);
	}

	private async Task HandleMainMenu(
		ITelegramBotClient botClient,
		CallbackQuery query,
		TelegramUserSession session,
		CancellationToken ct)
	{
		var lang = session.LanguageCode ?? "en";
		var loc = _localizationService;

		await botClient.EditMessageText(
			query.Message!.Chat.Id,
			query.Message.MessageId,
			loc.GetMessage("main_menu", lang),
			replyMarkup: GetMainMenuKeyboard(lang),
			cancellationToken: ct);
	}

	private InlineKeyboardMarkup GetMainMenuKeyboard(string languageCode)
	{
		var loc = _localizationService;
		var lang = languageCode;

		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("profile", lang),
					CallbackData.ProfileView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("balance", lang),
					CallbackData.BalanceView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("catalog", lang),
					CallbackData.CatalogView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("orders", lang),
					CallbackData.OrderHistory)
			},
		});
	}

	private InlineKeyboardMarkup GetBackKeyboard(string languageCode)
	{
		var loc = _localizationService;
		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("back", languageCode),
					CallbackData.MenuMain)
			},
		});
	}
}
