using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Paycor.Import.Mapping
{
    public interface IGlobalLookupTypeWriter
    {
        Task<Document> CreateLookupDefinition(string lookupType, string lookupKey, string lookupValue);
        Task<Document> UpdateLookupDefinition(string lookupType, string lookupKey, string lookupValue);
        Task<Document> DeleteLookupDefinition(string lookupType, string lookupKey);     
    }
}
