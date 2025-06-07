// Models.cs
using System;
using System.Security.Cryptography; // For ProtectedData
using System.Text;

namespace WhatsAppLinkerApp
{
    // Class for application-level settings (API credentials, owner number)
    // public class AppSettings
    // {
    //     public string? ApiUsername { get; set; }
    //     public string? EncryptedApiPassword { get; set; } // Stored encrypted
    //     public string? OwnerCountryCodeSelectedItem { get; set; } // Store the selected item string (e.g., "🇾🇪 +967")
    //     public string? OwnerPhoneNumber { get; set; }
    // }

    // Class for persisting metadata about each managed WhatsApp bot instance
    public class PersistedInstanceInfo
    {
        public string ClientId { get; set; }
        public string? PhoneNumber { get; set; } // The actual WhatsApp number (JID without @s.whatsapp.net), once known
        public string? Alias { get; set; } // User-defined friendly name for the instance
        public string LastKnownStatus { get; set; } // e.g., "connected", "disconnected", "linking_qr", "stopped", "offline (manager)"
        public DateTime LastSeen { get; set; } // Last time status was updated or instance was seen online by UI
        public string? ApiUsername { get; set; } // Original API Username used to create this instance
        public string? OwnerNumber { get; set; } // Original Owner Number used to create this instance

        public PersistedInstanceInfo()
        {
            ClientId = string.Empty; // Must be initialized
            LastKnownStatus = "unknown";
            LastSeen = DateTime.UtcNow;
        }
    }
}