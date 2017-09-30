namespace Paycor.Import.Employee.Status
{
    public enum EmployeeImportStatusEnum
    {
        InitiateEmployeeImport = 0,
        ImportFileData = 1,
        ValidateRecordTypes = 3,
        ValidateDataTypes = 4,
        ValidateGeneralRecord = 5,
        ValidateStaticRecord = 6,
        ValidateEarningRecord = 7,
        ValidateDeductionRecord = 8,
        ValidateBenefitRecord = 9,
        ValidateDirectDepositRecord = 10,
        ValidateRateRecord = 11,
        ValidateTaxRecord = 12,
        RetrieveValidationResults = 2, 
        EmployeeImportUpdateGeneral = 13,
        EmployeeImportUpdateGroupTermLifeEarnings = 14,
        EmployeeImportUpdateEarnings = 15,
        EmployeeImportUpdateDefaultEarnings = 16,
        EmployeeImportUpdateDeductions = 17,
        EmployeeImportUpdateTaxes = 18,
        EmployeeImportUpdateDirectDeposit = 19,
        EmployeeImportUpdateRates = 20,
        EmployeeImportUpdateBenefits = 21,
        EmployeeImportUpdateTimeAndAttendance = 22,
        ClearImportTables = 23,
        ImportComplete = 24,
        Queued = 98
    }

    public enum EmployeeImportIssueTypeEnum
    {
        Warning = 0,
        Error = 1
    }
}
