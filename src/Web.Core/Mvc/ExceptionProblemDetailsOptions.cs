namespace Web.Core.Mvc
{
    public class ExceptionProblemDetailsOptions
    {
        public const string ExceptionProblemDetails = "ExceptionProblemDetails";

        public DetailLevel Details { get; set; }
        public int Depth { get; set; }
    }
}
