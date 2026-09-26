using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Sentinelis.Cli.Models;
using Sentinelis.Core.Interfaces;

namespace engine.Clients
{

    // Client for interacting with the NPM registry to fetch package metadata.

    public class NpmRegistryClient : IRegistryClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public NpmRegistryClient()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Sentinelis-CI/1.0");
        }

        public async Task<DateTime?> GetVersionPublishDateAsync(string packageName, string version)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://registry.npmjs.org/{packageName}");
                if (!response.IsSuccessStatusCode) return null;

                var stream = await response.Content.ReadAsStreamAsync();
                var registryData = await JsonSerializer.DeserializeAsync(
                    stream,
                    NpmRegistryJsonContext.Default.NpmRegistryResponse
                );

                if (registryData?.Time != null && registryData.Time.TryGetValue(version, out var timeStr))
                {
                    // The "time" dictionary in NPM registry contains publish dates for all versions
                    if (DateTime.TryParse(timeStr, out var publishDate))
                    {
                        return publishDate;
                    }
                }
                return null;
            }
            catch
            {
                return null; // Fail open on network errors
            }
        }
    }
}