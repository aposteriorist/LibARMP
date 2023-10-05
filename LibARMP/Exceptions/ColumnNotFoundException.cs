using System;

namespace LibARMP.Exceptions
{
    [Serializable]
    public class ColumnNotFoundException : Exception
    {
        private static readonly string messageId = "No column with ID {0}.";
        private static readonly string messageName = "No column with name '{0}'.";
        private static readonly string messageNone = "No columns found.";

        public ColumnNotFoundException()
            : base(messageNone)
        {

        }

        internal ColumnNotFoundException(string message, Exception inner)
            : base(message, inner)
        {

        }

        public ColumnNotFoundException(int columnId)
            : base(GetMessage(columnId))
        {

        }

        public ColumnNotFoundException(string columnName)
            : base(GetMessage(columnName))
        {

        }


        private static string GetMessage(int columnId)
        {
            return string.Format(messageId, columnId);
        }

        private static string GetMessage(string columnName)
        {
            return string.Format(messageName, columnName);
        }
    }
}
