using System;
using System.Collections.Generic;
using System.Linq;

namespace Paycor.Import.Registration
{
    public static class MappingDescription
    {
        private static readonly List<KeyValuePair<string, string>> MappingDescriptions = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("Alias","Alias imports are designed to allow Paycor\'s system to interact/interface with Client systems with differing value names.  Aliases, if setup correctly, will  translate values between the two systems."),
            new KeyValuePair<string, string>("Client Deduction","Enables the import of the Client Deduction setup data. Client Deduction setup is a prerequisite to setting up employee profiles."),
            new KeyValuePair<string, string>("Client Earning","Enables the import of the Client Earning setup data. Client Earnings setup is a prerequisite to setting up employee profiles."),
            new KeyValuePair<string, string>("Employee","Enables the import of the basic employee information. Supports partial updates."),
            new KeyValuePair<string, string>("Employee Asset","Enables the import of employee asset data within Perform HR. Imported data can be found on the Employee Assets page within Perform."),
            new KeyValuePair<string, string>("Employee Certification","Enables the import of Certification data within Perform HR. Imported data can be found on the Employee Certification page within Perform."),
            new KeyValuePair<string, string>("Employee Custom Fields","Enables the import of Perform Custom Fields. Imported data can be viewed on the Custom Field page within the Employee Profile in Perform, which is different from the Custom Field information on Paycor Main."),
            new KeyValuePair<string, string>("Employee Discipline","Enables the import of employee conduct data within Perform HR. Imported data can be found on the Employee Coaching/Discipline page within Perform."),
            new KeyValuePair<string, string>("Employee Job Title","Enables the import of job title data within Perform. Imported data can be found on the Jobs page under Configure Company within Perform. "),
            new KeyValuePair<string, string>("Employee Leave Case","Enables the import of employee leave case data within Perform HR. Imported data can be found on the Leave Cases tab on the Employee Status page within Perform."),
            new KeyValuePair<string, string>("Employee Leave Case and Activities","Enables the import of activity for employee leave cases within Perform HR. Imported data can be found on the Leave Cases tab on the Employee Status page within Perform."),
            new KeyValuePair<string, string>("Employee Safety Incidents","Enables the import of Safety Incidents within Perform HR.  Imported data can be found on the Employee Safety Incidents page within Perform."),
            new KeyValuePair<string, string>("General Ledger","Enables to import General Ledger accounts for a client or a department."),
            new KeyValuePair<string, string>("Organization","Enables the import of department data to the client profile. The imported data can be seen on the Client Organization page within the Manage Company page. Supports add and change of departments. Does not support setup of local taxes on the org structure."),
            new KeyValuePair<string, string>("Point of Sales Pay Import","Enables the import of Time data into Paydata. "),
            new KeyValuePair<string, string>("Third Party Benefit","Enables the import of hours associated with an employee benefit code so this information can be printed on the paystub. "),
            new KeyValuePair<string, string>("Work Locations","Enable the import of Work Location data for the company setup."),
            new KeyValuePair<string, string>("Employee Direct Deposit","Enables the import of direct deposit information for an Employee. "),
            new KeyValuePair<string, string>("Client Taxes","Enables the import of the Client Tax setup data. Imported data can be found on the Client Taxes page within the Company Configuration in Perform."),
            new KeyValuePair<string, string>("Employee Accruals","Enables the import of accrual setup information for an Employee. Imported data can be found on the Accrual Setup page within the Employee Profile in Perform."),
            new KeyValuePair<string, string>("Employee Address","Enables the import of employee address information. Allows for direct interaction with address information. Imported data can be found on the Contact Information page within the Employee Profile in Perform."),
            new KeyValuePair<string, string>("Employee Deduction","Enables the import of deduction setup information for an Employee. Imported data can be found on the Deduction page within the Employee Profile in Perform."),
            new KeyValuePair<string, string>("Employee Earning","Enables the import of earning setup information for an Employee. Imported data can be found on the Additional Earning page within the Employee Profile in Perform."),
            new KeyValuePair<string, string>("Employee Pay Rates","Enables the import of earning setup information for an Employee. Imported data can be found on the Pay Rates page within the Employee Profile in Perform."),
            new KeyValuePair<string, string>("Employee Tax","Enables the import of tax setup information for an Employee. Imported data can be found on the Employee Tax page within the Employee Profile in Perform."),
            new KeyValuePair<string, string>("Employment Status History","Enables the import of status history records for an employee. Imported data can be found on the Status page within the Employee Profile in Perform."),
            new KeyValuePair<string, string>("Employee I-9","Use the Import Data tab in this template to import I-9 data for employees."),
            new KeyValuePair<string, string>("Employee Position History","Use the Import Data tab in this template to Import Employee Position History."),
            new KeyValuePair<string, string>("Employee Emergency Contact","Use the Import Data tab in this template to Import Employee Emergency Contacts."),
            new KeyValuePair<string, string>("Employee Performance Review","Use this template to import Employee Performance Review History to the Performance Review page under Manage Employees."),
            new KeyValuePair<string, string>("Employee Goals","Enables the add, edit, and delete of employee goals via import. Imported data can be found on the employee’s Goals page within the Employee Profile in Perform.")
        };

        public static string GetMappingDescription(string mappingName)
        {
            if (string.IsNullOrEmpty(mappingName))
                return null;

            return MappingDescriptions.Where(t => string.Equals(t.Key, mappingName, StringComparison.CurrentCultureIgnoreCase))
                .Select(t => t.Value)
                .FirstOrDefault();
        }
    }
}
