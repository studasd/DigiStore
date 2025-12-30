using DigiStore.TgBot.Application.Constants;
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

namespace DigiStore.TgBot.Application.Handlers;

/// <summary>
/// Обрабатывает основные команды: /start, /profile, /language, /balance
/// </summary>
public class CommandHandler
{
	private readonly ITelegramUserService _userService;
	private readonly ITelegramProfileService _profileService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<CommandHandler> _logger;
	public CommandHandler(
		ITelegramUserService userService,
		ITelegramProfileService profileService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<CommandHandler> logger)
	{
		_userService = userService;
		_profileService = profileService;
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}
	/// <summary>
	/// Handle /start command
	/// 1. Check if user exists
	/// 2. Create if not
	/// 3. Ask for language
	/// 4. Show profile after language selection
	/// </summary>
	public async Task HandleStartCommand(ITelegramBotClient botClient, Message message, CancellationToken ct)
	{
		try
		{
			var telegramId = message.From!.Id;
			var username = message.From.Username;
			var firstName = message.From.FirstName;
			var lastName = message.From.LastName;
			var defaultLanguage = message.From.LanguageCode ?? "en";
			_logger.LogInformation("Start command from Telegram ID: {TelegramId}, Name: {FirstName} {LastName}",
				telegramId, firstName, lastName);

			// Get or create user
			var userResult = await _userService.GetOrCreateUserAsync(
									telegramId,
									username,
									firstName,
									lastName,
									defaultLanguage,
									ct);

			if (!userResult.IsSuccess)
			{
				await SendErrorMessage(
					botClient,
					message.Chat.Id,
					"Failed to initialize user account",
					ct);
				return;
			}

			var user = userResult.Value!;
			// Get or create session
			var session = await _sessionService.GetOrCreateSessionAsync(telegramId, ct);
			session.UserId = user.Id;
			session.LanguageCode = user.LanguageCode;

			// Send greeting and language selection
			await SendLanguageSelection(
				botClient,
				message.Chat.Id,
				session.LanguageCode,
				isStartCommand: true,
				ct);

			// Update session - waiting for language selection
			session.SetState(BotState.AwaitingLanguageSelection);

			await _sessionService.UpdateSessionAsync(session, ct);

			_logger.LogInformation("User initialized: TelegramId: {TelegramId}, UserId: {UserId}",
			telegramId, user.Id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in HandleStartCommand");
			await SendErrorMessage(botClient, message.Chat.Id, "An error occurred", ct);
		}
	}


	/// <summary>
	/// Handle /profile command - show user profile with balance
	/// </summary>
	public async Task HandleProfileCommand(
	ITelegramBotClient botClient,
	Message message,
	CancellationToken ct)
	{
		try
		{
			var telegramId = message.From!.Id;
			var chatId = message.Chat.Id;

			_logger.LogInformation("Profile command from Telegram ID: {TelegramId}", telegramId);

			// Get session
			var session = await _sessionService.GetSessionAsync(telegramId, ct);
			if (session?.UserId == null)
			{
				await SendErrorMessage(
					botClient,
					chatId,
					_localizationService.GetMessage("session_expired", "en"),
					ct);
				return;
			}
			var userId = session.UserId.Value;
			var languageCode = session.LanguageCode ?? "en";

			// Get full profile
			var profileResult = await _profileService.GetFullProfileAsync(
									userId,
									telegramId,
									ct);
			if (!profileResult.IsSuccess)
			{
				await SendErrorMessage(
					botClient,
					chatId,
					_localizationService.GetMessage("error_occurred", languageCode),
					ct);
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
			await botClient.SendMessage(
				chatId,
				profileText,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: ct);

			_logger.LogInformation("Profile sent for user: {UserId}, TelegramId: {TelegramId}",
			userId, telegramId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in HandleProfileCommand");
			await SendErrorMessage(botClient, message.Chat.Id, "An error occurred", ct);
		}
	}
	/// <summary>
	/// Handle /language command - change language
	/// </summary>


	public async Task HandleLanguageCommand(
		ITelegramBotClient botClient,
		Message message,
		CancellationToken ct)
	{
		try
		{
			var telegramId = message.From!.Id;
			var chatId = message.Chat.Id;
			_logger.LogInformation("Language command from Telegram ID: {TelegramId}", telegramId, chatId);

			// Get session
			var session = await _sessionService.GetSessionAsync(telegramId, ct);
			if (session?.UserId == null)
			{
				await SendErrorMessage(
					botClient,
					chatId,
					_localizationService.GetMessage("session_expired", "en"),
					ct);
				return;
			}
			var currentLanguage = session.LanguageCode ?? "en";

			// Send language selection
			await SendLanguageSelection(
				botClient,
				chatId,
				currentLanguage,
				isStartCommand: false,
				ct);

			// Update session
			session.SetState(BotState.AwaitingLanguageChange);

			await _sessionService.UpdateSessionAsync(session, ct);

			_logger.LogInformation("Language selection sent for user: {TelegramId}", telegramId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in HandleLanguageCommand");
			await SendErrorMessage(botClient, message.Chat.Id, "An error occurred", ct);
		}
	}
	/// <summary>
	/// Handle /balance command - show wallet info
	/// </summary>


	public async Task HandleBalanceCommand(
		ITelegramBotClient botClient,
		Message message,
		CancellationToken ct)
	{
		try
		{
			var telegramId = message.From!.Id;
			var chatId = message.Chat.Id;

			// Get session
			var session = await _sessionService.GetSessionAsync(telegramId, ct);
			if (session?.UserId == null)
			{
				await SendErrorMessage(botClient, chatId, "Session expired", ct);
				return;
			}
			var languageCode = session.LanguageCode ?? "en";
			var loc = _localizationService;
			var profileResult = await _profileService.GetFullProfileAsync(
									session.UserId.Value,
									telegramId,
									ct);
			if (!profileResult.IsSuccess)
			{
				await SendErrorMessage(botClient, chatId, loc.GetMessage("error_occurred", languageCode), ct);
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
				cancellationToken: ct);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in HandleBalanceCommand");
			await SendErrorMessage(botClient, message.Chat.Id, "An error occurred", ct);
		}
	}


	private async Task SendLanguageSelection(
		ITelegramBotClient botClient,
		long chatId,
		string currentLanguage,
		bool isStartCommand,
		CancellationToken ct)
	{
		var languages = _localizationService.GetLanguages();
		var buttons = new List<List<InlineKeyboardButton>>();
		foreach (var lang in languages)
		{
			var callbackData = isStartCommand
								? $"{CallbackData.LanguagePrefix}{lang.Key}"
								: $"{CallbackData.LanguageChangePrefix}{lang.Key}";

			buttons.Add(new List<InlineKeyboardButton>
			{
				InlineKeyboardButton.WithCallbackData(lang.Value, callbackData)
			});
		}
		
		var keyboard = new InlineKeyboardMarkup(buttons);
		string text;
		if (isStartCommand)
		{
			text =	$"{_localizationService.GetMessage("greeting", currentLanguage)}\n\n" +
					$"{_localizationService.GetMessage("select_language", currentLanguage)}";
		}
		else
		{
			text = $"{_localizationService.GetMessage("select_language", currentLanguage)}";
		}

		await botClient.SendMessage(
			chatId,
			text,
			replyMarkup: keyboard,
			cancellationToken: ct);
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

	private async Task SendErrorMessage(
		ITelegramBotClient botClient,
		long chatId,
		string error,
		CancellationToken ct)
	{
		await botClient.SendMessage(
			chatId,
			$"❌ {error}",
			cancellationToken: ct);
	}
}