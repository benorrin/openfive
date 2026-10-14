using OpenFive.Engine.World;

namespace OpenFive.Engine.Game;

/// <summary>
/// A menu screen. Each button is a pair of objects from the screen's ".res" file: the normal
/// picture and a highlighted one (same name plus "1"), swapped while the pointer is over it.
/// </summary>
public abstract class MenuScreen : LocationScript
{
    /// <summary>A button: its object name, position and what clicking it does.</summary>
    protected sealed record Button(string Name, int X, int Y, Action Click);

    /// <summary>The buttons currently on the screen.</summary>
    protected readonly List<Button> Buttons = new();

    /// <summary>True when the game is played in English (menus have a picture per language).</summary>
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

    /// <summary>Puts a button at (x, y); an invisible one is placed but cannot be clicked.</summary>
    protected void AddButton(string name, int x, int y, Action click, bool visible = true)
    {
        World.MoveObject(name, x, y);
        World.MoveObject(HighlightOf(name), x, y);
        World.ShowObject(name, visible);
        World.ShowObject(HighlightOf(name), false);
        if (visible)
            Buttons.Add(new Button(name, x, y, click));
    }

    /// <summary>Shows (or hides) a picture of the screen at (x, y).</summary>
    protected void Show(string name, int x, int y, bool on = true)
    {
        World.MoveObject(name, x, y);
        World.ShowObject(name, on);
    }

    /// <summary>Removes all buttons from the screen.</summary>
    protected void ClearButtons()
    {
        foreach (var b in Buttons)
        {
            World.ShowObject(b.Name, false);
            World.ShowObject(HighlightOf(b.Name), false);
        }
        Buttons.Clear();
        _hover = null;
    }

    /// <summary>Highlights the button under the pointer (and un-highlights the previous one).</summary>
    public override void Hover(int x, int y)
    {
        var b = ButtonAt(x, y);
        if (b == _hover)
            return;
        if (_hover != null && Buttons.Contains(_hover))
            Highlight(_hover, false);
        _hover = b;
        if (b != null)
            Highlight(b, true);
    }

    /// <summary>Menus handle every click themselves: a button's action, or nothing.</summary>
    public override bool HandleClick(int x, int y)
    {
        ButtonAt(x, y)?.Click();
        return true;
    }

    /// <summary>Returns to the game, or does nothing when no game is running.</summary>
    protected void BackToGame()
    {
        if (Session.InGame && Session.GameLocation.Length > 0)
            Session.ChangeLocation(Session.GameLocation);
    }

    /// <summary>Goes to the main menu.</summary>
    protected void BackToMenu() => Session.ChangeLocation("menue");

    /// <summary>True when (x, y) is on a shown object of the screen.</summary>
    protected bool IsOnObject(string name, int x, int y) =>
        World.IsObjectOn(name) && World.FindObject(name) is { } o && o.Contains(x, y);

    Button? ButtonAt(int x, int y) => Buttons.FirstOrDefault(b => World.FindObject(b.Name)?.Contains(x, y) == true);

    void Highlight(Button b, bool on)
    {
        World.ShowObject(b.Name, !on);
        World.ShowObject(HighlightOf(b.Name), on);
    }

    static string HighlightOf(string name) => name + "1";
}

/// <summary>The title logo shown after the intro; clicking it goes on to the main menu.</summary>
public sealed class LogoScreen : LocationScript
{
    /// <inheritdoc/>
    public override string Location => "logo";

    /// <summary>Shows the pointer, so the logo can be clicked.</summary>
    public override void Enter(string from) => World.CursorVisible = true;

    /// <summary>A click on the logo goes to the main menu half a second later.</summary>
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
    /// <inheritdoc/>
    public override string Location => "menue";

    /// <summary>Shows the main buttons; the music carries on only when coming from the logo.</summary>
    public override void Enter(string from)
    {
        if (!from.Equals("logo", StringComparison.OrdinalIgnoreCase))
            World.Audio.StopBackgroundTrack();
        HideAll();
        ShowMain();
    }

    void ShowMain()
    {
        ClearButtons();
        bool e = English;
        AddButton(e ? "MENUE_START" : "MENUE_STARTEN", 190, 100, Session.NewGame);
        AddButton(e ? "MENUE_LOAD" : "MENUE_LADEN", 190, 150, () => Session.ChangeLocation("load"));
        AddButton(e ? "MENUE_SAVE" : "MENUE_SPEICHE", 190, 200, () => Session.ChangeLocation("save"), Session.InGame);
        AddButton(e ? "MENUE_QUIT" : "MENUE_BEENDEN", 190, 250, AskQuit);
        AddButton(e ? "MENUE_TOOPTIONS" : "MENUE_ZUOPTIONS", 120, 500, () => Session.ChangeLocation("options"));
        AddButton(e ? "MENUE_TOGAME" : "MENUE_ZUMSPIEL", 490, 500, BackToGame, Session.InGame);
    }

