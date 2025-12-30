namespace DigiStore.TgBot.Application.Constants;

/// <summary>
/// Bot conversation states
/// </summary>
public static class BotState
{
	public const string Start = "Start";
	public const string AwaitingLanguageSelection = "AwaitingLanguageSelection";
	public const string LanguageSelected = "LanguageSelected";
	public const string MainMenu = "MainMenu";
	public const string ViewingProfile = "ViewingProfile";
	public const string ViewingBalance = "ViewingBalance";
	public const string AwaitingLanguageChange = "AwaitingLanguageChange";
	public const string AwaitingWithdrawal = "AwaitingWithdrawal";
	public const string ConfirmingWithdrawal = "ConfirmingWithdrawal";
	public const string ViewingCatalog = "ViewingCatalog";
	public const string ViewingProduct = "ViewingProduct";
	public const string AwaitingCheckout = "AwaitingCheckout";
}
