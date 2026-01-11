using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.TgBot.Application.DTOs;


public class TelegramUserDto
{
	public Guid Id { get; set; }
	public long TelegramId { get; set; }
	public string Email { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
	public string? TelegramUsername { get; set; }
	public string LanguageCode { get; set; } = "en";
	public bool IsActive { get; set; }
	public List<string> Roles { get; set; } = new();
	
	// Indicates that the user was created during GetOrCreateUserAsync
	public bool IsNew { get; set; } = false;
}
