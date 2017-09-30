using Paycor.Import.ImportHubTest.ApiTest.TestData;

namespace Paycor.Import.ImportHubTest.ApiTest.EmployeeImport
{
    public class EmployeeImportRecord
    {
        public DataRecord GeneralInfo { get; private set; }
        public DataRecord StaticInfo { get; private set; }
        public DataRecord Earnings { get; private set; }
        public DataRecord Deductions { get; private set; }
        public DataRecord Taxes { get; private set; }
        public DataRecord Benefits { get; private set; }
        public DataRecord PayRate { get; private set; }
        public DataRecord DirectDeposit { get; private set; }
        public FileType EeImportFileType { get; private set; }
        
        public EmployeeImportRecord()
        {
            GeneralInfo = new DataRecord(ref EmployeeImportRecordTemplates.Benefits);
            GeneralInfo = new DataRecord(ref EmployeeImportRecordTemplates.Deductions);
            GeneralInfo = new DataRecord(ref EmployeeImportRecordTemplates.DirectDeposit);
            GeneralInfo = new DataRecord(ref EmployeeImportRecordTemplates.Earnings);
            GeneralInfo = new DataRecord(ref EmployeeImportRecordTemplates.GeneralInfo);
            GeneralInfo = new DataRecord(ref EmployeeImportRecordTemplates.PayRate);
            GeneralInfo = new DataRecord(ref EmployeeImportRecordTemplates.StaticInfo);
            GeneralInfo = new DataRecord(ref EmployeeImportRecordTemplates.Taxes);
            EeImportFileType = new FileType("\t", "txt");
        }

        public void SetPreValidationRequirments(DataRecord record,
            string recordType, 
            string modificationType,
            string clientId,
            string empNumber,
            string effectiveDate,
            string seqNumber)
        {
            record.SetValues("FileType", recordType);
            record.SetValues("ModificationType", modificationType);
            record.SetValues("ClientID", clientId);
            record.SetValues("EmpNumber", empNumber);
            record.SetValues("EffectiveDate", effectiveDate);
            record.SetValues("SeqNumber", seqNumber);
        }
    }
}
