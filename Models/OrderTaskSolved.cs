namespace SystemWspomaganiaNauczania.Models
{
    public class OrderTaskSolved
    {
        public int ID { get; set; }
        public int OrderTaskID { get; set; }
        public int ProfileID { get; set; }
        public bool Solved { get; set; }
        public Profile Profile { get; set; }
        public OrderTask OrderTask { get; set; }
    }
}