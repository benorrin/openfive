namespace OpenFive.Engine.Story;

/// <summary>Names of the story variables shared between locations (saved with the game).</summary>
public static class Vars
{
    /// <summary>The bucket in the garden has been taken.</summary>
    public const string GardenBucketTaken = "garden.bucketTaken";
    /// <summary>The barrel in the garden has been moved.</summary>
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

    /// <summary>Set while the shed puts the axe in place.</summary>
    public const string ShedPlacingAxe = "v63be03";

    /// <summary>Set while the shed puts the rope in place.</summary>
    public const string ShedPlacingRope = "v63be04";

    /// <summary>Set while the shed puts the oil can in place.</summary>
    public const string ShedPlacingOilCan = "v63be05";

    /// <summary>Set while the shed puts the screwdriver in place.</summary>
    public const string ShedPlacingScrewdriver = "v63be06";

    /// <summary>The axe in the shed can be taken (story record 0x650214).</summary>
    public const string AxeAvailable = "v650219";

    /// <summary>The axe has been taken from the shed.</summary>
    public const string AxeTaken = "v65021c";

    /// <summary>The rope in the shed can be taken (story record 0x65024c).</summary>
    public const string RopeAvailable = "v650251";

    /// <summary>The rope has been taken from the shed.</summary>
    public const string RopeTaken = "v650254";

    /// <summary>The oil can in the shed can be taken (story record 0x650284).</summary>
    public const string OilCanAvailable = "v650289";

    /// <summary>The oil can has been taken from the shed.</summary>
    public const string OilCanTaken = "v65028c";

    /// <summary>The screwdriver in the shed can be taken (story record 0x6502bc).</summary>
    public const string ScrewdriverAvailable = "v6502c1";

    /// <summary>The screwdriver has been taken from the shed.</summary>
    public const string ScrewdriverTaken = "v6502c4";

    /// <summary>A pointer position recorded by the original's rucksack code (x). OpenFive never sets it, so it reads 0.</summary>
    public const string RucksackPointerX = "v4ace9c";

    /// <summary>A pointer position recorded by the original's rucksack code (y). OpenFive never sets it, so it reads 0.</summary>
    public const string RucksackPointerY = "v4acea0";

    /// <summary>Timmy has been put into the bucket at the bottom of the well shaft.</summary>
    public const string TimmyInBucket = "v650ecc";

    /// <summary>Dick has called down to Timmy in the well shaft.</summary>
    public const string TimmyCalledInWell = "v650e94";

    /// <summary>The hero has been down the well shaft (George no longer warns about it).</summary>
    public const string WellShaftVisited = "v650e5c";

    /// <summary>Timmy has been pulled up out of the well.</summary>
    public const string TimmyRescued = "v6511a4";

    /// <summary>How far talking to George at the dungeon stairs has got (quest 9).</summary>
    public const string GeorgeStairsTalk = "v650f3c";

    /// <summary>Dick and George have talked on first coming down the dungeon stairs.</summary>
    public const string StairsSceneSeen = "v650f74";

    /// <summary>The professor and his accomplice have appeared in the treasure chamber (George turns to them).</summary>
    public const string ProfessorArrived = "v63beec";

    /// <summary>Dick has said once that he will not leave Anne behind in the dungeon passage (quest 10).</summary>
    public const string DickStaysWithAnne = "v651364";

    /// <summary>Dick and George have talked on first reaching the dungeon passage (quest 9).</summary>
    public const string PassageSceneSeen = "v65101c";

    /// <summary>How far talking to Julian in the cell has got (quest 10).</summary>
    public const string JulianCellTalk = "v6512bc";

    /// <summary>How far talking to Anne in the cell has got (quest 10).</summary>
    public const string AnneCellTalk = "v6512f4";

    /// <summary>Something has been tried on the lock of the cabin cupboard.</summary>
    public const string CupboardLockTried = "v64eff8";

    /// <summary>The wreck cabin has been entered (on quest 6, step 3).</summary>
    public const string CabinVisited = "v65075c";

