using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The locked door to the treasure chamber (loc24). On quest 9 Dick and George find it; George
/// goes back for the others, the hero breaks the door open with the axe, and the others arrive.
/// </summary>
public sealed class TreasureDoor : StoryScript
{
    /// <inheritdoc/>
    public override string Location => "loc24";

    /// <summary>The hero comes in; on quest 9 George is here. Then the door: whole, or broken (open).</summary>
    /// <remarks>The original's FUN_00457476, FUN_004575ba and FUN_004574cd.</remarks>
    protected override void Setup()
    {
        Hero.Follower = null;
        if (FromId == 25)
        {
            Place(Hero, 256, 255, StandDownLeft);
            Show(Hero);
            Walk(Hero, 193, 500);
        }
        else
        {
            Place(Hero, 193, 500, StandDownLeft);
            Show(Hero);
        }
        if (At(9, 1))
        {
            Place(George, 363, 253, StandLeft);
            if (Var(Vars.TreasureDoorDiscussed) == 0)
            {
                PointerOff();
                // As in the original, the hero is made their own follower, so nobody follows.
                Hero.Follower = Hero;
                PointerAfter(true, null, 120, 1, 1);
            }
        }
        ShowParty();

        Obj("LOC24Tuer", false);
        MoveObj("LOC24Tuer", 396, 80);
        Depth(@"bmp\obj\locs\loc24\l24tue.brg", 7500);
        AnimPos("Tuer", 412, 110);
        AnimPos("TuerClose", 412, 110);
        StopAnim("TuerClose");
        StopAnim("Tuer");
        if (Var(Vars.TreasureDoorBroken) == 0)
            Obj("LOC24Tuer", true);
        else
            Anim("TuerClose", -1);
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>Talking to George, the way back, or the treasure door.</summary>
    public override bool Click(string name)
    {
        if (World.Actors.FirstOrDefault(a => a.Name == name) is { } actor)
            ClickActor(actor);
        else
            ClickObject(name);
        return true;
    }

    /// <summary>
    /// The way back (not while waiting for the others on quest 9), and the treasure door: shut
    /// until broken, then the way into the treasure chamber.
    /// </summary>
    /// <remarks>The original's FUN_0045702a.</remarks>
    void ClickObject(string name)
    {
        if (Same(name, "LOC24ExitToLoc25"))
        {
            Walk(Hero, 363, 253);
            Chain = State(Hero, StandRight);
            if (At(9, 1) && Var(Vars.TreasureDoorDiscussed) == 1)
                Say(Who.Dick, "LABYRINTH0046", 12, last: true);
            else
                ChangeLocation("loc25", Chain);
        }
        else if (Same(name, "LOC24ExitToLoc27"))
        {
            Walk(Hero, 399, 417);
            var reached = Chain = State(Hero, TakeMidRight);
            if (At(9, 1))
            {
                if (Var(Vars.TreasureDoorDiscussed) == 0)
                {
                    // Locked: the hero rattles it and says so.
                    Sound(@"wav\fx\tuerruet.wav", reached, 3);
                    Say(Who.Dick, "LABYRINTH0046", 1, last: true);
                    return;
                }
                if (Var(Vars.TreasureDoorBroken) < 1)
                {
                    Sound(@"wav\fx\tuerruet.wav", reached, 3);
                    return;
                }
                PointerAfter(false, reached, x: 400, y: 400);
                SetVar(Vars.OriginalPlaceFlag, 1);
            }
            StopAnimAfter("TuerClose", reached);
            Anim("Tuer", after: reached);
            ChangeLocation("loc27");
        }
    }

    /// <summary>On quest 9, before the door has been discussed, George and Dick talk about it.</summary>
    /// <remarks>The original's FUN_004572a5.</remarks>
    void ClickActor(Actor actor)
    {
        if (actor != George || !At(9, 1) || Var(Vars.TreasureDoorDiscussed) >= 1)
            return;
        var turned = Chain = State(Hero, FacingGeorge);
        // As in the original, every line waits for the hero to turn.
        Say(Who.George, "LABYRINTH0046", 2, after: turned);
        Say(Who.Dick, "LABYRINTH0046", 3, after: turned);
        Say(Who.George, "LABYRINTH0046", 4, after: turned);
        Say(Who.Dick, "LABYRINTH0046", 5, last: true, after: turned);
        SetText(StoryTexts.TreasureDoorScene, "geh");
    }

    /// <summary>The hero and George face each other; Julian and Anne have their usual poses.</summary>
    protected override TalkPoses? PosesOf(int speaker) => HeroAndGeorgePoses(speaker) ?? speaker switch
    {
        Who.Julian => new TalkPoses(Julian, SpeakDownLeft, StandDownLeft),
        Who.Anne => new TalkPoses(Anne, SpeakDownRight, StandDownRight),
        _ => null,
    };

    /// <summary>The speaker takes their talking pose.</summary>
    /// <remarks>The original's FUN_004576d2.</remarks>
    public override void LineStarting(GameWorld world, int speaker) => StartTalking();

    /// <summary>The speaker stands again after their line.</summary>
    /// <remarks>The original's FUN_004577f0.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end) => StopTalking(end);

    /// <summary>The last speaker stands again; on quest 9 the next part of the door scene follows.</summary>
    /// <remarks>The original's FUN_0045790e.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        StopTalking(end);
        if (At(9, 1))
        {
            switch (Text(StoryTexts.TreasureDoorScene))
            {
                case "geh":
                    GeorgeFetchesTheOthers(end);
                    break;
                case "geh2":
                    BreakTheDoor(end);
                    return;
                case "Tuer":
                    TheOthersArrive(end);
                    break;
            }
        }
        SetText(StoryTexts.TreasureDoorScene, "");
    }

