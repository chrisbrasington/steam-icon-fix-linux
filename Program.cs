using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System.Diagnostics;

namespace App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();            
        }

        private static async Task MainAsync(string[] args)
        {
            HttpClient _client = new HttpClient();

            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string shortcutDir = $"{homeDir}/.local/share/applications";
            string iconDir = $"{homeDir}/.local/share/icons/hicolor";
            string iconCache = $"{homeDir}/.local/share/icons/hicolor/icon-theme.cache";

            HashSet<Game> icons = await GetIconUrls(_client, args[0], args[1]);

            foreach(Game g in icons)
            {
                Console.WriteLine($"{g.name} {g.appid} {g.ToString()}");
            }

            string[] files = Directory.GetFiles(shortcutDir, "*.desktop");

            foreach(string f in files)
            {
                Console.WriteLine(f);
                bool modified = false;
                string[]? contents = null;
                
                try
                {
                    contents = File.ReadLines(f).ToArray();

                    string appId = "";
                    int lineCount = 0;

                    foreach(string line in contents)
                    {
                        if(line.StartsWith("Exec"))
                        {
                            appId = line.Split('/').Last();
                        }

                        if(line.StartsWith("Icon"))
                        {
                            if(line == "Icon=steam" && !string.IsNullOrEmpty(appId))
                            {
                                Game? game = icons.Where(g => g.appid.ToString() == appId).FirstOrDefault();

                                if(game != null)
                                {
                                    await DownloadIcon(_client, game, iconDir);

                                    contents[lineCount] = $"Icon=steam_icon_{game.appid}";
                                    modified = true;
                                }
                            }
                        }
                        lineCount++;
                    }
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }

                if(modified && contents != null)
                {
                    Console.WriteLine($"  Modifying icon in file: {f}");
                    try
                    {
                        File.WriteAllLines(f, contents);
                    }
                    catch(Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                    }
                                    
                    //break;
                }
            }

            try
            {
                File.Delete(iconCache);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            //string refreshCommand = "gnome-terminal -- sh -c \"bash -c 'sudo gtk-update-icon-cache --force /usr/share/icons/hicolor; exec bash'\"";
            string refreshCommand = "sudo gtk-update-icon-cache --force /usr/share/icons/hicolor";

            Console.WriteLine("Don't forget to run...");
            Console.WriteLine(refreshCommand);
        }

        static async Task<HashSet<Game>> GetIconUrls(HttpClient client, string key, string id)
        {
            string api = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={key}&format=json&steamid={id}&include_appinfo=1";

            HashSet<Game> icons = new HashSet<Game>();

            using(HttpResponseMessage response = client.GetAsync(api).Result)
            {
                try
                {
                    response.EnsureSuccessStatusCode();

                    using (HttpContent content = response.Content)
                    {
                        string pageContent = await content.ReadAsStringAsync();

                        var data = JsonConvert.DeserializeObject<SteamDataResponse>(pageContent);

                        if(data != null)
                        {
                            foreach(Game g in data.response.games)
                            {
                                icons.Add(g);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            return icons;
        }

        static async Task<bool> DownloadIcon(HttpClient client, Game game, string outputDir)
        {
            Console.WriteLine($"  Downloading icon appID: {game.appid}");
            //Console.WriteLine(game.ToString());

            using(HttpResponseMessage response = client.GetAsync(game.ToString()).Result)
            {
                try
                {
                    response.EnsureSuccessStatusCode();

                    using (HttpContent content = response.Content)
                    {
                        using(Image img = Image.Load(await content.ReadAsStreamAsync()))
                        {
                            outputDir += $"/{img.Width}x{img.Width}";
                            DirectoryCheck(outputDir);

                            outputDir += "/apps";
                            DirectoryCheck(outputDir);

                            string iconFile = outputDir + $"/steam_icon_{game.appid}.png";

                            if(!File.Exists(iconFile))
                            {
                                img.Save(iconFile); 
                            }
                        }  
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return false;
                }
            }
            return false;
        }

        public static bool DirectoryCheck(string outputDir)
        {
            if(!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            return true;
        }
    }

    class SteamDataResponse
    {
        public response response {get;set;}
    }

    class response
    {
        public int game_count {get;set;}
        public Game[] games {get;set;}

    }

    class Game 
    {
        public int appid {get;set;}
        public string name {get;set;}
        public string img_icon_url {get;set;}

        public override string ToString()
        {
            return $"https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/{appid}/{img_icon_url}.jpg";
        }
    }
}