
var tokenResponse = await GetTokenResponse(new()
{
    {"client_id", "af2499f1-e7fa-401f-b2f6-89fa14c98797" },
    {"scope", "udelt:test-api/api" },
    {"grant_type", "client_credentials" },
    {"client_secret", "-uUWR6cugits6WGauRYi1NhTEcZQ-Ede43WtW-Nz9vgPW9EytVrRZhFqG7Y3rpEc" }
});



Console.ReadLine();
Console.WriteLine("Got the following response:");
PrintJson(tokenResponse);

Console.ReadLine();
Console.WriteLine("Decoding the Access Token:");
PrintJwt(GetAccessToken(tokenResponse));






async Task<string> GetTokenResponse(Dictionary<string, string> parameters)
{
    var content = new FormUrlEncodedContent(parameters);

    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Calling the token endpoint with parameters:");

    foreach(var parameter in parameters)
    {
        Console.WriteLine($"{parameter.Key} => {parameter.Value}");
    }

    var client = new HttpClient();

    var response = await client.PostAsync("https://localhost:44366/connect/token", content);
    var tokenResponse = await response.Content.ReadAsStringAsync(); 
    if (response.IsSuccessStatusCode)
    {
        return tokenResponse;
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

string GetAccessToken(string tokenResponse)
{
    using var jDoc = System.Text.Json.JsonDocument.Parse(tokenResponse);
    var at = jDoc.RootElement.GetProperty("access_token").GetString() ?? "";
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