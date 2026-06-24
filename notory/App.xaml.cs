using System.Windows;
using System.Windows.Threading;
using notory.Services;

// Enabling WinForms (for the tray icon) pulls the System.Windows.Forms version
// of Application into scope too, so spell out the WPF one; also disambiguate from
// System.Windows.Localization.
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Localization = notory.Services.Localization;

namespace notory;

/// <summary>
/// Application entry point. Runs notory as a tray application: no window on
/// startup, lives in the system tray, exits only on "Quit".
///
/// The core flow: press Ctrl+Shift+N → a small note pops up (press again to hide
/// it). Whatever you type is saved automatically and restored next time.
/// </summary>
public partial class App : Application
{
    private Mutex? _singleInstanceMutex;
    private SettingsStore _settings = null!;
    private NoteStore _noteStore = null!;
    private HotkeyService _hotkey = null!;
    private TrayIcon _tray = null!;
    private NoteWindow _note = null!;
    private AboutWindow? _aboutWindow;

    private UpdateService _updates = null!;
    // Periodically re-checks for updates so a long-running instance still notices.
    private DispatcherTimer? _updateTimer;
    // The newer release found by the background check, awaiting the user's nod.
    private UpdateService.AvailableUpdate? _pendingUpdate;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Only one notory should own the global hotkey at a time.
        _singleInstanceMutex = new Mutex(initiallyOwned: true,
            @"Local\notory.SingleInstance", out var isFirstInstance);
        if (!isFirstInstance)
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Apply saved language + theme before any UI is built, then persist changes.
        _settings = new SettingsStore();
        Localization.Instance.Language = _settings.LoadLanguage();
        ThemeService.Apply(_settings.LoadTheme());
        Localization.Instance.LanguageChanged += SavePreferences;
        ThemeService.Changed += SavePreferences;

        // The note window is created once (loading the saved note) and reused; it
        // hides instead of closing.
        _noteStore = new NoteStore();
        _note = new NoteWindow(_noteStore);
        _note.AboutRequested += ShowAbout;

        // Ctrl+Shift+N toggles the note.
        _hotkey = new HotkeyService();
        _hotkey.Pressed += ToggleNote;

        _tray = new TrayIcon();
        _tray.OpenRequested += ShowNote;
        _tray.AboutRequested += ShowAbout;
        _tray.UpdateRequested += InstallPendingUpdate;
        _tray.CheckUpdateRequested += () => _ = CheckForUpdateAsync(announceWhenCurrent: true);
        _tray.QuitRequested += Shutdown;

        // Quietly ask GitHub whether a newer notory exists; if so the tray will
        // offer it. Fire-and-forget so a slow network never delays startup.
        _updates = new UpdateService();
        _ = CheckForUpdateAsync(announceWhenCurrent: false);

        // Re-check every few hours so an instance left running for days still
        // notices a new release without needing a restart.
        _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromHours(6) };
        _updateTimer.Tick += (_, _) => _ = CheckForUpdateAsync(announceWhenCurrent: false);
        _updateTimer.Start();

        if (e.Args.Contains("--open"))
            ShowNote();
    }

    /// <summary>
    /// Background check for a newer release. The await resumes on the UI thread,
    /// so touching the tray here is safe. Silent on failure by design.
    /// </summary>
    private async Task CheckForUpdateAsync(bool announceWhenCurrent)
    {
        _pendingUpdate = await _updates.CheckForUpdateAsync();
        if (_pendingUpdate is not null)
            _tray.ShowUpdateAvailable(_pendingUpdate.Version.ToString(3));
        else if (announceWhenCurrent)
            _tray.ShowUpToDate();   // give feedback only for a manual check
    }

    /// <summary>
    /// Downloads and launches the installer for the pending update, then quits so
    /// it can replace notory's files. Tells the user if the download fails.
    /// </summary>
    private async void InstallPendingUpdate()
    {
        if (_pendingUpdate is null)
            return;

        try
        {
            await _updates.DownloadAndLaunchInstallerAsync(_pendingUpdate);
            Shutdown();
        }
        catch
        {
            MessageBox.Show(Localization.Instance["UpdateFailed"], "notory",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // Hotkey: show the note if it's hidden, hide it if it's already up.
    private void ToggleNote()
    {
        if (_note.IsVisible)
            _note.Hide();
        else
            ShowNote();
    }

    private void ShowNote()
    {
        if (!_note.IsVisible)
            _note.Show();
        if (_note.WindowState == WindowState.Minimized)
            _note.WindowState = WindowState.Normal;
        _note.Activate();
        _note.FocusNote();
    }

    private void SavePreferences()
        => _settings.Save(Localization.Instance.Language, ThemeService.Theme);

    /// <summary>Shows the About window, reusing it if already open.</summary>
    private void ShowAbout()
    {
        if (_aboutWindow is not null)
        {
            _aboutWindow.Activate();
            return;
        }

        _aboutWindow = new AboutWindow();
        _aboutWindow.Closed += (_, _) => _aboutWindow = null;
        _aboutWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _hotkey?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
