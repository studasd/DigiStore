namespace DigiStore.TgBot.Application.Handlers.Attributes;

/// <summary>
/// Атрибут для маркировки обработчиков команд
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
	/// <summary>
	/// Команда, которую обрабатывает хэндлер (например, "/start")
	/// </summary>
	public string Command { get; }

	public CommandAttribute(string command)
	{
		Command = command;
	}
}

