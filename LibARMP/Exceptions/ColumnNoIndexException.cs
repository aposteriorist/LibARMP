using System;

namespace LibARMP.Exceptions
{
    [Serializable]
    public class ColumnNoIndexException : Exception
    {
        private static readonly string message = "The table has no column indices.";

        public ColumnNoIndexException()
            : base(message)
        {

        }

        internal ColumnNoIndexException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