    /// <summary>The visits to the wreck: 1 first talk on deck, 2 left again or into the cabin, 3 talk after the cabin.</summary>
    public const string WreckVisit = "v650874";

    /// <summary>Back at the wreck with the tin (quest 7): the talk about it is running; its menu follows.</summary>
    public const string WreckTinTalkPending = "v63be58";

    /// <summary>The menu about the tin at the wreck has not been offered yet (1 in a new game).</summary>
    public const string WreckTinMenuAvailable = "v64ef10";

    /// <summary>The guest-room door record is available (always, from a new game): the door is shown closing behind the hero.</summary>
    public const string GuestRoomDoorAvailable = "v64f141";

    /// <summary>The carpet in the guest room is shown (1 in a new game).</summary>
    public const string CarpetAvailable = "v64f1b1";

    /// <summary>The guest-room carpet: 0 flat, 1 rolled back (the diary showing), 2 flat again for good.</summary>
    public const string CarpetState = "v64f1b4";

    /// <summary>Where the guest-room carpet lies (x; 350 in a new game).</summary>
    public const string CarpetX = "v64f1b8";

    /// <summary>Where the guest-room carpet lies (y; 480 in a new game).</summary>
    public const string CarpetY = "v64f1bc";

    /// <summary>The diary in the guest room is shown (1 in a new game).</summary>
    public const string DiaryAvailable = "v64f1e9";

    /// <summary>The diary: 0 under the carpet, 1 taken, 2 put on the shelf.</summary>
    public const string DiaryState = "v64f1ec";

    /// <summary>The wardrobe in the guest room is shown (1 in a new game).</summary>
    public const string WardrobeAvailable = "v64f179";

    /// <summary>The guest-room wardrobe: 0 shut, 1 open, 2 open with the coat gone.</summary>
    public const string WardrobeState = "v64f17c";

    /// <summary>The suitcase key in the coat pocket can be taken (story record 0x64f21c).</summary>
    public const string SuitcaseKeyAvailable = "v64f221";

    /// <summary>The suitcase key has been taken from the coat pocket.</summary>
    public const string SuitcaseKeyTaken = "v64f224";

    /// <summary>How far George's remarks about looking under the guest-room bed have got (2: an item was used there).</summary>
    public const string UnderBedTalk = "v64f294";

    /// <summary>Set while the guest room puts the diary in place.</summary>
    public const string GuestRoomPlacingDiary = "v63bda4";

    /// <summary>Set while the guest room shows or hides the coat pocket with the key.</summary>
    public const string GuestRoomPlacingKey = "v63bda5";

    /// <summary>The hero is looking under the guest-room bed.</summary>
    public const string LookingUnderBed = "v63bda6";

    /// <summary>UnderBedTalk before an item was used under the bed (restored afterwards).</summary>
    public const string UnderBedTalkBefore = "v63bda8";

    /// <summary>The professor has got up (he no longer sleeps in the guest room).</summary>
    public const string ProfessorUp = "v6514ec";

    /// <summary>The torch in the hideout: 0 lying there, 1 being picked up, 2 in the rucksack.</summary>
    public const string TorchTaken = "v65124c";

    /// <summary>The lantern in the hideout: 0 there, 1 being picked up, 2 in the rucksack.</summary>
    public const string LanternTaken = "v651214";

    /// <summary>The binoculars have been taken from the hideout (they are no longer shown).</summary>
    public const string BinocularsTaken = "v651284";

    /// <summary>How far talking to Anne in the hideout has got (quest 8; 2 once the hero has the torch and the axe).</summary>
    public const string AnneHideoutTalk = "v6502fc";

    /// <summary>How far talking to Dick in the hideout has got (quest 8).</summary>
    public const string DickHideoutTalk = "v650334";

    /// <summary>How far talking to Julian in the hideout has got (quest 8).</summary>
    public const string JulianHideoutTalk = "v65036c";

    /// <summary>Quest 8: 1 the children are ready to leave the hideout for the beach, 2 they have gone.</summary>
    public const string HideoutLeaving = "v6503a4";

