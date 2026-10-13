using OpenFive.Engine.World;

namespace OpenFive.Engine.Game;

/// <summary>
/// A menu screen. Each button is a pair of objects from the screen's ".res" file: the normal
/// picture and a highlighted one (same name plus "1"), swapped while the pointer is over it.
/// </summary>
public abstract class MenuScreen : LocationScript
{
    protected sealed record Button(string Name, int X, int Y, Action Click);

    protected readonly List<Button> Buttons = new();
    protected bool English => World.Data.Language == Language.English;
    Button? _hover;

    /// <summary>Menu screens decide what is shown themselves: start with everything hidden.</summary>
    protected void HideAll()
    {
        if (World.Scene == null)
            return;
        foreach (var o in World.Scene.Script.Objects)
            if (!o.Name.EndsWith("StartClick", StringComparison.OrdinalIgnoreCase))
                World.ShowObject(o.Name, false);
        World.ShowObject("COMMON_FLAG_D", false);
        World.ShowObject("COMMON_FLAG_GBR", false);
    }

    protected void AddButton(string name, int x, int y, Action click, bool visible = true)
    {
        World.MoveObject(name, x, y);
        World.MoveObject(name + "1", x, y);
        World.ShowObject(name, visible);
        World.ShowObject(name + "1", false);
        if (visible)
            Buttons.Add(new Button(name, x, y, click));
    }

    protected void Show(string name, int x, int y, bool on = true)
    {
        World.MoveObject(name, x, y);
        World.ShowObject(name, on);
    }

    protected void ClearButtons()
    {
        foreach (var b in Buttons)
        {
            World.ShowObject(b.Name, false);
            World.ShowObject(b.Name + "1", false);
        }
        Buttons.Clear();
        _hover = null;
    }

    Button? ButtonAt(int x, int y)
    {
        foreach (var b in Buttons)
            if (World.FindObject(b.Name) is { } o && o.Click.Length >= 3 && Geometry.Contains(o.Click, x, y))
                return b;
        return null;
    }

    /// <summary>Called by the session every tick with the pointer position.</summary>
    public override void Hover(int x, int y)
    {
        var b = ButtonAt(x, y);
        if (b == _hover)
            return;
        if (_hover != null && Buttons.Contains(_hover))
        {
            World.ShowObject(_hover.Name, true);
            World.ShowObject(_hover.Name + "1", false);
        }
        _hover = b;
        if (b != null)
        {
            World.ShowObject(b.Name, false);
            World.ShowObject(b.Name + "1", true);
        }
    }

    public override bool HandleClick(int x, int y)
    {
        ButtonAt(x, y)?.Click();
        return true;
    }

    /// <summary>Returns to the game, or to the main menu when no game is running.</summary>
    protected void BackToGame()
    {
        if (Session.InGame && Session.GameLocation.Length > 0)
            Session.ChangeLocation(Session.GameLocation);
    }

    protected void BackToMenu() => Session.ChangeLocation("menue");
}

/// <summary>The title logo shown after the intro; clicking it goes on to the main menu.</summary>
public sealed class LogoScreen : LocationScript
{
    public override string Location => "logo";

    public override void Enter(string from) => World.CursorVisible = true;

    public override bool Click(string objectName)
    {
        if (objectName.Equals("logoStartClick", StringComparison.OrdinalIgnoreCase))
            World.Tasks.Add(new ActionTask("CHANGELOC", _ => Session.ChangeLocation("menue")) { Delay = 5 });
        return true;
    }
}

/// <summary>The main menu: new game, load, save, quit, options, back to the game.</summary>
public sealed class MainMenu : MenuScreen
{
    public override string Location => "menue";

    public override void Enter(string from)
    {
        // The intro's music goes on into the menu from the logo; otherwise the menu is silent.
        if (!from.Equals("logo", StringComparison.OrdinalIgnoreCase))
            World.Audio.StopBackgroundTrack();
        HideAll();
        ShowMain();
    }

    void ShowMain()
    {
        ClearButtons();
        bool e = English;
        AddButton(e ? "MENUE_START" : "MENUE_STARTEN", 190, 100, NewGame);
        AddButton(e ? "MENUE_LOAD" : "MENUE_LADEN", 190, 150, () => Session.ChangeLocation("load"));
        AddButton(e ? "MENUE_SAVE" : "MENUE_SPEICHE", 190, 200, () => Session.ChangeLocation("save"), Session.InGame);
        AddButton(e ? "MENUE_QUIT" : "MENUE_BEENDEN", 190, 250, AskQuit);
        AddButton(e ? "MENUE_TOOPTIONS" : "MENUE_ZUOPTIONS", 120, 500, () => Session.ChangeLocation("options"));
        AddButton(e ? "MENUE_TOGAME" : "MENUE_ZUMSPIEL", 490, 500, BackToGame, Session.InGame);
    }

