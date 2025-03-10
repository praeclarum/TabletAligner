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
        public string RawAtf { get; set; } = "";
    }

    public class TextArea {
        public string Name { get; set; } = "";
        public bool HasComments { get; set; } = false;
        public List<TextLine> Lines { get; set; } = new List<TextLine> ();
        public bool IsEmpty => Lines.Count == 0 && !HasComments;        
    }

    public class TextLine {
        public string Number { get; set; } = "";
        public string Text { get; set; } = "";
        public Dictionary<string, string> Languages { get; set; } = new Dictionary<string, string> ();
    }

        public class CdliService
    {
        public string DownloadsDirectory {
            get {
                var dataDir = TabletAligner.Data.DataDirectory.Get ();
                return Path.Combine(dataDir, "cdli");
            }
        }

        public string UnblockedAtfUrl => "https://github.com/cdli-gh/data/raw/refs/heads/master/cdliatf_unblocked.atf";

        public async Task<string> GetUnblockedAtfAsync(HttpClient http) {
            string atf = "";
            string cachedFilePath = System.IO.Path.Join(DownloadsDirectory, "cdliatf_unblocked.atf");
            if (File.Exists(cachedFilePath)) {
                atf = File.ReadAllText(cachedFilePath);
            } else {
                var response = await http.GetAsync(UnblockedAtfUrl).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                atf = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try {
                    Directory.CreateDirectory(DownloadsDirectory);
                    if (File.Exists(cachedFilePath))
                        File.Delete(cachedFilePath);
                    File.WriteAllText(cachedFilePath, atf);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
            return atf;
        }

        public async Task<Publication[]> GetPublicationsAsync(HttpClient http) {
            string atf = await GetUnblockedAtfAsync(http).ConfigureAwait(false);
            return ParseAtf(atf);
        }

        Publication[] ParseAtf (string atf)
        {
            var publications = new List<Publication> ();
            Publication pub = null;
            TextArea text = null;
            TextLine tline = null;

            var atfLines = atf.Split (new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var currentObject = "";
            foreach (var rawLine in atfLines) {
                var line = rawLine.Replace("\t", "").Trim ();
                if (line.Length < 1)
                    continue;
                if (line[0] == '&') {
                    pub = new Publication {
                        Id = line.Substring (1).Split(' ')[0].Trim(),
                        RawAtf = line,
                    };
                    publications.Add (pub);
                    currentObject = "";
                    continue;
                }
                if (pub != null) {
                    pub.RawAtf += "\n" + line;
                }
                if (line[0] == '@') {
                    text = new TextArea {
                        Name = line.Substring (1).Trim (),
                    };
                    pub.TextAreas.Add (text);
                }
                else if (line[0] == '$') {
                    if (text != null) {
                        text.HasComments = true;
                    }
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
                    if (text != null) {
                        text.HasComments = true;
                    }
                }
                else if (line.Length > 10 && line.StartsWith("#atf: lang")) {
                    var lang = line.Substring (10).Trim ();
                    if (pub is {} p) {
                        p.Language = lang;
                    }
                    if (text != null) {
                        text.HasComments = true;
                    }
                }
            }
            return publications.ToArray ();
        }

        public bool TextAreaNameIsObject(string name)
        {
            return name.StartsWith("tablet", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("object", StringComparison.OrdinalIgnoreCase);
        }
    }
}
