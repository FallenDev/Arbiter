using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Arbiter.App.Services;
using Avalonia.Markup.Xaml;
using Arbiter.App.ViewModels;
using Arbiter.App.Views;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.App;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Create the main window and register it as top level
            var window = new MainWindow();
            collection.AddSingleton<Window>(window);
            collection.AddSingleton<TopLevel>(window);
            collection.AddSingleton(window.StorageProvider);

            // Register all services and view models with the DI container
            RegisterServices(collection);
            RegisterViewModels(collection);

            // Build the service provider and get the main view model
            var services = collection.BuildServiceProvider();
            var vm = services.GetRequiredService<MainWindowViewModel>();
            window.DataContext = vm;

            // Set it as the main window to display
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
    
    private static void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IDialogService, DialogService>();
        services.AddTransient<IGameClientService, GameClientService>();
        services.AddTransient<ISettingsService, SettingsService>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddTransient<MessageBoxViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<MainWindowViewModel>();
    }
}