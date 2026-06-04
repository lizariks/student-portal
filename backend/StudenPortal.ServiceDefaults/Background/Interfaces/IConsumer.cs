namespace StudentPortal.ServiceDefaults.Background.Interfaces;

using System.Threading;
using System.Threading.Tasks;

public interface IConsumer<TEvent> where TEvent : class
{
    Task Consume(TEvent eventMessage, CancellationToken cancellationToken);
}