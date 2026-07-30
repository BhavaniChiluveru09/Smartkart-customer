
using System.Text.Json;
using System.Net.Http.Json;

public class CrmService



{
    private readonly HttpClient _httpClient;

    public CrmService(HttpClient httpClient)
    {
        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        _httpClient = new HttpClient(handler);
    }

    public async Task SendCustomerProfile(object data)
    {
        var url = "https://948a234d6c34ee6e86ab5c55aee4a0.d2.environment.api.powerplatform.com:443/powerautomate/automations/direct/cu/30/workflows/0fa31ff7cb2d4126a81144938cd8609f/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=HyHJKQ-fcEAbQUsJ3Bci67Jb-P3tC_bZDTn7LKScBc8";

        var response = await _httpClient.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateCustomerProfile(object data)
    {
        var url = "https://948a234d6c34ee6e86ab5c55aee4a0.d2.environment.api.powerplatform.com:443/powerautomate/automations/direct/cu/30/workflows/0fa31ff7cb2d4126a81144938cd8609f/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=HyHJKQ-fcEAbQUsJ3Bci67Jb-P3tC_bZDTn7LKScBc8";

        var response = await _httpClient.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendOrder(object data)
    {
        var url = "https://948a234d6c34ee6e86ab5c55aee4a0.d2.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/0873cb1fac8a4d1e9b91f4dd171a16fb/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=KJBB6CEQ91AMH_gocP0vko_qtCMaN0Y35INhVR3hdhk";

        var response = await _httpClient.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendSupportCase(object data)
    {
        var url = "https://948a234d6c34ee6e86ab5c55aee4a0.d2.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/6aba417b3a56481eb9428ca35e163324/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=HANuYFQXhf7KNZAs1D-to-8O7n7OZvCO4tO1odyZgAg";

        var response = await _httpClient.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateCustomerAddress(string userId, string address)
    {
        var url = "https://948a234d6c34ee6e86ab5c55aee4a0.d2.environment.api.powerplatform.com:443/powerautomate/automations/direct/cu/30/workflows/0fa31ff7cb2d4126a81144938cd8609f/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=HyHJKQ-fcEAbQUsJ3Bci67Jb-P3tC_bZDTn7LKScBc8";

        var response = await _httpClient.PostAsJsonAsync(url, new
        {
            userId = userId,
            address = address
        });

        response.EnsureSuccessStatusCode();
    }
    public async Task<string?> GetSupportCaseStatusByOrderId(string orderId)
    {
        var url = "https://948a234d6c34ee6e86ab5c55aee4a0.d2.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/75769ced6cf94884869a62f2e2da075f/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=2pKEARbt8YRBRm8TQak5Ho5W3JFOvInc48Tb9ftJcwg";

        var response = await _httpClient.PostAsJsonAsync(url, new
        {
            orderId = orderId
        });

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(json);
            var root = document.RootElement;

            // ✅ IMPORTANT FIX: field name = caseStatus
            if (root.TryGetProperty("caseStatus", out var status))
            {
                return status.GetString();
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}