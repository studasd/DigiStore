using DigiStore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.TgBot.Application.Interfaces.Services;


/// <summary>
/// Localization service for multi-language support
/// </summary>
public interface ILocalizationService
{
	/// <summary>
	/// Get localized message
	/// </summary>
	string GetMessage(string key, LanguageCodes langCode = LanguageCodes.en, object model = null);

	/// <summary>
	/// Get all available languages
	/// </summary>
	Dictionary<string, string> GetLanguages();
}
