using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.UserService.Contracts.Enums;
using DigiStore.TgBot.Application.Constants;

namespace DigiStore.TgBot.Application.Services;

public class LocalizationService : ILocalizationService
{
	private readonly Dictionary<LanguageCodes, Dictionary<string, string>> _localizations;
	public LocalizationService()
	{
		_localizations = InitializeLocalizations();
	}
	public string GetMessage(string key, LanguageCodes languageCode = LanguageCodes.en)
	{
		if (!_localizations.ContainsKey(languageCode))
			languageCode = LanguageCodes.en;

		if (_localizations[languageCode].TryGetValue(key, out var message))
			return message;

		_localizations[languageCode].TryGetValue(key, out var fallback);

		return fallback ?? key;
	}
	public Dictionary<string, string> GetLanguages()
	{
		return new Dictionary<string, string>
		{
			{ "en", "🇬🇧 English" },
			{ "ru", "🇷🇺 Русский" },
			{ "es", "🇪🇸 Español" },
			{ "de", "🇩🇪 Deutsch" },
			{ "fr", "🇫🇷 Français" }
		};
	}
	private Dictionary<LanguageCodes, Dictionary<string, string>> InitializeLocalizations()
	{
		return new Dictionary<LanguageCodes, Dictionary<string, string>>
		{
			{
				LanguageCodes.en, new Dictionary<string, string>
				{
					// Greeting & Navigation
					{ LocalKeys.Greetings.Greeting, "👋 Welcome to PetFamily Store!" },

					{ LocalKeys.Navigations.SelectLanguage, "📍 Please select your language:" },
					{ LocalKeys.Navigations.LanguageChanged, "✅ Language changed to English" },
					{ LocalKeys.Navigations.MainMenu, "🏠 Main Menu" },
					{ LocalKeys.Navigations.ChooseOption, "Choose an option below:" },
					// Commands
					{ LocalKeys.Commands.Profile, "👤 Profile" },
					{ LocalKeys.Commands.Balance, "💰 Balance" },
					{ LocalKeys.Commands.Catalog, "🛍️Catalog" },
					{ LocalKeys.Commands.Orders, "📦 My Orders" },
					{ LocalKeys.Commands.Settings, "⚙️ Settings" },
					{ LocalKeys.Commands.Help, "❓ Help" },
					{ LocalKeys.Commands.ChangeLanguage, "🌐 Change Language" },
					// Profile
					{ LocalKeys.Profiles.Info, "YOUR PROFILE" },
					{ LocalKeys.Profiles.Email, "Email" },
					{ LocalKeys.Profiles.FullName, "Name" },
					{ LocalKeys.Profiles.Username, "Telegram" },
					{ LocalKeys.Profiles.UserRoles, "Roles" },
					{ LocalKeys.Profiles.Status, "Status" },
					{ LocalKeys.Profiles.Roles, "Roles" },
					{ LocalKeys.Profiles.CreatedAt, "Registered" },
					{ LocalKeys.Profiles.UpdatedAt, "Last Updated" },
					{ LocalKeys.Profiles.Language, "Language" },
					// Balance
					{ LocalKeys.Balances.Info, "WALLET BALANCE" },
					{ LocalKeys.Balances.CurrentBalance, "Current Balance" },
					{ LocalKeys.Balances.TotalDeposited, "Total Deposited" },
					{ LocalKeys.Balances.TotalWithdrawn, "Total Withdrawn" },
					{ LocalKeys.Balances.LinkedAccounts, "Linked Accounts" },
					{ LocalKeys.Balances.InsufficientBalance, "❌ Insufficient balance" },
					// Buttons
					{ LocalKeys.Buttons.Back, "← Back" },
					{ LocalKeys.Buttons.Cancel, "❌ Cancel" },
					{ LocalKeys.Buttons.Ok, "✅ OK" },
					{ LocalKeys.Buttons.Yes, "Yes" },
					{ LocalKeys.Buttons.No, "No" },
					// Errors
					{ LocalKeys.Errors.Occurred, "❌ An error occurred" },
					{ LocalKeys.Errors.SessionExpired, "⏰ Session expired. Please use /start" },
					{ LocalKeys.Errors.UserNotFound, "👤 User not found" },
					{ LocalKeys.Errors.OperationFailed, "❌ Operation failed" }
				}
			},
			{
				LanguageCodes.ru, new Dictionary<string, string>
				{
					// Greeting & Navigation
					{ LocalKeys.Greetings.Greeting, "👋 Добро пожаловать в PetFamily Store!" },
					{ LocalKeys.Navigations.SelectLanguage, "📍 Выберите язык:" },
					{ LocalKeys.Navigations.LanguageChanged, "✅ Язык изменён на Русский" },
					{ LocalKeys.Navigations.MainMenu, "🏠 Главное меню" },
					{ LocalKeys.Navigations.ChooseOption, "Выберите опцию:" },
					// Commands
					{ LocalKeys.Commands.Profile, "👤 Профиль" },
					{ LocalKeys.Commands.Balance, "💰 Баланс" },
					{ LocalKeys.Commands.Catalog, "🛍️Каталог" },
					{ LocalKeys.Commands.Orders, "📦 Мои заказы" },
					{ LocalKeys.Commands.Settings, "⚙️ Настройки" },
					{ LocalKeys.Commands.Help, "❓ Помощь" },
					{ LocalKeys.Commands.ChangeLanguage, "🌐 Изменить язык" },
					// Profile
					{ LocalKeys.Profiles.Info, "ВАШ ПРОФИЛЬ" },
					{ LocalKeys.Profiles.Email, "Email" },
					{ LocalKeys.Profiles.FullName, "Имя" },
					{ LocalKeys.Profiles.Username, "Telegram" },
					{ LocalKeys.Profiles.UserRoles, "Роли" },
					{ LocalKeys.Profiles.Status, "Статус" },
					{ LocalKeys.Profiles.Roles, "Роли" },
					{ LocalKeys.Profiles.CreatedAt, "Дата регистрации" },
					{ LocalKeys.Profiles.UpdatedAt, "Последнее обновление" },
					{ LocalKeys.Profiles.Language, "Язык" },
					// Balance
					{ LocalKeys.Balances.Info, "БАЛАНС КОШЕЛЬКА" },
					{ LocalKeys.Balances.CurrentBalance, "Текущий баланс" },
					{ LocalKeys.Balances.TotalDeposited, "Всего пополнено" },
					{ LocalKeys.Balances.TotalWithdrawn, "Всего снято" },
					{ LocalKeys.Balances.LinkedAccounts, "Привязанные аккаунты" },
					{ LocalKeys.Balances.InsufficientBalance, "❌ Недостаточно средств" },
					// Buttons
					{ LocalKeys.Buttons.Back, "← Назад" },
					{ LocalKeys.Buttons.Cancel, "❌ Отмена" },
					{ LocalKeys.Buttons.Ok, "✅ ОК" },
					{ LocalKeys.Buttons.Yes, "Да" },
					{ LocalKeys.Buttons.No, "Нет" },
					// Errors
					{ LocalKeys.Errors.Occurred, "❌ Произошла ошибка" },
					{ LocalKeys.Errors.SessionExpired, "⏰ Сессия истекла. Используйте /start" },
					{ LocalKeys.Errors.UserNotFound, "👤 Пользователь не найден" },
					{ LocalKeys.Errors.OperationFailed, "❌ Операция не удалась" }
				}
			}
		};
	}
}
