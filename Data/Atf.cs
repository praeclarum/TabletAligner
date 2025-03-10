namespace TabletAligner.Data;

public class AtfPublication {
    public string Id { get; set; } = "";
    public List<AtfTextArea> TextAreas { get; set; } = [];
    public string Language { get; set; } = "";
    public string RawAtf { get; set; } = "";
}

public class AtfTextArea {
    public string Name { get; set; } = "";
    public bool HasComments { get; set; } = false;
    public List<AtfTextLine> Lines { get; set; } = [];
    public bool IsEmpty => Lines.Count == 0 && !HasComments;        
}

public class AtfTextLine {
    public string Number { get; set; } = "";
    public string Text { get; set; } = "";
    public Dictionary<string, string> Languages { get; set; } = [];
}

public static class AtfParser {
    public static AtfPublication[] ParseAtf (string atf)
    {
        var publications = new List<AtfPublication> ();
        AtfPublication? pub = null;
        AtfTextArea? text = null;
        AtfTextLine? tline = null;

        var atfLines = atf.Split (new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var rawLine in atfLines) {
            var line = rawLine.Replace("\t", " ").Trim ();
            if (line.Length < 1)
                continue;
            if (line[0] == '&') {
                pub = new AtfPublication {
                    Id = line.Substring (1).Split(' ')[0].Trim(),
                    RawAtf = line,
                };
                publications.Add (pub);
                continue;
            }
            if (pub != null) {
                pub.RawAtf += "\n" + line;
            }
            if (line[0] == '@') {
                text = new AtfTextArea {
                    Name = line.Substring (1).Trim (),
                };
                if (pub is {} p) {
                    p.TextAreas.Add (text);
                }
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
                tline = new AtfTextLine {
                    Number = lineNumber,
                    Text = t,
                };
                if (text is not null) {
                    text.Lines.Add (tline);
                }
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
                if (tline is not null) {
                    tline.Languages[lang] = t;
                }
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

    public static bool TextAreaNameIsObject(string name)
    {
        return name.StartsWith("tablet", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("object", StringComparison.OrdinalIgnoreCase);
    }
}
