using DigiStore.TgBot.Application.Interfaces;

namespace DigiStore.TgBot.Infrastructure;


public class LocalizationServiceOLD : ILocalizationService
{
	private readonly Dictionary<string, Dictionary<string, string>> _localizations;

	public LocalizationServiceOLD()
	{
		_localizations = InitializeLocalizations();
	}

	public string GetMessage(string key, string languageCode = "en")
	{
		languageCode = languageCode.ToLower();

		if (!_localizations.ContainsKey(languageCode))
			languageCode = "en";

		if (_localizations[languageCode].TryGetValue(key, out var message))
			return message;

		return key; // Return key if translation not found
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

	private Dictionary<string, Dictionary<string, string>> InitializeLocalizations()
	{
		return new Dictionary<string, Dictionary<string, string>>
		{
			{
				"en", new Dictionary<string, string>
				{
					{ "greeting", "👋 Welcome to PetFamily Store!" },
					{ "select_language", "📍 Please select your language:" },
					{ "main_menu", "🏠 Main Menu" },
					{ "profile", "👤 Profile" },
					{ "balance", "💰 Balance" },
					{ "catalog", "🛍️ Catalog" },
					{ "orders", "📦 My Orders" },
					{ "settings", "⚙️ Settings" },
					{ "help", "❓ Help" },
					{ "language_changed", "✅ Language changed to English" },
					{ "profile_info", "👤 Your Profile" },
					{ "email", "Email: {0}" },
					{ "full_name", "Name: {0}" },
					{ "telegram_username", "Telegram: @{0}" },
					{ "user_roles", "Roles: {0}" },
					{ "balance_info", "💰 Balance" },
					{ "current_balance", "Current Balance: {0} {1}" },
					{ "total_deposited", "Total Deposited: {0} {1}" },
					{ "total_withdrawn", "Total Withdrawn: {0} {1}" },
					{ "insufficient_balance", "❌ Insufficient balance" },
					{ "error_occurred", "❌ An error occurred: {0}" },
					{ "back", "← Back" },
					{ "cancel", "❌ Cancel" },
					{ "ok", "✅ OK" },
					{ "yes", "Yes" },
					{ "no", "No" }
				}
			},
			{
				"ru", new Dictionary<string, string>
				{
					{ "greeting", "👋 Добро пожаловать в PetFamily Store!" },
					{ "select_language", "📍 Выберите язык:" },
					{ "main_menu", "🏠 Главное меню" },
					{ "profile", "👤 Профиль" },
					{ "balance", "💰 Баланс" },
					{ "catalog", "🛍️ Каталог" },
					{ "orders", "📦 Мои заказы" },
					{ "settings", "⚙️ Настройки" },
					{ "help", "❓ Помощь" },
					{ "language_changed", "✅ Язык изменён на Русский" },
					{ "profile_info", "👤 Ваш профиль" },
					{ "email", "Email: {0}" },
					{ "full_name", "Имя: {0}" },
					{ "telegram_username", "Telegram: @{0}" },
					{ "user_roles", "Роли: {0}" },
					{ "balance_info", "💰 Баланс" },
					{ "current_balance", "Текущий баланс: {0} {1}" },
					{ "total_deposited", "Всего пополнено: {0} {1}" },
					{ "total_withdrawn", "Всего снято: {0} {1}" },
					{ "insufficient_balance", "❌ Недостаточно средств" },
					{ "error_occurred", "❌ Произошла ошибка: {0}" },
					{ "back", "← Назад" },
					{ "cancel", "❌ Отмена" },
					{ "ok", "✅ ОК" },
					{ "yes", "Да" },
					{ "no", "Нет" }
				}
			}
		};
	}
}
