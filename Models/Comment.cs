namespace SystemWspomaganiaNauczania.Models
{
    public class Comment
    {
        public int ID { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
     
        public int ProfileID { get; set; }
        public int? OrderTaskID { get; set; }
        public int? GroupTaskID { get; set; }
        public virtual Profile Profile { get; set; }
    }
}