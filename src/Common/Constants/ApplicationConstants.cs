namespace MedocIntegration.Common.Constants;

public static class ApplicationConstants
{
    public const string ApplicationName = "Medoc API COM Integration";

    public static class ServiceNames
    {
        public const string Service = "MedocService";
        public const string UI = "MedocUI";
    }

    public static class Logging
    {
        public const string LogDirectory = "logs";
        // Файли логів будуть: logs/medocservice-20260211.log, logs/medocui-20260211.log
    }

    public static class Api
    {
        public const string DefaultBaseUrl = "http://localhost:5000";
        public const int DefaultPort = 5000;
    }
}
