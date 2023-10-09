
// Create a client handler that uses the proxy
var httpClientHandler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};

var client = new HttpClient(httpClientHandler);

var tokenContent = new FormUrlEncodedContent(new[]
{
    new KeyValuePair<string, string>("client_id", "175089f1-f230-4e66-8151-100ffb2f24a5"),
    new KeyValuePair<string, string>("scope", "udelt:test-api/api"),
    new KeyValuePair<string, string>("grant_type", "client_credentials"),
    new KeyValuePair<string, string>("client_secret", "CGti1jJLqU-HRH7sbGjKWbQFqNhnCkJPaebR6bLgdvwgt5Y_L0wnUqIlQ8otJapa"),
  });


var response = await client.PostAsync("https://helseid-sts.utvikling.nhn.no/connect/token", tokenContent);


if(response.IsSuccessStatusCode)
{
    var result = await response.Content.ReadAsStringAsync();
    Console.WriteLine(result);
}


