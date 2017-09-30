namespace Paycor.Import.ImportHubTest.ApiTest
{
    public enum EEImportStatusCode
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
        EmployeeImportComplete = 24,
        Queued = 98,
        PrevalidationFailure = 99,
        ProcessingFailure = 100

    }
    
    public enum CsvImportStatus
    {
        ImportFileData = 1,
        ImportComplete = 24,
        Queued = 98,
        PrevalidationFailure = 99,
        ProcessingFailure = 100
    }

    public enum ImportHistoryStatus
    {
        Queued = 1,
        Processing,
        Completed,
        Unknown
    }
}
