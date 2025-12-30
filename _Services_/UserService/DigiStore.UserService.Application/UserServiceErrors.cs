using DigiStore.SharedKernel;

namespace DigiStore.UserService.Application;

/// <summary>
/// Domain errors for account operations
/// </summary>
public static class UserServiceErrors
{
	public static readonly Error UserNotFound = Error.NotFound(
		"user.not_found",
		"Пользователь не найден");

	public static readonly Error UserAlreadyExists = Error.Conflict(
		"user.already_exists",
		"Пользователь с таким email уже существует");

	public static readonly Error InvalidCredentials = Error.Authorization(
		"user.invalid_credentials",
		"Неверные учетные данные");

	public static readonly Error UserAlreadyRegistered = Error.Conflict(
		"user.already_registered",
		"Пользователь уже зарегистрирован");

	public static readonly Error TelegramIdAlreadyLinked = Error.Conflict(
		"user.telegram_id_already_linked",
		"Данный Telegram ID уже связан с другим аккаунтом");

	public static readonly Error RoleNotFound = Error.NotFound(
		"role.not_found",
		"Роль не найдена");

	public static readonly Error CannotDeleteSystemRole = Error.Forbidden(
		"role.cannot_delete_system",
		"Системная роль не может быть удалена");

	public static readonly Error PermissionDenied = Error.Forbidden(
		"user.permission_denied",
		"Недостаточно прав для выполнения операции");
}