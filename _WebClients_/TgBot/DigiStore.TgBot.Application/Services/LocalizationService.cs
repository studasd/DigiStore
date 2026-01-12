using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.UserService.Contracts.Enums;

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
					{ "greeting", "👋 Welcome to PetFamily Store!" },
					{ "select_language", "📍 Please select your language:" },
					{ "language_changed", "✅ Language changed to English" },
					{ "main_menu", "🏠 Main Menu" },
					{ "choose_option", "Choose an option below:" },
					// Commands
					{ "profile", "👤 Profile" },
					{ "balance", "💰 Balance" },
					{ "catalog", "🛍️Catalog" },
					{ "orders", "📦 My Orders" },
					{ "settings", "⚙️ Settings" },
					{ "help", "❓ Help" },
					{ "change_language", "🌐 Change Language" },
					// Profile
					{ "profile_info", "YOUR PROFILE" },
					{ "email", "Email" },
					{ "full_name", "Name" },
					{ "telegram_username", "Telegram" },
					{ "user_roles", "Roles" },
					{ "status", "Status" },
					{ "roles", "Roles" },
					{ "created_at", "Registered" },
					{ "updated_at", "Last Updated" },
					{ "language", "Language" },
					// Balance
					{ "balance_info", "WALLET BALANCE" },
					{ "current_balance", "Current Balance" },
					{ "total_deposited", "Total Deposited" },
					{ "total_withdrawn", "Total Withdrawn" },
					{ "linked_accounts", "Linked Accounts" },
					{ "insufficient_balance", "❌ Insufficient balance" },
					// Buttons
					{ "back", "← Back" },
					{ "cancel", "❌ Cancel" },
					{ "ok", "✅ OK" },
					{ "yes", "Yes" },
					{ "no", "No" },
					// Errors
					{ "error_occurred", "❌ An error occurred" },
					{ "session_expired", "⏰ Session expired. Please use /start" },
					{ "user_not_found", "👤 User not found" },
					{ "operation_failed", "❌ Operation failed" }
				}
			},
			{
				LanguageCodes.ru, new Dictionary<string, string>
				{
					// Greeting & Navigation
					{ "greeting", "👋 Добро пожаловать в PetFamily Store!" },
					{ "select_language", "📍 Выберите язык:" },
					{ "language_changed", "✅ Язык изменён на Русский" },
					{ "main_menu", "🏠 Главное меню" },
					{ "choose_option", "Выберите опцию:" },
					// Commands
					{ "profile", "👤 Профиль" },
					{ "balance", "💰 Баланс" },
					{ "catalog", "🛍️Каталог" },
					{ "orders", "📦 Мои заказы" },
					{ "settings", "⚙️ Настройки" },
					{ "help", "❓ Помощь" },
					{ "change_language", "🌐 Изменить язык" },
					// Profile
					{ "profile_info", "ВАШ ПРОФИЛЬ" },
					{ "email", "Email" },
					{ "full_name", "Имя" },
					{ "telegram_username", "Telegram" },
					{ "user_roles", "Роли" },
					{ "status", "Статус" },
					{ "roles", "Роли" },
					{ "created_at", "Дата регистрации" },
					{ "updated_at", "Последнее обновление" },
					{ "language", "Язык" },
					// Balance
					{ "balance_info", "БАЛАНС КОШЕЛЬКА" },
					{ "current_balance", "Текущий баланс" },
					{ "total_deposited", "Всего пополнено" },
					{ "total_withdrawn", "Всего снято" },
					{ "linked_accounts", "Привязанные аккаунты" },
					{ "insufficient_balance", "❌ Недостаточно средств" },
					// Buttons
					{ "back", "← Назад" },
					{ "cancel", "❌ Отмена" },
					{ "ok", "✅ ОК" },
					{ "yes", "Да" },
					{ "no", "Нет" },
					// Errors
					{ "error_occurred", "❌ Произошла ошибка" },
					{ "session_expired", "⏰ Сессия истекла. Используйте /start" },
					{ "user_not_found", "👤 Пользователь не найден" },
					{ "operation_failed", "❌ Операция не удалась" }
				}
			}
		};
	}
}
