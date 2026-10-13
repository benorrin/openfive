namespace OpenFive.Engine;

/// <summary>Diagnostic output for story debugging, enabled by setting OPENFIVE_TRACE.</summary>
public static class Trace
{
    public static readonly bool On = Environment.GetEnvironmentVariable("OPENFIVE_TRACE") is { Length: > 0 };

    public static void Log(string message)
    {
        if (On)
            Console.Error.WriteLine("trace: " + message);
    }
}
