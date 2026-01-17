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

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("RecurringPaymentBackgroundService запущен");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();
				var recurringService = scope.ServiceProvider
					.GetRequiredService<YooKassaRecurringService>();

				// Получить подписки готовые к обработке
				var duePayments = await recurringService.GetDueRecurringPaymentsAsync();

				_logger.LogInformation(
					$"Найдено {duePayments.Count} рекуррентных платежей для обработки");

				// Обработать каждый платеж
				foreach (var recurring in duePayments)
				{
					try
					{
						await recurringService.ProcessNextRecurringPaymentAsync(recurring.Id);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex,
							$"Ошибка при обработке рекуррентного платежа {recurring.Id}");
					}
				}

				// Ждать перед следующей проверкой
				await Task.Delay(_checkInterval, stoppingToken);
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