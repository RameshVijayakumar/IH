using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Paycor.Import.ImportHubTest.Common;

namespace Paycor.Import.ImportHubTest.ApiTest.TestData
{
    public class DataRecord : IDataRecord
    {
        public static readonly ILog _log = LogManager.GetLogger(typeof (DataRecord));
        private readonly OrderedDictionary _recordCollection = new OrderedDictionary();

        public DataRecord()
        {
            
        }

        public DataRecord(ref string[] recordTemplate)
        {
            ResetTemplate(ref recordTemplate);
        }

        public bool IsEmpty
        {
            get { return _recordCollection.Count == 0; }
        }

        public void ResetTemplate(ref string[] template)
        {
            if(!IsEmpty)
                _recordCollection.Clear();
            
            foreach (var key in template)
            {
                 _recordCollection.Add(key, String.Empty);
            }      
        }

        public void SetValue(string column, string value)
        {
                _recordCollection[column] = value;
        }

        /// <summary>
        /// use = delimited parameters e.g. "RecordType=N"
        /// </summary>
        /// <param name="args"></param>
        public void SetValues(params string[] args)
        {
            foreach (var keyValue in args)
            {
                try
                {
                    string[] kv = keyValue.Split('=');
                    SetValue(kv[0], kv[1]);
                }
                catch (Exception ex)
                {                   
                    throw new AutomationTestException(ex.StackTrace);
                }

            }
        }

        public bool ColumnExist(string column)
        {
            return _recordCollection.Contains(column);
        }

        public void RemoveColumn(string column)
        {
            if(ColumnExist(column))
                _recordCollection.Remove(column);
        }

        public void RemoveColumnAt(int index)
        {
            if (index <= _recordCollection.Count && index >= 0)
            {
                _recordCollection.RemoveAt(index);
            }
        }

        public void AddColumn(string column, string value)
        {
            try
            {
                    _recordCollection.Add(column, value);
            }
            catch (Exception ex)
            {
                throw new AutomationTestException(ex.Message);
            }
        }

        public void InsertColumnAt(int index, string column, string value)
        {
            try
            {
                _recordCollection.Insert(index, column, value);
            }
            catch (Exception ex)
            {
                throw new AutomationTestException(ex.Message);
            }
        }

        public string ToString(string delimiter)
        {
            StringBuilder builder = new StringBuilder();
            
            foreach (var key in _recordCollection.Keys)
            {
                builder.Append(_recordCollection[key]);
                builder.Append(delimiter);
            }
            string retString = builder.ToString(0, builder.Length - delimiter.Length);
            _log.InfoFormat(retString);
            return retString;
        }

    }
}
