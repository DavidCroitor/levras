using System.Threading.Tasks;

namespace levras.Presentation.Services;

public interface IFolderPickerService
{
    Task<string?> PickFolderAsync(); 
}