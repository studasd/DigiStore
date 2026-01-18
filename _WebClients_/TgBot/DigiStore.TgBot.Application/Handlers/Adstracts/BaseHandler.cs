using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.UserService.Contracts.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Adstracts;

/// <summary>
/// Базовый класс для хэндлеров с общими методами
/// </summary>
public abstract class BaseHandler
{
	protected readonly ITelegramBotClient _botClient;
	protected readonly ILocalizationService _localService;

	protected BaseHandler(
		ITelegramBotClient botClient,
		ILocalizationService localizationService)
	{
		_botClient = botClient;
		_localService = localizationService;
	}

	/// <summary>
	/// Отправляет сообщение об ошибке
	/// </summary>
	protected async Task SendErrorMessage(long chatId, string error, CancellationToken token = default)
	{
		try
		{
			await _botClient.SendMessage(chatId, $"❌ {error}", cancellationToken: token);
		}
		catch {	}
	}


	/// <summary>
	/// Отправляет ответ на callback query с ошибкой
	/// </summary>
	protected async Task AnswerCallbackQueryWithError(string callbackQueryId, LanguageCodes langCode, CancellationToken token = default)
	{
		try
		{
			await _botClient.AnswerCallbackQuery(
				callbackQueryId,
				_localService.GetMessage(LocalKeys.Errors.Occurred, langCode),
				showAlert: true,
				cancellationToken: token);
		}
		catch { }
	}


	/// <summary>
	/// Отправляет выбор языка
	/// </summary>
	protected async Task<UnitResult<Error>> SendLanguageSelection(long chatId, LanguageCodes currentLang, bool isStartCommand, CancellationToken token = default)
	{
		var languages = _localService.GetLanguages();
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
			text = $"{_localService.GetMessage(LocalKeys.Greetings.Greeting, currentLang)}\n\n" +
				   $"{_localService.GetMessage(LocalKeys.Navigations.SelectLanguage, currentLang)}";
		}
		else
		{
			text = _localService.GetMessage(LocalKeys.Navigations.SelectLanguage, currentLang);
		}


		try
		{
			await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: token);
		}
		catch(Exception ex)
		{
			await SendErrorMessage(chatId, "An error occurred", token);
			return Error.Failure("bot.send.error", "Failed to send language selection message");
		}

		return Result.Success<Error>();
	}


	/// <summary>
	/// Создает клавиатуру выбора языка для EditMessageText
	/// </summary>
	protected InlineKeyboardMarkup GetLanguageSelectionKeyboard(string languageChangePrefix)
	{
		var languages = _localService.GetLanguages();
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
	protected InlineKeyboardMarkup GetMainMenuKeyboard(LanguageCodes langCode)
	{
		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localService.GetMessage(LocalKeys.Commands.Profile, langCode),
					CallbackData.ProfileView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localService.GetMessage(LocalKeys.Commands.Balance, langCode),
					CallbackData.BalanceView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localService.GetMessage(LocalKeys.Commands.Catalog, langCode),
					CallbackData.CatalogView)
			},
		});
	}

	/// <summary>
	/// Создает клавиатуру профиля
	/// </summary>
	protected InlineKeyboardMarkup GetProfileKeyboard(LanguageCodes langCode)
	{
		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localService.GetMessage(LocalKeys.Commands.ChangeLanguage, langCode),
					CallbackData.LanguageChangePrefix + "select")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localService.GetMessage(LocalKeys.Navigations.MainMenu, langCode),
					CallbackData.MenuMain)
			},
		});
	}

	/// <summary>
	/// Создает клавиатуру с кнопкой "Назад" к главному меню
	/// </summary>
	protected InlineKeyboardMarkup GetBackToMainMenuKeyboard(LanguageCodes langCode)
	{
		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localService.GetMessage(LocalKeys.Buttons.BalanceUpYookassa, langCode), CallbackData.BalanceTopPrefix + PaymentAggregators.YooKassa),
				
				InlineKeyboardButton.WithCallbackData(
					_localService.GetMessage(LocalKeys.Buttons.BalanceUpFreekassa, langCode), CallbackData.BalanceTopPrefix + PaymentAggregators.FreeKassa)
			},

			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localService.GetMessage(LocalKeys.Buttons.Back, langCode), CallbackData.MenuMain)
			},
		});
	}
}
