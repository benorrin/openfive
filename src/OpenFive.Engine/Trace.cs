namespace OpenFive.Engine;

/// <summary>Diagnostic output for story debugging, enabled by setting OPENFIVE_TRACE.</summary>
public static class Trace
{
    /// <summary>True when OPENFIVE_TRACE is set (to anything).</summary>
    public static readonly bool On = Environment.GetEnvironmentVariable("OPENFIVE_TRACE") is { Length: > 0 };

    /// <summary>Writes a trace line to standard error (only when tracing is on).</summary>
    public static void Log(string message)
    {
        if (On)
            Console.Error.WriteLine("trace: " + message);
    }
}