    /// <summary>The children have talked on meeting in the hideout (quest 8).</summary>
    public const string HideoutSceneSeen = "v65108c";

    /// <summary>The answer to Julian's question in the hideout (quest 8): 1 leave now, 2 not yet.</summary>
    public const string HideoutAnswer = "v63be08";

    /// <summary>Dick has tried to push the big stone on his own (quest 8, step 5).</summary>
    public const string StoneTriedAlone = "v650d44";

    /// <summary>How far talking to Anne in the courtyard at night has got (quest 8; 2 once the stone moves).</summary>
    public const string AnneNightTalk = "v650e24";

    /// <summary>How far talking to George in the courtyard at night has got (quest 8).</summary>
    public const string GeorgeNightTalk = "v650dec";

    /// <summary>How far talking to Julian in the courtyard at night has got (quest 8; 2 once he helps push).</summary>
    public const string JulianNightTalk = "v650db4";

    /// <summary>Anne and Julian have talked on arriving in the courtyard at night (quest 8).</summary>
    public const string NightSceneSeen = "v650d7c";

    /// <summary>Where the big stone is drawn while it is pushed (x).</summary>
    public const string BigStoneX = "v63be88";

    /// <summary>Where the big stone is drawn while it is pushed (y).</summary>
    public const string BigStoneY = "v63be8c";

    /// <summary>The big stone moved during the last tick.</summary>
    public const string BigStoneMoving = "v63be90";

    /// <summary>How far talking to George on the beach has got.</summary>
    public const string GeorgeBeachTalk = "v65044c";

    /// <summary>How far Dick's remarks on the island beach have got (quest 8).</summary>
    public const string DickIslandTalk = "v650804";

    /// <summary>On the first landing the children chose to go to the island beach first.</summary>
    public const string ChoseIslandBeach = "v64eff6";

    /// <summary>On the first landing the children chose to go back to the mainland first.</summary>
    public const string ChoseMainland = "v64eff7";

    /// <summary>The barrel on the island beach has been opened.</summary>
    public const string IslandBarrelOpen = "v650724";

    /// <summary>The children have talked on the island beach after the escape (quest 11).</summary>
    public const string EscapeSceneSeen = "v65139c";

    /// <summary>Dick has remarked once on using something on the closed tin.</summary>
    public const string TinRemarked = "v6510c4";

    /// <summary>How far Dick has remarked on using items in the well shaft (quest 8).</summary>
    public const string WellShaftItemTalk = "v650f04";

    /// <summary>How often George has tried to feed Timmy the sandwich while Uncle Quentin watched.</summary>
    public const string TimmyFeedTries = "v65013c";

    /// <summary>The newspaper has been pushed under the study door (quest 7, step 4).</summary>
    public const string NewspaperUnderStudyDoor = "v64f764";

    /// <summary>The study door from the hall: 0 opens, 1 ajar (the professor inside), 2-4 locked (he answers), 5 tried with the empty oil can, 6 oiled.</summary>
    public const string StudyDoorState = "v64f684";

    /// <summary>Set while the hero tries the locked study door (its crack object is switched off).</summary>
    public const string TryingStudyDoor = "v63bdc5";

    /// <summary>The ajar study door has been shown; trying it now makes the professor speak.</summary>
    public const string StudyDoorAjarReady = "v63bdc4";

    /// <summary>Set while Anne is being talked to in the hall (her object is switched off).</summary>
    public const string TalkingToAnneInHall = "v63bdc6";

    /// <summary>Set while Julian is being talked to in the hall.</summary>
    public const string TalkingToJulianInHall = "v63bdc7";

    /// <summary>Set while Dick is being talked to in the hall.</summary>
    public const string TalkingToDickInHall = "v63bdc8";

    /// <summary>The hero has just looked through the guest-room keyhole (quest 5, step 1).</summary>
    public const string KeyholeLookedThrough = "v63bdc9";

    /// <summary>George has said the guest-room door is locked (quest 5, step 1): the keyhole can be looked at.</summary>
    public const string GuestRoomDoorTried = "v63bdca";

