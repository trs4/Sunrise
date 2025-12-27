using System.Threading.Tasks;
using Sunrise.Model.Communication;

namespace Sunrise.Services;

public interface IAppSyncService
{
    void Start(SyncDispatcher dispatcher);
    ValueTask ShutdownAsync();
}
