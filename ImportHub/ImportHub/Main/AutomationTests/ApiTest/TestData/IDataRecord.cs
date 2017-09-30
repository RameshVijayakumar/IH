using System;

namespace Paycor.Import.ImportHubTest.ApiTest.TestData
{
    public interface IDataRecord
    {
        bool IsEmpty { get; }
        bool ColumnExist(string column);
        void InsertColumnAt(int index, string column, string value);
        void AddColumn(string column, string value);
        void RemoveColumn(string column);
        void RemoveColumnAt(int index);
        void SetValue(string column, string value);
        void SetValues(params string[] args);
        string ToString(string delimiter);
        void ResetTemplate(ref string[] template);
    }
}
