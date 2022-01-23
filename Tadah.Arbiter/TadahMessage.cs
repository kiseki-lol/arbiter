namespace Tadah.Arbiter
{
    public class TadahMessage
    {
        public string Operation { get; set; }
        public string JobID { get; set; }
        public int Version { get; set; }
        public int PlaceID { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
