using System.ComponentModel;

namespace notory.Services;

public enum AppLanguage
{
    English,
    Turkish,
}

/// <summary>
/// The app's tiny translation table and current-language state.
///
/// UI elements bind to the string indexer (e.g. <c>[Placeholder]</c>) against
/// the shared <see cref="Instance"/>. When <see cref="Language"/> changes we
/// raise the special "Item[]" property change so every bound string re-reads
/// itself, giving a live language switch without rebuilding the UI. Non-WPF
/// consumers (the tray menu) can instead listen to <see cref="LanguageChanged"/>.
/// </summary>
public sealed class Localization : INotifyPropertyChanged
{
    public static Localization Instance { get; } = new();

    private AppLanguage _language = AppLanguage.English;

    private static readonly Dictionary<string, string> English = new()
    {
        ["TrayOpen"] = "Open note",
        ["TrayAutostart"] = "Start with Windows",
        ["TrayLanguage"] = "Language",
        ["TrayTheme"] = "Theme",
        ["ThemeSystem"] = "System",
        ["ThemeDark"] = "Dark",
        ["ThemeLight"] = "Light",
        ["TrayAbout"] = "About",
        ["TrayQuit"] = "Quit",
        // {0} is the new version, e.g. "Update to v1.1.0".
        ["TrayUpdate"] = "Update to v{0}",
        ["UpdateBalloonTitle"] = "notory update",
        ["UpdateBalloonText"] = "A new version is available. Click to install.",
        ["UpdateFailed"] = "Could not download the update. Please try again later.",
        ["UpToDate"] = "notory is up to date.",
        ["TrayCheckUpdate"] = "Check for updates",
        ["Placeholder"] = "Type a note… it saves automatically.",
        ["Clear"] = "Clear",
        ["Chars"] = "characters",
        ["AboutDescription"] = "A lightweight quick-note scratchpad.",
        ["AboutVersion"] = "Version",
        ["AboutClose"] = "Close",
    };

    private static readonly Dictionary<string, string> Turkish = new()
    {
        ["TrayOpen"] = "Notu aç",
        ["TrayAutostart"] = "Windows ile başlat",
        ["TrayLanguage"] = "Dil",
        ["TrayTheme"] = "Tema",
        ["ThemeSystem"] = "Sistem",
        ["ThemeDark"] = "Koyu",
        ["ThemeLight"] = "Açık",
        ["TrayAbout"] = "Hakkında",
        ["TrayQuit"] = "Çıkış",
        // {0} yeni sürüm numarası, örn. "v1.1.0 sürümüne güncelle".
        ["TrayUpdate"] = "v{0} sürümüne güncelle",
        ["UpdateBalloonTitle"] = "notory güncellemesi",
        ["UpdateBalloonText"] = "Yeni sürüm çıktı. Kurmak için tıklayın.",
        ["UpdateFailed"] = "Güncelleme indirilemedi. Lütfen daha sonra tekrar deneyin.",
        ["UpToDate"] = "notory güncel.",
        ["TrayCheckUpdate"] = "Güncellemeleri denetle",
        ["Placeholder"] = "Bir not yaz… otomatik kaydedilir.",
        ["Clear"] = "Temizle",
        ["Chars"] = "karakter",
        ["AboutDescription"] = "Hafif bir hızlı not defteri.",
        ["AboutVersion"] = "Sürüm",
        ["AboutClose"] = "Kapat",
    };

    /// <summary>The active language. Changing it refreshes all bound strings.</summary>
    public AppLanguage Language
    {
        get => _language;
        set
        {
            if (_language == value)
                return;

            _language = value;

            // "Item[]" tells WPF that every indexer binding should re-evaluate.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
            LanguageChanged?.Invoke();
        }
    }

    /// <summary>The translation for <paramref name="key"/> in the current language.</summary>
    public string this[string key]
    {
        get
        {
            var table = _language == AppLanguage.Turkish ? Turkish : English;
            return table.TryGetValue(key, out var value) ? value : key;
        }
    }

    /// <summary>Raised after the language changes (for non-binding consumers).</summary>
    public event Action? LanguageChanged;

    public event PropertyChangedEventHandler? PropertyChanged;
}
