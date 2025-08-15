
using System;
using System.Linq;
using Arbiter.App.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;

namespace Arbiter.App.Controls;

public partial class RawHexDisplay : UserControl
{
    private static readonly Key[] NavigationKeys =
        [Key.Left, Key.Right, Key.Up, Key.Down, Key.Home, Key.End, Key.PageUp, Key.PageDown];
    
    public RawHexDisplay()
    {
        InitializeComponent();
        
        HexTextBox.KeyUp += HexTextBox_OnKeyUp;
        HexTextBox.PointerReleased += HexTextBox_OnPointerReleased;
    }

    private void HexTextBox_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (sender is TextBox textBox && NavigationKeys.Contains(e.Key))
        {
            SnapSelectionToNearestByte(textBox);
        }
    }
    
    private void HexTextBox_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left && sender is TextBox textBox)
        {
            SnapSelectionToNearestByte(textBox);
        }
    }

    private void SnapSelectionToNearestByte(TextBox textBox)
    {
        if (DataContext is not RawHexViewModel viewModel)
        {
            return;
        }

        var totalLength = viewModel.RawHex.Length;
        if (totalLength == 0)
        {
            HexTextBox.ClearSelection();
        }
        
        var selectionStart = textBox.SelectionStart;
        var selectionEnd = textBox.SelectionEnd;

        if (selectionEnd <= selectionStart)
        {
            (selectionStart, selectionEnd) = (selectionEnd, selectionStart);
        }
        
        var startByte = Math.Clamp(selectionStart / 3, 0, totalLength - 1);
        var endByte = Math.Clamp((selectionEnd - 1) / 3, 0, totalLength - 1);

        var newSelectionStart = startByte * 3;
        var newSelectionLength = (endByte - startByte) * 3 + 2;

        if (newSelectionLength <= 0)
        {
            newSelectionLength = 2;
        }

        if (textBox.SelectionStart != newSelectionStart ||
            textBox.SelectionEnd != newSelectionStart + newSelectionLength)
        {
            textBox.SelectionStart = newSelectionStart;
            textBox.SelectionEnd = newSelectionStart + newSelectionLength;
        }

        viewModel.HexSelectionStart = newSelectionStart;
        viewModel.HexSelectionEnd = newSelectionStart + newSelectionLength;
    }
}