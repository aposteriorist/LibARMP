using System;

namespace LibARMP.Exceptions
{
    [Serializable]
    class TypeMismatchException : Exception
    {
        private static readonly string message = "Type mismatch. Expected {0} and got {1}.";
        public static Type ExpectedType { get; private set; }
        public static Type ActualType { get; private set; }

        internal TypeMismatchException()
        {

        }

        internal TypeMismatchException(string message, Exception inner)
            : base(message, inner)
        {

        }

        public TypeMismatchException(Type expected, Type actual)
            : base(GetMessage(expected, actual))
        {
            ExpectedType = expected;
            ActualType = actual;
        }


        private static string GetMessage(Type expected, Type actual)
        {
            return string.Format(message, expected, actual);
        }
    }
}
