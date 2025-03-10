namespace TabletAligner.Services.Cdli;

using TabletAligner.Data;

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

    public async Task<AtfPublication[]> GetPublicationsAsync(HttpClient http) {
        string atf = await GetUnblockedAtfAsync(http).ConfigureAwait(false);
        return AtfParser.ParseAtf(atf);
    }
}

