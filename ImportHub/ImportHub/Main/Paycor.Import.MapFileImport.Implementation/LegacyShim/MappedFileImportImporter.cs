using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using log4net;
using Paycor.Import.FileDataExtracter.LegacyShim;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.LegacyShim
{
    [ExcludeFromCodeCoverage]
    public class MappedFileImportImporter : BaseImportImporter, IMappedFileImportImporter
    {

        public MappedFileImportImporter(ILog log, IFileDataExtracterFactory<ImportContext> fileDataExtracterFactory,
            string storageConnectionString) : base(log, fileDataExtracterFactory, storageConnectionString)
        {
        }

        public IEnumerable<IDictionary<string, string>> Import(ImportContext context, MappingDefinition map)
        {
            Ensure.ThatArgumentIsNotNull(context, nameof(context));
            Ensure.ThatArgumentIsNotNull(map, nameof(map));

            Log.Debug($"{nameof(Import)} entered.");
            var blockBlob = GetCloudBlockBlob(context);
            var memoryStream = GetStreamFromBlob(blockBlob);
            var data = ImportFileData(context, map, memoryStream);
            // Basic tenet of memory mangement, creator of an object
            // should be responsible for disposing it.
            // TODO: add try/finally as appropriate.
            memoryStream.Dispose();

            RemoveBlobFromStorage(blockBlob);

            Log.Debug($"{nameof(Import)} returned.");
            return data;
        }

        private IEnumerable<IDictionary<string, string>> ImportFileData(ImportContext context, MappingDefinition map,
            MemoryStream stream)
        {
            if (null == context.UploadedFileName || null == stream)
                return null;

            var dataExtracter = FileDataExtracterFactory.GetFileDataExtracter(context.UploadedFileName);

            if (dataExtracter == null)
            {
                throw new Exception($"Unsupported File Type Extension:{context.UploadedFileName}");
            }

            return dataExtracter.ExtractData(context, map, stream);
        }
    }
}