    /// <summary>The guest-room key has been pushed out onto the newspaper; the door can be opened.</summary>
    public const string KeyPushedOut = "v63bdcb";

    /// <summary>The key has been seen in the guest-room lock.</summary>
    public const string KeySeenInLock = "v64f7d4";

    /// <summary>How far the plan to get the guest-room key has got: 1 talked about, 2 the newspaper is under the door.</summary>
    public const string GuestRoomKeyPlan = "v64f80c";

    /// <summary>How far talking to Anne in the hall has got (quest 5; above 6 after the suitcase).</summary>
    public const string AnneHallTalk = "v64f994";

    /// <summary>How far talking to Julian in the hall has got (quest 5).</summary>
    public const string JulianHallTalk = "v64f9cc";

    /// <summary>How far talking to Dick in the hall has got (quest 5).</summary>
    public const string DickHallTalk = "v64fa04";

    /// <summary>Uncle Quentin's suitcase in the hall: 0 shut, 1 opened, 2 lying down.</summary>
    public const string SuitcaseState = "v64f72c";

    /// <summary>The study key has been pushed out of the lock onto the newspaper (quest 7, step 4).</summary>
    public const string StudyKeyPushedThrough = "v64f614";

    /// <summary>The study has been entered on quest 7, step 4 (the fire is not lit again).</summary>
    public const string StudyTeaVisit = "v6510fc";

    /// <summary>The children have had breakfast in the kitchen (quest 5).</summary>
    public const string BreakfastOver = "v64f64c";

    /// <summary>The umbrella has been taken from the hall.</summary>
    public const string UmbrellaTaken = "v64f8b4";

    /// <summary>The first plant in the hall has been looked at.</summary>
    public const string HallPlant1Seen = "v64f924";

    /// <summary>The second plant in the hall has been looked at.</summary>
    public const string HallPlant2Seen = "v64f95c";

    /// <summary>The milk churn in the hall has been looked at.</summary>
    public const string MilkChurnSeen = "v64f8ec";

    /// <summary>The coat in the hall has been looked at.</summary>
    public const string HallCoatSeen = "v64f87c";

    /// <summary>The picture in the hall has been looked at.</summary>
    public const string HallPictureSeen = "v64f844";

    /// <summary>The butterfly collection in the hall has been looked at.</summary>
    public const string ButterfliesSeen = "v64f79c";

    /// <summary>The crate hiding the way to the hideout has been pushed aside (quest 2).</summary>
    public const string CratePushed = "v6504bc";

    /// <summary>The footprints on the beach have been examined (quest 2).</summary>
    public const string FootprintsFound = "v6506ec";

    /// <summary>Set while the hero kneels to examine the footprints.</summary>
    public const string ExaminingFootprints = "v63be1c";

    /// <summary>The crate was pushed aside on this visit: the hero already stands by the hideout entrance.</summary>
    public const string CrateJustPushed = "v63be1d";

    /// <summary>Where the crate on the beach is drawn while it is pushed (x).</summary>
    public const string CrateX = "v63be20";

    /// <summary>How far the rowing boat has been pushed out (x offset).</summary>
    public const string BoatX = "v63be24";

    /// <summary>The fisherman's hut has been looked at.</summary>
    public const string HutSeen = "v65052c";

    /// <summary>The hut's window has been looked at.</summary>
    public const string HutWindowSeen = "v650564";

    /// <summary>The bench on the beach has been looked at.</summary>
    public const string BenchSeen = "v65059c";

    /// <summary>The fishing net has been looked at.</summary>
    public const string NetSeen = "v6505d4";

    /// <summary>The sea has been looked at.</summary>
    public const string SeaSeen = "v650644";

    /// <summary>Kirrin Island has been looked at from the beach.</summary>
    public const string IslandSeen = "v65067c";

    /// <summary>How often Anne has stopped the hero on the way down to the beach (quest 8, step 2).</summary>
    public const string AnneAtTheGate = "v650aa4";

    /// <summary>How often Julian has come over at the tower door (quest 8, step 2).</summary>
    public const string JulianAtTheTower = "v650b14";

