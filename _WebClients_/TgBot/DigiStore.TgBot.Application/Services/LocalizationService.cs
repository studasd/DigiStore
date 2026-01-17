using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.UserService.Contracts.Enums;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using System.Linq;

namespace DigiStore.TgBot.Application.Services;

public class LocalizationService : ILocalizationService
{
	private readonly Dictionary<LanguageCodes, Dictionary<string, string>> _localizations;
	private readonly ILocalizationRepository _localizationRepository;

	public LocalizationService(ILocalizationRepository localizationRepository)
	{
		_localizationRepository = localizationRepository;
		_localizations = InitializeLocalizations();
	}


	public string GetMessage(string key, LanguageCodes languageCode = LanguageCodes.en)
	{
		if (!_localizations.ContainsKey(languageCode))
			languageCode = LanguageCodes.en;

		if (_localizations[languageCode].TryGetValue(key, out var message))
			return message;

		// try fallback to English
		if (languageCode != LanguageCodes.en && _localizations.TryGetValue(LanguageCodes.en, out var enDict) && enDict.TryGetValue(key, out var fallback))
			return fallback;

		return key;
	}

	public Dictionary<string, string> GetLanguages()
	{
		return new Dictionary<string, string>
		{
			{ "en", "🇬🇧 English" },
			{ "ru", "🇷🇺 Русский" },
			{ "de", "🇩🇪 Deutsch" },
		};
	}

	private Dictionary<LanguageCodes, Dictionary<string, string>> InitializeLocalizations()
	{
		// Load from database. This method blocks on async repository calls because constructor cannot be async.
		var entries = _localizationRepository.GetAllAsync(CancellationToken.None).GetAwaiter().GetResult();

		if(entries == null)
			throw new Exception("Failed to load localization entries from the database.");

		// Build dictionaries
		var enDictFinal = entries.ToDictionary(e => e.Key, e => e.En ?? e.Key);
		var ruDictFinal = entries.ToDictionary(e => e.Key, e => e.Ru ?? e.Key);

		return new Dictionary<LanguageCodes, Dictionary<string, string>>
		{
			{ LanguageCodes.en, enDictFinal },
			{ LanguageCodes.ru, ruDictFinal }
		};
	}
}
