using DigiStore.SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application;

/// <summary>
/// Wallet domain errors
/// </summary>
public static class WalletErrors
{
	public static readonly Error WalletNotFound = Error.NotFound("wallet.not_found", "Кошелек не найден");
	public static readonly Error InsufficientBalance = Error.Conflict("wallet.insufficient_balance", "Недостаточно средств");
	public static readonly Error WalletFrozen = Error.Forbidden("wallet.frozen", "Кошелек заморожен");
	public static readonly Error InvalidAmount = Error.Validation("wallet.invalid_amount", "Некорректная сумма");
	public static readonly Error TransactionFailed = Error.Internal("wallet.transaction_failed", "Ошибка при выполнении операции");
	public static readonly Error TransactionNotFound = Error.NotFound("transaction.not_found", "Транзакция не найдена");
}