    /// <summary>George walks off to fetch the others; the hero is left on their own.</summary>
    void GeorgeFetchesTheOthers(TaskHandle end)
    {
        PointerAfter(false, end, x: 400, y: 400);
        WalkAfter(George, 381, 250, end);
        var gone = State(George, StandLeft);
        PointerAfter(true, end, 100, 400, 400);
        HideAfter(George, gone);
        SetVar(Vars.TreasureDoorDiscussed, 1);
        Hero.Follower = null;
    }

    /// <summary>
    /// The hero breaks the door with the axe (George has gone for the others first, if he had not
    /// already); then the others arrive.
    /// </summary>
    void BreakTheDoor(TaskHandle end)
    {
        PointerAfter(false, end, x: 400, y: 400);
        WalkAfter(George, 381, 250, end);
        var gone = State(George, StandLeft);
        HideAfter(George, gone);
        SetVar(Vars.TreasureDoorDiscussed, 1);
        Hero.Follower = null;
        WalkAfter(Hero, 348, 380, null, 5);
        PointerAfter(false, end, x: 400, y: 400);
        var chopping = Chain = State(Hero, AxeRight);
        Sound(@"wav\fx\axt2.wav", chopping, 8);
        Sound(@"wav\fx\axt2.wav", chopping, 18);
        Sound(@"wav\fx\axt2.wav", chopping, 28);
        Say(Who.Dick, "LABYRINTH0046", 7, last: true, delay: 31);
        SetVar(Vars.TreasureDoorBroken, 1);
        ObjAfter("LOC24Tuer", false, chopping, 6);
        Anim("TuerClose", -1, chopping, 6);
        SetText(StoryTexts.TreasureDoorScene, "Tuer");
    }

    /// <summary>George comes back with Anne, Julian and Timmy, and they talk about the open door.</summary>
    void TheOthersArrive(TaskHandle end)
    {
        PointerAfter(false, end, x: 400, y: 400);
        PlaceAfter(George, 350, 253, StandLeft, end);
        PlaceAfter(Anne, 355, 253, StandLeft, end);
        PlaceAfter(Julian, 360, 253, StandLeft, end);
        PlaceAfter(Timmy, 365, 253, StandLeft, end);
        WalkAfter(George, 180, 400, end, 2);
        WalkAfter(Anne, 125, 288, end, 6);
        WalkAfter(Julian, 235, 253, end, 10);
        var arrived = Chain = StateAfter(George, StandDownRight, end);
        Say(Who.Dick, "LABYRINTH0046", 8);
        Say(Who.George, "LABYRINTH0046", 9);
        Say(Who.Anne, "LABYRINTH0046", 10);
        Say(Who.Julian, "LABYRINTH0046", 11, last: true);
        ToggleAfter(Vars.OriginalPlaceFlag, arrived);
    }
}
