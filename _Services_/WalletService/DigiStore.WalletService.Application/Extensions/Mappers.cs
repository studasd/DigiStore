using DigiStore.WalletService.Contracts.Responses;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Extensions;

public static class Mappers
{
	public static TransactionResponse MapToResponse(this TransactionDS transaction)
	{
		return new TransactionResponse
		(
			Id: transaction.Id,
			WalletId: transaction.WalletId,
			Amount: transaction.Amount,
			Type: transaction.Type.ToString(),
			Status: transaction.Status.ToString(),
			Description: transaction.Description,
			BalanceAfter: transaction.BalanceAfter,
			CreatedAt: transaction.CreatedAt
		);
	}

	public static WalletResponse MapToResponse(this WalletDS wallet)
	{
		return new WalletResponse
		(
			Id: wallet.Id,
			UserId: wallet.UserId,
			Balance: wallet.Balance,
			TotalDeposited: wallet.TotalDeposited,
			TotalWithdrawn: wallet.TotalWithdrawn,
			Currency: wallet.Currency,
			IsFrozen: wallet.IsFrozen,
			CreatedAt: wallet.CreatedAt,
			UpdatedAt: wallet.UpdatedAt
		);
	}
}
