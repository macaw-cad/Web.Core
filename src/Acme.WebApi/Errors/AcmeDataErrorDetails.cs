using System;
using Web.Core.Mvc;

namespace Acme.WebApi.Errors
{
    public class AcmeDataErrorDetails : IErrorDetails
    {
        public int IntValue { get; set; }
        public decimal DecimalValue { get; set; }
        public string StringValue { get; set; }
        public DateTime DateValue { get; set; }
        public bool BooleanValue { get; set; }
    }
}
