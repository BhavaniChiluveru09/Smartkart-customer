using System.Collections.Generic;

namespace SmartKart.Models
{
    public class RaiseCaseViewModel
    {
        public string CaseType { get; set; }
        public string OrderId { get; set; }
        public string Description { get; set; }

        public List<string> CaseTypes { get; set; } = new List<string>
        {
            "Return",
            "Exchange",
            "Complaint",
            "Order Query"
        };
    }
}