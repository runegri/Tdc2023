

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;

var clientId = "175089f1-f230-4e66-8151-100ffb2f24a5";
var scope = HttpUtility.UrlEncode("openid profile helseid://scopes/identity/pid udelt:test-api/api");
var redirectUri = HttpUtility.UrlEncode("http://localhost:12345/callback");

var authorizationServer = "https://helseid-sts.utvikling.nhn.no";

var authorizeUrl = $"{authorizationServer}/connect/authorize?client_id={clientId}&scope={scope}&redirect_uri={redirectUri}&response_type=code";

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine($"Running system browser against url {authorizeUrl}");

var authResponse = await RunBrowser(authorizeUrl, "http://localhost:12345/");
var code = authResponse["code"];

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine($"Authorization code received: {code}");
Console.WriteLine();
Console.ReadLine();

var tokenResponse = await GetTokenResponse(new() {
    { "client_id", clientId },
    { "grant_type", "authorization_code" },
    { "client_secret", "CGti1jJLqU-HRH7sbGjKWbQFqNhnCkJPaebR6bLgdvwgt5Y_L0wnUqIlQ8otJapa" },
    { "redirect_uri", "http://localhost:12345/callback"},
    { "code", code},
});

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Got the following response:");
PrintJson(tokenResponse);

Console.ReadLine();
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Decoding the Identity Token:");
PrintJwt(GetToken(tokenResponse, "id_token"));

Console.ReadLine();
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Decoding the Access Token:");
PrintJwt(GetToken(tokenResponse, "access_token"));



async Task<Dictionary<string, string>> RunBrowser(string url, string redirect)
{
    url = url.Replace("&", "^&");
    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });

    var listener = new HttpListener();
    listener.Prefixes.Add(redirect);

    listener.Start();

    var context = await listener.GetContextAsync();

    var response = Encoding.UTF8.GetBytes("<html><body>Code received!</body></html>");

    context.Response.ContentLength64 = response.Length;
    await context.Response.OutputStream.WriteAsync(response);

    var q = context.Request.QueryString;
    var request = q.AllKeys.ToDictionary(k => k, k => q[k]);

    listener.Stop();

    return request;
}

async Task<string> GetTokenResponse(Dictionary<string, string> parameters)
{
    var content = new FormUrlEncodedContent(parameters);

    Console.ResetColor();
    Console.WriteLine("Calling the token endpoint with parameters:");

    foreach (var parameter in parameters)
    {
        Console.WriteLine($"{parameter.Key} => {parameter.Value}");
    }

    var client = new HttpClient();

    var response = await client.PostAsync("https://helseid-sts.utvikling.nhn.no/connect/token", content);
    if (response.IsSuccessStatusCode)
    {
        return await response.Content.ReadAsStringAsync();
    }

    return $"Returned status code {response.StatusCode}";
}

void PrintJson(string json)
{
    try
    {
        using var jDoc = System.Text.Json.JsonDocument.Parse(json);
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(jDoc, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }
    catch
    {
        Console.WriteLine(json);
    }
}

string GetToken(string tokenResponse, string tokenType)
{
    using var jDoc = System.Text.Json.JsonDocument.Parse(tokenResponse);
    var at = jDoc.RootElement.GetProperty(tokenType).GetString() ?? "";
    return at;
}

void PrintJwt(string jwt)
{
    var jwtParts = jwt.Split('.');
    var header = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Decode(jwtParts[0]);
    var body = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Decode(jwtParts[1]);

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Header:");
    PrintJson(header);
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("Body:");
    PrintJson(body);
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Signature:");
    Console.WriteLine(jwtParts[2]);

}