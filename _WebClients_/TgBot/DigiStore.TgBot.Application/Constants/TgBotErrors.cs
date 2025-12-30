using DigiStore.SharedKernel;

namespace DigiStore.TgBot.Application.Constants;


public static class TgBotErrors
{
	public static readonly Error UserNotFound = Error.NotFound(
		"bot.user_not_found",
		"User not found in UserService");

	public static readonly Error SessionExpired = Error.Authorization(
		"bot.session_expired",
		"Session expired, please start again");

	public static readonly Error UnknownCommand = Error.Validation(
		"bot.unknown_command",
		"Unknown command");

	public static readonly Error OperationFailed = Error.Failure(
		"bot.operation_failed",
		"Operation failed");
}
