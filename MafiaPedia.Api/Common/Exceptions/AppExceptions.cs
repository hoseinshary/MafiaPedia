namespace MafiaPedia.Api.Common.Exceptions
{
    public class NotFoundAppException : Exception
    {
        public NotFoundAppException(string message = "رکورد مورد نظر یافت نگردید.") : base(message) { }
    }

    public class ConflictAppException : Exception
    {
        public ConflictAppException(string message) : base(message) { }
    }

    public class ForbiddenAppException : Exception
    {
        public ForbiddenAppException(string message = "عدم دسترسی.") : base(message) { }
    }

    public class ValidationAppException : Exception
    {
        public ValidationAppException(string message) : base(message) { }
    }
}
