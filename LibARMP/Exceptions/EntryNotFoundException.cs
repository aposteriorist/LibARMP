using System;

namespace LibARMP.Exceptions
{
    [Serializable]
    public class EntryNotFoundException : Exception
    {
        private static readonly string messageId = "No entry with ID {0}.";
        private static readonly string messageName = "No entry with name '{0}'.";
        private static readonly string messageNone = "No entries found.";

        public EntryNotFoundException()
            : base(messageNone)
        {

        }

        internal EntryNotFoundException(string message, Exception inner)
            : base(message, inner)
        {

        }

        public EntryNotFoundException(int entryId)
            : base(GetMessage(entryId))
        {

        }

        public EntryNotFoundException(string entryName)
            : base(GetMessage(entryName))
        {

        }


        private static string GetMessage(int entryId)
        {
            return string.Format(messageId, entryId);
        }

        private static string GetMessage(string entryName)
        {
            return string.Format(messageName, entryName);
        }
    }
}
