using System.Windows;
using System.Windows.Input;
using notory.Services;

// Disambiguate from System.Windows.Localization (pulled in via System.Windows).
using Localization = notory.Services.Localization;
using TextChangedEventArgs = System.Windows.Controls.TextChangedEventArgs;

namespace notory;

/// <summary>
/// The quick-note window: a single auto-saving scratchpad. Typing persists to the
/// <see cref="NoteStore"/> immediately. Settings (language, theme, start with
/// Windows, about) live in the tray menu. Closing hides it to the tray.
/// </summary>
public partial class NoteWindow : Window
{
    private readonly NoteStore _store;

    public NoteWindow(NoteStore store)
    {
        InitializeComponent();

        _store = store;
        NoteBox.Text = store.Load();
        UpdateCount();

        // The character count carries a localized "characters" suffix, so refresh
        // it when the language changes (from the tray) even without a keystroke.
        Localization.Instance.LanguageChanged += UpdateCount;
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

    // The window is borderless (no native title bar), so dragging the custom
    // header moves it.
    private void OnHeaderDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    // The custom close button hides the note to the tray, like pressing X did.
    private void OnCloseClick(object sender, RoutedEventArgs e) => Hide();

    private void UpdateCount()
        => CountText.Text = $"{NoteBox.Text.Length} {Localization.Instance["Chars"]}";

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Closing (X) hides the note to the tray; the app keeps running and is
        // shut down from the tray's Quit command. The text is already saved.
        e.Cancel = true;
        Hide();

        base.OnClosing(e);
    }
}
