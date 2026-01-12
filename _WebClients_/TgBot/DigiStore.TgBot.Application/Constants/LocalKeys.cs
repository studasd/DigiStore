namespace DigiStore.TgBot.Application.Constants;


public static class LocalKeys
{
	public static class Greetings
	{
		public const string Greeting = "greeting";
	}

	public static class Navigations
	{
		public const string SelectLanguage =	"navi_select_language";
		public const string LanguageChanged =	"navi_language_changed";
		public const string MainMenu =			"navi_main_menu";
		public const string ChooseOption =		"navi_choose_option";
	}

	public static class Commands
	{
		public const string Profile =		"command_profile";
		public const string Balance =		"command_balance";
		public const string Catalog =		"command_catalog";
		public const string Orders =		"command_orders";
		public const string Settings =		"command_settings";
		public const string Help =			"command_help";
		public const string ChangeLanguage = "command_change_language";
	}

	// Profile
	public static class Profiles
	{
		public const string Info =			"profile_info";
		public const string Email =			"profile_email";
		public const string FullName =		"profile_full_name";
		public const string Username =		"profile_telegram_username";
		public const string UserRoles =		"profile_user_roles";
		public const string Status =		"profile_status";
		public const string Roles =			"profile_roles";
		public const string CreatedAt =		"profile_created_at";
		public const string UpdatedAt =		"profile_updated_at";
		public const string Language =		"profile_language";
	}
	

	public static class Balances
	{
		public const string Info =				"balance_info";
		public const string CurrentBalance =	"balance_current_balance";
		public const string TotalDeposited =	"balance_total_deposited";
		public const string TotalWithdrawn =	"balance_total_withdrawn";
		public const string LinkedAccounts =	"balance_linked_accounts";
		public const string InsufficientBalance = "balance_insufficient_balance";
	}


	public static class Buttons
	{
		public const string Back =      "button_back";
		public const string Cancel =    "button_cancel";
		public const string Ok =        "button_ok";
		public const string Yes =       "button_yes";
		public const string No =        "button_no";
	}

	public static class Errors
	{
		public const string Occurred =          "error_occurred";
		public const string SessionExpired =    "error_session_expired";
		public const string UserNotFound =      "error_user_not_found";
		public const string OperationFailed =   "error_operation_failed";
	}
}
