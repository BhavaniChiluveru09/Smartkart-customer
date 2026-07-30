namespace SmartKart.Models.CRM
{
    public class SupportCaseDto
    {
        public string caseTitle { get; set; } = string.Empty;

        public string caseType { get; set; } = string.Empty;

        public string orderId { get; set; } = string.Empty;

        public string userId { get; set; } = string.Empty;

        public string priority { get; set; } = string.Empty;

        public string status { get; set; } = string.Empty;

        public string resolution { get; set; } = string.Empty;

        public string owner { get; set; } = string.Empty;
    }
}
