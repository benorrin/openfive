using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenFive.Platform;

/// <summary>Asks for the game's disc image with the desktop's file dialog.</summary>
internal static unsafe class DiscPicker
{
    static volatile bool _done;
    static string? _picked;

    /// <summary>The chosen file, or null if the player cancelled or there is no file dialog.</summary>
    public static string? Ask()
    {
        try
        {
            if (!Sdl.Init(Sdl.INIT_VIDEO))
                return null;
        }
        catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException)
        {
            return null;
        }
        Sdl.ShowSimpleMessageBox(Sdl.MESSAGEBOX_INFORMATION, "OpenFive",
            "OpenFive needs your Famous Five: Treasure Island CD.\n\n" +
            "Choose your disc image (a .cue, .ccd, .iso, .img or .bin file) in the next window. " +
            "If you have the CD itself, cancel, insert the CD and start OpenFive again.\n\n" +
            "The disc is remembered, so this is needed only once.", IntPtr.Zero);
        IntPtr name = Marshal.StringToCoTaskMemUTF8("Disc images");
        IntPtr pattern = Marshal.StringToCoTaskMemUTF8("cue;ccd;iso;img;bin");
        try
        {
            var filter = new Sdl.DialogFileFilter { Name = name, Pattern = pattern };
            _done = false;
            _picked = null;
            Sdl.ShowOpenFileDialog(&Picked, IntPtr.Zero, IntPtr.Zero, &filter, 1, IntPtr.Zero, false);
            // The answer arrives in Picked, on this thread or another, while events are pumped.
            while (!_done)
            {
                Sdl.PumpEvents();
                Sdl.Delay(10);
            }
            return _picked;
        }
        finally
        {
            Marshal.FreeCoTaskMem(name);
            Marshal.FreeCoTaskMem(pattern);
            Sdl.QuitSubSystem(Sdl.INIT_VIDEO);
        }
    }

    /// <summary>The dialog's answer: a null list on error, an empty list when cancelled.</summary>
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static void Picked(IntPtr userdata, IntPtr files, int filter)
    {
        if (files != IntPtr.Zero)
        {
            IntPtr first = Marshal.ReadIntPtr(files);
            if (first != IntPtr.Zero)
                _picked = Marshal.PtrToStringUTF8(first);
        }
        _done = true;
    }
}
