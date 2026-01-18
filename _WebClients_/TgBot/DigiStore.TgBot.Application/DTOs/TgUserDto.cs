using DigiStore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.TgBot.Application.DTOs;


public class TgUserDto
{
	public Guid Id { get; set; }
	public long TelegramId { get; set; }
	public string Email { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
	public string? Username { get; set; }
	public LanguageCodes LangCode { get; set; } = LanguageCodes.en;
	public bool IsActive { get; set; }
	public List<string> Roles { get; set; } = new();
	
	// Indicates that the user was created during GetOrCreateUserAsync
	public bool IsNew { get; set; } = false;
}
