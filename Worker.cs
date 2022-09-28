using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Options;

namespace GoogleDomains.Ddns.Client;

class DdnsOptions
{
    public string Domain { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

class Worker : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<DdnsOptions> _options;
    private readonly ILogger<Worker> _logger;

    public Worker(IHttpClientFactory httpClientFactory, IOptions<DdnsOptions> options, ILogger<Worker> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var myIp = await _httpClient.GetStringAsync("https://domains.google.com/checkip", cancellationToken);
            var dnsIp = await Dns.GetHostEntryAsync(_options.Value.Domain, cancellationToken);
            if (dnsIp.AddressList.Contains(IPAddress.Parse(myIp)))
            {
                _logger.LogInformation("Google DDNS already configured for IP {ip}", myIp);
                await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
                continue;
            }


            _logger.LogInformation("Updating Google DDNS with IP {ip}", myIp);

            var auth = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(_options.Value.Username + ":" + _options.Value.Password))
            );
            var req = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://domains.google.com/nic/update?hostname={WebUtility.UrlEncode(_options.Value.Domain)}&myip={myIp}"
            )
            {
                Headers =
                {
                    Authorization = auth,
                    UserAgent =
                    {
                        new ProductInfoHeaderValue(
                            Assembly.GetExecutingAssembly().GetName().Name!,
                            Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                        )
                    }
                }
            };

            var rsp = await _httpClient.SendAsync(req, cancellationToken);
            var content = await rsp.Content.ReadAsStringAsync(cancellationToken);
            
            if (rsp.IsSuccessStatusCode && (content.StartsWith("nochg") || content.StartsWith("good")))
            {
                var msg = content.StartsWith("nochg")
                    ? "No change to google DDNS config"
                    : "Updated google DDNS config";  // content.StartsWith("good")
                _logger.LogInformation(
                    "{msg}. Status code {code}. Content:\n{content}",
                    msg,
                    rsp.StatusCode,
                    content
                );
                await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
            }
            else
            {
                _logger.LogError(
                    "Couldn't update google DDNS. Status code {code}. Content:\n{content}",
                    rsp.StatusCode,
                    content
                );
                await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);
            }
        }
    }
}

