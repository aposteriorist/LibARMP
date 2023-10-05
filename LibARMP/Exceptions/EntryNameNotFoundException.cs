using System;

namespace LibARMP.Exceptions
{
    [Serializable]
    public class EntryNameNotFoundException : Exception
    {
        private static readonly string message = "No entry names found.";

        public EntryNameNotFoundException()
            : base(message)
        {

        }

        internal EntryNameNotFoundException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
