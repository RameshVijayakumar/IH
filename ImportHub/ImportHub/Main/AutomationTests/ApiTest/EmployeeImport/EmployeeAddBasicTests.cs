using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.ImportHubTest.ApiTest.TestData;

namespace Paycor.Import.ImportHubTest.ApiTest.EmployeeImport
{
    [TestClass]
    public class EmployeeAddBasicTests
    {
        readonly List<DataRecord> employeeImportRecords = new List<DataRecord>();

        public DataRecord GeneralInfo { get; private set; }
        public DataRecord StaticInfo { get; private set; }
        public DataRecord Earnings { get; private set; }
        public DataRecord Deductions { get; private set; }
        public DataRecord Taxes { get; private set; }
        public DataRecord Benefits { get; private set; }
        public DataRecord PayRate { get; private set; }
        public DataRecord DirectDeposit { get; private set; }
        public FileType EeImportFileType { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            Benefits = new DataRecord(ref EmployeeImportRecordTemplates.Benefits);
            Deductions = new DataRecord(ref EmployeeImportRecordTemplates.Deductions);
            DirectDeposit = new DataRecord(ref EmployeeImportRecordTemplates.DirectDeposit);
            Earnings = new DataRecord(ref EmployeeImportRecordTemplates.Earnings);
            GeneralInfo = new DataRecord(ref EmployeeImportRecordTemplates.GeneralInfo);
            PayRate = new DataRecord(ref EmployeeImportRecordTemplates.PayRate);
            StaticInfo = new DataRecord(ref EmployeeImportRecordTemplates.StaticInfo);
            Taxes = new DataRecord(ref EmployeeImportRecordTemplates.Taxes);
            EeImportFileType = new FileType("\t,", "txt");
        }

        [TestMethod]
        [TestCategory("Employee Import Tests")]
        public void EEImportPreValidationFieldsTest()
        {
            GeneralInfo.SetValues("RecordType=N", "ModificationType=A", "ClientID=102", "EmpNumber=1656", "EffectiveDate=09/20/2015", "SeqNumber=23");
            employeeImportRecords.Add(GeneralInfo);
            FileGenerator.SetFileType(EeImportFileType);
            String filename = "testfile_1";
            FileGenerator.ExportTestFile<DataRecord>(employeeImportRecords, filename);
            /*
            record.SetValues("FileType", recordType);
            record.SetValues("ModificationType", modificationType);
            record.SetValues("ClientID", clientId);
            record.SetValues("EmpNumber", empNumber);
            record.SetValues("EffectiveDate", effectiveDate);
            record.SetValues("SeqNumber", seqNumber);
             */

        }
    }
}
