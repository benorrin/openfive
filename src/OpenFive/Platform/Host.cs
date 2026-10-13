using OpenFive.Engine.Graphics;

namespace OpenFive.Platform;

/// <summary>Input state handed to the game each frame (mouse in 800x600 game coordinates).</summary>
public sealed class InputState
{
    public int MouseX, MouseY;
    public bool MouseInside;
    public bool ButtonDown;
    public bool Clicked;
    public bool RightClicked;
    public readonly Queue<int> KeysPressed = new();
    public bool QuitRequested;
}

/// <summary>Window, renderer and event handling on top of SDL3.</summary>
public sealed unsafe class Host : IDisposable
{
    IntPtr _window, _renderer, _texture;
    bool _fullscreen;
    bool _smooth = true;
    bool _pointerHidden;
    public InputState Input { get; } = new();
    static volatile bool s_signalled;
    readonly List<System.Runtime.InteropServices.PosixSignalRegistration> _signals = new();

    public Host(bool fullscreen, bool smooth)
    {
        _smooth = smooth;
        if (!Sdl.Init(Sdl.INIT_VIDEO | Sdl.INIT_EVENTS | Sdl.INIT_AUDIO))
            throw new InvalidOperationException("SDL_Init failed: " + Sdl.GetError());
        ulong flags = Sdl.WINDOW_RESIZABLE | Sdl.WINDOW_HIGH_PIXEL_DENSITY | (fullscreen ? Sdl.WINDOW_FULLSCREEN : 0);
        _window = Sdl.CreateWindow("OpenFive - The Famous Five: Treasure Island", 1200, 900, flags);
        if (_window == IntPtr.Zero)
            throw new InvalidOperationException("SDL_CreateWindow failed: " + Sdl.GetError());
        _fullscreen = fullscreen;
        Sdl.SetWindowMinimumSize(_window, 400, 300);
        _renderer = Sdl.CreateRenderer(_window, IntPtr.Zero);
        if (_renderer == IntPtr.Zero)
            throw new InvalidOperationException("SDL_CreateRenderer failed: " + Sdl.GetError());
        Sdl.SetRenderVSync(_renderer, 1);
        Sdl.SetRenderLogicalPresentation(_renderer, Framebuffer.Width, Framebuffer.Height, Sdl.LOGICAL_PRESENTATION_LETTERBOX);
        // Ctrl+C / termination: leave the main loop and shut audio and video down in order.
        foreach (var sig in new[] { System.Runtime.InteropServices.PosixSignal.SIGINT, System.Runtime.InteropServices.PosixSignal.SIGTERM })
            _signals.Add(System.Runtime.InteropServices.PosixSignalRegistration.Create(sig, ctx =>
            {
                ctx.Cancel = true;
                s_signalled = true;
            }));
        _texture = Sdl.CreateTexture(_renderer, Sdl.PIXELFORMAT_RGB565, Sdl.TEXTUREACCESS_STREAMING, Framebuffer.Width, Framebuffer.Height);
        ApplyScaleMode();
    }

    void ApplyScaleMode() => Sdl.SetTextureScaleMode(_texture, _smooth ? Sdl.SCALEMODE_LINEAR : Sdl.SCALEMODE_NEAREST);

    public void ToggleFullscreen()
    {
        _fullscreen = !_fullscreen;
        Sdl.SetWindowFullscreen(_window, _fullscreen);
    }

    public bool HasFocus => (Sdl.GetWindowFlags(_window) & Sdl.WINDOW_INPUT_FOCUS) != 0;

    /// <summary>Processes pending window events into <see cref="Input"/>.</summary>
    public void PumpEvents()
    {
        Input.Clicked = false;
        Input.RightClicked = false;
        if (s_signalled)
            Input.QuitRequested = true;
        while (Sdl.PollEvent(out var e))
        {
            switch (e.Type)
            {
                case Sdl.EVENT_QUIT:
                    Input.QuitRequested = true;
                    break;
                case Sdl.EVENT_MOUSE_MOTION:
                    UpdateMouse(e.X, e.Y);
                    break;
                case Sdl.EVENT_MOUSE_BUTTON_DOWN:
                case Sdl.EVENT_MOUSE_BUTTON_UP:
                    UpdateMouse(e.X, e.Y);
                    if (e.Button is Sdl.BUTTON_LEFT or Sdl.BUTTON_RIGHT)
                    {
                        bool down = e.Type == Sdl.EVENT_MOUSE_BUTTON_DOWN;
                        if (down && !Input.ButtonDown)
                        {
                            Input.Clicked = true;
                            Input.RightClicked = e.Button == Sdl.BUTTON_RIGHT;
                        }
                        Input.ButtonDown = down;
                    }
                    break;
                case Sdl.EVENT_KEY_DOWN:
                    if (e.KeyRepeat != 0)
                        break;
                    // F11 / Alt+Enter: fullscreen, F12: smooth/sharp scaling (handled here).
                    if (e.Scancode == Keys.F11 || (e.Scancode == Keys.Return && (e.Mod & 0x300) != 0))
                        ToggleFullscreen();
                    else if (e.Scancode == Keys.F12)
                    {
                        _smooth = !_smooth;
                        ApplyScaleMode();
                    }
                    else
                        Input.KeysPressed.Enqueue(e.Scancode);
                    break;
            }
        }
    }

    void UpdateMouse(float wx, float wy)
    {
        Sdl.RenderCoordinatesFromWindow(_renderer, wx, wy, out float gx, out float gy);
        Input.MouseX = (int)MathF.Round(gx);
        Input.MouseY = (int)MathF.Round(gy);
        Input.MouseInside = gx >= 0 && gy >= 0 && gx < Framebuffer.Width && gy < Framebuffer.Height;
    }

    /// <summary>Shows a frame; the system pointer is hidden while it is over the game picture (the game draws its own).</summary>
    public void Present(Framebuffer fb, bool hideSystemPointer = false)
    {
        if (hideSystemPointer != _pointerHidden)
        {
            _pointerHidden = hideSystemPointer;
            if (hideSystemPointer) Sdl.HideCursor(); else Sdl.ShowCursor();
        }
        fixed (ushort* p = fb.Pixels)
            Sdl.UpdateTexture(_texture, IntPtr.Zero, p, Framebuffer.Width * 2);
        Sdl.SetRenderDrawColor(_renderer, 0, 0, 0, 255);
        Sdl.RenderClear(_renderer);
        Sdl.RenderTexture(_renderer, _texture, null, null);
        Sdl.RenderPresent(_renderer);
    }

    public void Dispose()
    {
        foreach (var r in _signals)
            r.Dispose();
        if (_texture != IntPtr.Zero) Sdl.DestroyTexture(_texture);
        if (_renderer != IntPtr.Zero) Sdl.DestroyRenderer(_renderer);
        if (_window != IntPtr.Zero) Sdl.DestroyWindow(_window);
        Sdl.Quit();
    }
}

/// <summary>SDL3 scancodes used by the game.</summary>
public static class Keys
{
    public const int A = 4, Return = 40, Escape = 41, Backspace = 42, Tab = 43, Space = 44,
        F1 = 58, F2 = 59, F11 = 68, F12 = 69, Right = 79, Left = 80, Down = 81, Up = 82;
}
