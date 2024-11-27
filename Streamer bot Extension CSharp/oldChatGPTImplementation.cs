using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Streamer.bot.Common.Events;

public class CPHInline
{
    public bool Execute()
    {
        // Main logic to process ChatGPT responses for general input
        CPH.TryGetArg<string>("rawInput", out string messageInput);
        CPH.TryGetArg<string>("userName", out string user);
        CPH.TryGetArg<string>("userType", out string userType);
        CPH.TryGetArg<string>("msgId", out string msgId);

        string apiKey = CPH.GetGlobalVar<string>("chatGptApiKey", true);
        string gptModel = CPH.GetGlobalVar<string>("chatGptModel", true);
        string gptBehaviorGlobal = CPH.GetGlobalVar<string>("chatGptBehavior", true);

        string gptBehaviorAddon = "Keep responses under 975 characters, avoid complex explanations, and ensure responses are professional and concise.";
        string gptBehavior = gptBehaviorGlobal + gptBehaviorAddon;
        double gptTempValue = 1.0;

        List<string> exclusionList = CPH.GetGlobalVar<List<string>>("chatGptExclusions", true);
        if (exclusionList != null && exclusionList.Contains(user))
        {
            CPH.LogInfo($"User {user} is on the exclusion list. Skipping GPT processing.");
            return false;
        }

        messageInput = messageInput?.Replace("\"", "\\\"") ?? "Provide a witty response for an unexpected interruption.";

        ChatGptApiRequest chatGpt = new ChatGptApiRequest(apiKey);
        string response;

        try
        {
            response = chatGpt.GenerateResponse(messageInput, gptModel, gptBehavior, gptTempValue.ToString());
        }
        catch (Exception ex)
        {
            CPH.LogError($"ChatGPT ERROR: {ex.Message}");
            return false;
        }

        Root root = JsonConvert.DeserializeObject<Root>(response);
        string finalGpt = CleanResponse(root.choices[0].message.content);

        CPH.SetGlobalVar("_chatGptResponse", finalGpt, false);
        CPH.SetArgument("finalGpt", finalGpt);

        SendMessageToPlatform(userType, user, finalGpt, msgId);
        return true;
    }

    public bool ChatGptShoutouts()
    {
        // Logic for creating Twitch shoutout messages
        string targetUser = args["targetUser"].ToString();
        string targetDescription = args["targetDescription"].ToString();
        string targetGame = args["game"].ToString();
        string targetTags = args["tagsDelimited"].ToString();
        string userType = args["userType"].ToString();
        string targetLink = $"https://twitch.tv/{targetUser}";

        string prompt = $"Create a witty shoutout for @{targetUser} encouraging viewers to watch their stream. The response should include {targetUser} and {targetLink}, but no hashtags.";
        string apiKey = CPH.GetGlobalVar<string>("chatGptApiKey", true);
        string gptModel = CPH.GetGlobalVar<string>("chatGptModel", true);
        string gptBehavior = CPH.GetGlobalVar<string>("chatGptBehavior", true) + $" Include details: {targetUser}, {targetDescription}, {targetGame}, {targetTags}.";

        ChatGptApiRequest chatGpt = new ChatGptApiRequest(apiKey);
        string response;

        try
        {
            response = chatGpt.GenerateResponse(prompt, gptModel, gptBehavior, "1");
        }
        catch (Exception ex)
        {
            CPH.LogError($"ChatGPT ERROR: {ex.Message}");
            return false;
        }

        Root root = JsonConvert.DeserializeObject<Root>(response);
        string finalGpt = CleanResponse(root.choices[0].message.content);

        SendMessageToPlatform(userType, targetUser, finalGpt, null);
        return true;
    }

    public bool ChatGptGreetings()
    {
        // Sends a personalized greeting to new users in the chat
        string user = args["user"].ToString();
        string broadcastUser = args["broadcastUser"].ToString();
        string targetDescription = args["targetDescription"].ToString();
        string targetGame = args["game"].ToString();
        string actionQueuedAt = args["actionQueuedAt"].ToString();
        string userType = args["userType"].ToString();

        string prompt = $"Welcome the new chatter '{user}' to the stream. The message should introduce the bot as 'DuckTapeBot' and be under 320 characters.";
        string apiKey = CPH.GetGlobalVar<string>("chatGptApiKey", true);
        string gptModel = CPH.GetGlobalVar<string>("chatGptModel", true);
        string gptBehavior = CPH.GetGlobalVar<string>("chatGptBehavior", true) +
            $" Include details: {broadcastUser}, {targetDescription}, {targetGame}, and the time '{actionQueuedAt}'.";

        ChatGptApiRequest chatGpt = new ChatGptApiRequest(apiKey);
        string response;

        try
        {
            response = chatGpt.GenerateResponse(prompt, gptModel, gptBehavior, "1");
        }
        catch (Exception ex)
        {
            CPH.LogError($"ChatGPT ERROR: {ex.Message}");
            return false;
        }

        Root root = JsonConvert.DeserializeObject<Root>(response);
        string finalGpt = CleanResponse(root.choices[0].message.content);

        SendMessageToPlatform(userType, user, finalGpt, null);
        return true;
    }

