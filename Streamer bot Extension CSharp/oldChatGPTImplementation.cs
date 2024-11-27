//code bastardized by Mustached_Maniac
//https://ko-fi.com/mustached_maniac/tip
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
        CPH.TryGetArg<string>("rawInput", out string messageInput);
        CPH.TryGetArg<string>("userName", out string user);
        CPH.TryGetArg<string>("userType", out string userType);
        CPH.TryGetArg<string>("msgId", out string msgId);
        
        
        string apiKey = CPH.GetGlobalVar<string>("chatGptApiKey", true);
        string gptModel = CPH.GetGlobalVar<string>("chatGptModel", true);
        string gptBehaviorGlobal = CPH.GetGlobalVar<string>("chatGptBehavior", true);
        // (edit) string gptBehaviorAddon = "Strictly limit the response length to a maximum of 200 characters. Avoid complex explanations and focus on delivering straightforward, clear, and brief responses that directly address the user's query or comment.";
        //string gptBehaviorAddon = "Versuche die Antworten unter 200 Satzzeichen zuhalten und beschränke die Maximalantwortlänge der Antworten strikt auf unter 500 Satzzeichen. Vermeide dabei komplexe Erklärungen und konzentriere dich auf direkte, klare und kurze Antworten, die direkt auf die Anfrage oder den Kommentar des Benutzers eingehen.";
        //string gptBehaviorAddon = "Beschränken Sie die maximale Länge der Antworten strikt auf unter 1000 Satzzeichen und versuchen Sie, die Antwort so gut wie möglich unter 200 Satzzeichen zu halten. Vermeide komplexe Erklärungen und konzentriere dich sich auf direkte, klare und kurze Antworten.";
        //string gptBehaviorAddon = "Beschränken Sie die maximale Länge der Antworten strikt auf weniger als 1000 Satzzeichen und versuchen Sie, die Antwort so kurz wie möglich zu halten (unter 200 Satzzeichen). Vermeiden Sie komplexe Erklärungen und konzentrieren Sie sich auf direkte, klare und kurze Antworten. Vermeide dabei Nachfragen.";
        string gptBehaviorAddon = "Beschränken Sie die maximale Länge der Antworten strikt auf weniger als 975 Satzzeichen. Vermeiden Sie komplexe Erklärungen. Konzentrieren Sie sich auf eine wissenschaftliche, sachliche, professionelle, direkte und klare Antwort.";
        string gptBehavior = gptBehaviorGlobal + gptBehaviorAddon;
        string gptTemperature = "1";
        double gptTempValue = Convert.ToDouble(gptTemperature);
        //string messageInput = args["rawInput"].ToString();
        //string user = args["userName"].ToString();
        List<string> exclusionList = CPH.GetGlobalVar<List<string>>("chatGptExclusions", true);
        if (exclusionList != null && exclusionList.Contains(user))
        {
            CPH.LogInfo($"User {user} is on the exclusion list. Skipping GPT processing.");
            return false;
        }

        messageInput = messageInput.Replace("\"", "\\\"");
        if (string.IsNullOrWhiteSpace(messageInput))
        {
            messageInput = "send a snarky comment about how rude it is to interrupt somebody, or say their name without asking a question";
        }

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

        Root root = JsonConvert.DeserializeObject<Root>(response);
        string myString = root.choices[0].message.content;
        CPH.LogInfo("GPT myString " + myString);
        string myStringCleaned0 = myString.Replace(System.Environment.NewLine, " ");
        string mystringCleaned1 = Regex.Replace(myStringCleaned0, @"\r\n?|\n", " ");
        string myStringCleaned2 = Regex.Replace(mystringCleaned1, @"[\r\n]+", " ");
        string unescapedString = Regex.Unescape(myStringCleaned2);
        string finalGpt = unescapedString.Trim();
        CPH.SetGlobalVar("_chatGptResponse", finalGpt, false);
        CPH.SetArgument("finalGpt", finalGpt);
        //string userType = args["userType"].ToString();
        if (userType == "twitch")
        {
        	//string msgId = args["msgId"].ToString();
            string message = finalGpt;
            int maxLength = 473;
            int totalLength = message.Length;
            //string userL = "@{user} ";
            int firstMSG = 1;
            for (int i = 0; i < totalLength; i += maxLength)
            {
                // Berechne die Länge des aktuellen Abschnitts
                int length = Math.Min(maxLength, totalLength - i);
                string messagePart = message.Substring(i, length);
                ;
                if (firstMSG == 1)
                {
                    firstMSG = 0;
                    if (!string.IsNullOrEmpty(msgId))
                    {
                        CPH.TwitchReplyToMessage($"@{user} {messagePart}", msgId, true);
                    }
                    else
                    {
                        CPH.SendMessage(messagePart, true);
                    }
                }
                else
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

                CPH.LogInfo("Sent SO message to Twitch");
                CPH.Wait(250);
            }
        //CPH.SendMessage(finalGpt, true);
        //CPH.LogInfo("Sent SO message to Twitch");
        //CPH.SendMessage($"@{user} {finalGpt}", true);
        //CPH.LogInfo("Sent message to Twitch");
        }
        else if (userType == "youtube")
        {
            CPH.SendYouTubeMessage($"@{user} {finalGpt}", true);
            CPH.LogInfo("Sent message to YouTube");
        }
        else if (userType == "trovo")
        {
            CPH.SendTrovoMessage($"@{user} {finalGpt}", true);
            CPH.LogInfo("Sent message to Trovo");
        }

        return true;
    }

    public bool ChatGptShoutouts()
    {
        string targetUser = args["targetUser"].ToString();
        string targetDescription = args["targetDescription"].ToString();
        string targetGame = args["game"].ToString();
        string targetTags = args["tagsDelimited"].ToString();
        string userType = args["userType"].ToString();
        string targetLink = $"https://twitch.tv/{targetUser}";
        // (edit)
        // string prompt = $"Construct a witty message that doesn't exceed 400 characters, encouraging viewers to watch @{targetUser}'s stream.  Ensure the response includes @{targetUser} and {targetLink}, does NOT include hashtags and put a space after the link";
        string prompt = $"Schreiben Sie im Twitch-Live-Stream-Chat eine witzige Nachricht, die 450 Zeichen nicht überschreitet, die die Zuschauer dazu anregt, sich den Stream von {targetUser} anzusehen. Stellen Sie sicher, dass die Antwort {targetUser} und {targetLink} enthält, KEINE Hashtags enthält.";
        string apiKey = CPH.GetGlobalVar<string>("chatGptApiKey", true);
        string gptModel = CPH.GetGlobalVar<string>("chatGptModel", true);
        string gptBehaviorGlobal = CPH.GetGlobalVar<string>("chatGptBehavior", true);
        string gptShoutoutAddon = $"Erstellen Sie Ihre Antwort mit Informationen aus den folgenden Daten: {targetUser}, {targetDescription}, {targetGame}, {targetTags}";
        string gptBehavior = gptBehaviorGlobal + gptShoutoutAddon;
        string gptTemperature = "1";
        double gptTempValue = Convert.ToDouble(gptTemperature);
        ChatGptApiRequest chatGpt = new ChatGptApiRequest(apiKey);
        
        string response;
        try
        {
            response = chatGpt.GenerateResponse(prompt, gptModel, gptBehavior, gptTemperature);
        }
        catch (Exception ex)
        {
            CPH.LogError($"ChatGPT ERROR: {ex.Message}");
            return false;
        }

        Root root = JsonConvert.DeserializeObject<Root>(response);
        string finalGpt = root.choices[0].message.content;
        finalGpt = finalGpt.Trim('\"');
        CPH.SetArgument("finalGpt", finalGpt);
        if (userType == "twitch")
        {
            string message = finalGpt;
            int maxLength = 500;
            int totalLength = message.Length;
            for (int i = 0; i < totalLength; i += maxLength)
            {
                // Berechne die Länge des aktuellen Abschnitts
                int length = Math.Min(maxLength, totalLength - i);
                // Hole den Teilstring und sende ihn
                string messagePart = message.Substring(i, length);
                //SendMessages(messagePart);
                CPH.SendMessage(messagePart, true);
                CPH.LogInfo("Sent SO message to Twitch");
            }
        //CPH.SendMessage(finalGpt, true);
        //CPH.LogInfo("Sent SO message to Twitch");
        }
        else if (userType == "youtube")
        {
            CPH.SendYouTubeMessage("Sorry, ChatGPT shoutouts are only available to Twitch users at this time.", true);
            CPH.LogInfo("Sent message to YouTube-SO for Twitch only");
        }
        else if (userType == "trovo")
        {
            CPH.SendTrovoMessage("Sorry, ChatGPT shoutouts are only available to Twitch users at this time.", true);
            CPH.LogInfo("Sent message to Trovo-SO for Twitch only");
        }

        return true;
    }

    public bool ChatGptGreetings()
    {
        //string targetUser = args["targetUser"].ToString();
        string actionQueuedAt = args["actionQueuedAt"].ToString();
        string user = args["user"].ToString();
        //string inputEscaped0 = args["inputEscaped0"].ToString();
        string source = args["__source"].ToString();
        string targetDescription = args["targetDescription"].ToString();
        string targetGame = args["game"].ToString();
        //string targetTags = args["tagsDelimited"].ToString();
        string userType = args["userType"].ToString();
        //string targetLink = $"https://twitch.tv/{targetUser}";
        // (edit)
        string broadcastUser = args["broadcastUser"].ToString();
        string targetChannelTitle = args["targetChannelTitle"].ToString();
        
        /* string user;
        if (source != "TwitchChatMessage")
        {
            user = userName;
        }
        else
        {
            user = inputEscaped0;
        } */

        string prompt = $"Begrüßen Sie im Live-Stream-Chat den neuen Chatter '{user}' mit einer Willkommensnachricht, die 320 Zeichen nicht überschreitet. Stellen Sie sich dabei selbst vor; Du bist der ChatGPT-Chat-Bot names 'DuckTapeBot'.";
        string apiKey = CPH.GetGlobalVar<string>("chatGptApiKey", true);
        string gptModel = CPH.GetGlobalVar<string>("chatGptModel", true);
        string gptBehaviorGlobal = CPH.GetGlobalVar<string>("chatGptBehavior", true);
        //string gptShoutoutAddon = $"Erstelle die Antwort mit Hilfe von der folgenden Informationen: {targetDescription}";
        string gptShoutoutAddon = $"Erstelle die Antwort mit Hilfe von der folgenden Informationen: Der Live-Stream-Kanalname und der Name des Streamers ist '{broadcastUser}' - Die Beschreibung vom Live-Stream-Kanalname ist'{targetDescription}' - Der jetztige Zeitpunkt ist '{actionQueuedAt}' - Im Moment wird in der Kategorie:'{targetGame}' gestreamt mit dem Live-Stream-Titel:'{targetChannelTitle}'";
        string gptBehavior = gptBehaviorGlobal + gptShoutoutAddon;
        string gptTemperature = "1";
        double gptTempValue = Convert.ToDouble(gptTemperature);
        ChatGptApiRequest chatGpt = new ChatGptApiRequest(apiKey);
        string response;
        try
        {
            response = chatGpt.GenerateResponse(prompt, gptModel, gptBehavior, gptTemperature);
        }
        catch (Exception ex)
        {
            CPH.LogError($"ChatGPT ERROR: {ex.Message}");
            return false;
        }

        Root root = JsonConvert.DeserializeObject<Root>(response);
        string finalGpt = root.choices[0].message.content;
        finalGpt = finalGpt.Trim('\"');
        CPH.SetArgument("finalGpt", finalGpt);
        if (userType == "twitch")
        {
            CPH.SendMessage(finalGpt, true);
            CPH.LogInfo("Sent SO message to Twitch");
        }
        else if (userType == "youtube")
        {
            CPH.SendYouTubeMessage($"@{user} {finalGpt}", true);
            CPH.LogInfo("Sent message to YouTube");
        }
        else if (userType == "trovo")
        {
            CPH.SendTrovoMessage($"@{user} {finalGpt}", true);
            CPH.LogInfo("Sent message to Trovo");
        }

        return true;
    }

    public bool ChatGptFirstMessage()
    {
        string targetUser = args["targetUser"].ToString();
        //string userName = args["userName"].ToString();
        string user = args["user"].ToString();
        string actionQueuedAt = args["actionQueuedAt"].ToString();
        string targetPreviousActive = args["targetPreviousActive"].ToString();
        // (edit)
        //string inputEscaped0 = args["inputEscaped0"].ToString();
        //string source = args["__source"].ToString();
        //string targetDescription = args["targetDescription"].ToString();
        //string targetGame = args["game"].ToString();
        //string targetTags = args["tagsDelimited"].ToString();
        string userType = args["userType"].ToString();
        //string targetLink = $"https://twitch.tv/{targetUser}";
        // (edit)
        //string greetUser = userName;
        
        string broadcastUser = args["broadcastUser"].ToString();
        string broadcaster_targetChannelTitle = args["broadcaster_targetChannelTitle"].ToString();
        string broadcaster_game = args["broadcaster_game"].ToString();
        
        string prompt = $"Begrüße im Live-Stream-Chat den Live-Stream-Zuschauer {user} mit einer Chat-Nachricht, die weniger als 200 Zeichen sein sollte.";
        string apiKey = CPH.GetGlobalVar<string>("chatGptApiKey", true);
        string gptModel = CPH.GetGlobalVar<string>("chatGptModel", true);
        string gptBehaviorGlobal = CPH.GetGlobalVar<string>("chatGptBehavior", true);
        //string gptShoutoutAddon = $"Erstelle die Antwort mit Hilfe von der folgenden Informationen: {targetDescription}";
        string gptShoutoutAddon = $" Du kannst folgenden Informationen zur Hilfe nehmen bei der Erstellung der Antwort: Das jetztige Datum und Uhrzeit ist '{actionQueuedAt}' - Der heute aktive Zuschauer '{user}' war zuletzt am '{targetPreviousActive}' im Live-Stream-Chat aktiv - Der Livestream ist derzeit in der Kategorie '{broadcaster_game}' live auf Twitch mit dem Titel:'{broadcaster_targetChannelTitle}'";
        string gptBehavior = gptBehaviorGlobal + gptShoutoutAddon;
        string gptTemperature = "1";
        double gptTempValue = Convert.ToDouble(gptTemperature);
        ChatGptApiRequest chatGpt = new ChatGptApiRequest(apiKey);
        string response;
        try
        {
            response = chatGpt.GenerateResponse(prompt, gptModel, gptBehavior, gptTemperature);
        }
        catch (Exception ex)
        {
            CPH.LogError($"ChatGPT ERROR: {ex.Message}");
            return false;
        }

        Root root = JsonConvert.DeserializeObject<Root>(response);
        string finalGpt = root.choices[0].message.content;
        finalGpt = finalGpt.Trim('\"');
        CPH.SetArgument("finalGpt", finalGpt);
        if (userType == "twitch")
        {
            CPH.SendMessage(finalGpt, true);
            CPH.LogInfo("Sent SO message to Twitch");
        }
        else if (userType == "youtube")
        {
            CPH.SendYouTubeMessage($"@{user} {finalGpt}", true);
            CPH.LogInfo("Sent message to YouTube");
        }
        else if (userType == "trovo")
        {
            CPH.SendTrovoMessage($"@{user} {finalGpt}", true);
            CPH.LogInfo("Sent message to Trovo");
        }

        return true;
    }

    public bool ChatGptTimerTrigger()
    {
    	string broadcaster_game = args["broadcaster_game"].ToString();
    	string actionQueuedAt = args["actionQueuedAt"].ToString();
    	string broadcastUser = args["broadcastUser"].ToString();
    	
        //string messageInput = args["rawInput"].ToString();
        //string greetUser = args["user"].ToString();
        // string userType = args["userType"].ToString();
        string rawInput = CPH.GetGlobalVar<string>("ChatGPT_rawInput_save", true);
        string user = CPH.GetGlobalVar<string>("ChatGPT_user_save", true);
        // (edit)
        //string prompt = $"Schreiben im Twitch-Stream-Chat eine spaßige Antwort auf die folgende Chat-Nachricht des Zuschauers mit dem Benutzernamen '{user}': '{rawInput}'. Beginne deine Antwortnachricht mit dem Benutzernamen des Zuschauers. Füge immer ein '@' vor dem Benutzernamen hinzu.";
        string prompt = $"Schreibe im Twitch-Stream-Chat eine spaßige Antwort auf die folgende Chat-Nachricht des Zuschauers mit dem Benutzernamen '{user}': '{rawInput}'";
        string apiKey = CPH.GetGlobalVar<string>("chatGptApiKey", true);
        string gptModel = CPH.GetGlobalVar<string>("chatGptModel", true);
        string gptBehaviorGlobal = CPH.GetGlobalVar<string>("chatGptBehavior", true);
        //string gptShoutoutAddon = $"Erstelle die Antwort mit Hilfe von der folgenden Informationen: {targetDescription}";
        //"Beschränken Sie die maximale Länge der Antworten strikt auf weniger als 1000 Satzzeichen und versuchen Sie, die Antwort so kurz wie möglich zu halten (unter 200 Satzzeichen). Vermeiden Sie komplexe Erklärungen und konzentrieren Sie sich auf direkte, klare und kurze Antworten. Vermeide dabei Nachfragen."
        //string gptShoutoutAddon = $"Erstelle die Antwort mit Hilfe von der folgenden Informationen: 'Das jetztige Datum und Uhrzeit sind {actionQueuedAt}', 'Der heute aktive Zuschauer {greetUser} war zuletzt am {actionQueuedAt} im Live-Stream-Chat aktiv.', 'Der Streamer, dem die Zuschauer zusehen, streamt derzeit in der folgenden Kategorie auf Twitch: {broadcaster_game}'";
        string gptShoutoutAddon = $"Sie können Informationen verwenden, um ihre Antwort zu erstellen: Der Name des Livestream-Kanals und der Name des Streamers ist „{broadcastUser}“ – Das aktuelle Datum und die aktuelle Uhrzeit sind „{actionQueuedAt}“ – Der Livestream ist derzeit live auf Twitch in der Kategorie „{broadcaster_game}“";
        string gptBehavior = gptBehaviorGlobal + gptShoutoutAddon;
        string gptTemperature = "1";
        double gptTempValue = Convert.ToDouble(gptTemperature);
        ChatGptApiRequest chatGpt = new ChatGptApiRequest(apiKey);
        string response;

        string userType = "twitch";
        
        try
        {
            response = chatGpt.GenerateResponse(prompt, gptModel, gptBehavior, gptTemperature);
            CPH.LogError($"ChatGPT win: {response}");
        }
        catch (Exception ex)
        {
            CPH.LogError($"ChatGPT ERROR: {ex.Message}");
            return false;
        }

        Root root = JsonConvert.DeserializeObject<Root>(response);
        string finalGpt = root.choices[0].message.content;
        finalGpt = finalGpt.Trim('\"');
        CPH.SetArgument("finalGpt", finalGpt);
        
        if (userType == "twitch")
        {
        	string msgId = CPH.GetGlobalVar<string>("ChatGPT_msgId_save", true);
        	//string msgId = args["msgId"].ToString();
        	CPH.TwitchReplyToMessage(finalGpt, msgId, true);
            //CPH.SendMessage(finalGpt, true);
            CPH.LogInfo("Sent SO message to Twitch");
        }
        else if (userType == "youtube")
        {
            CPH.SendYouTubeMessage($"@{user} {finalGpt}", true);
            CPH.LogInfo("Sent message to YouTube");
        }
        else if (userType == "trovo")
        {
            CPH.SendTrovoMessage($"@{user} {finalGpt}", true);
            CPH.LogInfo("Sent message to Trovo");
        }

        return true;
    }
}