    /// <summary>"Do you really want to quit?" with yes and no.</summary>
    void AskQuit()
    {
        ClearButtons();
        bool e = English;
        Show("MENUE_MENUEBOX", 150, 240);
        Show(e ? "MENUE_WQUIT" : "MENUE_WBEENDEN", e ? 166 : 190, 250);
        AddButton(e ? "MENUE_NO" : "MENUE_NEIN", 450, 300, () =>
        {
            foreach (string box in new[] { "MENUE_MENUEBOX", "MENUE_WQUIT", "MENUE_WBEENDEN" })
                World.ShowObject(box, false);
            ShowMain();
        });
        AddButton(e ? "MENUE_YES" : "MENUE_JA", 350, 300, () => Session.QuitRequested = true);
    }
}

/// <summary>Load and save screens: ten slots with the names the player gave them.</summary>
public sealed class SaveLoadScreen : MenuScreen
{
    const int SlotsTop = 124, SlotStep = 30, MaxNameLength = 19;

    readonly bool _save;
    readonly List<TextItem> _labels = new();
    int _editing = -1;
    string _editName = "";

    /// <summary>Creates the save screen (<paramref name="save"/> true) or the load screen.</summary>
    public SaveLoadScreen(bool save) => _save = save;

    /// <inheritdoc/>
    public override string Location => _save ? "save" : "load";

    string Prefix => _save ? "SAVE_" : "LOAD_";

    string SlotButton(int slot) => Prefix + (_save ? "SGAME" : "LGAME") + slot;

    /// <summary>Shows the slots (as invisible buttons under their labels) and the way back.</summary>
    public override void Enter(string from)
    {
        bool e = English;
        HideAll();
        AddButton(Prefix + (e ? "EZUMMENUE" : "ZUMMENUE"), e ? 20 : 120, 500, BackToMenu);
        AddButton(Prefix + (e ? "EZUMSPIEL" : "ZUMSPIEL"), e ? 390 : 490, 500, BackToGame, Session.InGame);
        for (int slot = 0; slot < SaveGame.Slots; slot++)
        {
            int clicked = slot;
            AddButton(SlotButton(slot), 280, SlotsTop + SlotStep * slot, () => SlotClicked(clicked));
            World.ShowObject(SlotButton(slot), false);
        }
        RefreshLabels();
    }

    /// <summary>The slot buttons have no highlight.</summary>
    public override void Hover(int x, int y) { }

    /// <summary>Typing a save name: letters, digits and space; Enter saves, Escape cancels.</summary>
    public override bool KeyTyped(char c)
    {
        if (_editing < 0)
            return false;
        switch (c)
        {
            case '\r' or '\n':
                var save = Session.Capture(_editName.Length > 0 ? _editName : $"Game {_editing + 1}");
                if (save.Write(_editing))
                    Show(English ? "SAVEGAMESAVED_E" : "SAVEGAMESAVED_D", English ? 216 : 16, 279);
                _editing = -1;
                break;
            case '\u001b':
                _editing = -1;
                break;
            case '\b':
                if (_editName.Length > 0)
                    _editName = _editName[..^1];
                break;
            default:
                if (_editName.Length < MaxNameLength && World.Font.CharWidth(c) > 0 && !char.IsControl(c))
                    _editName += char.ToUpperInvariant(c);
                break;
        }
        RefreshLabels();
        return true;
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
        for (int slot = 0; slot < SaveGame.Slots; slot++)
        {
            var t = new TextItem { Text = SlotLabel(slot), X = 260, Y = SlotsTop + 4 + SlotStep * slot, Right = 560 };
            _labels.Add(t);
            World.Texts.Add(t);
        }
    }

    /// <summary>Loading: restores the slot. Saving: starts editing the slot's name.</summary>
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
}

/// <summary>The options screen: display toggles, speech and music volume, language.</summary>
public sealed class OptionsScreen : MenuScreen
{
    /// <summary>A volume slider: the area it reacts to and where its knob is drawn for volume 0.</summary>
    sealed record Slider(string Knob, int Left, int Top, int KnobX, int KnobY)
    {
        public bool Contains(int x, int y) => y >= Top && y < Top + 30 && x >= Left && x < Left + 125;
    }

    static readonly Slider SpeechSlider = new("OPTIONS_SLIDER1", 405, 245, 415, 254);
    static readonly Slider MusicSlider = new("OPTIONS_SLIDER21", 465, 295, 475, 304);

    /// <summary>The slider being dragged, if any.</summary>
    Slider? _dragging;

