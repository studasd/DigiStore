using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigiStore.TgBot.Infrastructure.Postgres.Data.Seeders;

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
		var entriesResult = await localRepository.GetAllAsync(token);
		if (entriesResult.IsFailure)
			return;

		// If DB is empty, seed it from embedded hard-coded values
		var entries = entriesResult.Value;
		if (entries == null || !entries.Any())
		{
			var en = new Dictionary<string, string>
			{
				// Buttons
				//{ LocalKeys.Buttons.Profile,		"👤 Profile" },
				//{ LocalKeys.Buttons.Balance,		"💰 Balance" },
				//{ LocalKeys.Buttons.Catalog,		"🛍️ Catalog" },
				//{ LocalKeys.Buttons.Orders,			"📦 My Orders" },
				//{ LocalKeys.Buttons.Settings,		"⚙️ Settings" },
				//{ LocalKeys.Buttons.Help,			"❓ Help" },
				//{ LocalKeys.Buttons.ChangeLanguage, "🌐 Change Language" },
				
				//// Buttons
				//{ LocalKeys.Buttons.Back,			"← Back" },
				//{ LocalKeys.Buttons.Cancel,			"❌ Cancel" },
				//{ LocalKeys.Buttons.Ok,				"✅ OK" },
				//{ LocalKeys.Buttons.Yes,			"Yes" },
				//{ LocalKeys.Buttons.No,				"No" },
				//{ LocalKeys.Buttons.BalanceUpYookassa,	"YooKassa" },
				//{ LocalKeys.Buttons.BalanceUpFreekassa, "FreeKassa" },
				//{ LocalKeys.Buttons.MainMenu,		"🏠 Main Menu" },

				//// Errors
				//{ LocalKeys.Errors.Occurred,		"❌ An error occurred" },
				//{ LocalKeys.Errors.SessionExpired,	"⏰ Session expired. Please use /start" },
				//{ LocalKeys.Errors.UserNotFound,	"👤 User not found" },
				//{ LocalKeys.Errors.OperationFailed, "❌ Operation failed" },

//				// Messages
//				{ LocalKeys.Messages.Welcome,           "👋 Welcome to PetFamily Store!" },
//				{ LocalKeys.Messages.SelectLanguage,    "📍 Please select your language:" },
//				{ LocalKeys.Messages.LanguageChanged,   "✅ Language changed to English" },
//				{ LocalKeys.Messages.TopUpAmountInputErrorAmount,		@"Enter the correct amount as a number (greater than 0):" },
//				{ LocalKeys.Messages.TopUpAmountInputErrorAggregator,	@"The payment method could not be determined. Open your balance and select the aggregator again:" },
//				{ LocalKeys.Messages.MainMenu, @"
//🏠 Main Menu

//Choose an option below:
//" },
//				{ LocalKeys.Messages.TopUpAmountRequest, @"Enter the deposit amount as a number (for example 1500):" },

//				// Templates
//				{ LocalKeys.Templates.BalanceView, @"
//💰 WALLET BALANCE

//Current Balance: <b>{{balance}} {{currency}}</b>
//📊 Total Deposited: {{total_deposited}} {{currency}}
//📤 Total Withdrawn: {{total_withdrawn}} {{currency}}

//Top up your balance via:
//" },
//				{ LocalKeys.Templates.Balance, @"
//💰 WALLET BALANCE

//Current Balance: <b>{{balance}} {{currency}}</b>
//🔗 Linked accounts
//👤 Telegram: {{username}}
//Top up your balance via:
//" },
//				{ LocalKeys.Templates.Profile, @"
//════════════════════════════════════
//  YOUR PROFILE 
//════════════════════════════════════
//👤 Name: {{fullname}}
//🆔 Telegram ID: {{telegramid}}
//📧 Email: {{email}}
//📱  Username: {{username}}

//💰 Balance: {{balance}} {{currency}}
//🌐 Language: {{language}}
//✅ Status: {{status}}
//🎖️ Roles: {{roles}}

//📅 Registration date: {{datecreated}}
//🔄 Last updated: {{dateupdated}}
//" },
//				{ LocalKeys.Templates.TopUpBalance, @"
//To top up your balance to {{amount}}rub. click on the link:
//{{url}}

//After payment, the profile balance will be replenished automatically.
//" },

			};



			var ru = new Dictionary<string, string>
			{
				// Buttons
				//{ LocalKeys.Buttons.Profile,		"👤 Профиль" },
				//{ LocalKeys.Buttons.Balance,		"💰 Баланс" },
				//{ LocalKeys.Buttons.Catalog,		"🛍️ Каталог" },
				//{ LocalKeys.Buttons.Orders,			"📦 Мои заказы" },
				//{ LocalKeys.Buttons.Settings,		"⚙️ Настройки" },
				//{ LocalKeys.Buttons.Help,			"❓ Помощь" },
				//{ LocalKeys.Buttons.ChangeLanguage, "🌐 Изменить язык" },
				
				//{ LocalKeys.Buttons.Back,			"← Назад" },
				//{ LocalKeys.Buttons.Cancel,			"❌ Отмена" },
				//{ LocalKeys.Buttons.Ok,				"✅ ОК" },
				//{ LocalKeys.Buttons.Yes,			"Да" },
				//{ LocalKeys.Buttons.No,				"Нет" },
				//{ LocalKeys.Buttons.BalanceUpYookassa,		"ЮКасса" },
				//{ LocalKeys.Buttons.BalanceUpFreekassa,		"FreeKassa" },
				//{ LocalKeys.Buttons.MainMenu,		"🏠 Главное меню" },
				
				//// Errors
				//{ LocalKeys.Errors.Occurred,        "❌ Произошла ошибка" },
				//{ LocalKeys.Errors.SessionExpired,  "⏰ Сессия истекла. Используйте /start" },
				//{ LocalKeys.Errors.UserNotFound,    "👤 Пользователь не найден" },
				//{ LocalKeys.Errors.OperationFailed, "❌ Операция не удалась" },


//				// Messages
//				{ LocalKeys.Messages.Welcome,		"👋 Добро пожаловать в PetFamily магазин!" },
//				{ LocalKeys.Messages.SelectLanguage, "📍 Выберите язык:" },
//				{ LocalKeys.Messages.LanguageChanged, "✅ Язык изменён на Русский" },
//				{ LocalKeys.Messages.TopUpAmountRequest, @"Введите сумму пополнения числом (например 1500):" },
//				{ LocalKeys.Messages.TopUpAmountInputErrorAmount, @"Введите корректную сумму числом (больше 0):" },
//				{ LocalKeys.Messages.TopUpAmountInputErrorAggregator, @"Не удалось определить способ оплаты. Откройте баланс и выберите агрегатор заново:" },
//				{ LocalKeys.Messages.MainMenu, @"
//🏠 Главное меню

//Выберите опцию:
//" },

//				// Templates
//				{ LocalKeys.Templates.BalanceView, @"
//💰 БАЛАНС КОШЕЛЬКА

//Текущий баланс: <b>{{balance}} {{currency}}</b>
//📊 Всего пополнено: {{total_deposited}} {{currency}}
//📤 Всего снято: {{total_withdrawn}} {{currency}}

//Пополнить баланс через:
//" },
//				{ LocalKeys.Templates.Balance, @"
//💰 БАЛАНС КОШЕЛЬКА

//Текущий баланс: <b>{{balance}} {{currency}}</b>
//🔗 Привязанные аккаунты
//👤 Telegram: {{username}}
//Пополнить баланс через:
//" },
//				{ LocalKeys.Templates.Profile, @"
//════════════════════════════════════
//  ВАШ ПРОФИЛЬ 
//════════════════════════════════════
//👤 Имя: {{fullname}}
//🆔 Telegram ID: {{telegramid}}
//📧 Email: {{email}}
//📱  Username: {{username}}

//💰 Баланс: {{balance}} {{currency}}
//🌐 Язык: {{language}}
//✅ Статус: {{status}}
//🎖️ Роли: {{roles}}

//📅 Дата регистрации: {{datecreated}}
//🔄 Последнее обновление: {{dateupdated}}
//" },
//				{ LocalKeys.Templates.TopUpBalance, @"
//Для пополнения баланса на {{amount}}р. перейди по ссылке:
//{{url}}

//После оплаты баланс профиля пополнится автоматически.
//" },
				
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