    /// <summary>How often Dick has come out from between the tower and the house (quest 8, step 2).</summary>
    public const string DickBehindTheHouse = "v650adc";

    /// <summary>The bush in the courtyard: 0 not noticed, 1 Timmy has run into it, 2 cut down with the axe.</summary>
    public const string BushState = "v650b4c";

    /// <summary>The bush in the courtyard has been cut down (it is no longer shown).</summary>
    public const string BushCut = "v65155c";

    /// <summary>The rope has been tied to the stone by the well (quest 8, step 3).</summary>
    public const string RopeTiedOnce = "v650c2c";

    /// <summary>A stone has been thrown at the bush (quest 8, step 3).</summary>
    public const string StonesAtBush = "v650c64";

    /// <summary>The bucket in the courtyard has been taken.</summary>
    public const string CourtyardBucketTaken = "v6509fc";

    /// <summary>Set while the hero takes the bucket in the courtyard.</summary>
    public const string TakingCourtyardBucket = "v63be6f";

    /// <summary>Set while the hero chops at the bush (Dick remarks once the chopping stops).</summary>
    public const string ChoppingBush = "v63be6c";

    /// <summary>Set while the hero ties the rope to the stone (the rope is shown once done).</summary>
    public const string TyingRope = "v63be6d";

    /// <summary>Quest 8, step 4: the talk by the well can start once the hero moves.</summary>
    public const string WellTalkReady = "v63be6e";

    /// <summary>The talk by the well about getting Timmy out has been held (quest 8, step 4).</summary>
    public const string WellTalkDone = "v650a34";

    /// <summary>The children have talked on finding Timmy fallen in the well (quest 8, step 4).</summary>
    public const string TimmyInWellSceneSeen = "v650d0c";

    /// <summary>The children have talked about the big stone (quest 8, step 5).</summary>
    public const string StoneSceneSeen = "v6509c4";

    /// <summary>The children have talked on arriving at the castle (quest 8, step 2).</summary>
    public const string CastleArrivalSeen = "v651524";

    /// <summary>Where the professor is drawn as he walks off (quest 11).</summary>
    public const string ProfessorX = "v63be74";

    /// <summary>How often Dick has said he cannot go out while the professor is watching (quest 11).</summary>
    public const string HidingFromProfessorTalk = "v651444";

    /// <summary>Quest 7, step 4: Uncle Quentin on the window: 1 he will not have it opened with the fire low, 2 opened with the fire high.</summary>
    public const string StudyWindowTalk = "v64f3ac";

    /// <summary>Quest 7, step 4: wood on the study fire: 1 Uncle Quentin objected, 2 it went on.</summary>
    public const string FirewoodTalk = "v64f3e4";

    /// <summary>Quest 7 in the kitchen: how far the plan to get into the study with the tea has got.</summary>
    public const string TeaPlanStage = "v651134";

    /// <summary>Uncle Quentin has once told George not to touch things in the study (quest 7, step 4).</summary>
    public const string QuentinDisturbed = "v64f2cc";

    /// <summary>How far talking to Uncle Quentin in the study has got (quest 2).</summary>
    public const string QuentinStudyTalk = "v64f4fc";

    /// <summary>Uncle Quentin has said his piece and will not talk again (quests 5-6).</summary>
    public const string QuentinWontTalk = "v64f534";

    /// <summary>How far talking to Anne in the study has got (quest 5, step 3).</summary>
    public const string AnneStudyTalk = "v64f56c";

    /// <summary>How far talking to Julian in the study has got (quest 5, step 3).</summary>
    public const string JulianStudyTalk = "v64f5a4";

    /// <summary>How far talking to Dick in the study has got (quest 5, step 3).</summary>
    public const string DickStudyTalk = "v64f5dc";

    /// <summary>Set while Anne is talking in the study.</summary>
    public const string TalkingToAnneInStudy = "v63bdac";

    /// <summary>Set while Julian is talking in the study.</summary>
    public const string TalkingToJulianInStudy = "v63bdae";

