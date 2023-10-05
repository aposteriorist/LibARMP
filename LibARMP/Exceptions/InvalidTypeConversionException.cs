using System;

namespace LibARMP.Exceptions
{
    [Serializable]
    public class InvalidTypeConversionException : InvalidCastException
    {
        private static readonly string message = "Cannot convert from {0} to {1}.";
        public static Type SourceType { get; private set; }
        public static Type TargetType { get; private set; }

        internal InvalidTypeConversionException()
        {

        }

        internal InvalidTypeConversionException(string message, Exception inner)
            : base(message, inner)
        {

        }

        public InvalidTypeConversionException(Type source, Type target)
            : base(GetMessage(source, target))
        {
            SourceType = source;
            TargetType = target;
        }


        private static string GetMessage(Type source, Type target)
        {
            return string.Format(message, source, target);
        }
    }
}
