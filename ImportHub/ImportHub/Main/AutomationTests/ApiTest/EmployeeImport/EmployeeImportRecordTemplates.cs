using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paycor.Import.ImportHubTest.ApiTest.EmployeeImport
{
    public static class EmployeeImportRecordTemplates
    {
        public static String[] GeneralInfo = new string[]
        {
            "RecordType",
            "ModificationType",
            "ClientID",
            "EmpNumber",
            "Last Name",
            "EffectiveDate",
            "SeqNumber",
            "First Name",
            "Mid Name",
            "Suffix",
            "Accredited",
            "Address 1",
            "Suite",
            "Address 2",
            "City",
            "State",
            "Zip",
            "Work Area Code",
            "Work Phone Number",
            "Work E-mail address",
            "Base Department",
            "Payroll",
            "Paygroup",
            "Check Sort Order",
            "Status",
            "Term. Date",
            "Termination Reason",
            "Rehire Date",
            "W2 Pension",
            "FLSA",
            "EE Status Type",
            "Actual Marital Status",
            "Job Title",
            "Adj. Hire date",
            "DependInsuranceEligible",
            "DependInsEligible Date",
            "Annual Hours",
            "OwnerOfficer",
            "Mobile Area Code",
            "Mobile Phone Number",
            "Home Area Code",
            "Home Phone Number",
            "Home Email Address",
            "Base Shift",
            "PA PSD Code",
            "PA PSD Rate"           
        };

        public static string[] StaticInfo = new string[]
        {
            "RecordType",
            "ModificationType",
            "ClientID",
            "EmpNumber",
            "Last Name",
            "EffectiveDate",
            "SeqNumber",
            "SSN",
            "Hire Date",
            "Birth Date",
            "Gender",
            "Ethnicity",
            "Seniority Date",
            "Clock #",
            "ToD Badge #",
            "Employee Type",
            "Custom Field 3",
            "Custom Field 2",
            "Custom Field 1",
            "Custom Field 4",
            "Custom Field 5",
            "Custom Date 1",
            "Custom Date 2",
            "Custom Date 3",
            "Custom Date 4",
            "Custom Date 5",
            "Maiden Name"
        };

        public static string[] Earnings = new string[]
        {
            "RecordType",
            "ModificationType",
            "ClientID",
            "Last Name",
            "EffectiveDate",
            "SeqNumber",
            "Earning Code",
            "Earning Amount",
            "Earning Rate",
            "Earning Hold Indicator",
            "Policy Amount",
            "Std Hours"
        };

        public static string[] Deductions = new string[]
        {
            "RecordType",
            "ModificationType",
            "ClientID",
            "EmpNumber",
            "Last Name",
            "EffectiveDate",
            "SeqNumber",
            "Deduction Code",
            "Deduction Rate",
            "Deduction Amount",
            "Deduction Case#",
            "Deduction Max Amt (Cap)",
            "Deduction Hold Indicator",
            "FIPS Code",
            "Insurance Indicator"
        };

        public static string[] Taxes = new string[]
        {
            "RecordType",
            "ModificationType",
            "ClientID",
            "EmpNumber",
            "Last Name",
            "EffectiveDate",
            "SeqNumber",
            "Tax Code",
            "Filing Status",
            "Exemptions",
            "Block Table Indicator",
            "Withholding Percent %",
            "Withholding Amt$",
            "Live In / Work In",
            "Tax Hold Indicator",
            "Spouse Work",
            "Add'l State Exemption",
            "NCCI Code"
        };

        public static string[] Benefits = new string[]
        {
            "RecordType",
            "ModificationType",
            "ClientID",
            "EmpNumber",
            "Last Name",
            "EffectiveDate",
            "SeqNumber",
            "Benefit Code",
            "Benefit Hold Indicator"
        };

        public static string[] PayRate = new string[]
        {
            "RecordType",
            "ModificationType",
            "ClientID",
            "EmpNumber",
            "Last Name",
            "EffectiveDate",
            "SeqNumber",
            "Rate Type",
            "Pay Rate",
            "Rate Description",
            "Rate Reason"
        };

        public static string[] DirectDeposit = new string[]
        {
            "RecordType",
            "ModificationType",
            "ClientID",
            "EmpNumber",
            "Last Name",
            "EffectiveDate",
            "SeqNumber",
            "Direct Deposit Code",
            "Bank Routing No.",
            "Bank Account No.",
            "Bank Account Type",
            "Deposit Percent",
            "Deposit Amount",
            "Dir Dep Hold Indicator"
        };
    }
}
