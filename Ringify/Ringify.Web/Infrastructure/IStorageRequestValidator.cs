namespace Ringify.Web.Infrastructure
{
    using System;
    using System.Web;

    public interface IStorageRequestValidator
    {
        bool DoesRequestApply(Uri resourceUri);

        bool ValidateRequest(string userId, HttpRequest request);
    }
}
