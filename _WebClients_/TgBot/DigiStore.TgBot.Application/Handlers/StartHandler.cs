using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers;


/// <summary>
/// Handles /start command and language selection
/// </summary>
public class StartHandler
{
	private readonly ITelegramUserService _userService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<StartHandler> _logger;

	public StartHandler(
		ITelegramUserService userService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<StartHandler> logger)
	{
		_userService = userService;
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task Handle(ITelegramBotClient botClient, Update update, CancellationToken ct)
	{
		var message = update.Message;
		if (message?.Text == null)
			return;

		try
		{
			var telegramId = message.From!.Id;
			_logger.LogInformation("Start command from Telegram ID: {TelegramId}", telegramId);

			// Get or create user
			var userResult = await _userService.GetOrCreateUserAsync(
								telegramId,
								message.From.Username,
								message.From.FirstName,
								message.From.LastName,
								message.From.LanguageCode ?? "en",
								ct);

			if (!userResult.IsSuccess)
			{
				await SendErrorMessage(botClient, message.Chat.Id, "Failed to create user", ct);
				return;
			}

			// Get or create session
			var session = await _sessionService.GetOrCreateSessionAsync(telegramId, ct);
			session.UserId = userResult.Value!.Id;
			session.LanguageCode = userResult.Value.LanguageCode;

			// Send language selection
			await SendLanguageSelection(botClient, message.Chat.Id, session.LanguageCode, ct);

			// Update session
			session.SetState(BotState.AwaitingLanguageSelection);
			await _sessionService.UpdateSessionAsync(session, ct);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in StartHandler");
			await SendErrorMessage(botClient, update.Message!.Chat.Id, "An error occurred", ct);
		}
	}


	private async Task SendLanguageSelection(
		ITelegramBotClient botClient,
		long chatId,
		string currentLanguage,
		CancellationToken ct)
	{
		var languages = _localizationService.GetLanguages();

		var buttons = new List<List<InlineKeyboardButton>>();
		foreach (var lang in languages)
		{
			buttons.Add(new List<InlineKeyboardButton>
			{
				InlineKeyboardButton.WithCallbackData(
					lang.Value,
					$"{CallbackData.LanguagePrefix}{lang.Key}")
			});
		}

		var keyboard = new InlineKeyboardMarkup(buttons);
		var text = _localizationService.GetMessage("greeting", currentLanguage);

		await botClient.SendMessage(
			chatId,
			text + "\n\n" + _localizationService.GetMessage("select_language", currentLanguage),
			replyMarkup: keyboard,
			cancellationToken: ct);
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
