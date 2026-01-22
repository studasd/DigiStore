using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Application.Interfaces.Services;
using Scriban;
using System.Globalization;
using System.Resources;

namespace DigiStore.TgBot.Application.Services;

public class LocalizationService : ILocalizationService
{
	private readonly Dictionary<LanguageCodes, Dictionary<string, string>> _localizations;
	private readonly ILocalizationRepository _localeRepository;
	private readonly bool _runtimeDbContentEnabled;

	public LocalizationService(ILocalizationRepository localizationRepository)
	{
		_localeRepository = localizationRepository;
		_runtimeDbContentEnabled = ReadRuntimeContentEnabled();
		_localizations = InitializeLocalizations().Value;
	}


	public string GetMessage(string key, LanguageCodes languageCode = LanguageCodes.en, object model = null)
	{
		// 1) DB (if enabled and key exists)
		if (_runtimeDbContentEnabled
			&& _localizations.TryGetValue(languageCode, out var dbDict)
			&& dbDict.TryGetValue(key, out var dbMessage)
			&& !string.IsNullOrWhiteSpace(dbMessage))
		{
			return model == null ? dbMessage : Template.Parse(dbMessage).Render(model);
		}

		// 2) Buttons.resx (base)
		var buttonMessage = GetButtonsMessage(key, languageCode);
		if (buttonMessage != null)
			return model == null ? buttonMessage : Template.Parse(buttonMessage).Render(model);

		// 3) fallback to en
		if (languageCode != LanguageCodes.en)
		{
			var enButtonMessage = GetButtonsMessage(key, LanguageCodes.en);
			if (enButtonMessage != null)
				return model == null ? enButtonMessage : Template.Parse(enButtonMessage).Render(model);

			if (_runtimeDbContentEnabled
				&& _localizations.TryGetValue(LanguageCodes.en, out var enDbDict)
				&& enDbDict.TryGetValue(key, out var enDbMessage)
				&& !string.IsNullOrWhiteSpace(enDbMessage))
			{
				return model == null ? enDbMessage : Template.Parse(enDbMessage).Render(model);
			}
		}

		// 4) fallback to key
		return key;
	}

	public Dictionary<string, string> GetLanguages()
	{
		return new Dictionary<string, string>
		{
			{ "en", "🇬🇧 English" },
			{ "ru", "🇷🇺 Русский" },
		};
	}

	private Result<Dictionary<LanguageCodes, Dictionary<string, string>>, Error> InitializeLocalizations()
	{
		if (!_runtimeDbContentEnabled)
		{
			return new Dictionary<LanguageCodes, Dictionary<string, string>>
			{
				{ LanguageCodes.en, new Dictionary<string, string>(StringComparer.Ordinal) },
				{ LanguageCodes.ru, new Dictionary<string, string>(StringComparer.Ordinal) }
			};
		}

		try
		{
			// Load from database. This method blocks on async repository calls because constructor cannot be async.
			var entriesResult = _localeRepository.GetAllAsync(CancellationToken.None).GetAwaiter().GetResult();
			if (entriesResult.IsFailure)
				return entriesResult.Error;

			var entries = entriesResult.Value;
			var enDictFinal = entries.ToDictionary(e => e.Key, e => e.En ?? e.Key, StringComparer.Ordinal);
			var ruDictFinal = entries.ToDictionary(e => e.Key, e => e.Ru ?? e.Key, StringComparer.Ordinal);

			return new Dictionary<LanguageCodes, Dictionary<string, string>>
			{
				{ LanguageCodes.en, enDictFinal },
				{ LanguageCodes.ru, ruDictFinal }
			};
		}
		catch
		{
			// If DB is unavailable at startup, keep bot running with resource-based messages.
			return new Dictionary<LanguageCodes, Dictionary<string, string>>
			{
				{ LanguageCodes.en, new Dictionary<string, string>(StringComparer.Ordinal) },
				{ LanguageCodes.ru, new Dictionary<string, string>(StringComparer.Ordinal) }
			};
		}
	}

	private static bool ReadRuntimeContentEnabled()
	{
		var value = Environment.GetEnvironmentVariable("TG_BOT_RUNTIME_LOCALIZATION_DB");
		if (string.IsNullOrWhiteSpace(value))
			return true;

		return value.Equals("1", StringComparison.OrdinalIgnoreCase)
			|| value.Equals("true", StringComparison.OrdinalIgnoreCase)
			|| value.Equals("yes", StringComparison.OrdinalIgnoreCase)
			|| value.Equals("on", StringComparison.OrdinalIgnoreCase);
	}

	private static string? GetButtonsMessage(string key, LanguageCodes languageCode)
	{
		try
		{
			var cultureName = languageCode switch
			{
				LanguageCodes.ru => "ru",
				_ => "en"
			};

			if (key.StartsWith("button_"))
			{
				var manager = new ResourceManager("DigiStore.TgBot.Application.Resources.Buttons", typeof(LocalizationService).Assembly);
				var culture = CultureInfo.GetCultureInfo(cultureName);
				return manager.GetString(key, culture);
			}
			else if (key.StartsWith("error_"))
			{
				var manager = new ResourceManager("DigiStore.TgBot.Application.Resources.Errors", typeof(LocalizationService).Assembly);
				var culture = CultureInfo.GetCultureInfo(cultureName);
				return manager.GetString(key, culture);
			}
			else
			{
				var manager = new ResourceManager("DigiStore.TgBot.Application.Resources.Messages", typeof(LocalizationService).Assembly);
				var culture = CultureInfo.GetCultureInfo(cultureName);
				return manager.GetString(key, culture);
			}
		}
		catch
		{
		}
		
		return null;
	}

}
