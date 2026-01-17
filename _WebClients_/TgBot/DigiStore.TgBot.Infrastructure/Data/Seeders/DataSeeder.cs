using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.TgBot.Infrastructure.Data.Seeders;

public interface IDataSeeder
{
	Task SeedAsync(TgBotDbContext context, IServiceProvider serviceProvider, CancellationToken token);
}



public class DataSeeder : IDataSeeder
{
	public async Task SeedAsync(TgBotDbContext context, IServiceProvider serviceProvider, CancellationToken token)
	{
		await SeedLocalizationAsync(context, serviceProvider, token);
	}


	private static async Task SeedLocalizationAsync(TgBotDbContext context, IServiceProvider serviceProvider, CancellationToken token)
	{
		if (await context.Localizations.AnyAsync())
			return;

		using var scope = serviceProvider.CreateScope();
		var localRepository = scope.ServiceProvider.GetRequiredService<ILocalizationRepository>();

		// Load from database. This method blocks on async repository calls because constructor cannot be async.
		var entries = await localRepository.GetAllAsync(token);

		// If DB is empty, seed it from embedded hard-coded values
		if (entries == null || !entries.Any())
		{
			var en = new Dictionary<string, string>
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
				{ LocalKeys.Balances.TopUpBalance, "Top up balance" },
				// Buttons
				{ LocalKeys.Buttons.Back, "← Back" },
				{ LocalKeys.Buttons.Cancel, "❌ Cancel" },
				{ LocalKeys.Buttons.Ok, "✅ OK" },
				{ LocalKeys.Buttons.Yes, "Yes" },
				{ LocalKeys.Buttons.No, "No" },
				{ LocalKeys.Buttons.BalanceUpYookassa, "YooKassa" },
				{ LocalKeys.Buttons.BalanceUpFreekassa, "FreeKassa" },
				// Errors
				{ LocalKeys.Errors.Occurred, "❌ An error occurred" },
				{ LocalKeys.Errors.SessionExpired, "⏰ Session expired. Please use /start" },
				{ LocalKeys.Errors.UserNotFound, "👤 User not found" },
				{ LocalKeys.Errors.OperationFailed, "❌ Operation failed" }
			};

			var ru = new Dictionary<string, string>
			{
				// Greeting & Navigation
				{ LocalKeys.Greetings.Greeting, "👋 Добро пожаловать в PetFamily магазин!" },
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
				{ LocalKeys.Balances.TopUpBalance,	 "Пополнить баланс" },
				// Buttons
				{ LocalKeys.Buttons.Back, "← Назад" },
				{ LocalKeys.Buttons.Cancel, "❌ Отмена" },
				{ LocalKeys.Buttons.Ok, "✅ ОК" },
				{ LocalKeys.Buttons.Yes, "Да" },
				{ LocalKeys.Buttons.No, "Нет" },
				{ LocalKeys.Buttons.BalanceUpYookassa, "ЮКасса" },
				{ LocalKeys.Buttons.BalanceUpFreekassa, "FreeKassa" },
				// Errors
				{ LocalKeys.Errors.Occurred, "❌ Произошла ошибка" },
				{ LocalKeys.Errors.SessionExpired, "⏰ Сессия истекла. Используйте /start" },
				{ LocalKeys.Errors.UserNotFound, "👤 Пользователь не найден" },
				{ LocalKeys.Errors.OperationFailed, "❌ Операция не удалась" }
			};

			var now = DateTime.UtcNow;
			var toSeed = en.Keys.Union(ru.Keys).Select(key => new Localization
			{
				Key = key,
				En = en.TryGetValue(key, out var ev) ? ev : null,
				Ru = ru.TryGetValue(key, out var rv) ? rv : null,
				CreatedAt = now,
				UpdatedAt = now
			}).ToList();

			foreach (var loc in toSeed)
			{
				await localRepository.AddOrUpdateAsync(loc, token);
			}
		}

		await context.SaveChangesAsync();
	}

}

