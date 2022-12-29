namespace SystemWspomaganiaNauczania.Models
{
    public class GroupTaskWord
    {
        public int ID { get; set; }
        public int GroupTaskID { get; set; }
        public int WordID { get; set; }
        public int ContainerID { get; set; }
        public virtual GroupTask GroupTask { get; set; }
        public virtual Word Word { get; set; }
        public virtual Container Container { get; set; }
    }
}