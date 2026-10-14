using System.Reflection;
using System.Runtime.InteropServices;

namespace OpenFive.Platform;

/// <summary>Minimal hand-written bindings to the parts of SDL3 the game uses.</summary>
internal static unsafe class Sdl
{
    const string Lib = "SDL3";

    static Sdl()
    {
        NativeLibrary.SetDllImportResolver(typeof(Sdl).Assembly, Resolve);
    }

    static IntPtr Resolve(string name, Assembly asm, DllImportSearchPath? path)
    {
        if (name != Lib)
            return IntPtr.Zero;
        string[] candidates = OperatingSystem.IsWindows() ? new[] { "SDL3.dll" }
            : OperatingSystem.IsMacOS() ? new[] { "libSDL3.0.dylib", "libSDL3.dylib" }
            : new[] { "libSDL3.so.0", "libSDL3.so" };
        foreach (string c in candidates)
        {
            if (NativeLibrary.TryLoad(c, asm, path, out IntPtr h))
                return h;
            if (NativeLibrary.TryLoad(c, out h))
                return h;
        }
        return IntPtr.Zero;
    }

    public const uint INIT_AUDIO = 0x10, INIT_VIDEO = 0x20, INIT_EVENTS = 0x4000;
    public const ulong WINDOW_FULLSCREEN = 0x1, WINDOW_RESIZABLE = 0x20, WINDOW_INPUT_FOCUS = 0x200,
        WINDOW_MOUSE_FOCUS = 0x400, WINDOW_HIGH_PIXEL_DENSITY = 0x2000;
    public const uint PIXELFORMAT_RGB565 = 0x15151002, PIXELFORMAT_ARGB8888 = 0x16362004;
    public const int TEXTUREACCESS_STREAMING = 1;
    public const int SCALEMODE_NEAREST = 0, SCALEMODE_LINEAR = 1;
    public const int LOGICAL_PRESENTATION_LETTERBOX = 2;
    public const uint EVENT_QUIT = 0x100, EVENT_WINDOW_FOCUS_GAINED = 0x20E, EVENT_WINDOW_FOCUS_LOST = 0x20F,
        EVENT_KEY_DOWN = 0x300, EVENT_KEY_UP = 0x301,
        EVENT_MOUSE_MOTION = 0x400, EVENT_MOUSE_BUTTON_DOWN = 0x401, EVENT_MOUSE_BUTTON_UP = 0x402;
    public const ushort AUDIO_F32 = 0x8120;
    public const uint AUDIO_DEVICE_DEFAULT_PLAYBACK = 0xFFFFFFFF;
    public const byte BUTTON_LEFT = 1, BUTTON_RIGHT = 3;

