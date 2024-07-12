using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace MovieHandlerService.Handlers;

internal class SheetsHandler
{
    private SheetsService MovieSheets { get; }
    private readonly string _sheetsId = Environment.GetEnvironmentVariable("SHEETS_ID") ??
                                        throw new InvalidDataException("Missing env: SHEETS_ID");

    public SheetsHandler()
    {
        GoogleCredential credential;
        using (var stream = new FileStream(
                   "credentials.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(
                SheetsService.Scope.Spreadsheets);
        }

        MovieSheets = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Get watched movies and dates"
        });
    }

    public IList<IList<object>> GetDataInRange(string topLeft, string bottomRight)
    {
        var range = $"watched!{topLeft}:{bottomRight}";
        var request = MovieSheets.Spreadsheets.Values.Get(_sheetsId, range);
        IList<IList<object>> values = request.Execute().Values;

        if (values == null || values.Count == 0)
        {
            // TODO: May need to check for valid but empty, fix this later
            throw new InvalidDataException();
        }

        return values;
    }
}