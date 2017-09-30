using System;
using System.Configuration;
using System.Drawing;
using System.Text;

namespace Paycor.Import.Service.Shared
{
    public class ReportUrlGenerator
    {
        private const string Pdf = "PDF";
        private const string OutputType = "OutputType";
        private const string ClientId = "ClientID";
        private const string AccentColor = "AccentColor";
        private const string Environment = "Environment";

        public string GetReportUrl(string clientId, string importReportName)
        {
            var urlCore = ConfigurationManager.AppSettings["ParameterizedReportUrlCore"];
            var environment = ConfigurationManager.AppSettings["Environment"];
            var accentColor = Color.Black;
            const string outputType = Pdf;

            return GenerateImportReportUrl(clientId, outputType, accentColor, urlCore, environment, importReportName);
        }

        private string GenerateImportReportUrl(
            string clientId, string outputType, 
            Color accentColor, string urlCore, 
            string environment, string importReportName)
        {
            const string symbolQuestion = "?";
            const string symbolAmpersand = "&";
            var baseUrl = ConfigurationManager.AppSettings["DocumentsAddress"];
            var rgbIntAccentColor = BitConverter.ToInt32(new byte[] { accentColor.B, accentColor.G, accentColor.R, 0x00 }, 0);

            var baseUri = new StringBuilder();
            baseUri.Append(baseUrl);
            baseUri.Append(urlCore);
            baseUri.Append(importReportName);
            if (clientId.Trim().Equals("0"))
            {
                baseUri.AppendFormat("{0}{1}={2}", symbolQuestion, OutputType, outputType);
            }
            else
            {
                baseUri.AppendFormat("{0}{1}={2}", symbolQuestion, ClientId, clientId);
                baseUri.AppendFormat("{0}{1}={2}", symbolAmpersand, OutputType, outputType);
            }
            
            baseUri.AppendFormat("{0}{1}={2}", symbolAmpersand, AccentColor, rgbIntAccentColor);
            baseUri.AppendFormat("{0}{1}={2}", symbolAmpersand, Environment, environment);

            return baseUri.ToString();
        }
    }
}