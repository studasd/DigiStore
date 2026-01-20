using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Application.Interfaces.Services;

namespace DigiStore.TgBot.Application.Services;

public class LocalizationService : ILocalizationService
{
	private readonly Dictionary<LanguageCodes, Dictionary<string, string>> _localizations;
	private readonly ILocalizationRepository _localeRepository;

	public LocalizationService(ILocalizationRepository localizationRepository)
	{
		_localeRepository = localizationRepository;
		_localizations = InitializeLocalizations().Value;
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

	private Result<Dictionary<LanguageCodes, Dictionary<string, string>>, Error> InitializeLocalizations()
	{
		// Load from database. This method blocks on async repository calls because constructor cannot be async.
		var entriesResult = _localeRepository.GetAllAsync(CancellationToken.None).GetAwaiter().GetResult();
		if(entriesResult.IsFailure)
			return entriesResult.Error;

		// Build dictionaries
		var entries = entriesResult.Value;
		var enDictFinal = entries.ToDictionary(e => e.Key, e => e.En ?? e.Key);
		var ruDictFinal = entries.ToDictionary(e => e.Key, e => e.Ru ?? e.Key);

		return new Dictionary<LanguageCodes, Dictionary<string, string>>
		{
			{ LanguageCodes.en, enDictFinal },
			{ LanguageCodes.ru, ruDictFinal }
		};
	}
}
