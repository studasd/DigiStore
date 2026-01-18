using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application;

/// <summary>
/// Background сервис для обработки рекуррентных платежей
/// </summary>
public class RecurringPaymentBackgroundService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<RecurringPaymentBackgroundService> _logger;
	private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

	public RecurringPaymentBackgroundService(
		IServiceProvider serviceProvider,
		ILogger<RecurringPaymentBackgroundService> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken token)
	{
		_logger.LogInformation("RecurringPaymentBackgroundService запущен");
		return;

		while (!token.IsCancellationRequested)
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();
				var recurringService = scope.ServiceProvider
					.GetRequiredService<PaymentRecurringService>();

				var recurringRopository = scope.ServiceProvider
					.GetRequiredService<IPaymentRecurringRepository>();

				// Получить подписки готовые к обработке
				var duePaymentsResult = await recurringRopository.GetDueAsync(token);
				
				if (duePaymentsResult.IsSuccess)
				{
					var duePayments = duePaymentsResult.Value;

					_logger.LogInformation(
					$"Найдено {duePayments.Count} рекуррентных платежей для обработки");

					// Обработать каждый платеж
					foreach (var recurring in duePayments)
					{
						try
						{
							await recurringService.ProcessNextRecurringPaymentAsync(recurring.Id, token);
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, $"Ошибка при обработке рекуррентного платежа {recurring.Id}");
						}
					}

				}

				// Ждать перед следующей проверкой
				await Task.Delay(_checkInterval, token);
			}
			catch (OperationCanceledException)
			{
				// Сервис останавливается
				break;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка в RecurringPaymentBackgroundService");
			}
		}

		_logger.LogInformation("RecurringPaymentBackgroundService остановлен");
	}
}