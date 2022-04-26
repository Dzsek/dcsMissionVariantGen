using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace missionify
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1 )
            {
                Console.WriteLine("No File");
                Environment.Exit(0);
            }

            var presets = Directory.GetFiles("presets");
            var dates = presets.Where(x => x.StartsWith("presets\\date_"));
            var times = presets.Where(x => x.StartsWith("presets\\time_"));
            var weathers = presets.Where(x => x.StartsWith("presets\\weather_"));
            var mods = presets.Where(x => x.StartsWith("presets\\mods")).FirstOrDefault();
            var mods1 = File.ReadAllText(mods);

            var finfo = new FileInfo(args[0]);
            var f = finfo.Name.Replace(finfo.Extension, string.Empty);
            if(!Directory.Exists("output"))
            {
                Directory.CreateDirectory("output");
            }

            foreach(var d in dates)
            {
                var d1 = d.Replace("presets\\date_", string.Empty).Replace(".lua",string.Empty);
                var d2 = File.ReadAllText(d);
                foreach(var t in times)
                {
                    var t1 = t.Replace("presets\\time_", string.Empty).Replace(".lua", string.Empty);
                    var t2 = File.ReadAllText(t);
                    foreach (var w in weathers)
                    {
                        var w1 = w.Replace("presets\\weather_", string.Empty).Replace(".lua", string.Empty);
                        var w2 = File.ReadAllText(w);

                        var file = $"output\\{f}_{d1}_{t1}_{w1}.miz";

                        Console.Write($"Generating {file}...");
                        File.Copy($"{f}.miz", file, true);


                        var zip = ZipFile.Open(file, ZipArchiveMode.Update);
                        var entry = zip.GetEntry("mission");
                        var result = "";
                        using (var sr = new StreamReader(entry.Open()))
                        {
                            var contents = sr.ReadToEnd();
                            result =  GenerateVariant(contents, d2, t2, w2, mods1);
                        }
                        
                        entry.Delete();

                        entry = zip.CreateEntry("mission");
                        using (var sw = new StreamWriter(entry.Open()))
                        {
                            sw.Write(result);
                            sw.Flush();
                        }

                        zip.Dispose();
                        Console.WriteLine($"Done");
                    }
                }
            }

        }

        static string GenerateVariant(string original, string date, string time, string weather, string mods)
        {
            var text = original;

            var datereg = new Regex("\\[\"date\"\\](.|\\n)+end of \\[\"date\"\\]", RegexOptions.Multiline);
            text = datereg.Replace(text, date);

            var weatherreg = new Regex("\\[\"weather\"\\](.|\\n)+end of \\[\"weather\"\\]", RegexOptions.Multiline);
            text = weatherreg.Replace(text, weather);

            var modsreg = new Regex("\\[\"requiredModules\"\\](.|\\n)+end of \\[\"requiredModules\"\\]", RegexOptions.Multiline);
            text = modsreg.Replace(text, mods);

            var timereg = new Regex("\\[\"start_time\"\\] = [0-9]+,\\n\\s*\\[\"forcedOptions\"\\]", RegexOptions.Multiline);
            text = timereg.Replace(text, time);

            return text;
        }
    }
}
