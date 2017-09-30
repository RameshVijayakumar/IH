using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Paycor.Import.Mapping;

namespace Paycor.Import.Azure
{
    public interface ITableStorageProvider
    {
        void Insert(ApiMappingAudit item);

        IEnumerable<ApiMappingAudit> GetItems(string mapName);
    }
}
