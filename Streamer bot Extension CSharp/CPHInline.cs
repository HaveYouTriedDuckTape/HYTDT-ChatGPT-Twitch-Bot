// using System;
using System.Text.RegularExpressions;

//namespace Streamer_bot_Extension_CSharp;
public class CPHInline
{
    public bool Execute()
    {
        CPH.TryGetArg<string>("rawInput", out string rawInput);
        CPH.TryGetArg<string>("reply.threadUserLogin", out string replythreadUserLogin);
        CPH.TryGetArg<string>("userName", out string userName);
        CPH.TryGetArg<string>("input0", out string input0);
        CPH.TryGetArg<string>("triggerName", out string triggerName);
        
        string ChatGPT_SkippedMessages = CPH.GetGlobalVar<string>("ChatGPT_SkippedMessages");
        
        Match regexChatcommandCheck = Regex.Match(rawInput, "^![^\\s]+");
        if (regexChatcommandCheck.Success)
        {
            CPH.LogDebug($"regexChatcommandCheck: '{rawInput}' was a chat command");
            return false;
        }
        
        if (!string.IsNullOrEmpty(replythreadUserLogin))
        {
            Match isducktapeBotInThreadCheck = Regex.Match(replythreadUserLogin, "ducktapeBot");
            if (isducktapeBotInThreadCheck.Success)
            {
                CPH.LogDebug($"isducktapeBotInThreadCheck: '{rawInput}' ducktapeBot is in Thread");
                return false;
            }
        }
        else
        {
            CPH.LogDebug($"isducktapeBotInThreadCheck: '{rawInput}' is null or empty");
        }

        return true;
    }

    public bool Shoutout()
    {
        // your main code goes here
        return true;
    }

    public bool ResponseToRandomChat()
    {
        // your main code goes here
        return true;
    }

    public bool AnsweringQuestions()
    {
        // your main code goes here
        return true;
    }
}