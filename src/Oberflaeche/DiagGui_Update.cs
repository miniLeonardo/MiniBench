// Update-Prüfung und Austausch der LMB.exe (Teilklasse DiagGui)
public partial class DiagGui
{
    void ApplyUpdateBadge(string newVer)
    {
        if (badgeUpdate != null)
        {
            badgeUpdate.Text = "Update verfügbar (" + newVer + ")";
            badgeUpdate.Visible = true;
        }
    }

    // Update-Prüfung (ab v4.06 überarbeitet). Bis v4.05: Ergebnis 24 Stunden im Cache, auch für "Jetzt suchen", nur die
    // GitHub-API mit 3 s Frist und jeder Fehler still. Eine Prüfung kurz vor einem Release meldete so einen Tag lang
    // "kein Update" (09.11.2026: 4.04 prüfte um 15:13, 4.05 erschien um 16:11). Die API erlaubt ohne Anmeldung 60 Abfragen
    // je Stunde und Adresse; hinter dem Netz einer Hochschule ist das schnell erschöpft (403).
    // Jetzt: Cache höchstens 2 Stunden und nur beim automatischen Start, "Jetzt suchen" fragt immer neu und meldet das
    // Ergebnis. Scheitert die API, liest die Prüfung die Weiterleitung von github.com/.../releases/latest (zählt nicht zur
    // API-Grenze) und die Prüfsumme aus dem Anhang LMB.exe.sha256. Prüfsumme sonst aus dem Text des Releases oder dem
    // digest des Anhangs. Systemproxy mit den Anmeldedaten des Benutzers.
    public const string UpdateRepo = "miniLeonardo/MiniBench";
    public const double UpdateCacheStunden = 2;

    public class UpdateInfo
    {
        public string Tag = "", Url = "", Sha256 = "", Body = "", Fehler = "", Quelle = "";
    }

    public static string ShaAusText(string text)
    {
        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(text ?? "", @"(?:SHA-?256(?:\s*\(LMB\.exe\))?[:\s]+)([a-fA-F0-9]{64})");
        if (m.Success) return m.Groups[1].Value.ToLowerInvariant();
        m = System.Text.RegularExpressions.Regex.Match((text ?? "").Trim(), @"^([a-fA-F0-9]{64})\b");
        return m.Success ? m.Groups[1].Value.ToLowerInvariant() : "";
    }

    // "https://github.com/x/y/releases/tag/v4.05" -> "v4.05"
    public static string TagAusAdresse(string url)
    {
        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(url ?? "", @"/releases/tag/([^/?#]+)");
        return m.Success ? Uri.UnescapeDataString(m.Groups[1].Value) : "";
    }

    public static string DownloadAdresse(string tag) { return "https://github.com/" + UpdateRepo + "/releases/download/" + tag + "/LMB.exe"; }

