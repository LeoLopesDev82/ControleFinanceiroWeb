using System.Net;

namespace ControleFinanceiroWeb.Responses
{
    public class SaveResult
    {
        public int? Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
