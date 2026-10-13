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
}
