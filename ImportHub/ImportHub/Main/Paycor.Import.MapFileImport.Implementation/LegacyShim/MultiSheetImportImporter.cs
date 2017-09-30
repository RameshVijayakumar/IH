
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using log4net;
using Paycor.Import.Extensions;
using Paycor.Import.FileDataExtracter.LegacyShim;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.LegacyShim
{
    [ExcludeFromCodeCoverage]
    public class MultiSheetImportImporter : BaseImportImporter, IMultiSheetImportImporter
    {
        public MultiSheetImportImporter(ILog log, IFileDataExtracterFactory<ImportContext> fileDataExtracterFactory,
            string storageConnectionString) : base(log, fileDataExtracterFactory, storageConnectionString)
        {
            
        }

        public IList<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>> Import(ImportContext context, IList<ApiMapping> mappings)
        {
            Ensure.ThatArgumentIsNotNull(context, nameof(context));
            Ensure.ThatArgumentIsNotNull(mappings, nameof(mappings));

            Log.Debug($"{nameof(Import)} entered.");
            var blockBlob = GetCloudBlockBlob(context);
            var memoryStream = GetStreamFromBlob(blockBlob);
            var data = ImportFileData(context, mappings, memoryStream);
            RemoveBlobFromStorage(blockBlob);

            Log.Debug($"{nameof(Import)} returned.");
            return data;
        }

        private IList<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>> ImportFileData(ImportContext context, IList<ApiMapping> mappings, MemoryStream stream)
        {
            var sheetData = new List<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>>();

            if (null == context.UploadedFileName || null == stream)
                return null;

            var dataExtracter = FileDataExtracterFactory.GetFileDataExtracter(context.UploadedFileName);
            if (dataExtracter == null)
            {
                throw new Exception($"Unsupported File Type Extension:{context.UploadedFileName}");
            }

            var sheetDataProvider = dataExtracter as IProvideSheetData;
            if (sheetDataProvider == null)
            {
                throw new Exception("Unsupported software configuration. The data extractor should work on XLSX multisheet files.");
            }

            var numberOfSheets = sheetDataProvider.GetNumberOfSheets(stream);
            var nonMappedSheets = new List<string>();
            for (var i = 1; i <= numberOfSheets; i++)
            {
                context.XlsxWorkSheetNumber = i;
                var sheetName = sheetDataProvider.GetSheetName(i, stream);
                var map = FindByMapName(mappings, sheetName);
                if (map == null)
                {
                    nonMappedSheets.Add(sheetName);
                    continue;
                }

                context.HasHeader = !map.IsHeaderlessCustomMap();

                var data = dataExtracter.ExtractData(context, map.Mapping, stream);
                var kvp = new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(map, data);
                sheetData.Add(kvp);
            }
            if(nonMappedSheets.Any())
                Log.Info($"Sheets: {nonMappedSheets.ConcatListOfString(",")} has not been mapped and will be ignored.");

            return sheetData;
        }

        private ApiMapping FindByMapName(IEnumerable<ApiMapping> mappings, string mapName)
        {
            var mapping = mappings.FirstOrDefault(x => string.Compare(x.MappingName, mapName, StringComparison.OrdinalIgnoreCase) == 0);
            return mapping;
        }
    }
}
