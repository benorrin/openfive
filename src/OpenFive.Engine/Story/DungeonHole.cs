using OpenFive.Engine.Game;
using OpenFive.Engine.World;
using static OpenFive.Engine.World.ActorStates;

namespace OpenFive.Engine.Story;

/// <summary>
/// The dungeon room with the hole down to the bottom of the well (loc22). On quest 9 Dick and
/// George find it; on quest 10, after the escape, the others wait here.
/// </summary>
public sealed class DungeonHole : StoryScript
{
    /// <summary>"Loch" while the conversation at the hole runs (the hero climbs down after it).</summary>
    const string AtTheHole = "t63bebc";

    /// <inheritdoc/>
    public override string Location => "loc22";

    /// <summary>The hero comes in from where they came from; on quests 9 and 10 the others are here.</summary>
    /// <remarks>The original's FUN_00456553 and FUN_004565df.</remarks>
    protected override void Setup()
    {
        Hero.Follower = null;
        TaskHandle? arrived = null;
        switch (FromId)
        {
            case 20: // up from the bottom of the well
                Place(Hero, 464, 449, StandDownRight);
                Walk(Hero, 464, 500);
                Show(Hero);
                break;
            case 25:
                Place(Hero, 499, 319, StandDownLeft);
                Show(Hero);
                Walk(Hero, 429, 400);
                arrived = State(Hero, StandDownRight);
                break;
            default:
                Place(Hero, 429, 400, StandDownLeft);
                Show(Hero);
                break;
        }
        if (At(9, 1))
        {
            Place(George, 561, 318, StandLeft);
            // As in the original, the hero is made their own follower, so nobody follows.
            Hero.Follower = Hero;
        }
        else if (At(10, 3))
        {
            PointerOff();
            Place(Timmy, 561, 200, StandLeft);
            Place(George, 561, 318, StandLeft);
            ToggleAfter(Vars.V683260, arrived);
            Place(Anne, 464, 300, StandDownRight);
            Place(Julian, 610, 344, StandDownLeft);
            Say(Who.Dick, "BRUNNEN0049", 1, after: arrived, delay: 3);
            Say(Who.Anne, "BRUNNEN0049", 2);
            Say(Who.Dick, "BRUNNEN0049", 3, last: true);
        }
        ShowParty();
    }

    /// <summary>The characters are placed by <see cref="Setup"/>.</summary>
    protected override void Arrive() { }

    /// <summary>Talking to George (quest 9), the hole, or the passage on.</summary>
    public override bool Click(string name)
    {
        if (World.Actors.FirstOrDefault(a => a.Name == name) is { } actor)
            ClickActor(actor);
        else
            ClickObject(name);
        return true;
    }

    /// <summary>
    /// The hole: on quest 9 Dick and George talk about it first, otherwise the hero climbs down.
    /// The way on is not taken on quest 10, when the others still have something to say.
    /// </summary>
    /// <remarks>The original's FUN_004560d8.</remarks>
    void ClickObject(string name)
    {
        if (Same(name, "LOC22Loch"))
        {
            Walk(Hero, 460, 548);
            if (At(9, 1))
            {
                var watching = Chain = State(Hero, WatchLeft);
                PointerAfter(false, watching, x: 400, y: 400);
                // As in the original, every line waits for the hero to look into the hole.
                Say(Who.George, "LABYRINTH0045", 2, after: watching);
                Say(Who.Dick, "LABYRINTH0045", 3, after: watching);
                Say(Who.George, "LABYRINTH0045", 4, after: watching);
                Say(Who.Dick, "LABYRINTH0045", 5, after: watching);
                Say(Who.George, "LABYRINTH0045", 6, last: true, after: watching);
                SetText(AtTheHole, "Loch");
            }
            else
            {
                SetVar(Vars.V683260, 1);
                var watching = State(Hero, WatchLeft);
                PointerAfter(false, watching, x: 400, y: 400);
                ChangeLocation("loc20", watching);
            }
        }
        else if (Same(name, "LOC22ExitToLOC25"))
        {
            if (At(10, 3))
            {
                State(Hero, StandUpRight);
                Chain = null;
                Say(Who.Julian, "BRUNNEN0049", 4);
                Say(Who.Dick, "BRUNNEN0049", 5, last: true);
            }
            else
            {
                Walk(Hero, 499, 319);
                ChangeLocation("loc25");
            }
        }
    }

    /// <summary>On quest 9 George has something to say; the hero turns to him.</summary>
    /// <remarks>The original's FUN_0045646b.</remarks>
    void ClickActor(Actor actor)
    {
        if (actor != George || !At(9, 1))
            return;
        Chain = State(Hero, FacingGeorge);
        Say(Who.George, "LABYRINTH0045", 1, last: true);
    }

    /// <summary>
    /// On quest 9 Dick and George face each other while talking; on quest 10 Dick, Julian and
    /// Anne take their usual poses.
    /// </summary>
    protected override TalkPoses? PosesOf(int speaker)
    {
        if (At(9, 1))
            return HeroAndGeorgePoses(speaker);
        if (At(10, 3))
        {
            return speaker switch
            {
                Who.Dick => new TalkPoses(Hero, SpeakDownRight, StandDownRight),
                Who.Julian => new TalkPoses(Julian, SpeakDownLeft, StandDownLeft),
                Who.Anne => new TalkPoses(Anne, SpeakDownRight, StandDownRight),
                _ => null,
            };
        }
        return null;
    }

    /// <summary>The speaker takes their talking pose.</summary>
    /// <remarks>The original's FUN_004568a0.</remarks>
    public override void LineStarting(GameWorld world, int speaker) => StartTalking();

    /// <summary>The speaker stands again after their line.</summary>
    /// <remarks>The original's FUN_00456a11.</remarks>
    public override void LineQueued(GameWorld world, TaskHandle end) => StopTalking(end);

    /// <summary>The last speaker stands again; after the talk at the hole, the hero climbs down.</summary>
    /// <remarks>The original's FUN_00456b7d.</remarks>
    public override void ConversationEnding(GameWorld world, TaskHandle end)
    {
        StopTalking(end);
        if (!At(9, 1))
            return;
        if (Text(AtTheHole) == "Loch")
        {
            PointerAfter(false, end, x: 400, y: 400);
            ChangeLocation("loc20", end, 3);
        }
        SetText(AtTheHole, "");
    }
}
