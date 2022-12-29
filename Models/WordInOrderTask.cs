namespace SystemWspomaganiaNauczania.Models
{
    public class WordInOrderTask
    {
        public int ID { get; set; }
        public int OrderTaskID { get; set; }
        public int OrderTaskWordID { get; set; }
        public virtual OrderTask OrderTask { get; set; }
        public virtual Word OrderTaskWord { get; set; }
    }
}