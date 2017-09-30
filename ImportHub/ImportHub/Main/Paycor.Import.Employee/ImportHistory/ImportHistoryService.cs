using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.Azure;
using Paycor.Import.Extensions;
using Paycor.Import.ImportHistory;
using Paycor.Import.Status;
using LINQtoCSV;

namespace Paycor.Import.Employee.ImportHistory
{
    public class ImportHistoryService : IImportHistoryService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IStatusStorageProvider _statusStorageProvider;
        private readonly ILog _log;
        private readonly IDocumentDbRepository<ImportHistoryMessage> _importHistoryRepository;
        private static bool IsUnEncryptedOperation
        {
            get
            {
                if (ConfigurationManager.AppSettings["EncryptAndSaveFailedRows"] == null)
                    return false;

                return Convert.ToBoolean(ConfigurationManager.AppSettings["EncryptAndSaveFailedRows"]) == false;
            }
        }

        public ImportHistoryService(IStorageProvider storageProvider, IStatusStorageProvider statusStorageProvider, ILog log, IDocumentDbRepository<ImportHistoryMessage> repository)
        {
            Ensure.ThatArgumentIsNotNull(storageProvider, nameof(storageProvider));
            Ensure.ThatArgumentIsNotNull(statusStorageProvider, nameof(statusStorageProvider));
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _storageProvider = storageProvider;
            _statusStorageProvider = statusStorageProvider;
            _log = log;
            _importHistoryRepository = repository;
        }

        public void SaveImportHistory(ImportHistoryMessage importHistoryMessage, string failedRecordsFile = null)
        {
            try
            {
                Ensure.ThatArgumentIsNotNull(importHistoryMessage, nameof(importHistoryMessage));
                if (!string.IsNullOrEmpty(failedRecordsFile))
                {
                    if (IsUnEncryptedOperation)
                    {
                        _storageProvider.Save(importHistoryMessage.TransactionId, failedRecordsFile);
                    }
                    else
                    {
                        SaveEncrypted(importHistoryMessage, failedRecordsFile);
                    }

                }
                _importHistoryRepository.UpsertItemAsync(importHistoryMessage).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.Error("SaveImportHistory Exception", e);
            }
        }

        public async Task SaveImportHistoryAsync(ImportHistoryMessage importHistoryMessage, string failedRecordsFile = null)
        {
            try
            {
                Ensure.ThatArgumentIsNotNull(importHistoryMessage, nameof(importHistoryMessage));
                if (!string.IsNullOrEmpty(failedRecordsFile))
                {
                    if (IsUnEncryptedOperation)
                    {
                        _storageProvider.Save(importHistoryMessage.TransactionId, failedRecordsFile);
                    }
                    else
                    {
                        SaveEncrypted(importHistoryMessage, failedRecordsFile);
                    }

                }
                await _importHistoryRepository.UpsertItemAsync(importHistoryMessage).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.Error("SaveImportHistory Exception", e);
            }
        }

        private void SaveEncrypted(ImportHistoryMessage importHistoryMessage, string failedRecordsFile)
        {
            // client ID is a piece of meta data passed to save it encrypted, 
            // Right not its hard coded to 1, have to figure out , how to save when its multi client
            // Also this code, appears to be not called, might have to clean up at some point
            _storageProvider.SendEncryptedTextToBlob(Guid.Parse(importHistoryMessage.TransactionId),
                failedRecordsFile, ContainerNames.FailedRecords,
                importHistoryMessage.FileName,
                1, ConfigurationManager.AppSettings["Paycor.Storage.Blob.SecretName"],
                "ImportHub ImportHistory");
        }

        public void SaveFailedRecords(int clientId, string transactionId, Stream failedRecordsFile)
        {
            clientId = (clientId == 0) ? -1 : clientId;

            if (IsUnEncryptedOperation)
            {
                _storageProvider.SaveStream(transactionId, failedRecordsFile);
            }
            else
            {
                _storageProvider.SendEncryptedStreamToBlob(Guid.Parse(transactionId),
                    failedRecordsFile, ContainerNames.FailedRecords,
                    "ImportHubFile",
                    clientId, ConfigurationManager.AppSettings["Paycor.Storage.Blob.SecretName"],
                    "ImportHub ImportHistory");
            }
        }

      
        public async Task<byte[]> GetFailedRecordsFile(string transactionId)
        {
            if (IsUnEncryptedOperation)
            {
                return _storageProvider.RetrieveStream(transactionId);

            }
            var failedRecordsFile = await _storageProvider.GetEncryptedStreamFromBlobAsync(Guid.Parse(transactionId),
                ConfigurationManager.AppSettings["Paycor.Storage.Blob.SecretName"], ContainerNames.FailedRecords)
                .ConfigureAwait(false);
            return failedRecordsFile;
        }

        public ImportHistoryMessage GetImportHistory(string id)
        {
            var historyItem = _importHistoryRepository.GetItemWithRetries(x => x.TransactionId == id);
            return historyItem;
        }

