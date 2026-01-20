using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Interfaces;

public interface IBotAPIClient
{

	bool IsDebugShortResponse { get; }
	string WebhookUrl { get; }
	bool IsWebhook { get; }


	Task<UnitResult<Error>> SendMessageAsync(
		ChatId chatId,
		string text,
		ParseMode parseMode = default,
		ReplyParameters? replyParameters = default,
		ReplyMarkup? replyMarkup = default,
		LinkPreviewOptions? linkPreviewOptions = default,
		int? messageThreadId = default,
		IEnumerable<MessageEntity>? entities = default,
		bool disableNotification = default,
		bool protectContent = default,
		string? messageEffectId = default,
		string? businessConnectionId = default,
		bool allowPaidBroadcast = default,
		long? directMessagesTopicId = default,
		SuggestedPostParameters? suggestedPostParameters = default,
		CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> AnswerCallbackQueryAsync(
		string callbackQueryId,
		string? text = default,
		bool showAlert = default,
		string? url = default,
		int? cacheTime = default,
		CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> EditMessageTextAsync(
		ChatId chatId,
		int messageId,
		string text,
		ParseMode parseMode = default,
		InlineKeyboardMarkup? replyMarkup = default,
		LinkPreviewOptions? linkPreviewOptions = default,
		IEnumerable<MessageEntity>? entities = default,
		string? businessConnectionId = default,
		CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> DeleteMessageAsync(
		ChatId chatId,
		int messageId,
		CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> SetMyCommandsAsync(
		IEnumerable<BotCommand> commands,
		BotCommandScope? scope = default,
		string? languageCode = default,
		CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> DeleteWebhookAsync(
		bool dropPendingUpdates = default,
		CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> ReceiveAsync(
		Func<ITelegramBotClient, Update, CancellationToken, Task> updateHandler,
		Func<ITelegramBotClient, Exception, CancellationToken, Task> errorHandler,
		ReceiverOptions? receiverOptions = default, CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> SetWebhookAsync(
		string url,
		InputFileStream? certificate = default,
		string? ipAddress = default,
		int? maxConnections = default,
		IEnumerable<UpdateType>? allowedUpdates = default,
		bool dropPendingUpdates = default,
		string? secretToken = default,
		CancellationToken cancellationToken = default);
}
