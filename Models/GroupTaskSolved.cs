namespace SystemWspomaganiaNauczania.Models
{
    public class GroupTaskSolved
    {
        public int ID { get; set; }
        public int GroupTaskID { get; set; }
        public int ProfileID { get; set; }
        public bool Solved { get; set; }
        public Profile Profile { get; set; }
        public GroupTask GroupTask { get; set; }

    }
}