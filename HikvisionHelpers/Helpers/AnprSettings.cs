namespace HikvisionHelpers.Helpers
{
    public class AnprSettings
    {
        public Credentials Credentials { get; set; }
        public Socket Device { get; set; }
        public Socket ListeningSocket { get; set; }
        public string Url { get; set; }
        public int Channel { get; set; }
        public int LaneNo { get; set; }
        public int OSDType { get; set; }
        public AnprEndpoints Endpoints { get; set; }
        public int ConnectionTimeout { get; set; }
        public string Path { get; set; }
    }

    public class Socket
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
    }

    public class AnprEndpoints
    {
        public string ApiAuthenticate { get; set; }
        public string DeviceInfo { get; set; }
        public string ManualTrigger { get; set; }
    }

}
