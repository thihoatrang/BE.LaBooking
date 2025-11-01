using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Appointments.Application.Services
{
    public static class JsonHelper
    {
        // JSON options for serializing Services list without Unicode escaping
        public static readonly JsonSerializerOptions ServicesJsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Don't escape Unicode characters (ư, ê, etc.)
            WriteIndented = false,
            PropertyNamingPolicy = null // Keep original property names
        };

        // Default JSON options for other uses
        public static readonly JsonSerializerOptions DefaultJsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}

