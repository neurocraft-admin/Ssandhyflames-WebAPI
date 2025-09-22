namespace WebAPI.Models
{
    public class CylinderInventoryRequest
    {
        public int CylinderTypeId { get; set; }
        public DateTime Date { get; set; }
        public int FilledIn { get; set; }
        public int EmptyIn { get; set; }
        public int FilledOut { get; set; }
        public int EmptyOut { get; set; }
    }
}
