using FlashSales.Domain.Results;

namespace FlashSales.Application.Abstractions
{
    public sealed class CommitResult
    {
        private CommitResult(
            bool isSuccess = true,
            PersistenceErrors errorType = PersistenceErrors.None,
            Error? error = null
            )
        {
            IsSuccess = isSuccess;
            ErrorType = errorType;
            Error = error;
        }

        public bool IsSuccess { get; }
        public PersistenceErrors ErrorType { get; }
        public Error? Error { get; }
        public bool IsFailure => !IsSuccess;

        public static CommitResult Success() => new();

        public static CommitResult ConcurrencyConflict() => new(
            false,
            PersistenceErrors.ConcurrencyConflict,
            Error.Conflict(
                "Persistence.ConcurrencyConflict",
                "The record was modified by another process. Please retry the operation."));

        public static CommitResult UniqueViolation() => new(
            false,
            PersistenceErrors.UniqueViolation,
            Error.Conflict(
                "Persistence.UniqueViolation",
                "A record with the same unique key already exists."));

        public static CommitResult ForeignKeyViolation() => new(
            false,
            PersistenceErrors.ForegeinKeyViolation,
            Error.Problem(
                "Persistence.ForeignKeyViolation",
                "The operation references a record that does not exist."));

        public static CommitResult ConnectionFailure() => new(
            false,
            PersistenceErrors.ConnectionFailure,
            Error.Problem(
                "Persistence.ConnectionFailure",
                "A database connection failure occurred. Please try again later."));
    }

    public enum PersistenceErrors
    {
        None,
        ConcurrencyConflict,
        UniqueViolation,
        ForegeinKeyViolation,
        ConnectionFailure
    }
}