        public HttpResponseMessage GetHistoryByDateRange(DateTime? start, DateTime? end)
        {
            if (!start.HasValue) throw new Exception("The start date must be specified.");
            if (!end.HasValue) throw new Exception("The end date must be specified.");
            if (start.Value > end.Value) throw new Exception($"The specified date range of {start.Value} to {end.Value} is invalid.");
            var epochStart = start.AsEpoch();
            var epochEnd = end.AsEpoch();

            var analysisRow = _importHistoryRepository.GetItems(x => x.ImportDateEpoch >= epochStart && x.ImportDateEpoch <= epochEnd).Select(x =>
            {
                var row = new ImportAnalysisRow
                {
                    StartTime = x.ImportDate,
                    EndTime = x.ImportCompletionDate,
                    ElapsedTime = GetElapsedTime(x.ImportDate, x.ImportCompletionDate),
                    TotalRecords = (x.FailedRecordCount ?? 0) + (x.ImportedRecordCount ?? 0) + (x.CancelledRecordCount ?? 0),
                    SuccessRecords = x.ImportedRecordCount ?? 0,
                    ErrorRecords = x.FailedRecordCount ?? 0,
                    CancelledRecords = x.CancelledRecordCount ?? 0,
                    OriginatingUser = x.User,
                    ImportType = x.ImportType,
                    UserName = x.UserName,
                    ClientId = x.ClientId
                };

                if (row.ElapsedTime != null && row.ElapsedTime.Value.Ticks > 0 && row.TotalRecords > 0)
                {
                    row.AverageTimeProcessPerRow = new TimeSpan(row.ElapsedTime.Value.Ticks/row.TotalRecords);
                }
                return row;
            }).ToList();

            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ImportConstants.Comma,
                FirstLineHasColumnNames = true
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new CsvContent<ImportAnalysisRow>(outputFileDescription, "importhub_analytics.csv", analysisRow)
            };

            return response;
        }

        public IEnumerable<ImportHistoryMessage> GetImportHistoryByUser(string user, int displayCount)
        {
            var allHistoryMessages = _importHistoryRepository.GetQueryableItems(
                e => e.User == user)
                .Where(e => e.IsMarkedForDelete.ToLower() == ImportConstants.False)
                .OrderByDescending(e => e.ImportDateEpoch)
                .Take(ImportConstants.MaxImportHistoryMessages)
                .ToList();

            var historyMessagesNotCompleted =
                allHistoryMessages.Where(e => e.ImportHistoryStatus != ImportHistoryStatusEnum.Completed)
                    .OrderBy(e => e.ImportHistoryStatus)
                    .Take(displayCount)
                    .ToList();

            var remainingMessagesNeeded = displayCount - historyMessagesNotCompleted.Count;

            var historyMessagesCompleted =
                allHistoryMessages.Where(e => e.ImportHistoryStatus == ImportHistoryStatusEnum.Completed)
                    .OrderByDescending(e => e.ImportDateEpoch)
                    .Take(remainingMessagesNeeded)
                    .ToList();

            historyMessagesNotCompleted.AddRange(historyMessagesCompleted);

            var historyMessages =
                historyMessagesNotCompleted.OrderBy(e => e.ImportHistoryStatus).ThenByDescending(e => e.ImportDateEpoch);

            return historyMessages;
        }

        public async Task DeleteImportHistoryByUser(string user)
        {
            var userHistoryMessages = _importHistoryRepository.GetQueryableItems(
                e => e.User == user).ToList();

            foreach (var message in userHistoryMessages)
            {
                await SetMarkedForDelete(message);
            }
            var blobNamesToBeDeleted = userHistoryMessages.Select(p => p.TransactionId).Distinct().ToList();

            await _storageProvider.DeleteTextFromBlobAsync(blobNamesToBeDeleted, ContainerNames.FailedRecords);
            await _statusStorageProvider.DeleteStatusAsync(ContainerNames.ImportStatus, blobNamesToBeDeleted);
        }

        private async Task SetMarkedForDelete(ImportHistoryMessage message)
        {
            message.IsMarkedForDelete = ImportConstants.True;
            await _importHistoryRepository.UpsertItemAsync(message);
        }

        public async Task<bool> DeleteImportHistory(string user, string id)
        {
            var userHistoryMessage = _importHistoryRepository.GetItem(
                e => e.User == user && e.TransactionId == id);
            if (userHistoryMessage == null)
            {
                return false;
            }

            await SetMarkedForDelete(userHistoryMessage);

            var items = new[] {userHistoryMessage.TransactionId};
            await _storageProvider.DeleteTextFromBlobAsync(items, ContainerNames.FailedRecords);
            await _statusStorageProvider.DeleteStatusAsync(ContainerNames.ImportStatus, items);
            return true;
        }


        private static TimeSpan? GetElapsedTime(DateTime start, DateTime? end)
        {
            if (end == null) return null;
            return end.Value - start;
        }
    }
}