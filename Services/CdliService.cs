using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace TabletAligner.Services.Cdli
{
    public class Publication {
        public string Id { get; set; } = "";
        public List<TextArea> TextAreas { get; set; } = new List<TextArea> ();
        public string Language { get; set; } = null;
    }

    public class TextArea {
        public string Name { get; set; } = "";
        public List<TextLine> Lines { get; set; } = new List<TextLine> ();
    }

    public class TextLine {
        public string Number { get; set; } = "";
        public string Text { get; set; } = "";
        public Dictionary<string, string> Languages { get; set; } = new Dictionary<string, string> ();
    }

    public class CdliService
    {
        public static string DownloadsDirectory = "cdli";

        public async Task<Publication[]> GetTransliteratedPublicationsAsync(HttpClient http) {
            string atf = "";
            string cachedFilePath = System.IO.Path.Join(DownloadsDirectory, "cdliatf_unblocked.atf");
            if (File.Exists(cachedFilePath)) {
                atf = File.ReadAllText(cachedFilePath);
            } else {
                var response = await http.GetAsync("https://github.com/cdli-gh/data/raw/master/cdliatf_unblocked.atf").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                atf = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try {
                    File.WriteAllText(cachedFilePath, atf);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
            return ParseAtf(atf);
        }

        Publication[] ParseAtf (string atf)
        {
            var publications = new List<Publication> ();
            Publication pub = null;
            TextArea text = null;
            TextLine tline = null;

            var atfLines = atf.Split (new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawLine in atfLines) {
                var line = rawLine.Replace("\t", "").Trim ();
                if (line.Length < 1)
                    continue;
                if (line[0] == '&') {
                    pub = new Publication {
                        Id = line.Substring (1).Split(' ')[0].Trim(),
                    };
                    publications.Add (pub);
                }
                else if (line[0] == '@') {
                    text = new TextArea {
                        Name = line.Substring (1).Trim (),
                    };
                    pub.TextAreas.Add (text);
                }
                else if (char.IsDigit(line[0])) {
                    var parts = line.Split (new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var lineNumber = parts.Length > 0 ? parts[0] : "";
                    var t = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
                    tline = new TextLine {
                        Number = lineNumber,
                        Text = t,
                    };
                    text.Lines.Add (tline);
                }
                else if (line.Length > 4 && line.StartsWith("#tr.")) {
                    var parts = line.Split (new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    var lang = parts.Length > 0 ? parts[0] : "";
                    var t = parts.Length > 1 ? string.Join(":", parts.Skip(1)) : "";
                    lang = lang.Substring (4).Trim ();
                    if (lang == "ts" && pub?.Language != null) {
                        lang = pub.Language + "ts";
                    }
                    t = string.Join(" ", t.Trim().Split(' '));
                    tline.Languages[lang] = t;
                }
            }
            return publications.ToArray ();
        }
    }
}
