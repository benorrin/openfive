using System.Runtime.InteropServices;
using OpenFive.Engine.Graphics;

namespace OpenFive.Platform;

/// <summary>Input state handed to the game each frame (mouse in 800x600 game coordinates).</summary>
public sealed class InputState
{
    /// <summary>The pointer in game coordinates (may lie outside the picture).</summary>
    public int MouseX, MouseY;

    /// <summary>Is the pointer over the game picture?</summary>
    public bool MouseInside;

    /// <summary>Is a mouse button held down?</summary>
    public bool ButtonDown;

    /// <summary>A button was pressed since the last <see cref="Host.PumpEvents"/>.</summary>
    public bool Clicked;

    /// <summary>Keys pressed (SDL scancodes), oldest first.</summary>
    public readonly Queue<int> KeysPressed = new();

    /// <summary>The window was closed or the process was asked to stop.</summary>
    public bool QuitRequested;
}

/// <summary>Window, renderer and event handling on top of SDL3.</summary>
public sealed unsafe class Host : IDisposable
{
    static volatile bool s_signalled;

    readonly List<PosixSignalRegistration> _signals = new();
    readonly IntPtr _window, _renderer, _texture;
    bool _fullscreen;
    bool _smooth;
    bool _pointerHidden;

    /// <summary>
    /// Opens a resizable window (or full screen) showing the 800x600 picture letterboxed, scaled
    /// smoothly or with sharp pixels.
    /// </summary>
    /// <exception cref="InvalidOperationException">SDL could not create the window.</exception>
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
        foreach (var sig in new[] { PosixSignal.SIGINT, PosixSignal.SIGTERM })
            _signals.Add(PosixSignalRegistration.Create(sig, ctx =>
            {
                ctx.Cancel = true;
                s_signalled = true;
            }));
        _texture = Sdl.CreateTexture(_renderer, Sdl.PIXELFORMAT_RGB565, Sdl.TEXTUREACCESS_STREAMING, Framebuffer.Width, Framebuffer.Height);
        ApplyScaleMode();
    }

    /// <summary>The input gathered by <see cref="PumpEvents"/>.</summary>
    public InputState Input { get; } = new();

    /// <summary>Does the window have the keyboard focus?</summary>
    public bool HasFocus => (Sdl.GetWindowFlags(_window) & Sdl.WINDOW_INPUT_FOCUS) != 0;

    void ApplyScaleMode() => Sdl.SetTextureScaleMode(_texture, _smooth ? Sdl.SCALEMODE_LINEAR : Sdl.SCALEMODE_NEAREST);

    /// <summary>Switches between window and full screen.</summary>
    public void ToggleFullscreen()
    {
        _fullscreen = !_fullscreen;
        Sdl.SetWindowFullscreen(_window, _fullscreen);
    }

    /// <summary>Chooses smooth (true) or sharp scaling of the picture.</summary>
    public void SetSmooth(bool smooth)
    {
        if (smooth == _smooth)
            return;
        _smooth = smooth;
        ApplyScaleMode();
    }

    /// <summary>Moves the system mouse to a point of the game picture (only while the window has focus).</summary>
    public void WarpMouse(int gx, int gy)
    {
        if (!HasFocus)
            return;
        Sdl.RenderCoordinatesToWindow(_renderer, gx, gy, out float wx, out float wy);
        Sdl.WarpMouseInWindow(_window, wx, wy);
    }

    /// <summary>Processes pending window events into <see cref="Input"/>.</summary>
    public void PumpEvents()
    {
        Input.Clicked = false;
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
                            Input.Clicked = true;
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
                        SetSmooth(!_smooth);
                    else
                        Input.KeysPressed.Enqueue(e.Scancode);
                    break;
            }
        }
    }

    /// <summary>Converts a window position into game coordinates.</summary>
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
            if (hideSystemPointer)
                Sdl.HideCursor();
            else
                Sdl.ShowCursor();
        }
        fixed (ushort* p = fb.Pixels)
            Sdl.UpdateTexture(_texture, IntPtr.Zero, p, Framebuffer.Width * 2);
        Sdl.SetRenderDrawColor(_renderer, 0, 0, 0, 255);
        Sdl.RenderClear(_renderer);
        Sdl.RenderTexture(_renderer, _texture, null, null);
        Sdl.RenderPresent(_renderer);
    }

    /// <summary>Closes the window and shuts SDL down.</summary>
    public void Dispose()
    {
        foreach (var r in _signals)
            r.Dispose();
        if (_texture != IntPtr.Zero)
            Sdl.DestroyTexture(_texture);
        if (_renderer != IntPtr.Zero)
            Sdl.DestroyRenderer(_renderer);
        if (_window != IntPtr.Zero)
            Sdl.DestroyWindow(_window);
        Sdl.Quit();
    }
}

/// <summary>SDL3 scancodes used by the game.</summary>
public static class Keys
{
    /// <summary>The letters A to Z (consecutive).</summary>
    public const int A = 4, Z = 29;

    /// <summary>The digits 1 to 9 (consecutive), then 0.</summary>
    public const int Digit1 = 30, Digit9 = 38, Digit0 = 39;

    /// <summary>Editing keys.</summary>
    public const int Return = 40, Escape = 41, Backspace = 42, Space = 44;

    /// <summary>Function keys: F1 menu, F11 full screen, F12 scaling.</summary>
    public const int F1 = 58, F11 = 68, F12 = 69;
}
