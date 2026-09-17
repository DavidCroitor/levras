using System.Threading.Tasks;

namespace levras.Presentation.Services;

public enum SaveChangesChoice
{
    Save,
    Discard,
    Cancel
};

public interface IDialogService
{
    Task<SaveChangesChoice> ConfirmSaveChangesAsync(string fileName);
    Task<bool> ConfirmAsync(string title, string message);
    Task ShowErrorAsync(string message);
    Task <string?> PromptNameAsync(string title, string message, string defaultValue="New Folder");
    
}