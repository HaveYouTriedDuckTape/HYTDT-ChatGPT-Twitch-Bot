using System;
using System.Text.RegularExpressions;

public class CPHInline
{
    private const string botName = "ducktapeBot";
    private const int minChatMessageLength = 3;
    private const string broadcasterName = "HaveYouTriedDuckTape";
    private const int chatMessageToSkip = 3;
    private string rawInput;
    private string replythreadUserLogin;
    private string userName;
    private string input0;
    private string triggerName;
    private int ChatGPT_SkippedMessages;
    private bool firstMessage;

    private const string groupName = "botGroup";
    private Platform platform;
    private string userId;

    // Initialisierung der Variablen
    private void InitializeVariables()
    {
        CPH.TryGetArg<string>("rawInput", out rawInput);
        CPH.TryGetArg<string>("reply.threadUserLogin", out replythreadUserLogin);
        CPH.TryGetArg<string>("userName", out userName);
        CPH.TryGetArg<string>("input0", out input0);
        CPH.TryGetArg<string>("triggerName", out triggerName);
        CPH.TryGetArg<bool>("firstMessage", out firstMessage);
        ChatGPT_SkippedMessages = CPH.GetGlobalVar<int>("ChatGPT_SkippedMessages");
        
        platform = Platform.Twitch;
        //string userId = args["userId"].ToString();
        CPH.TryGetArg<string>("userId", out userId);
    }
    
    // Message length check
    private bool MessageLengthCheck()
    {
        if (rawInput.Length < minChatMessageLength)
        {
            CPH.LogDebug($"Message is too short: {rawInput.Length} characters (minimum required: {minChatMessageLength})");
            return true; // Indicates the message is shorter than the required length
        }
        return false; // Indicates the message meets or exceeds the minimum length
    }
    
    // Is the user in the bot group?
    private bool IsUserInBotGroup()
    {
        //CPH.LogDebug($"userId: '{userId}' // platform: '{platform}' // groupName: '{groupName}'");
        if (CPH.UserIdInGroup(userId, platform, groupName))
        {
            CPH.LogDebug($"The user is in the bot group!");
            return true;
        }
        return false;
    }


    public bool Execute()
    {
        InitializeVariables();

        return true;
    }

    public bool Shoutout()
    {
        InitializeVariables();
        // Hauptcode hier
        return true;
    }

    public bool ResponseToRandomChat()
    {
        InitializeVariables();
        Match match;
        
        // Is a bot?
        if (IsUserInBotGroup())
        {
            CPH.LogDebug($"Is a bot!");
            return false;
        }
        
        // Is it a chat command?
        match = Regex.Match(rawInput, "^![^\\s]+");
        if (match.Success)
        {
            CPH.LogDebug($"regexChatcommandCheck: '{rawInput}' was a chat command");
            return false;
        }

        // Is the chat message a thread of the bot/answers to a bot message?
        if (!string.IsNullOrEmpty(replythreadUserLogin))
        {
            match = Regex.Match(replythreadUserLogin, botName);
            if (match.Success)
            {
                CPH.LogDebug($"isducktapeBotInThreadCheck: '{rawInput}' - {botName} is in a Thread");
                return false;
            }
        }

        // Is it a web address?
        match = Regex.Match(rawInput, "\\b(?:https?://)?(?:www\\.)?[a-zA-Z0-9-]+\\.[a-zA-Z]{2,}(?:/[^\\s]*)?\\b");
        if (match.Success)
        {
            CPH.LogDebug($"Is this a web address?:'{rawInput}'");
            return false;
        }

        /*// Is it a @username?
         // -->i change my mind - I want that it also triggers at @username
        match = Regex.Match(rawInput, "@[^\\s@]+");
        if (match.Success)
        {
            CPH.LogDebug($"Is this a @username? :'{rawInput}'");
            return false;
        }*/

        /*// Is userName = botName?
        // --> New method 'IsUserInBotGroup()' which does the same job
        match = Regex.Match(userName, botName);
        if (match.Success)
        {
            CPH.LogDebug($"Is userName = botName?:'{userName}' - botName:'{botName}'");
            return false;
        }*/

        // Is the arg of a command the botName?
        if (!string.IsNullOrEmpty(input0))
        {
            match = Regex.Match(input0, botName);
            if (match.Success)
            {
                CPH.LogDebug($"Is ChatGPT-Bot in thread?:'{input0}' - botName:'{botName}'");
                return false;
            }
        }
        
        // Is it a first time chatter? Or first message of the day?
        if (triggerName == "First Words" || firstMessage == true)
        {
            CPH.LogDebug($"Is it a first time chatter?:'{triggerName}'='First Words'");
            return false;
        }
        
        // Is the message longer than minChatMessageLength?
        if (MessageLengthCheck())
        {
            CPH.LogDebug($"Is the message longer than minChatMessageLength?: MessageLengthCheck(): {MessageLengthCheck()}");
            return false;
        }
        
        // Skip?
        if (ChatGPT_SkippedMessages<chatMessageToSkip)
        {
            CPH.LogDebug($"Skip! ChatGPT_SkippedMessages: {ChatGPT_SkippedMessages} // chatMessageToSkip: {chatMessageToSkip}");
            CPH.SetGlobalVar("ChatGPT_SkippedMessages", ++ChatGPT_SkippedMessages, true);
            return false;
        }
        

        CPH.LogDebug($"Could be a random message:'{rawInput}'");
        return true;
    }

    public bool AnsweringQuestions()
    {
        InitializeVariables();
        return true;
    }

    public bool FistTimeChatter()
    {
        InitializeVariables();
        return true;
    }

    public bool DailyGrettings()
    {
        // Is a bot?
        /*if (userName == "ducktapeBot" || userName == "dotabod")
        {
            CPH.LogDebug($"Is a bot!");
            return false;
        }*/
        
        // Is a bot?
        if (IsUserInBotGroup())
        {
            CPH.LogDebug($"Is a bot!");
            return false;
        }
        
        InitializeVariables();
        return true;
    }
    
}
