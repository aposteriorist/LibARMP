using System;

namespace LibARMP.Exceptions
{
    [Serializable]
    class ColumnNoValidityException : Exception
    {
        private static readonly string message = "The table has no column validity.";

        public ColumnNoValidityException()
            : base(message)
        {

        }

        internal ColumnNoValidityException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
