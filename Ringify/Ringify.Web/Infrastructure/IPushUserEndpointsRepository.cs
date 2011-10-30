namespace Ringify.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ringify.Web.Models;

    [CLSCompliant(false)]
    public interface IPushUserEndpointsRepository
    {
        IQueryable<PushUserEndpoint> PushUserEndpoints { get; }

        void AddPushUserEndpoint(string userId, Uri channelUri);

        void UpdatePushUserEndpoint(PushUserEndpoint pushUserEndpoint);

        void RemovePushUserEndpoint(string userId, Uri channelUri);

        IEnumerable<string> GetAllPushUsers();

        IEnumerable<PushUserEndpoint> GetPushUsersByName(string userId);

        IEnumerable<PushUserEndpoint> GetPushUsersByNameAndEndpoint(string userId, Uri channelUri);
    }
}
