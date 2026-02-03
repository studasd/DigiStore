using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces.Services;
using StudCoreKit.SharedKernel;
using StudTgBotApi.Contracts.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Adstracts;

/// <summary>
/// Базовый класс для хэндлеров с общими методами
/// </summary>
public abstract class BaseHandler
{
	protected readonly IBotAPIClient _botClient;
	protected readonly ILocalizationService _localService;

	protected BaseHandler(
		IBotAPIClient botClient,
		ILocalizationService localizationService)
	{
		_botClient = botClient;
		_localService = localizationService;
	}

	/// <summary>
	/// Отправляет сообщение об ошибке
	/// </summary>
	protected async Task<UnitResult<Error>> SendErrorMessage(long chatId, string error, CancellationToken token = default)
	{
		return await _botClient.SendMessageAsync(chatId, $"❌ {error}", cancellationToken: token);
	}


	/// <summary>
	/// Отправляет ответ на callback query с ошибкой
	/// </summary>
	protected async Task<UnitResult<Error>> AnswerCallbackQueryWithError(string callbackQueryId, LanguageCodes langCode, CancellationToken token = default)
	{
		return await _botClient.AnswerCallbackQueryAsync(
				callbackQueryId,
				_localService.GetMessage(LocalKeys.Errors.Occurred, langCode),
				showAlert: true,
				cancellationToken: token);
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
		string text = "";

		if (isStartCommand)
		{
			//👋 Добро пожаловать в PetFamily магазин!
			text = $"{_localService.GetMessage(LocalKeys.Messages.Welcome, currentLang)}\n\n";
		}

		//📍 Выберите язык:
		text = text + _localService.GetMessage(LocalKeys.Messages.SelectLanguage, currentLang);


		var sendResult = await _botClient.SendMessageAsync(chatId, text, replyMarkup: keyboard, cancellationToken: token);
		if(sendResult.IsFailure)
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
					_localService.GetMessage(LocalKeys.Buttons.Profile, langCode),
					CallbackData.ProfileCallback)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localService.GetMessage(LocalKeys.Buttons.Balance, langCode),
					CallbackData.BalanceCallback)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localService.GetMessage(LocalKeys.Buttons.Catalog, langCode),
					CallbackData.CatalogCallback)
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
					_localService.GetMessage(LocalKeys.Buttons.ChangeLanguage, langCode),
					CallbackData.LanguageChangePrefix + "select")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localService.GetMessage(LocalKeys.Buttons.MainMenu, langCode),
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
