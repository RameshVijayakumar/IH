using System;

namespace Paycor.Import
{
    // TODO: Missing unit tests
    public static class Ensure
    {
        public static void ThatStringIsNotNullOrEmpty(string argument, string argName)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentException(argName);
            }
        }

        public static void ThatArgumentIsNotNull<T>(T argument, string argName)
            where T : class
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        public static void ThatDateTimeIsNotMinValue(DateTime argument, string argumentName)
        {
            if (argument == DateTime.MinValue)
            {
                throw new ArgumentException("The DateTime argument " + argumentName + " equaled DateTime.MinValue.");
            }
        }

        public static void ThatPropertyIsInitialized<T>(T property, string argName)
            where T : class
        {
            if (property == null)
            {
                throw new InvalidOperationException($"{argName} must be initialized before this method can be invoked.");
            }
        }

    }
}
