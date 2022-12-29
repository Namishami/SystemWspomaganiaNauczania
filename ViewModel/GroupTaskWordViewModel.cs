using System.Collections.Generic;
using SystemWspomaganiaNauczania.Models;

namespace SystemWspomaganiaNauczania.ViewModel
{
    public class GroupTaskWordViewModel
    {
        public List<string> AreaName { get; set; }
        public List<Word> Wordslist1 { get; set; }
        public List<Word> WordsList2 { get; set; }
        public List<Word> WordsList3 { get; set; }

        //public Dictionary<string , List<Word>> Areas { get; set; } -- do poprawienia aby dało się dodawać kolejne kontenery
    }
}