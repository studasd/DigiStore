using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers;

/// <summary>
/// Базовый класс для хэндлеров с общими методами
/// </summary>
public abstract class BaseHandler
{
	protected readonly ITelegramBotClient _botClient;
	protected readonly ILocalizationService _localizationService;

	protected BaseHandler(
		ITelegramBotClient botClient,
		ILocalizationService localizationService)
	{
		_botClient = botClient;
		_localizationService = localizationService;
	}

	/// <summary>
	/// Отправляет сообщение об ошибке
	/// </summary>
	protected async Task SendErrorMessage(long chatId, string error, CancellationToken cancellationToken = default)
	{
		await _botClient.SendMessage(chatId, $"❌ {error}", cancellationToken: cancellationToken);
	}


	/// <summary>
	/// Отправляет ответ на callback query с ошибкой
	/// </summary>
	protected async Task AnswerCallbackQueryWithError(string callbackQueryId, string languageCode, CancellationToken cancellationToken = default)
	{
		try
		{
			await _botClient.AnswerCallbackQuery(
				callbackQueryId,
				_localizationService.GetMessage("error_occurred", languageCode),
				showAlert: true,
				cancellationToken: cancellationToken);
		}
		catch { }
	}


	/// <summary>
	/// Отправляет выбор языка
	/// </summary>
	protected async Task SendLanguageSelection(long chatId, string currentLanguage, bool isStartCommand, CancellationToken cancellationToken = default)
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
			text = $"{_localizationService.GetMessage("greeting", currentLanguage)}\n\n" +
				   $"{_localizationService.GetMessage("select_language", currentLanguage)}";
		}
		else
		{
			text = _localizationService.GetMessage("select_language", currentLanguage);
		}

		await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
	}


	/// <summary>
	/// Создает клавиатуру выбора языка для EditMessageText
	/// </summary>
	protected InlineKeyboardMarkup GetLanguageSelectionKeyboard(string languageChangePrefix)
	{
		var languages = _localizationService.GetLanguages();
		var buttons = new List<List<InlineKeyboardButton>>();

		foreach (var lang in languages)
		{
			buttons.Add(new List<InlineKeyboardButton>
			{
				InlineKeyboardButton.WithCallbackData(
					lang.Value,
					$"{languageChangePrefix}{lang.Key}")
			});
		}

		return new InlineKeyboardMarkup(buttons);
	}

	/// <summary>
	/// Создает клавиатуру главного меню
	/// </summary>
	protected InlineKeyboardMarkup GetMainMenuKeyboard(string languageCode)
	{
		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localizationService.GetMessage("profile", languageCode),
					CallbackData.ProfileView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localizationService.GetMessage("balance", languageCode),
					CallbackData.BalanceView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localizationService.GetMessage("catalog", languageCode),
					CallbackData.CatalogView)
			},
		});
	}

	/// <summary>
	/// Создает клавиатуру профиля
	/// </summary>
	protected InlineKeyboardMarkup GetProfileKeyboard(string languageCode)
	{
		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localizationService.GetMessage("change_language", languageCode),
					CallbackData.LanguageChangePrefix + "select")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localizationService.GetMessage("main_menu", languageCode),
					CallbackData.MenuMain)
			},
		});
	}

	/// <summary>
	/// Создает клавиатуру с кнопкой "Назад" к главному меню
	/// </summary>
	protected InlineKeyboardMarkup GetBackToMainMenuKeyboard(string languageCode)
	{
		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localizationService.GetMessage("back", languageCode),
					CallbackData.MenuMain)
			},
		});
	}
}
