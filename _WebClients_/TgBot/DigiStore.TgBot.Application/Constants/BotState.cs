namespace DigiStore.TgBot.Application.Constants;

/// <summary>
/// Bot conversation states
/// </summary>
public static class BotState
{
	public const string Start =							"Start";

	public const string LanguageSelectionAwaiting =		"LanguageSelectionAwaiting";
	public const string LanguageSelected =				"LanguageSelected";
	public const string LanguageChangeAwaiting =		"LanguageChangeAwaiting";

	public const string MainMenu =						"MainMenu";
	public const string ProfileViewing =				"ProfileViewing";
	public const string BalanceViewing =				"BalanceViewing";
	public const string TopUpBalance =					"TopUpBalance";
	public const string CatalogViewing =				"CatalogViewing";
	public const string ProductViewing =				"ProductViewing";

	public const string WithdrawalAwaiting =			"WithdrawalAwaiting";
	public const string WithdrawalConfirming =			"WithdrawalConfirming";
	public const string CheckoutAwaiting =				"AwaitingCheckout";
}
