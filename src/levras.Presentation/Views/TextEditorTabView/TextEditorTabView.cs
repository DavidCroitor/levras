using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;

namespace levras.Presentation.Views;

public partial class TextEditorTabView : UserControl
{
    public TextEditorTabView()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) =>
        {
            var editor = this.FindControl<TextEditor>("MainEditor");

            if (Application.Current?.TryGetResource("EditorSelectionBrush", out var resource) == true &&
                resource is IBrush selectionBrush)
            {
                editor.TextArea.SelectionBrush = selectionBrush;
            }
        };
    
    }
    
}