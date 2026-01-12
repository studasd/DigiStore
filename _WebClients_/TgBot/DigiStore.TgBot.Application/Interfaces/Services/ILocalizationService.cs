using DigiStore.UserService.Contracts.Enums;
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
	string GetMessage(string key, LanguageCodes langCode = LanguageCodes.en);

	/// <summary>
	/// Get all available languages
	/// </summary>
	Dictionary<string, string> GetLanguages();
}
