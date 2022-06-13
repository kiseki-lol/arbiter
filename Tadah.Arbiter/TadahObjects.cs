namespace Tadah.Arbiter
{
    public class TadahMessage
    {
        public string Operation { get; set; }
        public string JobId { get; set; }
        public int Version { get; set; }
        public int PlaceId { get; set; }
        public string Script { get; set; }
        public int ExpirationInSeconds { get; set; }
    }

    public class TadahResponse
    {
        public string Operation { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
