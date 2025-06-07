using System;

namespace WhatsAppLinkerApp.Models
{
    public class ApiSyncLog
    {
        public int Id { get; set; }
        public int BotInstanceId { get; set; }
        public string SyncType { get; set; } = string.Empty;
        public string SyncStatus { get; set; } = string.Empty;
        public int? ContactsFetched { get; set; }
        public int? ContactsAdded { get; set; }
        public int? ContactsUpdated { get; set; }
        public int? ContactsRemoved { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}