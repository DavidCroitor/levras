using System.Threading;
using System.Threading.Tasks;
using levras.Core.Abstractions;
using levras.Presentation.ViewModels;

namespace levras.Presentation.Services;

public interface ITabFactory
{
    Task<TabViewModelBase> CreateTabAsync(WorkspaceItemViewModel node, CancellationToken cancellationToken = default);
}