    /// <summary>Set while Dick is talking in the study.</summary>
    public const string TalkingToDickInStudy = "v63bdad";

    /// <summary>The professor's telephone call in the study is over (quest 4).</summary>
    public const string ProfessorCallOver = "v63bdaf";

    /// <summary>The professor stands by the telephone and talks with his own animation (quest 4).</summary>
    public const string ProfessorStanding = "v63bdb0";

    /// <summary>The walk path by the study door still has to be opened or closed (once the hero is in the room).</summary>
    public const string StudyPathPending = "v63bdb1";

    /// <summary>The model ship in the study is shown.</summary>
    public const string StudyShipShown = "v64f454";

    /// <summary>Uncle Quentin's cup is shown on the desk.</summary>
    public const string StudyCupShown = "v64f48c";

    /// <summary>The study door record is available (always, from a new game).</summary>
    public const string StudyDoorAvailable = "v64f339";

    /// <summary>The study window record is available (always, from a new game).</summary>
    public const string StudyWindowAvailable = "v64f371";

    /// <summary>The study fire record is available (always, from a new game).</summary>
    public const string StudyFireAvailable = "v64f301";

    /// <summary>The log basket record is available (always, from a new game).</summary>
    public const string StudyWoodAvailable = "v64f419";

    /// <summary>The study telephone record is available (always, from a new game).</summary>
    public const string TelephoneAvailable = "v64f4c1";

    /// <summary>The study telephone receiver is off the hook (the professor is calling).</summary>
    public const string TelephoneOffHook = "v64f4c4";

    /// <summary>Uncle Quentin is shown in the study (always, from a new game).</summary>
    public const string QuentinAvailable = "v64f4f9";

    /// <summary>Uncle Quentin has not yet had his tea and sandwiches (1 in a new game).</summary>
    public const string TeaNotBrought = "v64effa";

    /// <summary>Uncle Quentin has thanked George once for the tea (quest 7, step 4).</summary>
    public const string QuentinTeaTalk = "v65116c";

    /// <summary>Ticks until Uncle Quentin grumbles (in the study on quest 7; in the kitchen, Timmy begs). Each click in the kitchen adds some.</summary>
    public const string GrumbleTimer = "v64d464";

    /// <summary>The answer picked from the kitchen menu: 1 or 2, -1 none.</summary>
    public const string KitchenMenuAnswer = "v64ef24";

    /// <summary>How far George and Uncle Quentin's talk about the teapot has got (quest 3).</summary>
    public const string QuentinTeapotTalk = "v64fc6c";

    /// <summary>The kitchen cupboard with the olive oil is open.</summary>
    public const string Cupboard4Open = "v64fb54";

    /// <summary>The third kitchen cupboard is open.</summary>
    public const string Cupboard3Open = "v64fb1c";

    /// <summary>The low kitchen cupboard is open.</summary>
    public const string Cupboard6Open = "v64fbc4";

    /// <summary>The bread bin in the kitchen is open.</summary>
    public const string BreadBinOpen = "v64fd4c";

    /// <summary>The stove door in the kitchen is open.</summary>
    public const string StoveOpen = "v64fa3c";

    /// <summary>How often Uncle Quentin has stopped George leaving the breakfast table (quest 3).</summary>
    public const string GeorgeLeavingTalk = "v64fbfc";

    /// <summary>How often Uncle Quentin has noticed Timmy begging at the table (quest 3).</summary>
    public const string TimmyBegsTalk = "v6500cc";

    /// <summary>The olive oil has been taken from the kitchen cupboard.</summary>
    public const string OliveOilTaken = "v64fdbc";

    /// <summary>Set while the kitchen puts the olive oil in place (taking it does not count then).</summary>
    public const string PlacingOliveOil = "v63be00";

    /// <summary>Set while the hero takes the plate of sandwiches.</summary>
    public const string TakingToast = "v63bdec";

    /// <summary>Set while the hero takes the teapot.</summary>
    public const string TakingTeapot = "v63bded";

    /// <summary>Uncle Quentin has once noticed Timmy under the table (quest 3).</summary>
    public const string TimmyTableTalk = "v650104";

