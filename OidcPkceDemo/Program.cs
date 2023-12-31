﻿using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

var clientId = "af2499f1-e7fa-401f-b2f6-89fa14c98797";
var scope = HttpUtility.UrlEncode("openid profile udelt:test-api/api");
var redirectUri = HttpUtility.UrlEncode("http://localhost:12345/callback");

var codeVerifier = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
var codeChallenge = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)));

var state = Base64UrlEncoder.Encode("Min hemmelige state!");

var authorizationServer = "https://localhost:44366";

var authorizeUrl = $"{authorizationServer}/connect/authorize?client_id={clientId}&scope={scope}&redirect_uri={redirectUri}&response_type=code&code_challenge={codeChallenge}&code_challenge_method=S256&state={state}";

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine($"Running system browser against url {authorizeUrl}");
PrintQueryString(authorizeUrl);
Console.ReadLine();

var authResponse = await RunBrowser(authorizeUrl, "http://localhost:12345/");

Console.WriteLine("Got response:");

PrintKeyValuePairs(authResponse);

var code = authResponse["code"];

Console.WriteLine();
Console.ReadLine();

var tokenResponse = await GetTokenResponse(new() {
    { "client_id", clientId },
    { "grant_type", "authorization_code" },
    { "client_secret", "-uUWR6cugits6WGauRYi1NhTEcZQ-Ede43WtW-Nz9vgPW9EytVrRZhFqG7Y3rpEc" },
    { "redirect_uri", "http://localhost:12345/callback"},
    { "code", code},
    { "code_verifier", codeVerifier }
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

    Console.WriteLine("Calling the token endpoint with parameters:");

    PrintKeyValuePairs(parameters);

    var client = new HttpClient();

    var response = await client.PostAsync("https://localhost:44366/connect/token", content);
    var responseString = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
        return responseString;
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

void PrintKeyValuePairs(IEnumerable<KeyValuePair<string, string>> kvps)
{
    foreach (var kvp in kvps)
    {
        Console.WriteLine($"{kvp.Key} => {kvp.Value}");
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
    Console.WriteLine("Header:");
    PrintJson(header);
    Console.WriteLine();
    Console.WriteLine("Body:");
    PrintJson(body);
    Console.WriteLine();
    Console.WriteLine("Signature:");
    Console.WriteLine(jwtParts[2]);

}


void PrintQueryString(string url)
{
    var query = HttpUtility.ParseQueryString(new Uri(authorizeUrl).Query);

    Console.WriteLine();
    Console.WriteLine("Query:");
    foreach (var key in query.Keys)
    {
        Console.WriteLine($"{key} => {query.Get(key.ToString())}");
    }
}