using System.Collections.Generic;
using System.Text;
//TODO: No unit tests

namespace Paycor.Import.Mapping
{
    public class TransformerException : ImportException
    {
        public TransformerException(string message)
            : base(message)
        {

        }

        public string FailedSourcePropertyName { get; set; }
        public string FailedDestinationPropertyName { get; set; }
        public string Hint { get; set; }

        public IDictionary<string, string> FailedRecord { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine(Hint)
                   .AppendLine("\tDestination Property:" + FailedDestinationPropertyName)
                   .AppendLine("\tSource Property:" + FailedSourcePropertyName)
                   .AppendLine("Record:");

            if (FailedRecord == null) 
                return builder.ToString();

            foreach (var item in FailedRecord)
            {
                builder.AppendLine(item.Key + "\t" + item.Value);
            }

            return builder.ToString();
        }
    }
}
