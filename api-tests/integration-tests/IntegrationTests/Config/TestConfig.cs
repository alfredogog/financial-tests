namespace IntegrationTests.Config;

public static class TestConfig
{
    public static string ApiBaseUrl =>
        Environment.GetEnvironmentVariable("API_BASE_URL")
        ?? "http://localhost:5000/api/v1";
}