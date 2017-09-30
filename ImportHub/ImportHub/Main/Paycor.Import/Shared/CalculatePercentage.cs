//TODO: Incomplete unit tests
namespace Paycor.Import.Shared
{
    public static class CalculatePercentage
    {
        public static int GetPercentageComplete(int? currentRecord, int? totalRecords)
        {
            if ((currentRecord == 0) || (totalRecords == 0) || (currentRecord == null) || (totalRecords == null))
                return 0;

            return (int)((decimal)(currentRecord * 100) / totalRecords);
        }
    }
}