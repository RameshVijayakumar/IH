using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.FileDataExtracter.LegacyShim;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.LegacyShim
{
    public class CsvDataExtracter : IFileDataExtracter<ImportContext>
    {
        public string SupportedFileTypes()
        {
            return ".csv,.txt";
        }

        public CsvDataExtracter(ILog logger)
        {
            Ensure.ThatArgumentIsNotNull(logger, nameof(logger));
        }

        public IList<IDictionary<string, string>> ExtractData(ImportContext context, MappingDefinition map, MemoryStream memoryStream)
        {
            
            Ensure.ThatArgumentIsNotNull(context, nameof(context));
            Ensure.ThatArgumentIsNotNull(map, nameof(map));
            Ensure.ThatArgumentIsNotNull(memoryStream, nameof(memoryStream));

            var records = new List<IDictionary<string, string>>();
            var rowProgress = 1;

            try
            {
                var csvReader = GetCsvReaderFromText(memoryStream);
                csvReader.Configuration.IgnoreBlankLines = true;
                csvReader.Configuration.HasHeaderRecord = context.HasHeader;
                csvReader.Configuration.IgnoreHeaderWhiteSpace = true;
                csvReader.Configuration.IsHeaderCaseSensitive = false;

                while (csvReader.Read())
                {
                    var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var field in map.FieldDefinitions)
                    {
                        if (field.Source.IsLookupParameter())
                            continue;
                        // Remove any spaces from the header fields in the file, but use
                        // the original header name with spaces to retrieve the field from the file. If the
                        // value is a number, then fetch the data by it's ordinal position; otherwise,
                        // fetch the data using the column name reference.
                        int ordinal;
                        var headerField = field.Source;
                        if (field.Source == null && field.Destination != null)
                        {
                            record[field.Destination] = null;
                            continue;
                        }
                        var headerFieldUnspaced = headerField.RemoveWhiteSpaces();
                        if (field.SourceType == SourceTypeEnum.Const) continue;
                        string columnValue;
                        try
                        {
                            var sourceList = headerFieldUnspaced.SplitbyPipe();
                            foreach (var source in sourceList)
                            {
                                if (csvReader.TryGetField(source, 0, out columnValue))
                                {
                                    record[source] = columnValue.Trim();
                                }
                                else if (int.TryParse(source, out ordinal)
                                    &&
                                    csvReader.TryGetField(ordinal, out columnValue)
                                    )
                                {
                                    record[source] = columnValue.Trim();
                                }
                            }


                           
                        }
                        catch (CsvReaderException)
                        {
                            if (int.TryParse(headerFieldUnspaced, out ordinal)
                                &&
                                csvReader.TryGetField(ordinal, out columnValue)
                                )
                            {
                                record[headerField] = columnValue;
                            }
                        }
                    }
                    records.Add(record);
                    rowProgress++;
                }
            }
            catch (Exception ex)
            {
                throw new ChunkerException(rowProgress, rowProgress, "A failure occurred while either reading the uploaded file or during the chunking process.", null, ex);
            }
            records.RemoveAllEmptyDictonaries();
            return records;
}

        private static TextReader GetTextReaderFromStream(MemoryStream memoryStream)
        {
            var text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            var textReader = (new StringReader(text));

            memoryStream.Dispose();
            return textReader;
        }

        private static CsvReader GetCsvReaderFromText(MemoryStream memoryStream)
        {
            var csvReader = (new CsvReader(GetTextReaderFromStream(memoryStream)));
            return csvReader;
        }
    }
}

