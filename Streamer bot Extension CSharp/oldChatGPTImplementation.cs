using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class CPHInline
{
    /// <summary>
    /// Main method to process chat messages with ChatGPT and respond accordingly.
    /// </summary>
    public bool Execute()
    {
        // Retrieve arguments passed to the function
        CPH.TryGetArg<string>("rawInput", out string messageInput);
        CPH.TryGetArg<string>("userName", out string user);
        CPH.TryGetArg<string>("userType", out string userType);
        CPH.TryGetArg<string>("msgId", out string msgId);

        // Fetch required global variables
        string apiKey = CPH.GetGlobalVar<string>("chatGptApiKey", true);
        string gptModel = CPH.GetGlobalVar<string>("chatGptModel", true);
        string gptBehaviorGlobal = CPH.GetGlobalVar<string>("chatGptBehavior", true);
        string gptBehaviorAddon = "Beschränken Sie die maximale Länge der Antworten strikt auf weniger als 975 Zeichen. Vermeiden Sie komplexe Erklärungen. Konzentrieren Sie sich auf eine sachliche, professionelle, direkte und klare Antwort.";
        string gptBehavior = gptBehaviorGlobal + gptBehaviorAddon;

        double gptTemperature = 1.0;

        // Check if user is in the exclusion list
        List<string> exclusionList = CPH.GetGlobalVar<List<string>>("chatGptExclusions", true);
        if (exclusionList != null && exclusionList.Contains(user))
        {
            CPH.LogInfo($"User {user} is on the exclusion list. Skipping GPT processing.");
            return false;
        }

        // Default message if no input is provided
        if (string.IsNullOrWhiteSpace(messageInput))
        {
            messageInput = "Please share a witty comment about interrupting without a proper question.";
        }

        // Process the input and get a response from ChatGPT
        ChatGptApiRequest chatGpt = new ChatGptApiRequest(apiKey);
        string response;
        try
        {
            response = chatGpt.GenerateResponse(messageInput, gptModel, gptBehavior, gptTemperature);
        }
        catch (Exception ex)
        {
            CPH.LogError($"ChatGPT ERROR: {ex.Message}");
            return false;
        }

        // Deserialize and clean the response
        Root root = JsonConvert.DeserializeObject<Root>(response);
        string rawResponse = root.choices[0].message.content;
        string cleanedResponse = CleanResponse(rawResponse);

        // Save response as global variable and argument
        CPH.SetGlobalVar("_chatGptResponse", cleanedResponse, false);
        CPH.SetArgument("finalGpt", cleanedResponse);

        // Send response to the correct platform
        SendMessageToPlatform(userType, user, cleanedResponse, msgId);
        return true;
    }

    /// <summary>
    /// Cleans up the ChatGPT response by removing unnecessary whitespace and escaping sequences.
    /// </summary>
    private string CleanResponse(string response)
    {
        string result = Regex.Replace(response, @"[\r\n]+", " ");
        result = Regex.Unescape(result).Trim();
        return result;
    }

    /// <summary>
    /// Sends a message to the appropriate platform based on user type.
    /// </summary>
    private void SendMessageToPlatform(string userType, string user, string message, string msgId)
    {
        switch (userType)
        {
            case "twitch":
                SendTwitchMessage(user, message, msgId);
                break;
            case "youtube":
                CPH.SendYouTubeMessage($"@{user} {message}", true);
                break;
            case "trovo":
                CPH.SendTrovoMessage($"@{user} {message}", true);
                break;
            default:
                CPH.LogError("Unsupported platform user type.");
                break;
        }
    }

    /// <summary>
    /// Handles Twitch message splitting and sending.
    /// </summary>
    private void SendTwitchMessage(string user, string message, string msgId)
    {
        int maxLength = 473;
        for (int i = 0; i < message.Length; i += maxLength)
        {
            string part = message.Substring(i, Math.Min(maxLength, message.Length - i));
            if (!string.IsNullOrEmpty(msgId))
            {
                CPH.TwitchReplyToMessage($"@{user} {part}", msgId, true);
            }
            else
            {
                CPH.SendMessage(part, true);
            }
            CPH.Wait(250); // Prevent rate-limiting
        }
    }
}

/// <summary>
/// Handles ChatGPT API requests.
/// </summary>
class ChatGptApiRequest
{
    private readonly string _apiKey;
    private const string Endpoint = "https://api.openai.com/v1/chat/completions";

    public ChatGptApiRequest(string apiKey)
    {
        _apiKey = apiKey;
    }

    public string GenerateResponse(string prompt, string model, string behavior, double temperature)
    {
        string requestBody = JsonConvert.SerializeObject(new
        {
            model,
            max_tokens = 250,
            temperature,
            messages = new[]
            {
                new { role = "system", content = behavior },
                new { role = "user", content = prompt }
            }
        });

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Endpoint);
        request.Headers.Add("Authorization", "Bearer " + _apiKey);
        request.ContentType = "application/json";
        request.Method = "POST";

        using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
        {
            writer.Write(requestBody);
        }

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
            return reader.ReadToEnd();
        }
    }
}

// Models for deserializing the API response
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
    public string role { get; set; }
    public string content { get; set; }
}
