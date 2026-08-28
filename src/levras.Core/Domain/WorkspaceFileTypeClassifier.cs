namespace levras.Core.Domain;

public static class WorkspaceFileTypeClassifier
{
     private static readonly HashSet<string> MarkdownExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".md", ".markdown" };

    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };

    public static WorkspaceFileType Classify(WorkspaceItem item)
    {
        if(item.IsDirectory) return WorkspaceFileType.Directory;

        var ext = Path.GetFullPath(item.FullPath);
        if(MarkdownExtensions.Contains(ext)) return WorkspaceFileType.Markdown;
        if(ImageExtensions.Contains(ext)) return WorkspaceFileType.Image;
        return WorkspaceFileType.Unsupported;
    }
}