    /// <summary>The fifth kitchen cupboard has been looked at.</summary>
    public const string Cupboard5Seen = "v64fb8c";

    /// <summary>The first kitchen cupboard has been looked at.</summary>
    public const string Cupboard1Seen = "v64faac";

    /// <summary>The second kitchen cupboard has been looked at (never set: the original sets Cupboard3Open instead).</summary>
    public const string Cupboard2Seen = "v64fae4";

    /// <summary>Timmy's water bowl has been looked at.</summary>
    public const string WaterBowlSeen = "v64ff44";

    /// <summary>The kitchen bin has been looked at.</summary>
    public const string KitchenBinSeen = "v64ff0c";

    /// <summary>The dishcloth has been looked at.</summary>
    public const string DishclothSeen = "v64fed4";

    /// <summary>The fruit bowl has been looked at.</summary>
    public const string FruitBowlSeen = "v64fe64";

    /// <summary>The cups in the kitchen have been looked at.</summary>
    public const string CupsSeen = "v64fe2c";

    /// <summary>The vinegar bottles have been looked at.</summary>
    public const string VinegarSeen = "v64fd84";

    /// <summary>The cookbooks have been looked at.</summary>
    public const string CookbooksSeen = "v64fd14";

    /// <summary>The kitchen sink has been looked at.</summary>
    public const string SinkSeen = "v64fcdc";

    /// <summary>The kitchen window has been looked at.</summary>
    public const string KitchenWindowSeen = "v64fca4";

    /// <summary>The plate rack has been looked at.</summary>
    public const string PlateRackSeen = "v64fa74";

    /// <summary>Aunt Fanny can be in the kitchen (always, from a new game).</summary>
    public const string FannyKitchenAvailable = "v64ff79";

    /// <summary>The plate of sandwiches record is available (always, from a new game).</summary>
    public const string ToastAvailable = "v64fdf1";

    /// <summary>The bread bin record is available (always, from a new game).</summary>
    public const string BreadBinAvailable = "v64fd49";

    /// <summary>The teapot record is available (always, from a new game).</summary>
    public const string TeapotAvailable = "v64fc31";

    /// <summary>The kitchen door record is available (always, from a new game).</summary>
    public const string KitchenDoorAvailable = "v64fbf9";

    /// <summary>The low cupboard record is available (always, from a new game).</summary>
    public const string Cupboard6Available = "v64fbc1";

    /// <summary>The olive-oil cupboard record is available (always, from a new game).</summary>
    public const string Cupboard4Available = "v64fb51";

    /// <summary>The third cupboard record is available (always, from a new game).</summary>
    public const string Cupboard3Available = "v64fb19";

    /// <summary>The stove record is available (always, from a new game).</summary>
    public const string StoveAvailable = "v64fa39";

    /// <summary>The original's pointer to a menu entry (kept as the original sets it; OpenFive does not read it).</summary>
    public const string OriginalMenuEntry = "v64ee04";

    /// <summary>The original's menu-entry flag (kept as the original sets it; OpenFive does not read it).</summary>
    public const string OriginalMenuEntryShown = "v64e2f9";

    /// <summary>The courtyard menu's first row (see StoryScript's menu helpers).</summary>
    public const string CourtyardMenuSlot1 = "v64efa4";

    /// <summary>The courtyard menu's second row.</summary>
    public const string CourtyardMenuSlot2 = "v64efa8";

    /// <summary>The courtyard menu's third row.</summary>
    public const string CourtyardMenuSlot3 = "v64efac";

    /// <summary>The value of an empty row on the courtyard menu.</summary>
    public const string CourtyardMenuNone = "v64efb4";

    /// <summary>A flag the original sets and clears on arriving in some places (the island, the wreck, the dungeon, the kitchen); kept as the original sets it, OpenFive does not read it.</summary>
    public const string OriginalPlaceFlag = "v683260";

    /// <summary>1 from a new game until the children have talked about where to go on first landing on the island (quest 6, step 3).</summary>
    public const string FirstLandingTalk = "v64d468";
}