    public bool ChatGptFirstMessage()
    {
        // Welcomes a viewer posting their first message in chat
        string user = args["user"].ToString();
        string broadcastUser = args["broadcastUser"].ToString();
        string broadcasterGame = args["broadcaster_game"].ToString();
        string actionQueuedAt = args["actionQueuedAt"].ToString();
        string targetPreviousActive = args["targetPreviousActive"].ToString();
        string userType = args["userType"].ToString();

        string prompt = $"Welcome {user} for their first chat message. Keep the response under 200 characters.";
        string apiKey = CPH.GetGlobalVar<string>("chatGptApiKey", true);
        string gptModel = CPH.GetGlobalVar<string>("chatGptModel", true);
        string gptBehavior = CPH.GetGlobalVar<string>("chatGptBehavior", true) +
            $" Include details: '{broadcastUser}', '{broadcasterGame}', and last active '{targetPreviousActive}'.";

        ChatGptApiRequest chatGpt = new ChatGptApiRequest(apiKey);
        string response;

        try
        {
            response = chatGpt.GenerateResponse(prompt, gptModel, gptBehavior, "1");
        }
        catch (Exception ex)
        {
            CPH.LogError($"ChatGPT ERROR: {ex.Message}");
            return false;
        }

        Root root = JsonConvert.DeserializeObject<Root>(response);
        string finalGpt = CleanResponse(root.choices[0].message.content);

        SendMessageToPlatform(userType, user, finalGpt, null);
        return true;
    }

    public bool ChatGptTimerTrigger()
    {
        // Periodically triggered action
        string rawInput = CPH.GetGlobalVar<string>("ChatGPT_rawInput_save", true);
        string user = CPH.GetGlobalVar<string>("ChatGPT_user_save", true);
        string broadcastUser = args["broadcastUser"].ToString();
        string broadcasterGame = args["broadcaster_game"].ToString();
        string actionQueuedAt = args["actionQueuedAt"].ToString();
        string userType = "twitch"; // Default to Twitch

        string prompt = $"Write a fun response to the chat message from '{user}': '{rawInput}'.";
        string apiKey = CPH.GetGlobalVar<string>("chatGptApiKey", true);
        string gptModel = CPH.GetGlobalVar<string>("chatGptModel", true);
        string gptBehavior = CPH.GetGlobalVar<string>("chatGptBehavior", true) +
            $" Include details: '{broadcastUser}', '{broadcasterGame}', and the current time '{actionQueuedAt}'.";

        ChatGptApiRequest chatGpt = new ChatGptApiRequest(apiKey);
        string response;

        try
        {
            response = chatGpt.GenerateResponse(prompt, gptModel, gptBehavior, "1");
        }
        catch (Exception ex)
        {
            CPH.LogError($"ChatGPT ERROR: {ex.Message}");
            return false;
        }

        Root root = JsonConvert.DeserializeObject<Root>(response);
        string finalGpt = CleanResponse(root.choices[0].message.content);

        CPH.TwitchReplyToMessage(finalGpt, CPH.GetGlobalVar<string>("ChatGPT_msgId_save", true), true);
        return true;
    }

    private void SendMessageToPlatform(string userType, string user, string message, string msgId)
    {
        // Handles platform-specific message sending logic
        int maxLength = 473; // Max length for platforms
        for (int i = 0; i < message.Length; i += maxLength)
        {
            string messagePart = message.Substring(i, Math.Min(maxLength, message.Length - i));
            if (userType == "twitch")
            {
                if (!string.IsNullOrEmpty(msgId))
                {
                    CPH.TwitchReplyToMessage($"@{user} {messagePart}", msgId, true);
                }
                else
                {
                    CPH.SendMessage(messagePart, true);
                }
            }
            else if (userType == "youtube")
            {
                CPH.SendYouTubeMessage($"@{user} {messagePart}", true);
            }
            else if (userType == "trovo")
            {
                CPH.SendTrovoMessage($"@{user} {messagePart}", true);
            }
            CPH.Wait(250); // Delay to avoid spam
        }
    }

    private string CleanResponse(string response)
    {
        // Cleans up ChatGPT response strings
        string cleaned = Regex.Replace(response, @"\r\n?|\n", " ");
        cleaned = Regex.Replace(cleaned, @"[\r\n]+", " ");
        return Regex.Unescape(cleaned).Trim();
    }
}

class ChatGptApiRequest
{
    private readonly string _apiKey;
    private const string _endpoint = "https://api.openai.com/v1/chat/completions";

    public ChatGptApiRequest(string apiKey) => _apiKey = apiKey;

    public string GenerateResponse(string prompt, string gptModel, string content, string gptTempValue)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_endpoint);
        request.Headers.Add("Authorization", "Bearer " + _apiKey);
        request.ContentType = "application/json";
        request.Method = "POST";

        string requestBody = JsonConvert.SerializeObject(new
        {
            model = gptModel,
            max_tokens = 250,
            temperature = gptTempValue,
            messages = new[]
            {
                new { role = "system", content },
                new { role = "user", content = prompt }
            }
        });

        byte[] bytes = Encoding.UTF8.GetBytes(requestBody);
        request.ContentLength = bytes.Length;

        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(bytes, 0, bytes.Length);
        }

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream responseStream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
        {
            return reader.ReadToEnd();
        }
    }
}

public class Root
{
    public List<Choice> choices { get; set; }
}

public class Choice
{
    public Message message { get; set; }
}

public class Message
{
    public string content { get; set; }
}
