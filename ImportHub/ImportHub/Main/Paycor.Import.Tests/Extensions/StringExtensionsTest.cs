using System;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Extensions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Paycor.Import.Tests.Extensions
{
    /// <summary>
    /// Summary description for StringExtensionsTest
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class StringExtensionsTest
    {
        const int MaximumLength = 120;
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void TestStringExtension_ParseFirstQueryStringParamForValid()
        {
            const string endpoint = @"/employee/v2/clients/{clientId}/employees?employeeNumber={employeeNumber}";

            var param = endpoint.ParseFirstQueryStringParam();

            Assert.AreEqual("{employeeNumber}", param);
        }

        [ExpectedException(typeof(ImportException))]
        [TestMethod]
        public void TestStringExtension_ParseFirstQueryStringParamForInvalid()
        {
            var endpoint = @"/employee/v2/clients/{clientId}/employees?employeeNumbe";

            var param = endpoint.ParseFirstQueryStringParam();

            Assert.AreEqual("/employee/v2/clients/{clientId}/employees?employeeNumbe", param);
        }

        [TestMethod]
        public void Replace_Query_Param_With_ValueIn_EndPoint()
        {
            var endPoint = @"localhost:54911/import/referenceapi/v2/formula1/drivers?firstName={firstName}";
            var values = new Dictionary<string, string>
            {
                {"firstName", "Ramesh"}

            };
            endPoint = endPoint.ReplaceQueryStringParam(values);

            Assert.AreEqual("localhost:54911/import/referenceapi/v2/formula1/drivers?firstName=Ramesh", endPoint);

        }

        [TestMethod]
        public void Replace_Query_Param_With_StaticParam()
        {
            var endPoint =
                @"localhost:80/employeeservice/v2/employees?clientId={{clientId}}&employeeNumber={{employeeNumber}}&includeUid=true";
            var values = new Dictionary<string, string>
            {
                {"clientId", "102"},
                {"employeeNumber", "1221"}
            };
            endPoint = endPoint.ReplaceQueryStringParam(values);

            Assert.AreEqual(
                "localhost:80/employeeservice/v2/employees?clientId=102&employeeNumber=1221&includeUid=true", endPoint);
        }

        [TestMethod]
        public void Replace_Query_Param_With_Multiple_ValueIn_EndPoint()
        {
            var endPoint =
                @"localhost:80/employeeservice/employeeservice/v2/employees?clientId={{clientId}}&employeeNumber={{employeeNumber}}";
            var values = new Dictionary<string, string>
            {
                {"clientId", "102"},
                {"employeeNumber", "1221"}
            };
            endPoint = endPoint.ReplaceQueryStringParam(values);
            Assert.AreEqual(
                "localhost:80/employeeservice/employeeservice/v2/employees?clientId=102&employeeNumber=1221", endPoint);

        }

        [TestMethod]
        public void Replace_Query_Param_With_Empty_ValueIn_EndPoint()
        {
            var endPoint =
                @"localhost:80/employeeservice/employeeservice/v2/employees?clientId={{clientId}}&employeeNumber={{employeeNumber}}";
            var values = new Dictionary<string, string>
            {
                {"clientId", "102"},
                {"employeeNumber", ""}
            };
            endPoint = endPoint.ReplaceQueryStringParam(values);
            Assert.AreEqual("localhost:80/employeeservice/employeeservice/v2/employees?clientId=102&employeeNumber=",
                endPoint);

        }

        [TestMethod]
        public void Replace_Query_Param_With_Diff_QueryParam()
        {
            var endPoint =
                @"localhost:80/employeeservice/employeeservice/v2/employees?clientId={{clientId}}&code={{clientCode}}";
            var values = new Dictionary<string, string>
            {
                {"clientId", "102"},
                {"clientCode", "1221"}
            };
            endPoint = endPoint.ReplaceQueryStringParam(values);
            Assert.AreEqual("localhost:80/employeeservice/employeeservice/v2/employees?clientId=102&code=1221", endPoint);

        }

        [TestMethod]
        public void Replace_Query_Param_CaseSensitive()
        {
            var endPoint =
                @"http://devsbqtrweb01.dev.paycor.com/payrollservice/v2/employees/{employeeId}/accruals?accrualcode={AccrualCode}";
            var values = new Dictionary<string, string>
            {
                {"accrualCode", "AccSick"},
            };
            endPoint = endPoint.ReplaceQueryStringParam(values);
            Assert.AreEqual("http://devsbqtrweb01.dev.paycor.com/payrollservice/v2/employees/{employeeId}/accruals?accrualcode=AccSick", endPoint);
        }


        [ExpectedException(typeof(ImportException))]
        [TestMethod]
        public void Try_Replace_Query_Param_When_NoQuery_Param_Exists()
        {
            const string endPoint = @"localhost:54911/import/referenceapi/v2/formula1/drivers?firstName{firstName}";
            var values = new Dictionary<string, string>
            {
                {"firstName", "Ramesh"}

            };
            endPoint.ReplaceQueryStringParam(values);
        }


        [TestMethod]
        public void TestStringExtension_ReplaceQuotes()
        {
            //Case-1
            var inputData1 = @"""I live, """"Plano!""""Dallas""";
            var result1 = inputData1.ReplaceQuotes();
            var expectedResult1 = "I live, \"Plano!\"Dallas";
            Assert.AreEqual(expectedResult1, result1);

            //Case-2
            var inputData2 = @"""PO Box 990""";
            var result2 = inputData2.ReplaceQuotes();
            var expectedResult2 = "\"PO Box 990\"";
            Assert.AreEqual(expectedResult2, result2);

            //Case-3
            var inputData3 = "\"Technimark\nHourly\"";
            var result3 = inputData3.ReplaceQuotes();
            var expectedResult3 = "\"Technimark\nHourly\"";
            Assert.AreEqual(expectedResult3, result3);

            //Case-4
            var inputData4 = "";
            var result4 = inputData4.ReplaceQuotes();
            var expectedResult4 = "";
            Assert.AreEqual(expectedResult4, result4);

            //Case-5
            var inputData5 = @"""I live, """"Plano!""""Dallas""";
            var result5 = inputData5.ReplaceQuotes();
            var expectedResult5 = "\"I live, \"Plano\" Dallas";
            Assert.AreNotEqual(expectedResult5, result5);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestStringExtension_ContainsDoubleQuotes()
        {
            //Case-1
            var input2 = "\"Ashif\"";
            var result2 = input2.ContainsDoubleQuotes();
            var expectedResult2 = true;
            Assert.AreEqual(expectedResult2, result2);

            //Case-2
            var input3 = "This is a weekday";
            var result3 = input3.ContainsDoubleQuotes();
            var expectedResult3 = false;
            Assert.AreEqual(expectedResult3, result3);

            //Case-3
            var input1 = "";
            var result1 = input1.ContainsDoubleQuotes();
            Assert.IsNull(result1);


        }

        [TestMethod]
        public void Remove_WhiteSpaces_EmptySpace()
        {
            const string mystring = @"pub lish er";
            var result = mystring.RemoveWhiteSpaces();

            Assert.AreEqual("publisher", result);
        }

        [TestMethod]
        public void Remove_WhiteSpaces_NewLine_Tab_Etc()
        {
            var mystring = @"pub lish" + Environment.NewLine + "er" + "\t" + "\f" + "\r" + "\v";
            var result = mystring.RemoveWhiteSpaces();

            Assert.AreEqual("publisher", result);
        }

        [TestMethod]
        public void Remove_WhiteSpaces_For_EmptyString()
        {
            string mystring = string.Empty;
            var result = mystring.RemoveWhiteSpaces();

            Assert.AreEqual(String.Empty, result);
        }

        [TestMethod]
        public void Remove_WhiteSpaces_null()
        {
            const string mystring = null;
            var result = mystring.RemoveWhiteSpaces();

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void ReplaceRouteParamWithValue()
        {
            const string mystring = @"http://localhost:8083/import/referenceapi/v1/gamecatalog/{gameid}/games";
            var result = mystring.ReplaceRouteParamWithValue("{gameid}", "5");
            var resultEmptyReplaceValue = mystring.ReplaceRouteParamWithValue("{gameid}", "");
            Assert.AreEqual(@"http://localhost:8083/import/referenceapi/v1/gamecatalog/5/games", result);
            Assert.AreEqual(@"http://localhost:8083/import/referenceapi/v1/gamecatalog/{gameid}/games",
                resultEmptyReplaceValue);
        }

        [TestMethod]
        public void Ignore_Same_String_SameValues()
        {
            string mystring = @"test string";
            string anotherstring = @"test string";
            var result = mystring.IgnoreSameString(anotherstring);

            Assert.AreEqual(@"test string", result);
        }


        [TestMethod]
        public void Ignore_Same_String_DifferentValues()
        {
            string mystring = @"My string";
            string anotherstring = @"Another string";
            var result = mystring.IgnoreSameString(anotherstring);

            Assert.AreEqual(@"My string" + @"Another string", result);
        }

        [TestMethod]
        public void TestCamelCaseToWords()
        {
            var input = "myCamelCaseString";
            var expected = "My Camel Case String";
            var actual = input.CamelCaseToWords();

            Assert.AreEqual(expected, actual);

            input = "MyStringOfWords";
            expected = "My String Of Words";
            actual = input.CamelCaseToWords();
            Assert.AreEqual(expected, actual);

            input = "WebAPIForNoobs";
            expected = "Web API For Noobs";
            actual = input.CamelCaseToWords();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_RemoveIndexValueFromString_WithIndexValueInSubArray()
        {
            const string expected = "addresses[].city";
            var actual = "addresses[0].city";
            actual = actual.RemoveIndexValueFromString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_RemoveIndexValueFromString_WithNoIndexValueInSubArray()
        {
            const string expected = "addresses[].state";
            var actual = "addresses[].state";
            actual = actual.RemoveIndexValueFromString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_RemoveIndexValueFromString_WithNoSubArray()
        {
            const string expected = "address";
            var actual = "address";
            actual = actual.RemoveIndexValueFromString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_RemoveIndexValueFromString_WithIndexValueInStringArray()
        {
            const string expected = "linkcodes[]";
            var actual = "linkcodes[0]";
            actual = actual.RemoveIndexValueFromString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_RemoveIndexValueFromString_WithNoIndexValueInStringArray()
        {
            const string expected = "linkcodes[]";
            var actual = "linkcodes[]";
            actual = actual.RemoveIndexValueFromString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestContainsLookUpWithBraces()
        {
            var postEndPoint1 = @"http://localhost:8083/import/referenceapi/v2/formula1/teams/{teamId}/drivers";
            var result1 = postEndPoint1.ContainsOpenAndClosedBraces();
            Assert.AreEqual(true, result1);

            var postEndPoint2 = @"http://localhost:8083/import/referenceapi/v2/formula1/teams/102/drivers";
            var result2 = postEndPoint2.ContainsOpenAndClosedBraces();
            Assert.AreNotEqual(true, result2);

            var patchEndPoint1 = @"http://localhost:80/employeeservice/v2/employees/{id}";
            var result3 = patchEndPoint1.ContainsOpenAndClosedBraces();
            Assert.AreEqual(true, result3);

            var patchEndPoint2 = @"http://localhost:80/employeeservice/v2/employees/0000-0000-0000-0000";
            var result4 = patchEndPoint2.ContainsOpenAndClosedBraces();
            Assert.AreNotEqual(true, result4);
        }

        [TestMethod]
        public void TestStringExtension_ContainsCustomSubstring_WithSubstring()
        {
            const bool expected = true;
            const string input = "this is my string";
            const string substring = "STRING";

            var actual = input.ContainsSubstring(substring);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_ContainsCustomSubstring_WithNoSubstring()
        {
            const bool expected = false;
            const string input = "this is my string";
            const string substring = "word";

            var actual = input.ContainsSubstring(substring);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_GetSuffixFromString_WithSuffix()
        {
            const string suffix = "suffix";
            const string expected = suffix;
            const string input = "string.suffix";

            var actual = input.GetSuffixAfterSymbol(ImportConstants.Period);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_GetSuffixFromString_WithNoSuffix()
        {
            const string expected = "";
            const string input = "stringhasnosuffix.";

            var actual = input.GetSuffixAfterSymbol(ImportConstants.Period);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_GetSuffixFromString_WithNoSymbol()
        {
            const string expected = "stringhasnosuffix";
            const string input = "stringhasnosuffix";

            var actual = input.GetSuffixAfterSymbol(ImportConstants.Period);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_ReplaceStringIgnoreCase()
        {
            const string expected = "Replace this replacevalue";
            const string input = "Replace this {pattern}";
            const string pattern = "{Pattern}";
            const string replacement = "replacevalue";


            var actual = input.ReplaceStringIgnoreCase(pattern, replacement);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_ReplaceStringIgnoreCase_CaseSens()
        {
            const string expected = "accrualcode=AccSick";
            const string input = "accrualcode=AccrualCode";
            const string pattern = "=accrualCode";
            const string replacement = "=AccSick";


            var actual = input.ReplaceStringIgnoreCase(pattern, replacement);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringExtension_GetRouteParameters()
        {
            string route =
                "http://devsbqtrweb01.dev.paycor.com:80/employeeservice/v2/employees/{id}/certifications/{certId}";
            var results = route.GetFieldsFromBraces();

            CollectionAssert.Contains(results, "certId");
        }

        [TestMethod]
        public void TestStringExtension_GetRouteParametersFromQueryString()
        {
            string route =
                "http://devsbqtrweb01.dev.paycor.com:80/employeeservice/v2/employees?clientId={{clientId}}&employeeNumber={{employeeNumber}}";
            var results = route.GetFieldsFromBraces();

            CollectionAssert.Contains(results, "clientId");
        }

        [TestMethod]
        public void TestStringExtension_CsvEscape()
        {
            var test = "this, is, a test";
            var expected = "\"this, is, a test\"";
            var actual = test.CsvEscape();
            Assert.AreEqual(expected, actual);

            test = "this is a test";
            expected = test;
            actual = test.CsvEscape();
            Assert.AreEqual(expected, actual);

            test = "this \"is a test.\"";
            expected = "\"this \"\"is a test.\"\"\"";
            actual = test.CsvEscape();
            Assert.AreEqual(expected, actual);

            test = "Hello, my name is \"Alan\"";
            expected = "\"Hello, my name is \"\"Alan\"\"\"";
            actual = test.CsvEscape();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIsNumeric()
        {
            const string firstArg = "abc";
            Assert.AreEqual(firstArg.IsOrdinal(), false);

            const string secArg = "abc1";
            Assert.AreEqual(secArg.IsOrdinal(), false);

            const string thirdArg = "1";
            Assert.AreEqual(thirdArg.IsOrdinal(), true);

            const string forthArg = "0";
            Assert.AreEqual(forthArg.IsOrdinal(), true);
        }

        [TestMethod]
        public void Test_FormatFileName_DateTime_Null()
        {
            var fileName = "Test File";
            var formatType = ImportConstants.XlsxFileExtension;
            var importType = "Employee";
            var expected = "Employee_Test File.xlsx";
            var actual = fileName.FormatFileName(formatType, importType);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_FormatFileName_ImportType_Empty()
        {
            var fileName = "Test File";
            var formatType = ImportConstants.XlsxFileExtension;
            var importType = "";
            var expected = "_Test File.xlsx";
            var actual = fileName.FormatFileName(formatType, importType);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_FormatFileName()
        {
            var fileName = "Test File";
            var formatType = ImportConstants.XlsxFileExtension;
            var importType = "Employee";
            var dateTime = "08102016";
            var expected = "Employee_Test File_08102016.xlsx";
            var actual = fileName.FormatFileName(formatType, importType, dateTime);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_GetMediaTypeHeaderValue_Xlsx()
        {
            var formatType = ImportConstants.XlsxFileExtension;
            var expected = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var actual = formatType.GetMediaTypeHeaderValue();
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void Test_GetMediaTypeHeaderValue_Csv()
        {
            var formatType = ImportConstants.Csv;
            var expected = "text/csv";
            var actual = formatType.GetMediaTypeHeaderValue();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_GetNotContainedValues()
        {
            var input1 = new List<string>
            {
                "A",
                "B",
                "C",
                "D"
            };
            var input2 = new List<string>
            {
                "A",
                "B",
                "C"
            };
            var actual = input1.GetNotContainedValues(input2);
            Assert.AreEqual("D", actual[0]);
        }

        [TestMethod]
        public void Test_ConcatListOfString()
        {
            var input1 = new List<string>
            {
                "A",
                "B",
                "C",
                "D"
            };
            var actual = input1.ConcatListOfString(",");
            Assert.AreEqual("A,B,C,D", actual);
        }

        [TestMethod]
        public void Test_RemoveIndexValueFromString()
        {
            const string input = "clientId[10].code";
            var actual = input.RemoveIndexValueFromString();
            Assert.AreEqual("clientId[].code", actual);

            const string input1 = "clientId[1].code";
            var actual1 = input1.RemoveIndexValueFromString();
            Assert.AreEqual("clientId[].code", actual1);

            const string input2 = "clientId[123].code";
            var actual2 = input2.RemoveIndexValueFromString();
            Assert.AreEqual("clientId[].code", actual2);

            const string input3 = "c.[1234567890].c";
            var actual3 = input3.RemoveIndexValueFromString();
            Assert.AreEqual("c.[].c", actual3);

            const string input4 = "[1234]";
            var actual4 = input4.RemoveIndexValueFromString();
            Assert.AreEqual("[]", actual4);
        }

        [TestMethod]
        public void Test_EndsWithAnyInput()
        {
            const string supportedFileTypes = ".csv,.txt";
            const string supportedFileTypesExcel = ".xlsx";

            const string fileName1 = "pointOfSales.csv";
            const string fileName2 = "pointOfSales.txt";
            const string fileName3 = "pointOfSales.txt.xlsx.csv";
            const string fileName4 = "pointOfSales.txt.xlsx";


            var result = fileName1.EndsWithAnyInput(supportedFileTypes.Split(ImportConstants.Comma).ToList());
            Assert.AreEqual(true, result);
            var result1 = fileName2.EndsWithAnyInput(supportedFileTypes.Split(ImportConstants.Comma).ToList());
            Assert.AreEqual(true, result1);
            var result2 = fileName3.EndsWithAnyInput(supportedFileTypes.Split(ImportConstants.Comma).ToList());
            Assert.AreEqual(true, result2);
            var result3 = fileName4.EndsWithAnyInput(supportedFileTypes.Split(ImportConstants.Comma).ToList());
            Assert.AreEqual(false, result3);

            var resultExcel = fileName4.EndsWithAnyInput(supportedFileTypesExcel.Split(ImportConstants.Comma).ToList());
            Assert.AreEqual(true, resultExcel);
        }

        [TestMethod]
        public void ReplaceEmptyValuesWithOrdinals_Test()
        {
            var input = new List<string>
            {
                "",
                "",
                "Code",
                ""
            };

            var result = input.ReplaceEmptyValuesWithOrdinals();
            Assert.AreEqual("0", result[0]);
            Assert.AreEqual("1", result[1]);
            Assert.AreEqual("Code", result[2]);
            Assert.AreEqual("3", result[3]);
        }

        [TestMethod]
        public void Test_CheckGreaterThanLength_False()
        {
            var input = "This is a very small message";
            
            var actual = input.CheckGreaterThanLength(MaximumLength);
            var expected = "This is a very small message";
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void Test_CheckGreaterThanLength_True()
        {
            var largeMessage =
                "This message has greater than 120 characters. Reports & Analytics gives client the power to make your data work for them.  It's the end of wasted time from re-keying or maintaining several lists or databases of employee information. Reports & Analytics allows clients to find quick answers, because they can search through their information. And best of all, so long as they're online, they have access to whatever they need.";
            var result = largeMessage.CheckGreaterThanLength(MaximumLength);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Test_CheckGreaterThanLength_Null()
        {
            string input = null;

            var result = input.CheckGreaterThanLength(MaximumLength);
            Assert.IsNull(result);      
        }

        [TestMethod]
        public void Test_CheckGreaterThanLength_Empty()
        {
            string input = string.Empty;

            var actual = input.CheckGreaterThanLength(MaximumLength);
            var expected = string.Empty;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_AddBraces()
        {
            const string input = "temple";
            var actual = input.AddBraces();
            Assert.AreEqual("{temple}", actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_AddBraces_Exp()
        {
            const string input = null;
            var actual = input.AddBraces();
        }

        [TestMethod]
        public void SplitByPipe_IfPipeExists()
        {
            const string name = "First|Last";

            var splitedName = name.SplitbyPipe();
            Assert.AreEqual(splitedName[0], "First");
            Assert.AreEqual(splitedName[1], "Last");
        }

        [TestMethod]
        public void SplitByPipe_IfNoPipeExists()
        {
            const string name = "";

            var splitedName = name.SplitbyPipe();
            Assert.AreEqual(splitedName[0], "");
        }

        [TestMethod]
        public void IsConcatenation()
        {
            const string empty = "";
            Assert.IsFalse(empty.IsConcatenation());

            const string emptyNull = null;
            Assert.IsFalse(emptyNull.IsConcatenation());

            const string pipe = "A||B||C";
            Assert.IsTrue(pipe.IsConcatenation());
        }

        [TestMethod]
        public void GetFirstValueBeforePipe()
        {
            const string empty = "";
            Assert.AreEqual(string.Empty, empty.GetFirstValueBeforePipe());

            const string pipeA = "A|B|C";
            Assert.AreEqual("A", pipeA.GetFirstValueBeforePipe());

            const string nopipe = "ABC";
            Assert.AreEqual("ABC", nopipe.GetFirstValueBeforePipe());

            const string nullValue = null;
            Assert.AreEqual(string.Empty, nullValue.GetFirstValueBeforePipe());
        }

        [TestMethod]
        public void ExceptFirstValueAfterPipe()
        {
            const string empty = "";
            Assert.AreEqual("", empty.ExceptFirstValueAfterPipe().First());

            const string pipeA = "A|B|C";
            Assert.AreEqual("B", pipeA.ExceptFirstValueAfterPipe().First());
            Assert.AreEqual("C", pipeA.ExceptFirstValueAfterPipe().GetValue(1));

            const string nopipe = "ABC";
            Assert.AreEqual(0, nopipe.ExceptFirstValueAfterPipe().Length);

            const string nullValue = null;
            Assert.AreEqual(string.Empty, nullValue.GetFirstValueBeforePipe());
        }

        [TestMethod]
        public void MultipleClientIdsInRowLevelSecurity()
        {
            const string sql = "((((ClientId = 101)) OR ((ClientId = 102)) OR ((ClientId = 200))))";
            var clients = sql.GetListofPrivilegeClients();

            Assert.AreEqual(3, clients.Count());
            Assert.AreEqual("101", clients.ElementAt(0));
            Assert.AreEqual("102", clients.ElementAt(1));
            Assert.AreEqual("200", clients.ElementAt(2));
        }

        [TestMethod]
        public void SingleClientIdInRowLevelSecurity()
        {
            const string sql = "((((ClientId = 102))))";
            var clients = sql.GetListofPrivilegeClients();

            Assert.AreEqual(1, clients.Count());
            Assert.AreEqual("102", clients.ElementAt(0));
        }

        [TestMethod]
        public void DoesHaveAccessToAllClients()
        {
            const string sql = "((((1=1))))";
            Assert.IsTrue(sql.DoesHaveAccessToAllClients());
        }

        [TestMethod]
        public void DoesHaveAccessToAnyClient()
        {
            const string sql = "1=0";
            Assert.IsTrue(sql.DoesNotHaveAccessToAnyClient());
        }

        [TestMethod]
        public void GetLastWordFromSentence()
        {
            const string input = "Paycor.Import.Mapping.GlobalMapping";
            var result = input.GetLastWordFromSentence(".");
            Assert.AreEqual(result, "GlobalMapping");
        }

        [TestMethod]
        public void GetFormattedDateifStringIsDate()
        {
            const string input = "12-07-1989";
            var result = input.GetFormattedDateifStringIsDate();
            Assert.AreEqual(result, "12-07-1989");

            const string input1 = "12/09/2018";
            var result1 = input1.GetFormattedDateifStringIsDate();
            Assert.AreEqual(result1, "12-09-2018");
        }

        [TestMethod]
        public void GetPayCorAuthCookie()
        {
            var cookieList = new List<string>
            {
                "Tracker=cc641089-2626-499a-8601-ab4499cf3d14; path=/; HttpOnly",
                "paycorAuth=3B510B307E7CAC7801310031006600610037003700360032006400370066003300340062003200610038003000320065003200390064003100640037003800630065006100310037000000FF24968DC2B7D20100FFD0B989C9B7D201380064003800390033003600650031002D0062003500320033002D0065003700310031002D0062003500630034002D0030003000350030003500360062006400370038003600390000002F00000042FFE4BA406B652C42BB44404650F322608A6B9C; path =/; HttpOnly",
                "TrackerUser=APIKEY%5c11fa7762d7f34b2a802e29d1d78cea17; path=/; HttpOnly"
            };

            var cookie =  cookieList.GetPayCorAuthCookie();

            Assert.AreEqual(cookie, "3B510B307E7CAC7801310031006600610037003700360032006400370066003300340062003200610038003000320065003200390064003100640037003800630065006100310037000000FF24968DC2B7D20100FFD0B989C9B7D201380064003800390033003600650031002D0062003500320033002D0065003700310031002D0062003500630034002D0030003000350030003500360062006400370038003600390000002F00000042FFE4BA406B652C42BB44404650F322608A6B9C");
        }

        [TestMethod]
        public void ExistInTest()
        {
            var input1 = new List<string>
            {
                "clientId",
            };
            var input2 = new List<string>
            {
                "companyId",
                "ClientId"
            };

            var result = input1.ExistIn(input2);
            Assert.AreEqual(true, result);

            var input3 = new List<string>
            {
                "clientId"
            };
            var input4 = new List<string>();
            var result1 = input3.ExistIn(input4);
            Assert.AreEqual(false,result1);

        }
    }
}