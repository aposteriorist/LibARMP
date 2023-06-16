using System;

namespace LibARMP.Exceptions
{
    [Serializable]
    class ColumnNoDataException : Exception
    {
        private static readonly string messageId = "The column with ID {0} has no data.";
        private static readonly string messageName = "The column '{0}' has no data.";

        internal ColumnNoDataException()
        {

        }

        internal ColumnNoDataException(string message, Exception inner)
            : base(message, inner)
        {

        }

        public ColumnNoDataException(int columnId)
            : base(GetMessage(columnId))
        {

        }

        public ColumnNoDataException(string columnName)
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
