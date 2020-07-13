using Web.Core.Mvc;

namespace Acme.WebApi.Errors
{
    public class AcmeErrorDetails : IErrorDetails
    {
        public int IntValue { get; set; }
        public string StringValue { get; set; }
    }
}
