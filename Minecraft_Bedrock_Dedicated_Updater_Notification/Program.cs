using System;
using System.Text.RegularExpressions;
using Discord.Webhook;
using Discord;

namespace Minecraft_Bedrock_Dedicated_Updater_Notification
{
    class Program
    {
        public static void Main(string[] args)
        {
            Program program = new Program();
            program.MainAsync().GetAwaiter().GetResult();
        }
        
        private static string GetWebhookUrl()
        {
            string filePath = "config.txt";

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "webhook=<discord webhook>");
                throw new FileNotFoundException($"Configuration file not found. A new file has been created at {filePath}. Please edit it and add your webhook URL.");
            }

            string fileContent = File.ReadAllText(filePath);
            string pattern = @"webhook\s*=\s*(https?://[^\s]+)";
            
            Match match = Regex.Match(fileContent, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            throw new InvalidOperationException($"The configuration file does not contain a valid webhook URL. Please update the file at {filePath} with a valid webhook in the format: webhook=<discord webhook>");
        }
        private string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1);
        }

        public async Task MainAsync()
        {
            MinecraftNet minecraftNet = new MinecraftNet();
            minecraftNet.SetLang("de-de");
            minecraftNet.UpdateBedrockDedicatedServer += OnBedrockDedicatedServerUpdate;
            minecraftNet.InitEvents();

            await Task.Delay(Timeout.Infinite);
        }

        private async void OnBedrockDedicatedServerUpdate(object? sender, BedrockDedicatedServer e)
        {
            var webhookClient = new DiscordWebhookClient(GetWebhookUrl());

            var color = e.Type.ToLower() == "stable" ? Color.Green : Color.Orange;
            string platform;

            if (e.Os == "win")
            {
                platform = "Windows";
                
            }  else if (e.Os == "linux")
            {
                platform = "Linux";
            }
            else
            {
                platform = e.Os;
            }
            
            var embed = new EmbedBuilder()
                .WithTitle($"New Bedrock Dedicated Server Update!")
                .WithDescription($"A new version of the Bedrock Dedicated Server has been released.\n\n**Version:** {e.Version}\n**Operating System:** {platform}\n**Type:** {CapitalizeFirstLetter(e.Type)} ")
                .WithColor(color)
                .WithFooter(e.Version)
                .WithTimestamp(DateTimeOffset.Now)
                .Build();

            var button = new ButtonBuilder()
                .WithLabel("Download Now")
                .WithStyle(ButtonStyle.Link)
                .WithUrl(e.Url);

            var component = new ComponentBuilder()
                .WithButton(button)
                .Build();

            await webhookClient.SendMessageAsync(
                text: null,
                embeds: new[] { embed },
                components: component
            );
        }
    }
}