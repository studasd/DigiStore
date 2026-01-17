using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Configurations;

/// <summary>
/// Параметры конфигурации YooKassa
/// </summary>
public class YooKassaSettings
{
	public const string SectionName = "YooKassa";

	/// <summary>ID магазина YooKassa</summary>
	public string ShopId { get; set; } = string.Empty;

	/// <summary>Секретный ключ для API YooKassa</summary>
	public string SecretKey { get; set; } = string.Empty;

	/// <summary>Секрет для проверки подписей вебхуков</summary>
	public string WebhookSecret { get; set; } = string.Empty;

	/// <summary>URL возврата при успешной оплате</summary>
	public string SuccessReturnUrl { get; set; } = string.Empty;

	/// <summary>URL возврата при отмене оплаты</summary>
	public string FailReturnUrl { get; set; } = string.Empty;

	/// <summary>Минимальная сумма пополнения (руб.)</summary>
	public decimal MinDepositAmount { get; set; } = 100m;

	/// <summary>Максимальная сумма пополнения (руб.)</summary>
	public decimal MaxDepositAmount { get; set; } = 1000m;

	/// <summary>Минимальная сумма вывода (руб.)</summary>
	public decimal MinWithdrawalAmount { get; set; } = 500m;

	/// <summary>Максимальная сумма вывода (руб.)</summary>
	public decimal MaxWithdrawalAmount { get; set; } = 1000m;

	/// <summary>Комиссия при выводе (%)</summary>
	public decimal WithdrawalCommissionPercent { get; set; } = 5m;

	/// <summary>Timeout для HTTP запросов (сек)</summary>
	public int RequestTimeoutSeconds { get; set; } = 30;

	/// <summary>Максимальное количество повторов при ошибке</summary>
	public int MaxRetries { get; set; } = 3;

	/// <summary>Интервал между повторами (мс)</summary>
	public int RetryIntervalMs { get; set; } = 1000;
}