using System;
//TODO: Missing unit tests
namespace Paycor.Import.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime EpocStart = new DateTime(1970, 1, 1);

        public static double AsEpoch(this DateTime dateTime)
        {
            return (dateTime - EpocStart).TotalMilliseconds;
        }

        public static double AsEpoch(this DateTime? dateTime)
        {
            return !dateTime.HasValue ? 0 : AsEpoch(dateTime.Value);
        }
    }
}
