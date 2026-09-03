namespace levras.Core.Domain;

public sealed record WorkspaceItem(
    string Name,
    string FullPath,
    bool IsDirectory,
    IReadOnlyList<WorkspaceItem> Children
);