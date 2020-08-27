using System;
using Segment;
using Segment.Model;

namespace repro
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1 || string.IsNullOrEmpty(args[0]))
            {
                ShowUsageAndExit();
            }

            var writeKey = args[0];

            var client = CreateClient(writeKey);

            var eventTime = DateTime.UtcNow;
            const int numberOfEvents = 5000;

            for (int i = 0; i < numberOfEvents; i++)
            {
                ReleaseStatsUpdated(client: client,
                    userId: Guid.NewGuid().ToString(),
                    eventTime: eventTime,
                    totalReleasesAlbumSold: i,
                    totalReleasesAssetSold: i + 12,
                    totalReleasesAssetSoldIndividually: i + 24,
                    totalReleasesAssetSoldThroughAlbum: i + 36,
                    totalReleasesAssetStreamed: 1000 * i,
                    albumSold: 10 * i,
                    assetSold: 100 * i,
                    assetSoldIndividually: 20 * i,
                    assetSoldThroughAlbum: 80 * i,
                    assetStreamed: 500 * i);
            }

            Console.WriteLine("Flushing");
            client.Flush();
            Console.WriteLine("Done");
        }

        private static Client CreateClient(string writeKey)
        {
            var segmentConfig = new Config()
                .SetAsync(true);

            return new Client(writeKey, segmentConfig);
        }

        private static void ReleaseStatsUpdated(
            Client client,
            string userId,
            DateTime eventTime,
            // User properties
            long totalReleasesAlbumSold,
            long totalReleasesAssetSold,
            long totalReleasesAssetSoldIndividually,
            long totalReleasesAssetSoldThroughAlbum,
            long totalReleasesAssetStreamed,
            // Event properties
            long albumSold,
            long assetSold,
            long assetSoldIndividually,
            long assetSoldThroughAlbum,
            long assetStreamed
            )
        {
            var segmentContext = new Context()
            {
                { "active", false },
                { "app", new Dict()
                    {
                        { "name", "Backend" },
                    }
                }
            };

            var options = new Options()
                .SetTimestamp(eventTime)
                .SetContext(segmentContext);

            var userProperties = new Dict();
            userProperties.Add("L - Total Releases Album Sold", totalReleasesAlbumSold);
            userProperties.Add("L - Total Releases Asset Sold", totalReleasesAssetSold);
            userProperties.Add("L - Total Releases Asset Sold (Individually)", totalReleasesAssetSoldIndividually);
            userProperties.Add("L - Total Releases Asset Sold (Through Album)", totalReleasesAssetSoldThroughAlbum);
            userProperties.Add("L - Total Releases Asset Streamed", totalReleasesAssetStreamed);

            client.Identify(userId, userProperties, options);

            var eventProperties = new Dict();
            eventProperties.Add("L - Event Category", "Release");
            eventProperties.Add("L - Album Sold", albumSold);
            eventProperties.Add("L - Asset Sold", assetSold);
            eventProperties.Add("L - Asset Sold (Individually)", assetSoldIndividually);
            eventProperties.Add("L - Asset Sold (Through Album)", assetSoldThroughAlbum);
            eventProperties.Add("L - Asset Streamed", assetStreamed);
            eventProperties.Add("category", "Release");

            client.Track(userId, "Release Stats Updated", eventProperties, options);
        }

        private static void ShowUsageAndExit()
        {
            Console.WriteLine("USAGE: dotnet repro <segment_write_key>");
            Environment.Exit(1);
        }

    }
}
