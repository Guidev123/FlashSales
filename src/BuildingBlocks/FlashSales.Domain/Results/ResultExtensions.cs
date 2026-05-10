namespace FlashSales.Domain.Results
{
    public static class ResultExtensions
    {
        public static TOut Match<TOut>(
            this Result result,
            Func<TOut> onSuccess,
            Func<Result, TOut> onFailure)
        {
            return result.IsSuccess ? onSuccess() : onFailure(result);
        }

        public static TOut Match<TIn, TOut>(
            this Result<TIn> result,
            Func<TIn, TOut> onSuccess,
            Func<Result<TIn>, TOut> onFailure)
        {
            return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
        }

        public static Result<TOut> Bind<TIn, TOut>(
            this Result<TIn> result,
            Func<TIn, Result<TOut>> func)
            => result.IsFailure
                ? Result.Failure<TOut>(result.Error!)
                : func(result.Value);

        public static Result Bind<TIn>(
            this Result<TIn> result,
            Func<TIn, Result> func)
            => result.IsFailure
                ? Result.Failure(result.Error!)
                : func(result.Value);

        public static Result Bind(
            this Result result,
            Func<Result> func)
            => result.IsFailure
                ? result
                : func();

        public static Result<TOut> Map<TIn, TOut>(
            this Result<TIn> result,
            Func<TIn, TOut> func)
            => result.IsFailure
                ? Result.Failure<TOut>(result.Error!)
                : Result.Success(func(result.Value));

        public static Result<T> Tap<T>(
            this Result<T> result,
            Action<T> action)
        {
            if (result.IsSuccess)
                action(result.Value);

            return result;
        }

        public static Result Tap(
            this Result result,
            Action action)
        {
            if (result.IsSuccess)
                action();

            return result;
        }

        public static Result<T> Ensure<T>(
            this Result<T> result,
            Func<T, bool> predicate,
            Error error)
            => result.IsSuccess && !predicate(result.Value)
                ? Result.Failure<T>(error)
                : result;

        public static TOut Finally<TIn, TOut>(
            this Result<TIn> result,
            Func<Result<TIn>, TOut> func)
            => func(result);
    }

    public static class ResultAsyncExtensions
    {
        public static async Task<Result<TOut>> Bind<TIn, TOut>(
            this Result<TIn> result,
            Func<TIn, Task<Result<TOut>>> func)
            => result.IsFailure
                ? Result.Failure<TOut>(result.Error!)
                : await func(result.Value);

        public static async Task<Result> Bind<TIn>(
            this Result<TIn> result,
            Func<TIn, Task<Result>> func)
            => result.IsFailure
                ? Result.Failure(result.Error!)
                : await func(result.Value);

        public static async Task<Result> Bind(
            this Result result,
            Func<Task<Result>> func)
            => result.IsFailure
                ? result
                : await func();

        public static async Task<Result<TOut>> Bind<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<TIn, Result<TOut>> func)
        {
            var result = await resultTask;
            return result.IsFailure
                ? Result.Failure<TOut>(result.Error!)
                : func(result.Value);
        }

        public static async Task<Result> Bind<TIn>(
            this Task<Result<TIn>> resultTask,
            Func<TIn, Result> func)
        {
            var result = await resultTask;
            return result.IsFailure
                ? Result.Failure(result.Error!)
                : func(result.Value);
        }

        public static async Task<Result> Bind(
            this Task<Result> resultTask,
            Func<Result> func)
        {
            var result = await resultTask;
            return result.IsFailure ? result : func();
        }

        public static async Task<Result<TOut>> Bind<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<TIn, Task<Result<TOut>>> func)
        {
            var result = await resultTask;
            return result.IsFailure
                ? Result.Failure<TOut>(result.Error!)
                : await func(result.Value);
        }

        public static async Task<Result> Bind(
            this Task<Result> resultTask,
            Func<Task<Result>> func)
        {
            var result = await resultTask;
            return result.IsFailure ? result : await func();
        }

        public static async Task<Result<TOut>> Map<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<TIn, TOut> func)
        {
            var result = await resultTask;
            return result.IsFailure
                ? Result.Failure<TOut>(result.Error!)
                : Result.Success(func(result.Value));
        }

        public static async Task<Result<TOut>> Map<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<TIn, Task<TOut>> func)
        {
            var result = await resultTask;
            return result.IsFailure
                ? Result.Failure<TOut>(result.Error!)
                : Result.Success(await func(result.Value));
        }

        public static async Task<Result<T>> Tap<T>(
            this Result<T> result,
            Func<T, Task> func)
        {
            if (result.IsSuccess)
                await func(result.Value);

            return result;
        }

        public static async Task<Result> Tap(
            this Result result,
            Func<Task> func)
        {
            if (result.IsSuccess)
                await func();

            return result;
        }

        public static async Task<Result<T>> Tap<T>(
            this Task<Result<T>> resultTask,
            Action<T> action)
        {
            var result = await resultTask;
            if (result.IsSuccess)
                action(result.Value);

            return result;
        }

        public static async Task<Result<T>> Tap<T>(
            this Task<Result<T>> resultTask,
            Func<T, Task> func)
        {
            var result = await resultTask;
            if (result.IsSuccess)
                await func(result.Value);

            return result;
        }

        public static async Task<Result> Tap(
            this Task<Result> resultTask,
            Func<Task> func)
        {
            var result = await resultTask;
            if (result.IsSuccess)
                await func();

            return result;
        }

        public static async Task<Result<T>> Ensure<T>(
            this Task<Result<T>> resultTask,
            Func<T, bool> predicate,
            Error error)
        {
            var result = await resultTask;
            return result.IsSuccess && !predicate(result.Value)
                ? Result.Failure<T>(error)
                : result;
        }

        public static async Task<Result<T>> Ensure<T>(
            this Task<Result<T>> resultTask,
            Func<T, Task<bool>> predicate,
            Error error)
        {
            var result = await resultTask;
            if (result.IsFailure) return result;

            return !await predicate(result.Value)
                ? Result.Failure<T>(error)
                : result;
        }

        public static async Task<TOut> Finally<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<Result<TIn>, TOut> func)
        {
            var result = await resultTask;
            return func(result);
        }

        public static async Task<TOut> Finally<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<Result<TIn>, Task<TOut>> func)
        {
            var result = await resultTask;
            return await func(result);
        }
    }
}