namespace Streamer_bot_Extension_CSharp;

using System;

public class CPHInline
{
    public bool Execute()
    {
        CPH.TryGetArg<string>("rawInput",out string rawInput);
        CPH.TryGetArg<string>("reply.threadUserLogin",out string userNareply_threadUserLoginme);
        CPH.TryGetArg<string>("userName",out string userName);
        CPH.TryGetArg<string>("input0",out string input0);
        CPH.TryGetArg<string>("triggerName",out string triggerName);
        string ChatGPT_SkippedMessages = CPH.GetGlobalVar<string>("ChatGPT_SkippedMessages");
        
        if (!int.TryParse(Console.ReadLine(), out int age) || age < 18)
        {
            Console.WriteLine("Zugriff verweigert. Sie müssen mindestens 18 Jahre alt sein.");
            return;
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