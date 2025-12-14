#pragma once

#ifdef INPUTHOOK_EXPORTS
#define INPUTHOOK_API __declspec(dllexport)
#else
#define INPUTHOOK_API __declspec(dllimport)
#endif

// Callback function types for C# P/Invoke
typedef void(__stdcall* MouseMoveCallback)(int x, int y, double timestamp);
typedef void(__stdcall* MouseButtonCallback)(int button, bool isDown, int x, int y, double timestamp);
typedef void(__stdcall* KeyboardCallback)(int keyCode, bool isDown, double timestamp);
typedef void(__stdcall* MouseWheelCallback)(int delta, int x, int y, double timestamp);

extern "C" {
    /// <summary>
    /// Starts the low-level mouse hook
    /// </summary>
    INPUTHOOK_API bool StartMouseHook(MouseMoveCallback moveCallback, MouseButtonCallback buttonCallback, MouseWheelCallback wheelCallback);

    /// <summary>
    /// Starts the low-level keyboard hook
    /// </summary>
    INPUTHOOK_API bool StartKeyboardHook(KeyboardCallback keyCallback);

    /// <summary>
    /// Stops all hooks
    /// </summary>
    INPUTHOOK_API void StopHooks();

    /// <summary>
    /// Checks if hooks are currently active
    /// </summary>
    INPUTHOOK_API bool IsHookActive();

    /// <summary>
    /// Gets the current timestamp in milliseconds (high precision)
    /// </summary>
    INPUTHOOK_API double GetTimestamp();
}
