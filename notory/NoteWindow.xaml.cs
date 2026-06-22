using System.Windows;
using notory.Services;

// Disambiguate from System.Windows.Localization (pulled in via System.Windows).
using Localization = notory.Services.Localization;
using TextChangedEventArgs = System.Windows.Controls.TextChangedEventArgs;

namespace notory;

/// <summary>
/// The quick-note window: a single auto-saving scratchpad. Typing persists to the
/// <see cref="NoteStore"/> immediately; a menu mirrors the tray settings
/// (language, theme, start with Windows, about). Closing hides it to the tray.
/// </summary>
public partial class NoteWindow : Window
{
    private readonly NoteStore _store;

    /// <summary>Raised when the user picks About from the menu.</summary>
    public event Action? AboutRequested;

    public NoteWindow(NoteStore store)
    {
        InitializeComponent();

        _store = store;
        NoteBox.Text = store.Load();
        UpdateCount();

        RefreshMenuChecks();
        Activated += (_, _) => RefreshMenuChecks();
    }

    /// <summary>Puts the caret in the note, ready to type.</summary>
    public void FocusNote()
    {
        NoteBox.Focus();
        NoteBox.CaretIndex = NoteBox.Text.Length;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _store.Save(NoteBox.Text);
        UpdateCount();
    }

    private void OnClear(object sender, RoutedEventArgs e)
    {
        NoteBox.Clear();
        NoteBox.Focus();
    }

    private void UpdateCount()
        => CountText.Text = $"{NoteBox.Text.Length} {Localization.Instance["Chars"]}";

    private void OnEnglish(object sender, RoutedEventArgs e)
    {
        Localization.Instance.Language = AppLanguage.English;
        RefreshMenuChecks();
        UpdateCount();
    }

    private void OnTurkish(object sender, RoutedEventArgs e)
    {
        Localization.Instance.Language = AppLanguage.Turkish;
        RefreshMenuChecks();
        UpdateCount();
    }

    private void OnThemeSystem(object sender, RoutedEventArgs e) => SetTheme(AppTheme.System);
    private void OnThemeDark(object sender, RoutedEventArgs e) => SetTheme(AppTheme.Dark);
    private void OnThemeLight(object sender, RoutedEventArgs e) => SetTheme(AppTheme.Light);

    private void SetTheme(AppTheme theme)
    {
        ThemeService.Apply(theme);
        RefreshMenuChecks();
    }

    private void OnToggleAutoStart(object sender, RoutedEventArgs e)
        => AutoStart.SetEnabled(AutoStartMenuItem.IsChecked);

    private void OnAbout(object sender, RoutedEventArgs e) => AboutRequested?.Invoke();

    private void RefreshMenuChecks()
    {
        EnglishMenuItem.IsChecked = Localization.Instance.Language == AppLanguage.English;
        TurkishMenuItem.IsChecked = Localization.Instance.Language == AppLanguage.Turkish;
        AutoStartMenuItem.IsChecked = AutoStart.IsEnabled();
        ThemeSystemItem.IsChecked = ThemeService.Theme == AppTheme.System;
        ThemeDarkItem.IsChecked = ThemeService.Theme == AppTheme.Dark;
        ThemeLightItem.IsChecked = ThemeService.Theme == AppTheme.Light;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Closing (X) hides the note to the tray; the app keeps running and is
        // shut down from the tray's Quit command. The text is already saved.
        e.Cancel = true;
        Hide();

        base.OnClosing(e);
    }
}
