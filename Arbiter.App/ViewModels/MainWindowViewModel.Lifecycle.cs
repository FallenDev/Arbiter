﻿using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class MainWindowViewModel
{
    internal async Task OnOpened()
    {
        Settings = await _settingsService.LoadFromFileAsync();
        LaunchClientCommand.NotifyCanExecuteChanged();

        ApplySettings();
        
        RestoreWindowPosition();
        RestoreLayout();
    }

    internal async Task OnLoaded()
    {
        await StartProxyAsync();

        if (Settings.TraceOnStartup)
        {
            Trace.StartTracing();
        }
    }

    internal async Task<bool> OnClosing(WindowCloseReason reason)
    {
        if (Trace.IsRunning)
        {
            Trace.StopTracing();
        }

        // Autosave live traces on exit
        if (Settings.TraceAutosave && Trace is { IsLive: true, IsEmpty: false })
        {
            try
            {
                await Trace.AutoSaveTraceAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to autosave trace: {Message}", ex.Message);
            }
        }

        // Update entity sort oder to persist
        Settings.EntitySorting = EntityManager.SortOrder;

        SaveWindowPosition();
        SaveLayout();

        await _settingsService.SaveToFileAsync(Settings);
        return true;
    }
}