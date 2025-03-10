namespace TabletAligner.Data;

using SQLite;

class PublicationData
{
    [PrimaryKey]
    public string Id { get; set; } = "";
    public string Language { get; set; } = "";
    public string RawAtf { get; set; } = "";
}

class TextAreaData
{
    [Indexed]
    public string PublicationId { get; set; } = "";
    [Indexed]
    public string Name { get; set; } = "";
    [Indexed]
    public int Index { get; set; } = 0;
}

class LineData
{
    [Indexed]
    public string PublicationId { get; set; } = "";
    [Indexed]
    public string TextAreaName { get; set; } = "";
    [Indexed]
    public int TextAreaIndex { get; set; } = 0;
    public string Number { get; set; } = "";
    public string Text { get; set; } = "";
}

enum ImageType {
    Photo = 0,
    Lineart = 1,
}

class ImageLinesBoundingBoxData
{
    [Indexed]
    public string PublicationId { get; set; } = "";
    [Indexed]
    public string TextAreaName { get; set; } = "";
    [Indexed]
    public int TextAreaIndex { get; set; } = 0;
    public string StartLineNumber { get; set; } = "";
    public int NumberOfLines { get; set; } = 0;
    public double CenterXPixels { get; set; } = 0;
    public double CenterYPixels { get; set; } = 0;
    public double WidthPixels { get; set; } = 0;
    public double HeightPixels { get; set; } = 0;
    public int ImageWidth { get; set; } = 0;
    public int ImageHeight { get; set; } = 0;
    public string ImageUrl { get; set; } = "";
    public ImageType ImageType { get; set; } = ImageType.Photo;    
}

public static class DataDirectory {
    public static string Get() {
        var dataDir = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrEmpty(dataDir)) {
            dataDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        return dataDir;
    }
}

class AlignerData {
    readonly SQLiteAsyncConnection _db;
    public AlignerData() {
        var dataDir = DataDirectory.Get();
        var dbPath = Path.Combine(dataDir, "TabletAlignments.db");
        _db = new SQLiteAsyncConnection(dbPath);
    }
    public async Task CreateTables() {
        await _db.CreateTableAsync<PublicationData>();
        await _db.CreateTableAsync<TextAreaData>();
        await _db.CreateTableAsync<LineData>();
        await _db.CreateTableAsync<ImageLinesBoundingBoxData>();
    }
    public async Task<bool> PublicationExists(string id) {
        return (await _db.Table<PublicationData>().CountAsync (x => x.Id == id)) > 0;
    }
}
