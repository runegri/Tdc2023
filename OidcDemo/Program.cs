

using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;

var clientId = "175089f1-f230-4e66-8151-100ffb2f24a5";
var scope = HttpUtility.UrlEncode("openid profile helseid://scopes/identity/pid udelt:test-api/api");
var redirectUri = HttpUtility.UrlEncode("http://localhost:12345/callback");

var authorizeUrl = $"http://helseid-sts.utvikling.nhn.no/connect/authorize?client_id={clientId}&scope={scope}&redirect_uri={redirectUri}&response_type=code";

var authResponse = await RunBrowser(authorizeUrl, "http://localhost:12345/");

Console.WriteLine(authResponse["code"]);

var code = authResponse["code"];

// Create a client handler that uses the proxy
var httpClientHandler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};

var client = new HttpClient(httpClientHandler);

var tokenContent = new FormUrlEncodedContent(new[]
{
    new KeyValuePair<string, string>("client_id", "175089f1-f230-4e66-8151-100ffb2f24a5"),
    new KeyValuePair<string, string>("code", code),
    new KeyValuePair<string, string>("redirect_uri", "http://localhost:12345/callback"),
    new KeyValuePair<string, string>("grant_type", "authorization_code"),
    new KeyValuePair<string, string>("client_secret", "CGti1jJLqU-HRH7sbGjKWbQFqNhnCkJPaebR6bLgdvwgt5Y_L0wnUqIlQ8otJapa"),
  });


var response = await client.PostAsync("https://helseid-sts.utvikling.nhn.no/connect/token", tokenContent);

var result = await response.Content.ReadAsStringAsync();

if (response.IsSuccessStatusCode)
{
    Console.WriteLine(result);
}


async Task<Dictionary<string,string>> RunBrowser(string url, string redirect)
{
    url = url.Replace("&", "^&");
    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });

    var listener = new HttpListener();
    listener.Prefixes.Add(redirect);

    listener.Start();

    var context = await listener.GetContextAsync();

    var q = context.Request.QueryString;

    var response = q.AllKeys.ToDictionary(k => k, k => q[k]);

    listener.Stop();
    
    return response;
}