    void NewGame()
    {
        Session.NewGame();
    }

    void AskQuit()
    {
        ClearButtons();
        bool e = English;
        Show("MENUE_MENUEBOX", 150, 240);
        Show(e ? "MENUE_WQUIT" : "MENUE_WBEENDEN", e ? 166 : 190, 250);
        AddButton(e ? "MENUE_NO" : "MENUE_NEIN", 450, 300, () =>
        {
            CloseQuitBox();
            ShowMain();
        });
        AddButton(e ? "MENUE_YES" : "MENUE_JA", 350, 300, () => Session.QuitRequested = true);
    }

    void CloseQuitBox()
    {
        foreach (string n in new[] { "MENUE_MENUEBOX", "MENUE_WQUIT", "MENUE_WBEENDEN" })
            World.ShowObject(n, false);
    }
}

/// <summary>Load and save screens: ten slots with the names the player gave them.</summary>
public sealed class SaveLoadScreen : MenuScreen
{
    readonly bool _save;
    readonly List<TextItem> _labels = new();
    int _editing = -1;
    string _editName = "";

    public SaveLoadScreen(bool save) => _save = save;
    public override string Location => _save ? "save" : "load";

    string Prefix => _save ? "SAVE_" : "LOAD_";

    public override void Enter(string from)
    {
        bool e = English;
        HideAll();
        AddButton(Prefix + (e ? "EZUMMENUE" : "ZUMMENUE"), e ? 20 : 120, 500, BackToMenu);
        AddButton(Prefix + (e ? "EZUMSPIEL" : "ZUMSPIEL"), e ? 390 : 490, 500, BackToGame, Session.InGame);
        for (int i = 0; i < SaveGame.Slots; i++)
        {
            int slot = i;
            AddButton(Prefix + (_save ? "SGAME" : "LGAME") + i, 280, 124 + 30 * i, () => SlotClicked(slot));
            World.ShowObject(Prefix + (_save ? "SGAME" : "LGAME") + i, false);
        }
        RefreshLabels();
    }

    string SlotLabel(int slot)
    {
        string name = slot == _editing ? _editName + "_" : SaveGame.Load(slot)?.Name ?? (English ? "EMPTY" : "LEER");
        return $"{slot + 1}.) {name}";
    }

    void RefreshLabels()
    {
        foreach (var t in _labels)
            World.Texts.Remove(t);
        _labels.Clear();
        for (int i = 0; i < SaveGame.Slots; i++)
        {
            var t = new TextItem { Text = SlotLabel(i), X = 260, Y = 128 + 30 * i, Right = 560 };
            _labels.Add(t);
            World.Texts.Add(t);
        }
    }

    public override void Hover(int x, int y) { }

    void SlotClicked(int slot)
    {
        if (!_save)
        {
            if (SaveGame.Load(slot) is { } s)
                Session.Restore(s);
            return;
        }
        _editing = slot;
        _editName = SaveGame.Load(slot)?.Name ?? "";
        RefreshLabels();
    }

    /// <summary>Typing a save name: letters, digits and space; Enter saves, Escape cancels.</summary>
    public override bool KeyTyped(char c)
    {
        if (_editing < 0)
            return false;
        if (c == '\r' || c == '\n')
        {
            var save = Session.Capture(_editName.Length > 0 ? _editName : $"Game {_editing + 1}");
            if (save.Write(_editing))
                Show(English ? "SAVEGAMESAVED_E" : "SAVEGAMESAVED_D", English ? 216 : 16, 279);
            _editing = -1;
        }
        else if (c == '\u001b')
            _editing = -1;
        else if (c == '\b')
        {
            if (_editName.Length > 0)
                _editName = _editName[..^1];
        }
        else if (_editName.Length < 19 && World.Font.CharWidth(c) > 0 && !char.IsControl(c))
            _editName += char.ToUpperInvariant(c);
        RefreshLabels();
        return true;
    }
}

