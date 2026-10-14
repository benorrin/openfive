using OpenFive.Engine.Game;

namespace OpenFive.Engine.Story;

/// <summary>
/// The travel map (loc01): the places the children can go to by boat or on foot at this point
/// of the story — Kirrin Island, Kirrin Cottage and the beach — and a button to the main menu.
/// Each destination has a German ("g") and an English ("e") picture.
/// </summary>
public sealed class Loc01 : StoryScript
{
    /// <summary>The destinations, numbered as in their object names ("LOC01l01o2e").</summary>
    static readonly Dictionary<int, string> Targets = new() { [1] = "loc14", [2] = "loc06", [3] = "loc09" };

    /// <summary>Where each destination's picture goes, per language.</summary>
    static readonly Dictionary<char, (int X, int Y)[]> Spots = new()
    {
        ['e'] = new[] { (75, 136), (395, 65), (314, 372) },
        ['g'] = new[] { (75, 136), (410, 44), (341, 345) },
    };

    /// <inheritdoc/>
    public override string Location => "loc01";

    char LanguageSuffix => German ? 'g' : 'e';

    /// <summary>
    /// Stops the characters, hides every picture, then shows the greeting, the menu button and
    /// the destinations open now, in the game's language.
    /// </summary>
    /// <remarks>The original's FUN_00422dea and FUN_00422e90.</remarks>
    protected override void Setup()
    {
        foreach (var a in new[] { Dick, Anne, George, Timmy, Julian })
            ClearQueue(a);
        foreach (char language in "eg")
        {
            for (int i = 0; i < 3; i++)
                MoveObj(Destination(i + 1, language), Spots[language][i].X, Spots[language][i].Y);
            for (int i = 0; i < 3; i++)
                Obj(Destination(i + 1, language), false);
        }
        Obj("LOC01greet", false);
        Obj("LOC01gruss", false);
        foreach (string button in new[] { "LOC01_ZUMMENUE", "LOC01_ZUMMENUE1", "LOC01_EZUMMENUE", "LOC01_EZUMMENUE1" })
            ObjAfter(button, false, null);
        if (German)
            ShowGreeting("LOC01gruss", "LOC01_ZUMMENUE", 30, 550);
        else
            ShowGreeting("LOC01greet", "LOC01_EZUMMENUE", -50, 560);

        var (cottage, beach, island) = OpenDestinations();
        if (island)
            Obj(Destination(1, LanguageSuffix), true);
        if (cottage)
            Obj(Destination(2, LanguageSuffix), true);
        if (beach)
            Obj(Destination(3, LanguageSuffix), true);
    }

    /// <summary>The characters are not shown on the map.</summary>
    protected override void Arrive() { }

    /// <summary>A destination travels there; the picture itself goes back; the button goes to the main menu.</summary>
    /// <remarks>The original's FUN_00422c5e.</remarks>
    public override bool Click(string name)
    {
        foreach (var (number, target) in Targets)
            if (Same(name, Destination(number, 'e')) || Same(name, Destination(number, 'g')))
            {
                ChangeLocationNow(target);
                return true;
            }
        if (Same(name, "LOC01StartClick"))
            ChangeLocationNow(From);
        else if (Same(name, "LOC01_ZUMMENUE") || Same(name, "LOC01_EZUMMENUE"))
            ChangeLocationNow("menue");
        return true;
    }

    /// <summary>The object of destination <paramref name="number"/> in a language ('e' or 'g').</summary>
    static string Destination(int number, char language) => $"LOC01l01o{number}{language}";

    /// <summary>Shows the greeting and the main-menu button (its highlight hidden) of the game's language.</summary>
    void ShowGreeting(string greeting, string button, int x, int y)
    {
        Obj(greeting, true);
        MoveObj(button, x, y);
        ObjAfter(button, true, null);
        MoveObj(button + "1", x, y);
        ObjAfter(button + "1", false, null);
    }

    /// <summary>
    /// Which places can be travelled to at this point of the story (the original's
    /// v63bda0-v63bda2). Coming from the island or the castle (location 17 and on), none.
    /// </summary>
    (bool Cottage, bool Beach, bool Island) OpenDestinations()
    {
        var none = (false, false, false);
        if (FromId > 16)
            return none;
        switch (Quest)
        {
            case 3:
                bool open = Episode != 3;
                return (open, open, false);
            case 5:
                return none;
            case < 5:
                return (true, true, false);
            case 6:
                if (Episode >= 3)
                    return (true, true, true);
                return Episode == 1 ? none : (true, true, false);
            case 7:
                if (Episode >= 3)
                    return (true, true, false);
                return Episode == 1 ? none : (true, true, true);
            case 8:
                // Leaving the study with the map, the mainland is closed; once the hero has the
                // rope, the axe and the torch, the island opens.
                bool mainland = !(Episode == 1 && FromId == 3);
                bool island = Episode == 2 && Has(Item.Rope) && Has(Item.Axe) && Has(Item.Torch);
                return (mainland, mainland, island);
            case 11:
                return (false, false, true);
            default:
                return none;
        }
    }
}
