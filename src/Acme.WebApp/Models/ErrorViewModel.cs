using System;
using System.ComponentModel.DataAnnotations;

namespace Acme.WebApp.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class MyData
    {
        [Required]
        [Range(33,60)]
        public int? MyProperty1 { get; set; }

        public string MyProperty2 { get; set; }
    }
}
