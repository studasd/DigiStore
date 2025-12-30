using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Infrastructure.Handlers;

/// <summary>
/// Handles inline button callbacks - language selection, menu navigation
/// </summary>
public class CallbackQueryHandler
{
	private readonly ITelegramUserService _userService;
	private readonly ITelegramWalletService _walletService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ITelegramProfileService _profileService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<CallbackQueryHandler> _logger;

	public CallbackQueryHandler(
		ITelegramUserService userService,
		ITelegramWalletService walletService,
		ITelegramSessionService sessionService,
		ITelegramProfileService profileService,
		ILocalizationService localizationService,
		ILogger<CallbackQueryHandler> logger)
	{
		_userService = userService;
		_walletService = walletService;
		_sessionService = sessionService;
		_profileService = profileService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task Handle(
		ITelegramBotClient botClient,
		Update update,
		CancellationToken ct)
	{
		var query = update.CallbackQuery;
		if (query?.Data == null || query.Message == null)
			return;

		try
		{
			var telegramId = query.From.Id;
			var session = await _sessionService.GetSessionAsync(telegramId, ct)
				?? throw new InvalidOperationException("Session not found");

			var data = query.Data;

			_logger.LogInformation(
				"Callback query from TelegramId: {TelegramId}, Data: {Data}",
				telegramId, data);

			// Handle language selection from /start
			if (data.StartsWith(CallbackData.LanguagePrefix))
			{
				await HandleLanguageSelectionFromStart(botClient, query, session, data, ct);
			}
			// Handle language change from /language
			else if (data.StartsWith(CallbackData.LanguageChangePrefix))
			{
				await HandleLanguageChange(botClient, query, session, data, ct);
			}
			// Handle profile view
			else if (data == CallbackData.ProfileView)
			{
				await HandleProfileView(botClient, query, session, ct);
			}
			// Handle balance view
			else if (data == CallbackData.BalanceView)
			{
				await HandleBalanceView(botClient, query, session, ct);
			}
			// Handle main menu
			else if (data == CallbackData.MenuMain)
			{
				await HandleMainMenu(botClient, query, session, ct);
			}

			// Answer callback query (remove loading state)
			await botClient.AnswerCallbackQuery(query.Id, cancellationToken: ct);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in CallbackQueryHandler");
			try
			{
				await botClient.AnswerCallbackQuery(
					query!.Id,
					_localizationService.GetMessage("error_occurred", "en"),
					showAlert: true,
					cancellationToken: ct);
			}
			catch { }
		}
	}

	/// <summary>
	/// Handle language selection from /start command
	/// </summary>
	private async Task HandleLanguageSelectionFromStart(
		ITelegramBotClient botClient,
		CallbackQuery query,
		TelegramUserSession session,
		string data,
		CancellationToken ct)
	{
		var languageCode = data.Replace(CallbackData.LanguagePrefix, "");

		if (!session.UserId.HasValue)
			return;

		_logger.LogInformation(
			"Language selected from /start: {LanguageCode}, UserId: {UserId}",
			languageCode, session.UserId);

		// Update user language in UserService
		var updateResult = await _profileService.UpdateUserLanguageAsync(
			session.UserId.Value,
			languageCode,
			ct);

		if (!updateResult.IsSuccess)
		{
			await botClient.AnswerCallbackQuery(
				query.Id,
				_localizationService.GetMessage("error_occurred", languageCode),
				showAlert: true,
				cancellationToken: ct);
			return;
		}

		// Update session
		session.LanguageCode = languageCode;
		session.SetState(BotState.LanguageSelected);
		await _sessionService.UpdateSessionAsync(session, ct);

		// Get full profile
		var profileResult = await _profileService.GetFullProfileAsync(
			session.UserId.Value,
			session.TelegramId,
			ct);

		if (!profileResult.IsSuccess)
		{
			await botClient.AnswerCallbackQuery(
				query.Id,
				_localizationService.GetMessage("error_occurred", languageCode),
				showAlert: true,
				cancellationToken: ct);
			return;
		}

		var profile = profileResult.Value!;

		// Cache profile in session
		session.CachedProfile = new CachedUserProfile
		{
			UserId = profile.UserId,
			TelegramId = profile.TelegramId,
			Email = profile.Email,
			FirstName = profile.FullName.Split(' ').FirstOrDefault() ?? string.Empty,
			LastName = profile.FullName.Split(' ').LastOrDefault() ?? string.Empty,
			TelegramUsername = profile.TelegramUsername,
			LanguageCode = profile.LanguageCode,
			IsActive = profile.IsActive,
			Roles = profile.Roles,
			Balance = profile.Balance,
			Currency = profile.Currency,
			CreatedAt = profile.CreatedAt,
			UpdatedAt = profile.UpdatedAt
		};
		session.SetState(BotState.ViewingProfile);
		await _sessionService.UpdateSessionAsync(session, ct);

		// Format and send profile
		var profileText = _profileService.FormatProfileText(profile, languageCode);
		var keyboard = GetProfileKeyboard(languageCode);

		await botClient.EditMessageText(
			query.Message!.Chat.Id,
			query.Message.MessageId,
			profileText,
			parseMode: ParseMode.Html,
			replyMarkup: keyboard,
			cancellationToken: ct);

		_logger.LogInformation(
			"Profile shown after language selection for user: {UserId}",
			session.UserId);
	}

	/// <summary>
	/// Handle language change from /language command
	/// </summary>
	private async Task HandleLanguageChange(
		ITelegramBotClient botClient,
		CallbackQuery query,
		TelegramUserSession session,
		string data,
		CancellationToken ct)
	{
		var languageCode = data.Replace(CallbackData.LanguageChangePrefix, "");
		var currentLanguage = session.LanguageCode ?? "en";

		// Handle "select" case - shows all languages
		if (languageCode == "select")
		{
			var languages = _localizationService.GetLanguages();
			var buttons = new List<List<InlineKeyboardButton>>();

			foreach (var lang in languages)
			{
				buttons.Add(new List<InlineKeyboardButton>
				{
					InlineKeyboardButton.WithCallbackData(
						lang.Value,
						$"{CallbackData.LanguageChangePrefix}{lang.Key}")
				});
			}

			var keyboard0 = new InlineKeyboardMarkup(buttons);
			var text = _localizationService.GetMessage("select_language", currentLanguage);

			await botClient.EditMessageText(
				query.Message!.Chat.Id,
				query.Message.MessageId,
				text,
				replyMarkup: keyboard0,
				cancellationToken: ct);

			return;
		}

		// Change language
		if (!session.UserId.HasValue)
			return;

		_logger.LogInformation(
			"Language changed: {OldLanguage} -> {NewLanguage}, UserId: {UserId}",
			currentLanguage, languageCode, session.UserId);

		// Update user language
		var updateResult = await _profileService.UpdateUserLanguageAsync(
			session.UserId.Value,
			languageCode,
			ct);

		if (!updateResult.IsSuccess)
		{
			await botClient.AnswerCallbackQuery(
				query.Id,
				_localizationService.GetMessage("error_occurred", currentLanguage),
				showAlert: true,
				cancellationToken: ct);
			return;
		}

		// Update session
		session.LanguageCode = languageCode;
		session.SetState(BotState.MainMenu);
		await _sessionService.UpdateSessionAsync(session, ct);

		var confirmText = _localizationService.GetMessage("language_changed", languageCode);
		var keyboard = GetMainMenuKeyboard(languageCode);

		await botClient.EditMessageText(
			query.Message!.Chat.Id,
			query.Message.MessageId,
			confirmText,
			replyMarkup: keyboard,
			cancellationToken: ct);

		_logger.LogInformation(
			"Language updated successfully for user: {UserId}",
			session.UserId);
	}

	/// <summary>
	/// Handle profile view callback
	/// </summary>
	private async Task HandleProfileView(
		ITelegramBotClient botClient,
		CallbackQuery query,
		TelegramUserSession session,
		CancellationToken ct)
	{
		if (!session.UserId.HasValue)
			return;

		var languageCode = session.LanguageCode ?? "en";

		var profileResult = await _profileService.GetFullProfileAsync(
			session.UserId.Value,
			session.TelegramId,
			ct);

		if (!profileResult.IsSuccess)
		{
			await botClient.AnswerCallbackQuery(
				query.Id,
				_localizationService.GetMessage("error_occurred", languageCode),
				showAlert: true,
				cancellationToken: ct);
			return;
		}

		var profile = profileResult.Value!;
		var profileText = _profileService.FormatProfileText(profile, languageCode);
		var keyboard = GetProfileKeyboard(languageCode);

		await botClient.EditMessageText(
			query.Message!.Chat.Id,
			query.Message.MessageId,
			profileText,
			parseMode: ParseMode.Html,
			replyMarkup: keyboard,
			cancellationToken: ct);
	}

	/// <summary>
	/// Handle balance view callback
	/// </summary>
	private async Task HandleBalanceView(
		ITelegramBotClient botClient,
		CallbackQuery query,
		TelegramUserSession session,
		CancellationToken ct)
	{
		if (!session.UserId.HasValue)
			return;

		var languageCode = session.LanguageCode ?? "en";
		var loc = _localizationService;

		var walletResult = await _walletService.GetBalanceAsync(session.UserId.Value, ct);

		if (!walletResult.IsSuccess)
		{
			await botClient.AnswerCallbackQuery(
				query.Id,
				loc.GetMessage("error_occurred", languageCode),
				showAlert: true,
				cancellationToken: ct);
			return;
		}

		var wallet = walletResult.Value!;
		var text = $@"
💰 {loc.GetMessage("balance_info", languageCode)}

{loc.GetMessage("current_balance", languageCode)}: <b>{wallet.Balance:F2} {wallet.Currency}</b>
📊 {loc.GetMessage("total_deposited", languageCode)}: {wallet.TotalDeposited:F2} {wallet.Currency}
📤 {loc.GetMessage("total_withdrawn", languageCode)}: {wallet.TotalWithdrawn:F2} {wallet.Currency}
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

		await botClient.EditMessageText(
			query.Message!.Chat.Id,
			query.Message.MessageId,
			text,
			parseMode: ParseMode.Html,
			replyMarkup: keyboard,
			cancellationToken: ct);
	}

	/// <summary>
	/// Handle main menu
	/// </summary>
	private async Task HandleMainMenu(
		ITelegramBotClient botClient,
		CallbackQuery query,
		TelegramUserSession session,
		CancellationToken ct)
	{
		var languageCode = session.LanguageCode ?? "en";
		var loc = _localizationService;

		var text = $"{loc.GetMessage("main_menu", languageCode)}\n\n" +
				  $"{loc.GetMessage("choose_option", languageCode)}";

		var keyboard = GetMainMenuKeyboard(languageCode);

		await botClient.EditMessageText(
			query.Message!.Chat.Id,
			query.Message.MessageId,
			text,
			replyMarkup: keyboard,
			cancellationToken: ct);
	}

	private InlineKeyboardMarkup GetMainMenuKeyboard(string languageCode)
	{
		var loc = _localizationService;

		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("profile", languageCode),
					CallbackData.ProfileView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("balance", languageCode),
					CallbackData.BalanceView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("catalog", languageCode),
					CallbackData.CatalogView)
			},
		});
	}

	private InlineKeyboardMarkup GetProfileKeyboard(string languageCode)
	{
		var loc = _localizationService;

		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("change_language", languageCode),
					CallbackData.LanguageChangePrefix + "select")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("main_menu", languageCode),
					CallbackData.MenuMain)
			},
		});
	}
}