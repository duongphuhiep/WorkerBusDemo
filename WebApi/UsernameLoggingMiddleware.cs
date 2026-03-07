namespace WebApi;

public class UsernameLoggingMiddleware(
    RequestDelegate next,
    ILogger<UsernameLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.Headers.TryGetValue("x-user", out var userNames);
        var userName = userNames.Count > 0 ? userNames[0] : "Not specified";
        
        // Here you could also use the real user name from context.User.Identity.Name
        var scope = new Dictionary<string, string>
        {
            {"Username",  userName  ?? "Not specified"}
        };

        using (logger.BeginScope(scope))
        {
            await next(context);
        }
    }
}