using System.Security.Cryptography;
using System.Text;

namespace Resolva.Api.Middleware;

public class InternalHmacMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _secret;

    public InternalHmacMiddleware(RequestDelegate next, IConfiguration cfg)
    {
        _next = next;
        _secret = cfg["InternalAuth:HmacSecret"] ?? "";
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/internal"))
        {
            await _next(context);
            return;
        }

        // Must have secret configured
        if (string.IsNullOrWhiteSpace(_secret))
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal HMAC secret not configured");
            return;
        }

        var ts = context.Request.Headers["X-Resolva-Timestamp"].FirstOrDefault();
        var sig = context.Request.Headers["X-Resolva-Signature"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(ts) || string.IsNullOrWhiteSpace(sig))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: missing signature headers");
            return;
        }

        if (!long.TryParse(ts, out var tsNum))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: invalid timestamp");
            return;
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - tsNum) > 300)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: timestamp out of range");
            return;
        }

        context.Request.EnableBuffering();

        string body;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var signed = $"{ts}.{body}";
        var expected = ComputeHmacHex(_secret, signed);

        if (!ConstantTimeEqualsHex(expected, sig))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: bad signature");
            return;
        }

        await _next(context);
    }

    private static string ComputeHmacHex(string secret, string data)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var bytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(key);
        return Convert.ToHexString(hmac.ComputeHash(bytes)).ToLowerInvariant();
    }

    private static bool ConstantTimeEqualsHex(string a, string b)
    {
        try
        {
            var ab = Convert.FromHexString(a);
            var bb = Convert.FromHexString(b);
            if (ab.Length != bb.Length) return false;
            return CryptographicOperations.FixedTimeEquals(ab, bb);
        }
        catch { return false; }
    }
}