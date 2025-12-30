using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Contracts.Requests;


/// <summary>
/// Request to update user profile
/// </summary>
public record UpdateUserProfileRequest
{
	public Guid UserId { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? LanguageCode { get; set; }
}
