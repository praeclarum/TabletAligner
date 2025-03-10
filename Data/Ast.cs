namespace TabletAligner.Data;

public class AstPublication {
    public string Id { get; set; } = "";
    public List<AstTextArea> TextAreas { get; set; } = new List<AstTextArea> ();
    public string Language { get; set; } = null;
    public string RawAtf { get; set; } = "";
}

public class AstTextArea {
    public string Name { get; set; } = "";
    public bool HasComments { get; set; } = false;
    public List<AstTextLine> Lines { get; set; } = new List<AstTextLine> ();
    public bool IsEmpty => Lines.Count == 0 && !HasComments;        
}

public class AstTextLine {
    public string Number { get; set; } = "";
    public string Text { get; set; } = "";
    public Dictionary<string, string> Languages { get; set; } = new Dictionary<string, string> ();
}
