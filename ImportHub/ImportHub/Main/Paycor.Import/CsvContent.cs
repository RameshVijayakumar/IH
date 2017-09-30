using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LINQtoCSV;
// TODO: No unit tests
namespace Paycor.Import
{
    public class CsvContent<T> : HttpContent
    {
        private readonly Stream _stream = new MemoryStream();

        public CsvContent(CsvFileDescription outputFileDescription, string filename, IEnumerable<T> data)
        {
            var cc = new CsvContext();
            var writer = new StreamWriter(_stream);
            cc.Write(data, writer, outputFileDescription);
            writer.Flush();
            _stream.Position = 0;

            Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = filename
            };
        } 

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
             return _stream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _stream.Length;
            return true;
        }
    }
}
