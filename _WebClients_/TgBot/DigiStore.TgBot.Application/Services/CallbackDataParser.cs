using StudTgBotApi.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.TgBot.Application.Services;

internal class CallbackDataParser : ICallbackDataParser
{
    public bool TryParse(string callbackData, out string type, out string action)
    {
        throw new NotImplementedException();
    }
}
