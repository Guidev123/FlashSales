using FlashSales.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Launches.Infrastructure.Database.Repositories;
using System.Reflection;

namespace Modules.Launches.Infrastructure.Inbox
{
    internal sealed class LaunchesInboxRepositoryRegistration : IInboxRepositoryRegistration
    {
        private static readonly Assembly _handlerAssembly = Assembly.GetExecutingAssembly();
        private static readonly Type[] _handlerTypes = _handlerAssembly.GetTypes();

        public bool Matches(Type commandType)
        {
            var handlerInterface = typeof(INotificationHandler<>).MakeGenericType(commandType);
            return _handlerTypes.Any(t => !t.IsAbstract && handlerInterface.IsAssignableFrom(t));
        }

        public IInboxRepository Resolve(IServiceProvider sp)
            => sp.GetRequiredService<InboxRepository>();
    }
}