    [StructLayout(LayoutKind.Explicit, Size = 128)]
    public struct Event
    {
        [FieldOffset(0)] public uint Type;
        // keyboard
        [FieldOffset(24)] public int Scancode;
        [FieldOffset(28)] public uint Key;
        [FieldOffset(32)] public ushort Mod;
        [FieldOffset(36)] public byte KeyDown;
        [FieldOffset(37)] public byte KeyRepeat;
        // mouse motion: x/y at 28/32; mouse button: button at 24, x/y at 28/32
        [FieldOffset(24)] public byte Button;
        [FieldOffset(28)] public float X;
        [FieldOffset(32)] public float Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AudioSpec
    {
        public int Format;
        public int Channels;
        public int Freq;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FRect
    {
        public float X, Y, W, H;
    }

    [DllImport(Lib, EntryPoint = "SDL_Init")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool Init(uint flags);
    [DllImport(Lib, EntryPoint = "SDL_Quit")] public static extern void Quit();
    [DllImport(Lib, EntryPoint = "SDL_GetError")] static extern IntPtr GetErrorPtr();
    public static string GetError() => Marshal.PtrToStringUTF8(GetErrorPtr()) ?? "";
    [DllImport(Lib, EntryPoint = "SDL_SetHint")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool SetHint([MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
    [DllImport(Lib, EntryPoint = "SDL_GetTicks")] public static extern ulong GetTicks();
    [DllImport(Lib, EntryPoint = "SDL_Delay")] public static extern void Delay(uint ms);

    public const uint MESSAGEBOX_ERROR = 0x10, MESSAGEBOX_INFORMATION = 0x40;

    [DllImport(Lib, EntryPoint = "SDL_ShowSimpleMessageBox")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool ShowSimpleMessageBox(uint flags, [MarshalAs(UnmanagedType.LPUTF8Str)] string title,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message, IntPtr window);

    [DllImport(Lib, EntryPoint = "SDL_CreateWindow")]
    public static extern IntPtr CreateWindow([MarshalAs(UnmanagedType.LPUTF8Str)] string title, int w, int h, ulong flags);
    [DllImport(Lib, EntryPoint = "SDL_DestroyWindow")] public static extern void DestroyWindow(IntPtr window);
    [DllImport(Lib, EntryPoint = "SDL_GetWindowFlags")] public static extern ulong GetWindowFlags(IntPtr window);
    [DllImport(Lib, EntryPoint = "SDL_SetWindowFullscreen")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool SetWindowFullscreen(IntPtr window, [MarshalAs(UnmanagedType.U1)] bool fullscreen);
    [DllImport(Lib, EntryPoint = "SDL_SetWindowMinimumSize")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool SetWindowMinimumSize(IntPtr window, int w, int h);

    [DllImport(Lib, EntryPoint = "SDL_CreateRenderer")]
    public static extern IntPtr CreateRenderer(IntPtr window, IntPtr name);
    [DllImport(Lib, EntryPoint = "SDL_DestroyRenderer")] public static extern void DestroyRenderer(IntPtr renderer);
    [DllImport(Lib, EntryPoint = "SDL_SetRenderVSync")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool SetRenderVSync(IntPtr renderer, int vsync);
    [DllImport(Lib, EntryPoint = "SDL_SetRenderLogicalPresentation")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool SetRenderLogicalPresentation(IntPtr renderer, int w, int h, int mode);
    [DllImport(Lib, EntryPoint = "SDL_RenderCoordinatesFromWindow")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool RenderCoordinatesFromWindow(IntPtr renderer, float wx, float wy, out float x, out float y);
    [DllImport(Lib, EntryPoint = "SDL_RenderCoordinatesToWindow")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool RenderCoordinatesToWindow(IntPtr renderer, float x, float y, out float wx, out float wy);
    [DllImport(Lib, EntryPoint = "SDL_SetRenderDrawColor")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool SetRenderDrawColor(IntPtr renderer, byte r, byte g, byte b, byte a);
    [DllImport(Lib, EntryPoint = "SDL_RenderClear")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool RenderClear(IntPtr renderer);
    [DllImport(Lib, EntryPoint = "SDL_RenderTexture")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool RenderTexture(IntPtr renderer, IntPtr texture, FRect* src, FRect* dst);
    [DllImport(Lib, EntryPoint = "SDL_RenderPresent")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool RenderPresent(IntPtr renderer);

    [DllImport(Lib, EntryPoint = "SDL_CreateTexture")]
    public static extern IntPtr CreateTexture(IntPtr renderer, uint format, int access, int w, int h);
    [DllImport(Lib, EntryPoint = "SDL_DestroyTexture")] public static extern void DestroyTexture(IntPtr texture);
    [DllImport(Lib, EntryPoint = "SDL_UpdateTexture")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool UpdateTexture(IntPtr texture, IntPtr rect, void* pixels, int pitch);
    [DllImport(Lib, EntryPoint = "SDL_SetTextureScaleMode")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool SetTextureScaleMode(IntPtr texture, int mode);

    [DllImport(Lib, EntryPoint = "SDL_PollEvent")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool PollEvent(out Event e);

    [DllImport(Lib, EntryPoint = "SDL_CreateSurfaceFrom")]
    public static extern IntPtr CreateSurfaceFrom(int w, int h, uint format, void* pixels, int pitch);
    [DllImport(Lib, EntryPoint = "SDL_ScaleSurface")]
    public static extern IntPtr ScaleSurface(IntPtr surface, int w, int h, int mode);
    [DllImport(Lib, EntryPoint = "SDL_DestroySurface")] public static extern void DestroySurface(IntPtr surface);
    [DllImport(Lib, EntryPoint = "SDL_CreateColorCursor")]
    public static extern IntPtr CreateColorCursor(IntPtr surface, int hotX, int hotY);
    [DllImport(Lib, EntryPoint = "SDL_SetCursor")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool SetCursor(IntPtr cursor);
    [DllImport(Lib, EntryPoint = "SDL_DestroyCursor")] public static extern void DestroyCursor(IntPtr cursor);
    [DllImport(Lib, EntryPoint = "SDL_ShowCursor")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool ShowCursor();
    [DllImport(Lib, EntryPoint = "SDL_HideCursor")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool HideCursor();
    [DllImport(Lib, EntryPoint = "SDL_WarpMouseInWindow")]
    public static extern void WarpMouseInWindow(IntPtr window, float x, float y);
    [DllImport(Lib, EntryPoint = "SDL_GetMouseState")]
    public static extern uint GetMouseState(out float x, out float y);

    [DllImport(Lib, EntryPoint = "SDL_OpenAudioDeviceStream")]
    public static extern IntPtr OpenAudioDeviceStream(uint device, ref AudioSpec spec,
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, int, void> callback, IntPtr userdata);
    [DllImport(Lib, EntryPoint = "SDL_ResumeAudioStreamDevice")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool ResumeAudioStreamDevice(IntPtr stream);
    [DllImport(Lib, EntryPoint = "SDL_PauseAudioStreamDevice")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool PauseAudioStreamDevice(IntPtr stream);
    [DllImport(Lib, EntryPoint = "SDL_PutAudioStreamData")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool PutAudioStreamData(IntPtr stream, void* buf, int len);
    [DllImport(Lib, EntryPoint = "SDL_DestroyAudioStream")] public static extern void DestroyAudioStream(IntPtr stream);
}
