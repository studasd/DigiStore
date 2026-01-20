using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Application.Updates.Dispatchers;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Updates;




/// <summary>
/// Универсальный обработчик Update, который автоматически находит и вызывает нужный хэндлер
/// </summary>
public class UpdateHandler
{
	private readonly ISessionService _sessionService;
	private readonly ITgUserService _userService;
	private readonly ITgUserRepository _userRepository;
	private readonly UpdatePipeline _pipeline;
    private readonly ILogger<UpdateHandler> _logger;

	public UpdateHandler(
		ISessionService sessionService,
		ITgUserService userService,
		ITgUserRepository userRepository,
		UpdatePipeline pipeline,
		ILogger<UpdateHandler> logger,
		HandlerCollections registry)
    {
		_sessionService = sessionService;
		_userService = userService;
		_userRepository = userRepository;
		_pipeline = pipeline;
        _logger = logger;
    }


	/// <summary>
	/// Обрабатывает Update
	/// </summary>
	public async Task HandleUpdateAsync(Update update, CancellationToken token = default)
	{
		try
		{
			// Before dispatching handlers ensure we have session and linked user
			long? telegramId = null;
			string? username = null;
			string? firstName = null;
			string? lastName = null;

			if (update.Message?.From != null)
			{
				telegramId = update.Message.From.Id;
				username = update.Message.From.Username;
				firstName = update.Message.From.FirstName;
				lastName = update.Message.From.LastName;
			}
			else if (update.CallbackQuery?.From != null)
			{
				telegramId = update.CallbackQuery.From.Id;
				username = update.CallbackQuery.From.Username;
				firstName = update.CallbackQuery.From.FirstName;
				lastName = update.CallbackQuery.From.LastName;
			}


			if (telegramId.HasValue)
			{
				var sessionResult = await _sessionService.GetOrCreateSessionAsync(telegramId.Value, token);
				if (sessionResult.IsFailure)
				{
					_logger.LogWarning("Failed to get or create user from Session for TelegramId {TelegramId}: {Error}", telegramId.Value, sessionResult.Error?.GetMessage());
					return;
				}

				var session = sessionResult.Value!;
				if (session.UserId == default)
				{
					var lang = session.LangCode;
					var userResult = await _userService.GetOrCreateUserAsync(telegramId.Value, username, firstName, lastName, lang, token);

					if (userResult.IsSuccess)
					{
						var userDto = userResult.Value!;

						var tgUser = new TgUser
						{
							Id = Guid.NewGuid(),
							TelegramId = userDto.TelegramId,
							UserId = userDto.Id,
							FirstName = firstName ?? string.Empty,
							LastName = lastName ?? string.Empty,
							Username = username,
							IsActive = userDto.IsActive,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow
						};

						await _userRepository.AddOrUpdateAsync(tgUser, token);


						// Set session.UserId and optionally cache profile
						session.UserId = userDto.Id;
						session.CachedProfile = new CachedUserProfileVO
						{
							UserId = userDto.Id,
							TelegramId = userDto.TelegramId,
							FirstName = userDto.FullName?.Split(' ').FirstOrDefault() ?? string.Empty,
							LastName = userDto.FullName?.Split(' ').LastOrDefault() ?? string.Empty,
							Username = userDto.Username,
							LangCode = userDto.LangCode,
							IsActive = userDto.IsActive,
							Roles = userDto.Roles,
						};

						await _sessionService.UpdateSessionAsync(session, token);

						// Notify UserService about activity
						_ = _userService.UpdateActivityAsync(userDto.Id, token);
					}
					else
					{
						_logger.LogWarning("Failed to get or create user from UserService for TelegramId {TelegramId}: {Error}", telegramId.Value, userResult.Error?.GetMessage());
					}
				}
			}

			var dispatchResult = await _pipeline.DispatchAsync(update, token);
			if (dispatchResult.IsFailure)
				_logger.LogError("Bad error UpdatePipeline: {errors}", dispatchResult.Error.GetMessage());
			return;

			_logger.LogWarning("Unhandled update type: {UpdateType}", update.Type);

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing update");
		}
	}

}
