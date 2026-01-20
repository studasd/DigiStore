using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Infrastructure.BotAPI;


public class BotAPIClient : IBotAPIClient
{
    private readonly ITelegramBotClient _client;
    private readonly TelegramOptions _telegramOptions;
    private readonly ILogger<BotAPIClient> _logger;

    public BotAPIClient(ITelegramBotClient client, IOptions<TelegramOptions> telegramOptions, ILogger<BotAPIClient> logger)
    {
        _client = client;
        _telegramOptions = telegramOptions.Value;
        _logger = logger;
    }


	public bool IsDebugShortResponse => _telegramOptions.IsDebugShortResponse;
	public string WebhookUrl => _telegramOptions.WebhookUrl;
	public bool IsWebhook => _telegramOptions.IsWebhook;


	public async Task<UnitResult<Error>> SendMessageAsync(
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
		CancellationToken cancellationToken = default)
	{
		try
		{
			await _client.SendMessage(
				chatId,
				text,
				parseMode,
				replyParameters,
				replyMarkup,
				linkPreviewOptions,
				messageThreadId,
				entities,
				disableNotification,
				protectContent,
				messageEffectId,
				businessConnectionId,
				allowPaidBroadcast,
				directMessagesTopicId,
				suggestedPostParameters,
				cancellationToken
			);

			return Result.Success<Error>();
		}
		catch(Exception ex)
		{
			_logger.LogError(ex, "Failed to send message to ChatId: {ChatId}", chatId);
			return Error.Failure("bot.sendmessage.error", $"Failed to send message to ChatId: {chatId}");
		}
	}
	
	
	
	public async Task<UnitResult<Error>> AnswerCallbackQueryAsync(
		string callbackQueryId,
		string? text = default,
		bool showAlert = default,
		string? url = default,
		int? cacheTime = default,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await _client.AnswerCallbackQuery(
				callbackQueryId,
				text,
				showAlert,
				url,
				cacheTime,
				cancellationToken
			);
			return Result.Success<Error>();
		}
		catch(Exception ex)
		{
			_logger.LogError(ex, "Failed to answer callback query: {CallbackQueryId}", callbackQueryId);
			return Error.Failure("bot.answercallbackquery.error", $"Failed to answer callback query: {callbackQueryId}");
		}
	}


	public async Task<UnitResult<Error>> EditMessageTextAsync(
		ChatId chatId,
		int messageId,
		string text,
		ParseMode parseMode = default,
		InlineKeyboardMarkup? replyMarkup = default,
		LinkPreviewOptions? linkPreviewOptions = default,
		IEnumerable<MessageEntity>? entities = default,
		string? businessConnectionId = default,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await _client.EditMessageText(
				chatId,
				messageId,
				text,
				parseMode,
				replyMarkup,
				linkPreviewOptions,
				entities,
				businessConnectionId,
				cancellationToken
			);
			return Result.Success<Error>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to edit message text. ChatId: {ChatId}, MessageId: {MessageId}", chatId, messageId);
			return Error.Failure("bot.editmessagetext.error", $"Failed to edit message text. ChatId: {chatId}, MessageId: {messageId}");
		}
	}


	public async Task<UnitResult<Error>> DeleteMessageAsync(
		ChatId chatId,
		int messageId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await _client.DeleteMessage(
				chatId,
				messageId,
				cancellationToken
			);
			return Result.Success<Error>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to delete message. ChatId: {ChatId}, MessageId: {MessageId}", chatId, messageId);
			return Error.Failure("bot.deletemessage.error", $"Failed to delete message. ChatId: {chatId}, MessageId: {messageId}");
		}
	}
	
	
	
	public async Task<UnitResult<Error>> SetMyCommandsAsync(
		IEnumerable<BotCommand> commands,
		BotCommandScope? scope = default,
		string? languageCode = default,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await _client.SetMyCommands(
				commands,
				scope,
				languageCode,
				cancellationToken
			);
			return Result.Success<Error>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to set bot commands");
			return Error.Failure("bot.setmycommands.error", $"Failed to set bot commands");
		}
	}
	
	
	public async Task<UnitResult<Error>> DeleteWebhookAsync(
		bool dropPendingUpdates = default,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await _client.DeleteWebhook(
				dropPendingUpdates,
				cancellationToken
			);
			return Result.Success<Error>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to delete webhook");
			return Error.Failure("bot.deletewebhook.error", $"Failed to delete webhook");
		}
	}
	
	
	
	public async Task<UnitResult<Error>> ReceiveAsync(
		Func<ITelegramBotClient, Update, CancellationToken, Task> updateHandler,
		Func<ITelegramBotClient, Exception, CancellationToken, Task> errorHandler,
		ReceiverOptions? receiverOptions = default, CancellationToken cancellationToken = default)
	{
		try
		{
			await _client.ReceiveAsync(
				updateHandler,
				errorHandler,
				receiverOptions,
				cancellationToken);

			return Result.Success<Error>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to delete webhook");
			return Error.Failure("bot.receiveasync.error", $"Failed to start receiving updates");
		}
	}



	public async Task<UnitResult<Error>> SetWebhookAsync(
		string url,
		InputFileStream? certificate = default,
		string? ipAddress = default,
		int? maxConnections = default,
		IEnumerable<UpdateType>? allowedUpdates = default,
		bool dropPendingUpdates = default,
		string? secretToken = default,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await _client.SetWebhook(
				url,
				certificate,
				ipAddress,
				maxConnections,
				allowedUpdates,
				dropPendingUpdates,
				secretToken,
				cancellationToken);

			return Result.Success<Error>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to set webhook");
			return Error.Failure("bot.setwebhook.error", $"Failed to set webhook");
		}
	}
}
