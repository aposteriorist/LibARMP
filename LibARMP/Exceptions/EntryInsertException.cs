using System;

namespace LibARMP.Exceptions
{
    [Serializable]
    public class EntryInsertException : Exception
    {
        private static readonly string message = "ID {0} is greater than the amount of entries in the table.";

        internal EntryInsertException()
        {

        }

        internal EntryInsertException(string message, Exception inner)
            : base(message, inner)
        {

        }

        public EntryInsertException(int columnId)
            : base(GetMessage(columnId))
        {

        }


        private static string GetMessage(int columnId)
        {
            return string.Format(message, columnId);
        }
    }
}
