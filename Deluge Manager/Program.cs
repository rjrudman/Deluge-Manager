using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeRinseRepeat.Deluge;
using Newtonsoft.Json;

namespace Deluge_Manager
{
	internal class Program
	{ 
	    private static void Main()
        {
			Console.WriteLine($"Using settings from {Path.GetFullPath(ConfigurationManager.CONFIG_PATH)}: \n{JsonConvert.SerializeObject(ConfigurationManager.Settings, Formatting.Indented)}");
            var client = new DelugeClient(ConfigurationManager.Settings.LocalDelugeUri);
            client.Login(ConfigurationManager.Settings.LocalDelugePassword);

            var torrents = client.GetTorrents();

            var torrentsRequiringFix = torrents.Where(torrent => torrent.Trackers.Any(t => t.Url != ConfigurationManager.Settings.PrivateTracker)).ToList();
			
	        FixTorrentLocally(torrentsRequiringFix, client);
			UploadToSeedbox(torrentsRequiringFix);

			Console.WriteLine("Finished");
        }

	    private static void FixTorrentLocally(IEnumerable<Torrent> torrentsRequiringFix, DelugeClient client)
	    {
		    foreach (var torrent in torrentsRequiringFix)
		    {
			    Console.Write("Fixing {0} locally...", torrent.Name);
			    client.SetTrackers(torrent.Hash, new Tracker {Tier = 0, Url = ConfigurationManager.Settings.PrivateTracker });
			    Console.WriteLine(" done.");
		    }
	    }

	    private static void UploadToSeedbox(IReadOnlyCollection<Torrent> torrents)
        {
            if (!torrents.Any())
                return;

            Console.WriteLine("Uploading " + torrents.Count + " torrents to seedbox.");

            var client = new DelugeClient(ConfigurationManager.Settings.RemoteDelugeUri);
			client.Login(ConfigurationManager.Settings.RemoteDelugePassword);

			foreach (var torrent in torrents)
            {
                var magnetURI = GetMagnetURI(torrent);

                Console.Write("Uploading {0} ...", torrent.Name);
                client.AddMagnetURI(magnetURI);

                Console.Write(" fixing trackers...");

                client.SetTrackers(torrent.Hash,
                    torrent.Trackers.Union(new[]
                    {
                        new Tracker
                        {
                            Tier = torrent.Trackers.Max(t => t.Tier) + 1,
                            Url = ConfigurationManager.Settings.PrivateTracker
						}
                    }).ToArray()
                ); 

                Console.WriteLine(" done.");
            }
        }

        private static string GetMagnetURI(Torrent torrent)
        {
            return $"magnet:?xt=urn:btih:{torrent.Hash}&dn={torrent.Name}";
        }
    }
}
