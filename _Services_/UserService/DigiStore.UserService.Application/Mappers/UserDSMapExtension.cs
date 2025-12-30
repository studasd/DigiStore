using DigiStore.UserService.Contracts.Responses;
using DigiStore.UserService.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Application.Mappers;

public static class UserDSMapExtension
{
	public static UserResponse ToUserResponse(this UserDS user, IEnumerable<string> roles)
	{
		var response = new UserResponse
		(
			Id: user.Id,
			Email: user.Email,
			FullName: user.FirstName,
			TelegramId: user.TelegramId,
			TelegramUsername: user.TelegramUsername,
			PhoneNumber: user.PhoneNumber,
			LanguageCode: user.LanguageCode,
			IsActive: user.IsActive,
			Source: user.Source.ToString(),
			Roles: roles.ToList(),
			CreatedAt: user.CreatedAt,
			UpdatedAt: user.UpdatedAt
		);

		return response;
	}
}
