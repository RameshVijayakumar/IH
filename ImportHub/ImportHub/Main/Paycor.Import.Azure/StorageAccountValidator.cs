using System;
using System.Collections.Generic;
using Paycor.SystemCheck;

namespace Paycor.Import.Azure
{
    public class StorageAccountValidator : IEnvironmentValidator
    {
        public IStorageProvider StorageProvider { get; set; }
        public StorageAccountValidator(IStorageProvider storageProvider)
        {
            StorageProvider = storageProvider;
        }
        
        public IEnumerable<EnvironmentValidation> EnvironmentValidate()
        {
            try
            {
                return new List<EnvironmentValidation>()
                {
                    new EnvironmentValidation()
                    {
                        CurrentSetting = "BlobStorageAccount",
                        Name = "BlobContainers",
                        Result = EnvironmentValidationResult.Pass,
                        AdditionalInformation = $"BlobContainerNames:{StorageProvider.GetAllBlobNames()}"
                    }
                };
            }
            catch (Exception e)
            {
                return new List<EnvironmentValidation>()
                {
                    new EnvironmentValidation()
                    {
                        CurrentSetting = "BlobStorageAccount",
                        Name = "BlobContainers",
                        Result = EnvironmentValidationResult.Fail,
                        AdditionalInformation = e.ToString()
                    }
                };
            }
        }
    }
}