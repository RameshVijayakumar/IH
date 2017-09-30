using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.ImportHubTest.ApiTest.Types;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.ApiTest.TestData
{
    public class MappedImportDataSheet 
    {
        private readonly Dictionary<string, MappingField> _columnAttributeDictionary = new Dictionary<string, MappingField>();

        public List<List<string>> DataSheet { get; } = new List<List<string>>();

        public List<string> Headers => DataSheet[0];

        public string HeaderString => string.Join(",", Headers);

        public int Size => DataSheet.Count();

        public bool IsAutoGenerateFromMap { get; private set; }

        public MappedImportDataSheet() { }

        public MappedImportDataSheet(List<string> headers)
        {
            DataSheet[0] = headers;
        }
        public MappedImportDataSheet(FieldDefinitionCollection fieldDefinitionCollection )
        {

            if( !fieldDefinitionCollection.FieldDefinitions.Any())
                throw new AutomationTestException("FieldDefinitionCollection is empty, cannot auto generate test data");

            var fieldDefinitions = fieldDefinitionCollection.FieldDefinitions;
            var headers = new List<string>();
            foreach (var fieldDefinition in fieldDefinitions)
            {
                _columnAttributeDictionary.Add(fieldDefinition.Source, fieldDefinition);
                headers.Add(fieldDefinition.Source);
            }
            DataSheet.Add(headers);
            IsAutoGenerateFromMap = true;
        }

        public void GenerateRandomData(int numOfRows)
        {
            for (var r = 0; r < numOfRows; r++)
            {
                var row = new List<string>();
                foreach (var columnName in Headers)
                {
                    row.Add(GenerateRandomValueBySourceType(columnName));
                }
                DataSheet.Add(row);
            }
        }

        private string GenerateRandomValueBySourceType(string source)
        {
            string data = null;
            //if (source = "id" || "{id}") Generate Id string

            if (_columnAttributeDictionary[source].Type.CompareString("int", Utils.ItStringCompareOptions.Exact))
            {
                var rand = new Random();
                data = rand.Next(0, Int32.MaxValue).ToString();
            }

            return data;
        }

        public void SetHeadersFromString( string headerString, char delimiter )
        {
            DataSheet[0] = headerString?.Split(delimiter).ToList();
        }

        public List<List<string>> GetDataRowSample(int startRow, int endRow)
        {
            List<List<string>> data = new List<List<string>>();
            for (var i = startRow; i < endRow; i++)
            {
                data.Add(DataSheet[i]);
            }
            return data;
        }

        //public List<List<string>> GetDataSampleWithHeader()
        //{
        //    //if (_size == 0 || maxOfRows <= 0)
        //    //{
        //    //    throw new AutomationTestException($"Empty datasheet or out of index {maxOfRows}");
        //    //}
        //    //maxOfRows = maxOfRows < _size ? maxOfRows : _size;
        //    //var  newSheet = new List<List<string>>();
        //    //for(var i = 0; i <= maxOfRows; i++)
        //    //{
        //    //    newSheet.Add(_dataSheet[i]);
        //    //}
        //    //return newSheet;
        //}
    }
}
