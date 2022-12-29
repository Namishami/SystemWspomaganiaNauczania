namespace SystemWspomaganiaNauczania.Models
{
    public class FontStyle
    {
        public int ID { get; set; }
        public int Size { get; set; }
        public string FontFace { get; set; }
        public string Color { get; set; }
        public virtual Profile Profile { get; set; }
    }
}