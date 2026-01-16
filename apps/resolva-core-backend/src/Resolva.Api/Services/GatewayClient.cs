using System.Text.Json;

namespace Resolva.Api.Services;

public class GatewayClient
{
    private readonly HttpClient _http;
    public GatewayClient(HttpClient http) => _http = http;

    public async Task<CreateFlowResult> CreateFlowAsync(string name, string[] categories)
    {
        var res = await _http.PostAsJsonAsync("/integrations/whatsapp/flows/create", new
        {
            name,
            categories
        });

        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception($"CreateFlow failed: {body}");

        var doc = JsonDocument.Parse(body);
        var flowId = doc.RootElement.GetProperty("flowId").GetString()!;
        return new CreateFlowResult(flowId);
    }

    public async Task<UploadAssetResult> UploadFlowJsonAsync(string flowId, JsonElement flowJson)
    {
        var res = await _http.PostAsJsonAsync($"/integrations/whatsapp/flows/{flowId}/assets", new
        {
            filename = "flow.json",
            content = flowJson
        });

        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
        {
            return new UploadAssetResult(false, TryParseJson(body));
        }

        return new UploadAssetResult(true, null);
    }

    public async Task PublishFlowAsync(string flowId)
    {
        var res = await _http.PostAsync($"/integrations/whatsapp/flows/{flowId}/publish", null);
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception($"PublishFlow failed: {body}");
    }

    private static JsonElement? TryParseJson(string body)
    {
        try
        {
            var doc = JsonDocument.Parse(body);
            return doc.RootElement.Clone();
        }
        catch { return null; }
    }

    public record CreateFlowResult(string FlowId);
    public record UploadAssetResult(bool Ok, JsonElement? Errors);
}