/// <summary>The options screen: display toggles, speech and music volume, language.</summary>
public sealed class OptionsScreen : MenuScreen
{
    public override string Location => "options";
    bool _dragSpeech, _dragMusic;

    public override void Enter(string from)
    {
        HideAll();
        Build();
    }

    void Toggle(string on, string off, int x, int y, bool value, Action flip)
    {
        World.ShowObject(on, false);
        World.ShowObject(off, false);
        AddButton(value ? on : off, x, y, () =>
        {
            flip();
            Session.Settings.Save();
            Rebuild();
        });
    }

    void Rebuild()
    {
        ClearButtons();
        HideAll();
        Build();
    }

    void Build()
    {
        var st = Session.Settings;
        bool e = English;
        string P(string g, string en) => "OPTIONS_" + (e ? en : g);
        Toggle(P("SCHATTE", "SHADOWE"), P("SCHATTA", "SHADOWA"), 190, 100, st.Shadows, () => st.Shadows = !st.Shadows);
        Toggle(P("HINTERE", "BANIME"), P("HINTERA", "BANIMA"), 190, 150, st.AnimatedBackgrounds,
            () => st.AnimatedBackgrounds = !st.AnimatedBackgrounds);
        Toggle(P("FILTERE", "EFILTERE"), P("FILTERA", "EFILTERA"), 190, 200, st.SmoothScaling,
            () => st.SmoothScaling = !st.SmoothScaling);
        Toggle(P("DIALOGE", "EDIALOGE"), P("DIALOGA", "EDIALOGA"), 190, 350, st.DialogueBox, () => st.DialogueBox = !st.DialogueBox);
        Toggle(P("MAUSE", "EMAUSE"), P("MAUSA", "EMAUSA"), 190, 400, st.HoverLabels, () => st.HoverLabels = !st.HoverLabels);
        Show(P("SPRACHE", "ESPRACHE"), e ? 120 : 200, 252);
        Show(P("LEISE", "ELEISE"), 290, 250);
        Show("OPTIONS_SLIDER", 390, 258);
        Show("OPTIONS_SLIDER1", st.SpeechVolume + 415, 254);
        Show(P("MUSIK", "EMUSIK"), e ? 120 : 140, 300);
        Show(P("LEISE2", "ELEISE2"), 350, 300);
        Show("OPTIONS_SLIDER2", 450, 308);
        Show("OPTIONS_SLIDER21", st.MusicVolume + 475, 304);
        AddButton(P("ZUMMENUE", "EZUMMENUE"), e ? 20 : 120, 500, BackToMenu);
        AddButton(P("ZUMSPIEL", "EZUMSPIEL"), e ? 390 : 490, 500, BackToGame, Session.InGame);
        // Language: the two flags of the common set.
        Show("COMMON_FLAG_GBR", 600, 240, !e);
        Show("COMMON_FLAG_D", 600, 240, e);
    }

    public override bool HandleClick(int x, int y)
    {
        if (y is >= 245 and < 275 && x is >= 405 and < 530)
            _dragSpeech = true;
        else if (y is >= 295 and < 325 && x is >= 465 and < 590)
            _dragMusic = true;
        else if (World.FindObject(English ? "COMMON_FLAG_D" : "COMMON_FLAG_GBR") is { } flag && World.IsObjectOn(flag.Name) &&
                 flag.Click.Length >= 3 && Geometry.Contains(flag.Click, x, y))
        {
            Session.SwitchLanguage(English ? Language.German : Language.English);
            return true;
        }
        else
            return base.HandleClick(x, y);
        Drag(x);
        return true;
    }

    public override void Hover(int x, int y)
    {
        base.Hover(x, y);
        if (_dragSpeech || _dragMusic)
            Drag(x);
    }

    public override void ButtonReleased()
    {
        if (_dragSpeech || _dragMusic)
            Session.Settings.Save();
        _dragSpeech = _dragMusic = false;
    }

    void Drag(int x)
    {
        var st = Session.Settings;
        if (_dragSpeech)
        {
            st.SpeechVolume = Math.Clamp(x - 415, 0, 100);
            World.MoveObject("OPTIONS_SLIDER1", st.SpeechVolume + 415, 254);
        }
        if (_dragMusic)
        {
            st.MusicVolume = Math.Clamp(x - 475, 0, 100);
            World.MoveObject("OPTIONS_SLIDER21", st.MusicVolume + 475, 304);
        }
        World.Audio.SetVolumes(st.MusicVolume, 100, st.SpeechVolume);
    }
}
