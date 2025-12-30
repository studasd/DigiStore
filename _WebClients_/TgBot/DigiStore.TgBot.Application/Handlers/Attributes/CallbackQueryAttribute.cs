namespace DigiStore.TgBot.Application.Handlers.Attributes;

/// <summary>
/// Атрибут для маркировки обработчиков колбэков
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CallbackQueryAttribute : Attribute
{
	/// <summary>
	/// Префикс или точное значение callback data, которое обрабатывает хэндлер
	/// </summary>
	public string CallbackData { get; }

	/// <summary>
	/// Если true, то CallbackData используется как префикс для поиска
	/// </summary>
	public bool IsPrefix { get; }

	public CallbackQueryAttribute(string callbackData, bool isPrefix = false)
	{
		CallbackData = callbackData;
		IsPrefix = isPrefix;
	}
}

