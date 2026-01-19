using CSharpFunctionalExtensions;
using DigiStore.Core.Validation;
using DigiStore.Enums;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Application.Validators;
using DigiStore.WalletService.Contracts.Requests.Payments;
using DigiStore.WalletService.Contracts.Responses.Payments;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Features.Payments;


public record CreatePaymentCommand(
	Guid UserId, 
	PaymentAggregators Aggregator, 
	decimal Amount, 
	string Description, 
	string ReturnUrl);


/// Создать платеж (пополнить баланс)
public sealed class CreatePayment : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("createPayment/{userId}", async Task<EndpointResult<CreatePaymentResponse>> (
			[FromRoute] Guid userId,
			[FromBody] CreatePaymentRequest request,
			[FromServices] CreatePaymentHandler handler,
			CancellationToken token) =>
		{
			return await handler.Handle(new CreatePaymentCommand(
				userId, 
				request.Aggregator, 
				request.Amount, 
				request.Description,
				request.ReturnUrl
				), token);
		});
	}
}


public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
	public CreatePaymentCommandValidator(PaymentValidator paymentValidator)
	{

		RuleFor(x => x.UserId)
			.Must(id => id != Guid.Empty).WithError(Error.Validation("userid.is.invalid", $"UserId не может быть пустым"));

		RuleFor(x => x.Aggregator)
			.Must(agg => agg != PaymentAggregators.None).WithError(Error.Validation("aggregator.is.invalid", $"Нет указанного агрегатора"));

		RuleFor(x => x.Amount).Custom((amount, context) =>
		{
			var validation = paymentValidator.ValidateDepositAmount(amount);
			if (validation.IsFailure)
			{
				var err = Error.Validation("userid.is.invalid", "UserId не может быть пустым");
				var failure = new ValidationFailure(nameof(CreatePaymentCommand.UserId), err.GetMessage())
				{
					// передаём сам объект Error для последующей обработки
					CustomState = err,
					// можно также заполнить ErrorCode если нужно
					ErrorCode = "userid.is.invalid"
				};
				context.AddFailure(failure);
			}
		});

		RuleFor(x => x.Description)
			.NotNull().WithError(Error.Validation("description.is.invalid", $"Описание не может быть пустым"))
			.MaximumLength(500).WithError(Error.Validation("description.is.too.long", $"Описание не может быть длиннее 500 символов"));
	}
}



public sealed class CreatePaymentHandler : IWalletServiceHandler
{
	private readonly ILogger<CreatePaymentHandler> _logger;
    private readonly IPaymentService _paymentService;
    private readonly IValidator<CreatePaymentCommand> _validator;
    private readonly IWalletRepository _walletRepository;

	public CreatePaymentHandler(
		ILogger<CreatePaymentHandler> logger,
		IPaymentService paymentService,
		IValidator<CreatePaymentCommand> validator,
		IWalletRepository walletRepository)
	{
		_logger = logger;
        _paymentService = paymentService;
        _validator = validator;
        _walletRepository = walletRepository;
	}


	public async Task<Result<CreatePaymentResponse, Error>> Handle(CreatePaymentCommand command, CancellationToken token)
	{
		var validationResult = await _validator.ValidateAsync(command, token);
		if (!validationResult.IsValid)
			return validationResult.ToError();

		// Получить кошелек пользователя
		var wallet = await _walletRepository.GetOrCreateByUserIdAsync(command.UserId, token);


		if (wallet.IsFailure)
			return wallet.Error;


		var paymentResult = await _paymentService.CreatePaymentAsync(
			command.UserId, 
			wallet.Value.Id, 
			command.Amount, 
			command.Aggregator, 
			command.Description,
			command.ReturnUrl);

		if (paymentResult.IsFailure)
			return paymentResult.Error;

		var payment = paymentResult.Value;

		// Получить ссылку на оплату
		var confirmationUrlResult = await _paymentService.GetPaymentConfirmationUrlAsync(payment!.Id, token);

		if (confirmationUrlResult.IsFailure)
			return confirmationUrlResult.Error;

		return new CreatePaymentResponse(payment.Id, confirmationUrlResult.Value, payment.Amount, payment.Status);
	}
}