class ChatGptApiRequest
{
    private string _apiKey;
    private string _endpoint = "https://api.openai.com/v1/chat/completions";
    public ChatGptApiRequest(string apiKey)
    {
        _apiKey = apiKey;
    }

    public string GenerateResponse(string prompt, string gptModel, string content, string gptTempValue)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_endpoint);
        request.Headers.Add("Authorization", "Bearer " + _apiKey);
        request.ContentType = "application/json";
        request.Method = "POST";
        string requestBody = "{\"model\": \"" + gptModel + "\",\"max_tokens\": 250, \"temperature\": " + gptTempValue + ", \"messages\": [{\"role\": \"system\", \"content\": \"" + content + "\"}, {\"role\": \"user\", \"content\": \"" + prompt + "\"}]}";
        byte[] bytes = Encoding.UTF8.GetBytes(requestBody);
        request.ContentLength = bytes.Length;
        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(bytes, 0, bytes.Length);
        }

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        string responseBody;
        using (Stream responseStream = response.GetResponseStream())
        {
            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
            responseBody = reader.ReadToEnd();
        }

        return responseBody;
    }
}

public class Message
{
    public string role { get; set; }
    public string content { get; set; }
}

public class Choice
{
    public Message message { get; set; }
    public int index { get; set; }
    public object logprobs { get; set; }
    public string finish_reason { get; set; }
}

public class Root
{
    public string id { get; set; }
    public string @object { get; set; }
    public int created { get; set; }
    public string model { get; set; }
    public List<Choice> choices { get; set; }
    public Usage usage { get; set; }
}

public class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
} //code bastardized by Mustached_Maniac
//https://ko-fi.com/mustached_maniac/tip
