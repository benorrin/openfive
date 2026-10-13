namespace OpenFive.Engine.Story;

/// <summary>Names of the story variables shared between locations (saved with the game).</summary>
public static partial class Vars
{
    public const string GardenBucketTaken = "garden.bucketTaken";
    public const string BarrelMoved = "garden.barrelMoved";
    /// <summary>The living-room window seen from the garden: 0 closed, 1 open.</summary>
    public const string LivingRoomWindow = "house.livingRoomWindow";
    /// <summary>Uncle Quentin is busy with his tea (the moment to feed Timmy the sandwich).</summary>
    public const string Distracted = "kitchen.quentinDistracted";

    /// <summary>The story's quest (the same as the engine's quest; the scripts also set it through this).</summary>
    public const string QuestNumber = "v64efe4";
    /// <summary>The story's episode within the quest (the same as the engine's episode).</summary>
    public const string EpisodeNumber = "v64efe8";
    /// <summary>Language of the game: 1 German, 2 English (read only).</summary>
    public const string LanguageNumber = "v64efd4";
    /// <summary>How far talking to Dick has got at Kirrin Cottage and the cave; reset as the story moves on.</summary>
    public const string DickTalk = "v64ffec";
    /// <summary>How far talking to Anne has got at Kirrin Cottage and the cave.</summary>
    public const string AnneTalk = "v650024";
    /// <summary>How far talking to Julian has got at Kirrin Cottage and the cave.</summary>
    public const string JulianTalk = "v65005c";
    /// <summary>How far talking to Aunt Fanny has got.</summary>
    public const string FannyTalk = "v64ff7c";
    /// <summary>How far talking to Uncle Quentin in the kitchen has got.</summary>
    public const string QuentinKitchenTalk = "v64ffb4";
    /// <summary>How far talking to Dick on the beach has got.</summary>
    public const string DickBeachTalk = "v6504f4";
    /// <summary>How far talking to Anne on the beach has got.</summary>
    public const string AnneBeachTalk = "v650414";
    /// <summary>How far talking to Julian on the beach has got.</summary>
    public const string JulianBeachTalk = "v650484";
    /// <summary>How far talking to Alf on the beach has got.</summary>
    public const string AlfTalk = "v6506b4";
    /// <summary>The fisherman: 0 not met, 1 met, 2 asking him questions, 3 he has nothing more to say.</summary>
    public const string FishermanTalk = "v6503dc";
    /// <summary>The rowing boat on the beach (1 looked at; 3-5 what the fisherman has said about it).</summary>
    public const string BeachBoat = "v65060c";
    /// <summary>How far talking to Anne on the island beach has got.</summary>
    public const string AnneIslandTalk = "v650794";
    /// <summary>How far talking to Julian on the island beach has got.</summary>
    public const string JulianIslandTalk = "v6507cc";
    /// <summary>How far talking to George on the island beach has got.</summary>
    public const string GeorgeIslandTalk = "v65083c";
    /// <summary>How far talking to Anne at the wreck has got.</summary>
    public const string AnneWreckTalk = "v6508ac";
    /// <summary>How far talking to Julian at the wreck has got.</summary>
    public const string JulianWreckTalk = "v6508e4";
    /// <summary>How far talking to George at the wreck has got.</summary>
    public const string GeorgeWreckTalk = "v650954";
    /// <summary>How far talking to Anne in the castle courtyard has got.</summary>
    public const string AnneCastleTalk = "v650bbc";
    /// <summary>How far talking to George in the castle courtyard has got.</summary>
    public const string GeorgeCastleTalk = "v650b84";
    /// <summary>How far talking to Julian in the castle courtyard has got.</summary>
    public const string JulianCastleTalk = "v650bf4";
    /// <summary>How far talking to George in the dungeon has got.</summary>
    public const string GeorgeDungeonTalk = "v651054";
    /// <summary>The cupboard in the wreck's cabin has been unscrewed.</summary>
    public const string CabinCupboardOpen = "v65098c";
    /// <summary>The tin in the cabin: 1 being picked up, 2 in the rucksack.</summary>
    public const string TinTaken = "v6511dc";
    /// <summary>The tin has been opened with the tin opener.</summary>
    public const string TinOpened = "v64fe9c";
    /// <summary>The sandwich in the kitchen at night: 0 not there, 1 ready, 2 taken.</summary>
    public const string KitchenSandwich = "v64fdf4";
    /// <summary>The teapot in the kitchen at night: 2 taken.</summary>
    public const string KitchenTeapot = "v64fc34";
    /// <summary>The study fire: 1 burning, 0 put out with tea, 2 wood put on it.</summary>
    public const string StudyFire = "v64f304";
    /// <summary>The study fire was put out with tea while Quentin worked.</summary>
    public const string StudyFireOut = "v64eff9";
    /// <summary>The study window has been opened.</summary>
    public const string StudyWindowOpen = "v64f374";
    /// <summary>Wood has been taken from the study's log basket.</summary>
    public const string StudyWoodTaken = "v64f41c";
    /// <summary>The rope is tied to the stone by the well (taken back later).</summary>
    public const string RopeOnStone = "v650a6c";
    /// <summary>The rope has been tied to the ivy by the dungeon stairs.</summary>
    public const string RopeOnIvy = "v6514b4";
    /// <summary>How many stones have been taken from the pile in the courtyard.</summary>
    public const string StonesTaken = "v650c9c";
    /// <summary>How often the professor has been interrupted on the phone by a stone.</summary>
    public const string StonesThrown = "v65140c";
    /// <summary>Stones thrown at the bush on quest 11: 1 one, 2 the professor walks off.</summary>
    public const string ProfessorDistracted = "v65147c";
    /// <summary>Quest 11 in the courtyard: 0 the hero may only go to the stone pile, 1 going there, 2 the professor has gone.</summary>
    public const string CourtyardFree = "v6513d4";
    /// <summary>The big stone by the castle has been pushed aside (the dungeon stairs are open).</summary>
    public const string BigStonePushed = "v651594";
    /// <summary>The children have talked about breaking the treasure room door.</summary>
    public const string TreasureDoorDiscussed = "v650fac";
    /// <summary>The treasure room door has been broken with the axe.</summary>
    public const string TreasureDoorBroken = "v650fe4";
    /// <summary>Timmy has got out through the crack under the cell door.</summary>
    public const string CellCrackUsed = "v65132c";
    /// <summary>The rope ladder in the well: climbing state while going up or down.</summary>
    public const string WellRopeState = "v63be94";
    /// <summary>Vertical position of the rope ladder in the well.</summary>
    public const string WellRopeY = "v63be98";
}
