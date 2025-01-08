using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Minecraft_Bedrock_Dedicated_Updater_Notification
{
    public class BedrockDedicatedServer
    {
        public string Os { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
    }

    public class MinecraftNet
    {
        private readonly HttpClient _httpClient = new();
        private string _baseUrl;
        private string _lang;

        public event EventHandler<BedrockDedicatedServer> UpdateBedrockDedicatedServer;

        private List<BedrockDedicatedServer> _lastKnownServers = new();

        public MinecraftNet(string lang = "en-us")
        {
            _lang = lang;
            _baseUrl = $"https://www.minecraft.net/{_lang}/";
        }

        public void SetLang(string lang)
        {
            _lang = lang;
            _baseUrl = $"https://www.minecraft.net/{_lang}/";
        }

        public async Task<List<BedrockDedicatedServer>> GetBedrockDedicatedServerInfo()
        {
            try
            {
                string url = _baseUrl + "download/server/bedrock";
                string htmlContent = await _httpClient.GetStringAsync(url);

                string pattern = @"https:\/\/www\.minecraft\.net\/bedrockdedicatedserver\/bin-(win|linux)(-preview)?\/bedrock-server-(\d+\.\d+\.\d+\.\d+)\.zip";
                var matches = Regex.Matches(htmlContent, pattern);

                var servers = new List<BedrockDedicatedServer>();
                foreach (Match match in matches)
                {
                    servers.Add(new BedrockDedicatedServer
                    {
                        Os = match.Groups[1].Value,
                        Type = string.IsNullOrEmpty(match.Groups[2].Value) ? "stable" : "preview",
                        Version = match.Groups[3].Value,
                        Url = match.Value
                    });
                }

                return servers;
            }
            catch
            {
                return new List<BedrockDedicatedServer>();
            }
        }

        private async Task MonitorBedrockDedicatedServerUpdatesAsync()
        {
            while (true)
            {
                try
                {
                    List<BedrockDedicatedServer> currentServers = await GetBedrockDedicatedServerInfo();
                    
                    foreach (var server in currentServers)
                    {
                        if (!_lastKnownServers.Exists(s =>
                                s.Version == server.Version &&
                                s.Os == server.Os &&
                                s.Type == server.Type))
                        {
                            UpdateBedrockDedicatedServer?.Invoke(this, server);
                        }
                    }

                    _lastKnownServers = currentServers;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await Task.Delay(3000);
            }
        }

        public void InitEvents()
        {
            Task.Run(async () => await MonitorBedrockDedicatedServerUpdatesAsync());
        }
    }
}
