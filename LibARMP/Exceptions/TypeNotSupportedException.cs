using System;

namespace LibARMP.Exceptions
{
    [Serializable]
    public class TypeNotSupportedException : Exception
    {
        private static readonly string message = "The armp format does not support the type '{0}'.";

        public TypeNotSupportedException(Type type)
            : base(GetMessage(type))
        {

        }

        internal TypeNotSupportedException(string message, Exception inner)
            : base(message, inner)
        {

        }


        private static string GetMessage(Type type)
        {
            return string.Format(message, type.Name);
        }
    }
}