    // Antwort von /releases/latest der API lesen: Tag, Text, Adresse und Prüfsumme von LMB.exe (Text, sonst digest)
    public static UpdateInfo LiesReleaseJson(string json)
    {
        UpdateInfo u = new UpdateInfo(); u.Quelle = "GitHub-API";
        System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
        Dictionary<string, object> d = js.DeserializeObject(json) as Dictionary<string, object>;
        if (d == null) { u.Fehler = "Antwort der GitHub-API nicht lesbar."; return u; }
        object o;
        if (d.TryGetValue("tag_name", out o) && o != null) u.Tag = o.ToString().Trim();
        if (d.TryGetValue("body", out o) && o != null) u.Body = o.ToString();
        string digest = "";
        if (d.TryGetValue("assets", out o) && o is object[])
            foreach (object a in (object[])o)
            {
                Dictionary<string, object> ad = a as Dictionary<string, object>;
                if (ad == null || !ad.ContainsKey("name") || ad["name"] == null) continue;
                if (!ad["name"].ToString().Equals("LMB.exe", StringComparison.OrdinalIgnoreCase)) continue;
                if (ad.ContainsKey("browser_download_url") && ad["browser_download_url"] != null) u.Url = ad["browser_download_url"].ToString();
                if (ad.ContainsKey("digest") && ad["digest"] != null) digest = ad["digest"].ToString();
            }
        u.Sha256 = ShaAusText(u.Body);
        if (u.Sha256.Length == 0 && digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)) u.Sha256 = ShaAusText(digest.Substring(7));
        if (u.Tag.Length == 0) u.Fehler = "Die GitHub-API nennt kein Release.";
        return u;
    }

    static void NetzVorbereiten()
    {
        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072; // Tls12
        try { if (System.Net.WebRequest.DefaultWebProxy != null) System.Net.WebRequest.DefaultWebProxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials; } catch { }
    }

    static System.Net.HttpWebRequest NeueAnfrage(string url, int fristMs)
    {
        System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
        req.Timeout = fristMs; req.ReadWriteTimeout = fristMs; req.UserAgent = "LeosMinibench"; req.Method = "GET";
        return req;
    }

    static string FehlerText(Exception ex)
    {
        System.Net.WebException we = ex as System.Net.WebException;
        if (we != null && we.Response is System.Net.HttpWebResponse)
        {
            int c = (int)((System.Net.HttpWebResponse)we.Response).StatusCode;
            if (c == 403 || c == 429) return "GitHub hat die Abfrage abgelehnt (" + c + ", Grenze von 60 Abfragen je Stunde und Netz erreicht)";
            return "GitHub antwortet mit " + c;
        }
        if (we != null && we.Status == System.Net.WebExceptionStatus.Timeout) return "keine Antwort von GitHub innerhalb der Frist";
        return ex.Message;
    }

    // Neuestes Release abfragen: erst die API, sonst die Weiterleitung von releases/latest und LMB.exe.sha256
    static UpdateInfo FrageGitHub()
    {
        NetzVorbereiten();
        string apiFehler = "";
        try
        {
            System.Net.HttpWebRequest req = NeueAnfrage("https://api.github.com/repos/" + UpdateRepo + "/releases/latest", 10000);
            req.Accept = "application/vnd.github+json";
            using (System.Net.HttpWebResponse resp = (System.Net.HttpWebResponse)req.GetResponse())
            using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
            {
                UpdateInfo u = LiesReleaseJson(sr.ReadToEnd());
                if (u.Fehler.Length == 0)
                {
                    if (u.Url.Length == 0) u.Url = DownloadAdresse(u.Tag);
                    if (u.Sha256.Length == 0) u.Sha256 = LiesShaAnhang(u.Tag);
                    return u;
                }
                apiFehler = u.Fehler;
            }
        }
        catch (Exception ex) { apiFehler = FehlerText(ex); }

        UpdateInfo w = new UpdateInfo(); w.Quelle = "github.com (Weiterleitung)";
        try
        {
            System.Net.HttpWebRequest req = NeueAnfrage("https://github.com/" + UpdateRepo + "/releases/latest", 10000);
            req.AllowAutoRedirect = false;
            using (System.Net.HttpWebResponse resp = (System.Net.HttpWebResponse)req.GetResponse())
            {
                w.Tag = TagAusAdresse(resp.Headers["Location"] ?? "");
                if (w.Tag.Length == 0) w.Tag = TagAusAdresse(resp.ResponseUri != null ? resp.ResponseUri.ToString() : "");
            }
            if (w.Tag.Length == 0) { w.Fehler = "API: " + apiFehler + "; github.com nennt kein Release."; return w; }
            w.Url = DownloadAdresse(w.Tag);
            w.Sha256 = LiesShaAnhang(w.Tag);
        }
        catch (Exception ex) { w.Fehler = "API: " + apiFehler + "; github.com: " + FehlerText(ex); }
        return w;
    }

    // Prüfsumme aus dem Anhang LMB.exe.sha256 (legt Veroeffentlichen.ps1 ab v4.06 an); leer, wenn es ihn nicht gibt
    static string LiesShaAnhang(string tag)
    {
        try
        {
            System.Net.HttpWebRequest req = NeueAnfrage("https://github.com/" + UpdateRepo + "/releases/download/" + tag + "/LMB.exe.sha256", 10000);
            using (System.Net.HttpWebResponse resp = (System.Net.HttpWebResponse)req.GetResponse())
            using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                return ShaAusText(sr.ReadToEnd());
        }
        catch { return ""; }
    }

    void StartUpdateCheck() { StartUpdateCheck(false); }

    bool updateCheckLaeuft;
    void StartUpdateCheck(bool manuell)
    {
        if (!manuell && !LoadUpdateCheckPreference()) return;
        if (updateCheckLaeuft) return;
        updateCheckLaeuft = true;
        System.Threading.Thread t = new System.Threading.Thread(delegate() {
            UpdateInfo u = null;
            try
            {
                string cacheDir = Path.Combine(dataDir, "Cache");
                string cacheFile = Path.Combine(cacheDir, "UpdateCheck.json");
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                if (!manuell && File.Exists(cacheFile) && DateTime.UtcNow.Subtract(new FileInfo(cacheFile).LastWriteTimeUtc).TotalHours < UpdateCacheStunden)
                {
                    try
                    {
                        Dictionary<string, object> cd = js.DeserializeObject(File.ReadAllText(cacheFile, Encoding.UTF8)) as Dictionary<string, object>;
                        if (cd != null && cd.ContainsKey("Tag") && cd["Tag"] != null && cd["Tag"].ToString().Length > 0)
                        {
                            u = new UpdateInfo(); u.Quelle = "Cache";
                            u.Tag = cd["Tag"].ToString();
                            if (cd.ContainsKey("DownloadUrl") && cd["DownloadUrl"] != null) u.Url = cd["DownloadUrl"].ToString();
                            if (cd.ContainsKey("Sha256") && cd["Sha256"] != null) u.Sha256 = cd["Sha256"].ToString();
                            if (cd.ContainsKey("Body") && cd["Body"] != null) u.Body = cd["Body"].ToString();
                        }
                    }
                    catch { u = null; }
                }
                if (u == null)
                {
                    u = FrageGitHub();
                    if (u.Fehler.Length == 0)
                    {
                        try
                        {
                            if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);
                            Dictionary<string, string> c = new Dictionary<string, string>();
                            c["CheckedAt"] = DateTime.UtcNow.ToString("o"); c["Tag"] = u.Tag; c["DownloadUrl"] = u.Url; c["Sha256"] = u.Sha256; c["Body"] = u.Body; c["Quelle"] = u.Quelle;
                            File.WriteAllText(cacheFile, js.Serialize(c), Encoding.UTF8);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex) { u = new UpdateInfo(); u.Fehler = ex.Message; }

            bool neuer = u.Fehler.Length == 0 && u.Tag.Length > 0 && IsNewerVersion(u.Tag.TrimStart('v', 'V', ' '), this.version.TrimStart('v', 'V', ' '));
            if (neuer)
            {
                AvailableUpdateVersion = u.Tag; AvailableUpdateUrl = u.Url; AvailableUpdateSha256 = u.Sha256; AvailableUpdateBody = u.Body;
                HasUpdateAvailable = true;
            }
            UpdateInfo r = u;
            try
            {
                BeginInvoke(new MethodInvoker(delegate() {
                    updateCheckLaeuft = false;
                    if (neuer) ApplyUpdateBadge(r.Tag);
                    if (!manuell) return;
                    if (r.Fehler.Length > 0) UI.Hinweis("Die Suche nach Updates ist fehlgeschlagen:\r\n\r\n" + r.Fehler, "Minibench-Update", this);
                    else if (neuer) UI.Hinweis("Version " + r.Tag.TrimStart('v', 'V') + " ist verfügbar. Oben rechts auf „Update verfügbar“ klicken, um sie zu installieren." + (r.Sha256.Length == 0 ? "\r\n\r\nDas Release nennt keine Prüfsumme; die Datei kann nach dem Herunterladen nicht geprüft werden." : ""), "Minibench-Update", this);
                    else UI.Hinweis("Leos Minibench " + this.version + " ist aktuell (neuestes Release auf GitHub: " + r.Tag + ").", "Minibench-Update", this);
                }));
            }
            catch { updateCheckLaeuft = false; }
        });
        t.IsBackground = true;
        t.Start();
    }

    // Versionen sind Dezimalzahlen (3.882 < 3.9 < 4.0 < 4.01 < 4.1). System.Version hielt 4.03 für größer als 4.1
    // und 3.882 für größer als 3.9 (ab v4.03 korrigiert).
    public static bool IsNewerVersion(string remote, string local)
    {
        decimal r, l;
        System.Globalization.CultureInfo inv = System.Globalization.CultureInfo.InvariantCulture;
        if (Decimal.TryParse((remote ?? "").Trim().TrimStart('v', 'V'), System.Globalization.NumberStyles.Number, inv, out r) &&
            Decimal.TryParse((local ?? "").Trim().TrimStart('v', 'V'), System.Globalization.NumberStyles.Number, inv, out l))
            return r > l;
        return String.Compare(remote, local, StringComparison.OrdinalIgnoreCase) > 0;
    }

    internal void TriggerUpdateDownload()
    {
        if (UpdateDownloadAbbrechen()) return;
        if (String.IsNullOrEmpty(AvailableUpdateUrl))
        {
            MessageBox.Show(this, "Keine Download-Adresse für das Update gefunden.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // ab v4.11: ohne bekannte Prüfsumme kein Austausch (bis v4.10 wurde die Datei dann ungeprüft eingesetzt)
        if (String.IsNullOrEmpty(AvailableUpdateSha256))
        {
            UI.Hinweis(UpdateOhnePruefsummeText(AvailableUpdateVersion), "Minibench-Update", this);
            return;
        }
        if (running) { UI.Hinweis("Während eines Laufs ist kein Update möglich.", "Minibench-Update", this); return; }
        if (HintergrundBelegt("Das Update")) return;

        string msg = "Update auf Version " + AvailableUpdateVersion + " jetzt herunterladen und LMB.exe austauschen?\r\n\r\n" +
                     "Die Datei wird vor dem Austausch mit der SHA-256-Prüfsumme des Releases verglichen. Leos Minibench schließt sich dafür. Die bisherige Programmdatei und ein Skript daneben (LeosMinibench.ps1) werden im Datenordner unter Archiv gesichert, danach startet die neue Version.";
        // ab v4.14: was die neue Version bringt (Text des Releases); ohne Text bleibt die Rückfrage wie bisher
        string neu = WasIstNeuText(AvailableUpdateBody, AvailableUpdateVersion);
        if (!UI.AskMitText(msg, "Was ist neu in " + AvailableUpdateVersion.TrimStart('v', 'V') + ":", neu, "Minibench Update", this))
            return;
        if (running || HintergrundBelegt("Das Update")) return;

        string targetExe;
        try
        {
            string updateDir = Path.Combine(dataDir, "Update");
            if (!Directory.Exists(updateDir)) Directory.CreateDirectory(updateDir);
            targetExe = Path.Combine(updateDir, "LMB.exe");
            if (File.Exists(targetExe)) File.Delete(targetExe);
            NetzVorbereiten(); // Tls12 und Proxy, auch ohne vorherige Prüfung im selben Prozess
        }
        catch (Exception ex)
        {
            UI.Hinweis("Das Update kann nicht vorbereitet werden:\r\n\r\n" + ex.Message, "Update Fehler", this);
            return;
        }
        StarteUpdateDownload(AvailableUpdateUrl, targetExe, AvailableUpdateSha256);
    }

    // ab v4.11: Herunterladen im Hintergrund (bis v4.10 DownloadFile im Oberflächen-Thread, bis 100 s "Keine Rückmeldung").
    // Fortschritt steht im Etikett der Kopfzeile; ein Klick darauf bricht ab, ebenso Schließen.
    System.Net.WebClient updateWc;

    void StarteUpdateDownload(string url, string targetExe, string sha256)
    {
        HintergrundArbeit arbeit = HintergrundBeginnen("Das Herunterladen des Updates");
        if (arbeit == null) return;
        string altText = badgeUpdate != null ? badgeUpdate.Text : "";
        System.Net.WebClient wc = new System.Net.WebClient();
        wc.Headers["User-Agent"] = "LeosMinibench";
        updateWc = wc;
        arbeit.Abbrechen = delegate { try { wc.CancelAsync(); } catch { } };
        if (badgeUpdate != null)
        {
            SetText(badgeUpdate, "Update wird geladen ...");
            Tip(badgeUpdate, "Das Update wird heruntergeladen. Klicken bricht das Herunterladen ab.");
        }
        wc.DownloadProgressChanged += delegate(object s, System.Net.DownloadProgressChangedEventArgs e) {
            if (badgeUpdate == null || badgeUpdate.IsDisposed) return;
            SetText(badgeUpdate, UpdateFortschrittText(e.BytesReceived, e.TotalBytesToReceive));
        };
        wc.DownloadFileCompleted += delegate(object s, System.ComponentModel.AsyncCompletedEventArgs e) {
            updateWc = null;
            HintergrundEnde(arbeit);
            try { wc.Dispose(); } catch { }
            if (IsDisposed || hintergrundGeschlossen) { try { File.Delete(targetExe); } catch { } return; }
            if (badgeUpdate != null)
            {
                SetText(badgeUpdate, altText);
                Tip(badgeUpdate, "Ein Update für Leos Minibench ist verfügbar. Klicken zum Herunterladen und Aktualisieren.");
            }
            if (e.Cancelled || arbeit.Abgebrochen)
            {
                try { File.Delete(targetExe); } catch { }
                UI.Hinweis("Das Herunterladen des Updates wurde abgebrochen. Leos Minibench bleibt unverändert.", "Minibench-Update", this);
                return;
            }
            if (e.Error != null)
            {
                try { File.Delete(targetExe); } catch { }
                UI.Hinweis("Fehler beim Herunterladen des Updates:\r\n\r\n" + FehlerText(e.Error) + "\r\n\r\nLeos Minibench bleibt unverändert.", "Update Fehler", this);
                return;
            }
            string meldung;
            if (!PruefeUpdateDatei(targetExe, sha256, out meldung)) { UI.Hinweis(meldung, "Update-Sicherheitsfehler", this); return; }
            if (running) { try { File.Delete(targetExe); } catch { } UI.Hinweis("Während eines Laufs ist kein Update möglich.", "Minibench-Update", this); return; }
            ExecuteSelfReplace(targetExe);
        };
        try { wc.DownloadFileAsync(new Uri(url), targetExe); }
        catch (Exception ex)
        {
            updateWc = null; HintergrundEnde(arbeit);
            try { wc.Dispose(); } catch { }
            if (badgeUpdate != null) SetText(badgeUpdate, altText);
            UI.Hinweis("Fehler beim Herunterladen des Updates:\r\n\r\n" + FehlerText(ex), "Update Fehler", this);
        }
    }

    // Klick auf das Etikett während des Herunterladens: nachfragen und abbrechen
    bool UpdateDownloadAbbrechen()
    {
        if (updateWc == null) return false;
        if (UI.Ask("Das Update wird gerade heruntergeladen. Herunterladen abbrechen?", "Minibench-Update", this) && hintergrund != null) HintergrundBeenden();
        return true;
    }

    // ab v4.14: "Was ist neu" für die Update-Rückfrage aus dem Text des Releases. Das ist der Abschnitt aus CHANGELOG.md,
    // den Veroeffentlichen.ps1 schreibt, gefolgt von der Download- und der Prüfsummenzeile; beide entfallen hier.
    // Überschriften (##), Fettschrift (**), Code (`), Verweise und Aufzählungszeichen werden Klartext, jeder Punkt
    // bleibt eine eigene Zeile. Mehr als WasIstNeuZeilen Zeilen werden gekürzt, mit Verweis auf das Release.
    public const int WasIstNeuZeilen = 15;
    public static string WasIstNeuText(string body, string tag)
    {
        if (String.IsNullOrEmpty(body)) return "";
        System.Text.RegularExpressions.RegexOptions io = System.Text.RegularExpressions.RegexOptions.IgnoreCase;
        List<string> z = new List<string>();
        int inhalt = 0;
        bool gekuerzt = false;
        foreach (string roh in body.Replace("\r\n", "\n").Split('\n'))
        {
            string l = roh.Trim();
            if (System.Text.RegularExpressions.Regex.IsMatch(l, @"^[*_]*Download:", io) || System.Text.RegularExpressions.Regex.IsMatch(l, @"SHA-?256\s*\(LMB\.exe\)", io)) continue;
            if (l.Length == 0) { if (z.Count > 0 && z[z.Count - 1].Length > 0) z.Add(""); continue; }
            if (inhalt >= WasIstNeuZeilen) { gekuerzt = true; break; }
            l = System.Text.RegularExpressions.Regex.Replace(l, @"^#+\s*", "");
            l = System.Text.RegularExpressions.Regex.Replace(l, @"^[*\-+]\s+", "");
            l = System.Text.RegularExpressions.Regex.Replace(l, @"\[([^\]]+)\]\([^)]*\)", "$1");
            l = l.Replace("**", "").Replace("__", "").Replace("`", "").Trim();
            if (l.Length == 0) continue;
            z.Add(l); inhalt++;
        }
        while (z.Count > 0 && z[z.Count - 1].Length == 0) z.RemoveAt(z.Count - 1);
        if (z.Count == 0) return "";
        string t = String.Join("\r\n", z.ToArray());
        if (gekuerzt) t += "\r\n\r\nWeitere Änderungen stehen im Release auf https://github.com/" + UpdateRepo + "/releases" + (String.IsNullOrEmpty(tag) ? "" : "/tag/" + tag) + ".";
        return t;
    }

    public static string UpdateFortschrittText(long geladen, long gesamt)
    {
        CultureInfo de = new CultureInfo("de-DE");
        if (gesamt > 0) return "Update wird geladen: " + Math.Min(100, (int)(geladen * 100 / gesamt)) + " %";
        return "Update wird geladen: " + (geladen / 1048576.0).ToString("0.0", de) + " MB";
    }

    public static string UpdateOhnePruefsummeText(string version)
    {
        string tag = String.IsNullOrEmpty(version) ? "" : version;
        return "Release ohne Prüfsumme, Update von Hand.\r\n\r\nDas Release " + tag + " nennt keine SHA-256-Prüfsumme für LMB.exe. Ohne sie lässt sich die heruntergeladene Datei nicht prüfen, deshalb tauscht Leos Minibench die Programmdatei nicht selbst aus.\r\n\r\n" +
               "Die neue LMB.exe steht auf https://github.com/" + UpdateRepo + "/releases" + (tag.Length > 0 ? "/tag/" + tag : "") + " und lässt sich dort von Hand herunterladen.";
    }

    // Prüft die heruntergeladene Datei gegen die Prüfsumme des Releases. Ohne Prüfsumme oder bei Abweichung wird die Datei
    // verworfen und meldung erklärt warum; true nur bei Übereinstimmung.
    public static bool PruefeUpdateDatei(string datei, string erwartet, out string meldung)
    {
        meldung = "";
        if (String.IsNullOrEmpty(datei) || !File.Exists(datei)) { meldung = "Die heruntergeladene Datei fehlt."; return false; }
        if (String.IsNullOrEmpty(erwartet))
        {
            try { File.Delete(datei); } catch { }
            meldung = "Release ohne Prüfsumme, Update von Hand. Die heruntergeladene Datei wurde verworfen.";
            return false;
        }
        string hash = "";
        try
        {
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            using (FileStream fs = File.OpenRead(datei))
            {
                byte[] bytes = sha.ComputeHash(fs);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
                hash = sb.ToString();
            }
        }
        catch (Exception ex) { hash = ""; meldung = ex.Message; }
        if (hash.Length > 0 && hash.Equals(erwartet.Trim(), StringComparison.OrdinalIgnoreCase)) return true;
        try { File.Delete(datei); } catch { }
        meldung = "Sicherheitsprüfung fehlgeschlagen:\r\n\r\n" +
                  "Die SHA-256-Prüfsumme der heruntergeladenen Datei stimmt nicht mit der Vorgabe des Releases überein:\r\n\r\n" +
                  "Erwartet: " + erwartet + "\r\n" +
                  "Berechnet: " + (hash.Length > 0 ? hash : "nicht lesbar (" + meldung + ")") + "\r\n\r\n" +
                  "Die Datei wurde aus Sicherheitsgründen verworfen.";
        return false;
    }

    // Programmdatei austauschen (ab v4.03 neu). Bis v4.02 hielt das Austauschskript this.psExe für die Programmdatei:
    // Das ist powershell.exe. "move" scheiterte, und "start" öffnete am Ende nur ein PowerShell-Fenster; LMB.exe blieb alt.
    // Jetzt: Pfad der exe aus LEOSMINIBENCH_EXE (setzt das Startprogramm), Warten, bis die exe nicht mehr gesperrt ist
    // (Startprogramm und Oberfläche beendet), altes Skript neben der exe mit archivieren (sonst lädt die neue exe weiter
    // das alte Skript), Protokoll in Update.log, Rückweg bei Fehlern.
    void ExecuteSelfReplace(string newExePath)
    {
        try
        {
            string updateDir = Path.GetDirectoryName(newExePath);
            string updateCmd = Path.Combine(updateDir, "Update.cmd");
            string logFile = Path.Combine(updateDir, "Update.log");
            string exe = Environment.GetEnvironmentVariable("LEOSMINIBENCH_EXE") ?? "";
            if (exe.Length == 0 || !exe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                // ohne Startprogramm gestartet (LeosMinibench.cmd): LMB.exe neben das Skript legen
                string sdir = File.Exists(script) && !script.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase) ? Path.GetDirectoryName(script) : (dataDir.Length > 0 ? Path.GetDirectoryName(dataDir.TrimEnd('\\')) : "");
                if (sdir.Length == 0) { UI.Hinweis("Der Ordner der Programmdatei ist unbekannt. Die neue LMB.exe liegt in:\r\n" + newExePath, "Update", this); return; }
                exe = Path.Combine(sdir, "LMB.exe");
            }
            string exeDir = Path.GetDirectoryName(exe);
            string ps1Beside = Path.Combine(exeDir, "LeosMinibench.ps1");
            string archivDir = Path.Combine(dataDir.Length > 0 ? dataDir : exeDir, Path.Combine("Archiv", "v" + this.version));

            List<string> c = new List<string>();
            c.Add("@echo off");
            c.Add("chcp 65001 >nul");
            c.Add("setlocal");
            c.Add("title Leos Minibench Update");
            c.Add("set \"EXE=" + exe + "\"");
            c.Add("set \"NEU=" + newExePath + "\"");
            c.Add("set \"ARCH=" + archivDir + "\"");
            c.Add("set \"PS1=" + ps1Beside + "\"");
            c.Add("set \"LOG=" + logFile + "\"");
            c.Add(">>\"%LOG%\" echo %date% %time% Update von " + this.version + " auf " + AvailableUpdateVersion + ": %EXE%");
            c.Add("if not exist \"%ARCH%\" mkdir \"%ARCH%\"");
            c.Add("if not exist \"%EXE%\" goto kopieren");
            c.Add("set /a n=0");
            // bis zu 90 s warten, bis Oberfläche und Startprogramm beendet sind und die exe frei ist
            c.Add(":warte");
            c.Add("set /a n+=1");
            c.Add("if %n% gtr 90 goto gesperrt");
            c.Add("ping -n 2 127.0.0.1 >nul");
            c.Add("move /y \"%EXE%\" \"%ARCH%\\LMB.exe\" >nul 2>&1");
            c.Add("if errorlevel 1 goto warte");
            c.Add(":kopieren");
            c.Add("if exist \"%PS1%\" move /y \"%PS1%\" \"%ARCH%\\LeosMinibench.ps1\" >nul 2>&1");
            c.Add("copy /y \"%NEU%\" \"%EXE%\" >nul");
            c.Add("if errorlevel 1 goto zurueck");
            c.Add(">>\"%LOG%\" echo %date% %time% erfolgreich, alte Version in %ARCH%");
            c.Add("del \"%NEU%\" >nul 2>&1");
            c.Add("start \"\" \"%EXE%\"");
            c.Add("goto ende");
            c.Add(":zurueck");
            c.Add(">>\"%LOG%\" echo %date% %time% Kopieren fehlgeschlagen, alte Version zurück");
            c.Add("if exist \"%ARCH%\\LMB.exe\" move /y \"%ARCH%\\LMB.exe\" \"%EXE%\" >nul");
            c.Add("if exist \"%ARCH%\\LeosMinibench.ps1\" if not exist \"%PS1%\" move /y \"%ARCH%\\LeosMinibench.ps1\" \"%PS1%\" >nul");
            c.Add("start \"\" \"%EXE%\"");
            c.Add("goto ende");
            c.Add(":gesperrt");
            c.Add(">>\"%LOG%\" echo %date% %time% LMB.exe blieb gesperrt, Update nicht ausgeführt. Neue Datei: %NEU%");
            c.Add(":ende");
            c.Add("del \"%~f0\"");
            File.WriteAllText(updateCmd, String.Join("\r\n", c.ToArray()) + "\r\n", new UTF8Encoding(false));

            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c \"\"" + updateCmd + "\"\"");
            psi.CreateNoWindow = true; psi.UseShellExecute = false; psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.WorkingDirectory = updateDir;
            Process.Start(psi);
            Close();
        }
        catch (Exception ex)
        {
            UI.Hinweis("Fehler beim Vorbereiten des Programmaustauschs:\r\n\r\n" + ex.Message, "Update Fehler", this);
        }
    }
}
