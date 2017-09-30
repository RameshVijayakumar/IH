using System;
using System.Collections.Generic;
using Paycor.Import.Mapping;

namespace Paycor.Import.Messaging
{
    /// <summary>
    /// Defines the data that is passed between the different actions
    /// during the translation worker processes. The name is used to
    /// identify the file that is being processed.
    /// </summary>
    /// <typeparam name="T">
    /// Defines the type of records being retrieved from the file, i.e.
    /// FascoreRecord
    /// </typeparam>
    public class FileTranslationData<T>
    {
        public string Name { get; set; }
        public DateTime ProcessingStartTime { get; set; }
        public MappingDefinition MappingDefinition { get; set; }
        public IEnumerable<T> Records { get; set; }
        public string ApiEndpoint { get; set; }
    }
}
