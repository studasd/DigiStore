namespace DigiStore.TgBot.Application.Constants;

/// <summary>
/// Bot callback data prefixes (for inline buttons)
/// </summary>
public static class CallbackData
{
	public const string LanguagePrefix =		"lang:";
	public const string LanguageChangePrefix =	"lang_change:";
	public const string ProfileCallback =		"profile_cb";
	public const string BalanceCallback =		"balance_cb";
	public const string BalanceTopPrefix =		"balance_top:";
	public const string BalanceUpPrefix =		"balance_up_do:";
	public const string MenuMain =				"menu_main";
	public const string MenuBack =				"menu_back";
	public const string CatalogCallback =		"catalog_cb";
	public const string ProductCallback =		"product:";
	public const string OrderHistory =			"orders_history";
}
