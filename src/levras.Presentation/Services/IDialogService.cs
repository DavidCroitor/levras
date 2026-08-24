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
}