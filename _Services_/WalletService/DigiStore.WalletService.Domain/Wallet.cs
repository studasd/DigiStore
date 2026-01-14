namespace DigiStore.WalletService.Domain;

/// <summary>
/// Wallet entity - баланс пользователя
/// </summary>
public class Wallet
{
	/// <summary>
	/// Wallet ID (usually same as User ID)
	/// </summary>
	public Guid Id { get; set; }
	/// <summary>
	/// User ID from UserService
	/// </summary>
	public Guid UserId { get; set; }
	/// <summary>
	/// Current balance in default currency (e.g., rubles)
	/// </summary>
	public decimal Balance { get; set; }
	/// <summary>
	/// Total amount deposited (for statistics)
	/// </summary>
	public decimal TotalDeposited { get; set; }
	/// <summary>
	/// Total amount withdrawn
	/// </summary>
	public decimal TotalWithdrawn { get; set; }
	/// <summary>
	/// Currency code (RUB, USD, etc.)
	/// </summary>
	public string Currency { get; set; } = "RUB";
	/// <summary>
	/// Is wallet frozen (cannot withdraw or spend)
	/// </summary>
	public bool IsFrozen { get; set; } = false;

	/// <summary>
	/// Creation timestamp
	/// </summary>
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	/// <summary>
	/// Last update timestamp
	/// </summary>
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	/// <summary>
	/// Navigation to transactions
	/// </summary>
	public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
	/// <summary>
	/// Check if wallet has sufficient balance
	/// </summary>
	public bool HasSufficientBalance(decimal amount) => Balance >= amount;
	/// <summary>
	/// Deposit money
	/// </summary>
	public void Deposit(decimal amount)
	{
		if (amount <= 0)
			throw new InvalidOperationException("Deposit amount must be positive");
		Balance += amount;
		TotalDeposited += amount;
		UpdatedAt = DateTime.UtcNow;
	}
	/// <summary>
	/// Withdraw money
	/// </summary>
	public void Withdraw(decimal amount)
	{
		if (amount <= 0)
			throw new InvalidOperationException("Withdrawal amount must be positive");
		if (!HasSufficientBalance(amount))
			throw new InvalidOperationException("Insufficient balance");
		Balance -= amount;
		TotalWithdrawn += amount;
		UpdatedAt = DateTime.UtcNow;
	}
	/// <summary>
	/// Freeze wallet
	/// </summary>
	public void Freeze(string reason = "")
	{
		IsFrozen = true;
		UpdatedAt = DateTime.UtcNow;
	}
	/// <summary>
	/// Unfreeze wallet
	/// </summary>
	public void Unfreeze()
	{
		IsFrozen = false;
		UpdatedAt = DateTime.UtcNow;
	}
}