    /// <inheritdoc/>
    public override string Location => "options";

    /// <summary>Shows the options with their current values.</summary>
    public override void Enter(string from)
    {
        HideAll();
        Build();
    }

    /// <summary>A click on a slider starts dragging it; the flag switches the language.</summary>
    public override bool HandleClick(int x, int y)
    {
        if (SpeechSlider.Contains(x, y))
            _dragging = SpeechSlider;
        else if (MusicSlider.Contains(x, y))
            _dragging = MusicSlider;
        else if (IsOnObject(English ? "COMMON_FLAG_D" : "COMMON_FLAG_GBR", x, y))
        {
            Session.SwitchLanguage(English ? Language.German : Language.English);
            return true;
        }
        else
            return base.HandleClick(x, y);
        Drag(x);
        return true;
    }

    /// <summary>Highlights buttons and moves a slider being dragged.</summary>
    public override void Hover(int x, int y)
    {
        base.Hover(x, y);
        if (_dragging != null)
            Drag(x);
    }

    /// <summary>Letting go of a slider saves the new volume.</summary>
    public override void ButtonReleased()
    {
        if (_dragging != null)
            Session.Settings.Save();
        _dragging = null;
    }

    void Build()
    {
        var st = Session.Settings;
        bool e = English;
        string P(string german, string english) => "OPTIONS_" + (e ? english : german);
        Toggle(P("SCHATTE", "SHADOWE"), P("SCHATTA", "SHADOWA"), 190, 100, st.Shadows, () => st.Shadows = !st.Shadows);
        Toggle(P("HINTERE", "BANIME"), P("HINTERA", "BANIMA"), 190, 150, st.AnimatedBackgrounds,
            () => st.AnimatedBackgrounds = !st.AnimatedBackgrounds);
        Toggle(P("FILTERE", "EFILTERE"), P("FILTERA", "EFILTERA"), 190, 200, st.SmoothScaling,
            () => st.SmoothScaling = !st.SmoothScaling);
        Toggle(P("DIALOGE", "EDIALOGE"), P("DIALOGA", "EDIALOGA"), 190, 350, st.DialogueBox,
            () => st.DialogueBox = !st.DialogueBox);
        Toggle(P("MAUSE", "EMAUSE"), P("MAUSA", "EMAUSA"), 190, 400, st.HoverLabels, () => st.HoverLabels = !st.HoverLabels);
        Show(P("SPRACHE", "ESPRACHE"), e ? 120 : 200, 252);
        Show(P("LEISE", "ELEISE"), 290, 250);
        Show("OPTIONS_SLIDER", 390, 258);
        Show(SpeechSlider.Knob, SpeechSlider.KnobX + st.SpeechVolume, SpeechSlider.KnobY);
        Show(P("MUSIK", "EMUSIK"), e ? 120 : 140, 300);
        Show(P("LEISE2", "ELEISE2"), 350, 300);
        Show("OPTIONS_SLIDER2", 450, 308);
        Show(MusicSlider.Knob, MusicSlider.KnobX + st.MusicVolume, MusicSlider.KnobY);
        AddButton(P("ZUMMENUE", "EZUMMENUE"), e ? 20 : 120, 500, BackToMenu);
        AddButton(P("ZUMSPIEL", "EZUMSPIEL"), e ? 390 : 490, 500, BackToGame, Session.InGame);
        // Language: the flag of the other language.
        Show("COMMON_FLAG_GBR", 600, 240, !e);
        Show("COMMON_FLAG_D", 600, 240, e);
    }

    /// <summary>An on/off option: shows the picture for its current value; a click flips and saves it.</summary>
    void Toggle(string on, string off, int x, int y, bool value, Action flip)
    {
        World.ShowObject(on, false);
        World.ShowObject(off, false);
        AddButton(value ? on : off, x, y, () =>
        {
            flip();
            Session.Settings.Save();
            ClearButtons();
            HideAll();
            Build();
        });
    }

    /// <summary>Sets the volume of the slider being dragged from the pointer's x position.</summary>
    void Drag(int x)
    {
        var st = Session.Settings;
        if (_dragging == SpeechSlider)
            st.SpeechVolume = Math.Clamp(x - SpeechSlider.KnobX, 0, 100);
        else if (_dragging == MusicSlider)
            st.MusicVolume = Math.Clamp(x - MusicSlider.KnobX, 0, 100);
        else
            return;
        int volume = _dragging == SpeechSlider ? st.SpeechVolume : st.MusicVolume;
        World.MoveObject(_dragging.Knob, _dragging.KnobX + volume, _dragging.KnobY);
        World.Audio.SetVolumes(st.MusicVolume, 100, st.SpeechVolume);
    }
}
