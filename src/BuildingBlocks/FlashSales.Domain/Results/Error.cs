using System.Text.Json.Serialization;

namespace FlashSales.Domain.Results
{
    public class Error
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Problem);

        public static readonly Error NullValue = new(
            "General.Null",
            "Null value was provided",
            ErrorType.Problem);

        public Error(string code, string description, ErrorType type)
        {
            Code = code;
            Description = description;
            Type = type;
        }

        public string Code { get; }

        public string Description { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ErrorType Type { get; }

        public static Error NotFound(string code, string description) =>
            new(code, description, ErrorType.NotFound);

        public static Error Problem(string code, string description) =>
            new(code, description, ErrorType.Problem);

        public static Error Conflict(string code, string description) =>
            new(code, description, ErrorType.Conflict);

        public static Error Invalid(string code, string description) =>
            new(code, description, ErrorType.Validation);
    }

    public enum ErrorType
    {
        Validation,
        Problem,
        NotFound,
        Conflict
    }
}