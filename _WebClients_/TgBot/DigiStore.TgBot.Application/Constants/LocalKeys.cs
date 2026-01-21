namespace DigiStore.TgBot.Application.Constants;


public static class LocalKeys
{
	public static class Messages
	{
		public const string MainMenu =					"msg_main_menu";
		public const string TopUpAmountRequest =		"msg_topup_amount_request";
		public const string TopUpAmountInputErrorAmount =		"msg_topup_amount_input_error_mount";
		public const string TopUpAmountInputErrorAggregator =	"msg_topup_amount_input_error_aggregator";
		public const string SelectLanguage =			"msg_select_language";
		public const string Welcome =					"msg_welcome";
		public const string LanguageChanged =			"msg_language_changed";
	}

	public static class Templates
	{
		public const string BalanceView =		"tmpl_balance_view";
		public const string Balance =			"tmpl_balance";
        public const string Profile =			"tmpl_profile";
		public const string TopUpBalance =		"tmpl_topup_balance";
	}


	public static class Buttons
	{
		public const string Back =      "button_back";
		public const string Cancel =    "button_cancel";
		public const string Ok =        "button_ok";
		public const string Yes =       "button_yes";
		public const string No =        "button_no";
		public const string BalanceUpYookassa =		"button_balance_up_yookassa";
		public const string BalanceUpFreekassa =	"button_balance_up_freekassa";
		
		public const string MainMenu =  "button_main_menu";
		public const string Profile =	"button_profile";
		public const string Balance =	"button_balance";
		public const string Catalog =	"button_catalog";
		public const string Orders =	"button_orders";
		public const string Settings =	"button_settings";
		public const string Help =		"button_help";
		public const string ChangeLanguage =		"button_change_language";
	}


	public static class Errors
	{
		public const string Occurred =          "error_occurred";
		public const string SessionExpired =    "error_session_expired";
		public const string UserNotFound =      "error_user_not_found";
		public const string OperationFailed =   "error_operation_failed";
	}
}
