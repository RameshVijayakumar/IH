using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Paycor.Import.MapFileImport.Implementation.LegacyShim
{
    [ExcludeFromCodeCoverage]
    public class ChunkerException : Exception
    {
        public int RowFailure { get; private set; }
        public int TotalRows { get; private set; }
        public string ImportType { get; private set; }

        public ChunkerException(int rowFailure, int totalRows, string message) : base(message)
        {
            RowFailure = rowFailure;
            TotalRows = totalRows;
        }

        public ChunkerException(int rowFailure, int totalRows, string message, string importType, Exception innerException)
            : base(message, innerException)
        {
            RowFailure = rowFailure;
            TotalRows = totalRows;
            ImportType = importType;
        }

        public ChunkerException(int rowFailure, int totalRows, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            RowFailure = rowFailure;
            TotalRows = totalRows;
        }

        public ChunkerException(int rowFailure, int totalRows)
        {
            RowFailure = rowFailure;
            TotalRows = totalRows;
        }
    }
}
