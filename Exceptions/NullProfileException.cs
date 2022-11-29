using System;

namespace SystemWspomaganiaNauczania.Exceptions
{
    public class NullProfileException : Exception
    {
        public NullProfileException():base() { }
        public NullProfileException(string message):base(message) { }
        
    }
}