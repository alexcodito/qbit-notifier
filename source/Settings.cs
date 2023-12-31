namespace QbitNotifier
{
    public record Settings
    {
        public string Url { get; set; }
        public required string Port { get; set; }
        public string Address => $"http://{Url}:{Port}";
    }
}
