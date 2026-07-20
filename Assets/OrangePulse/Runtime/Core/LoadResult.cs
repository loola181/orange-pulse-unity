namespace OrangePulse.Core
{
    public sealed class LoadResult<T>
    {
        public T Data { get; }
        public bool FromCache { get; }
        public string Error { get; }
        public bool IsSuccess => string.IsNullOrEmpty(Error);

        private LoadResult(T data, bool fromCache, string error)
        {
            Data = data;
            FromCache = fromCache;
            Error = error;
        }

        public static LoadResult<T> Fresh(T data) => new(data, false, null);
        public static LoadResult<T> Cached(T data) => new(data, true, null);
        public static LoadResult<T> Failed(string error) => new(default, false, error);
    }
}

