#include "InputHook.h"
#include <Windows.h>
#include <chrono>

// Global variables
static HHOOK g_mouseHook = NULL;
static HHOOK g_keyboardHook = NULL;
static MouseMoveCallback g_mouseMoveCallback = nullptr;
static MouseButtonCallback g_mouseButtonCallback = nullptr;
static MouseWheelCallback g_mouseWheelCallback = nullptr;
static KeyboardCallback g_keyboardCallback = nullptr;
static std::chrono::high_resolution_clock::time_point g_startTime;
static bool g_isInitialized = false;

// Initialize high-resolution timer
void InitializeTimer()
{
    if (!g_isInitialized)
    {
        g_startTime = std::chrono::high_resolution_clock::now();
        g_isInitialized = true;
    }
}

// Get current timestamp in milliseconds
double GetCurrentTimestamp()
{
    auto now = std::chrono::high_resolution_clock::now();
    auto duration = std::chrono::duration_cast<std::chrono::microseconds>(now - g_startTime);
    return duration.count() / 1000.0; // Convert to milliseconds
}

// Low-level mouse hook procedure
LRESULT CALLBACK LowLevelMouseProc(int nCode, WPARAM wParam, LPARAM lParam)
{
    if (nCode >= 0)
    {
        MSLLHOOKSTRUCT* mouseStruct = (MSLLHOOKSTRUCT*)lParam;
        double timestamp = GetCurrentTimestamp();

        switch (wParam)
        {
        case WM_MOUSEMOVE:
            if (g_mouseMoveCallback)
                g_mouseMoveCallback(mouseStruct->pt.x, mouseStruct->pt.y, timestamp);
            break;

        case WM_LBUTTONDOWN:
            if (g_mouseButtonCallback)
                g_mouseButtonCallback(0, true, mouseStruct->pt.x, mouseStruct->pt.y, timestamp);
            break;

        case WM_LBUTTONUP:
            if (g_mouseButtonCallback)
                g_mouseButtonCallback(0, false, mouseStruct->pt.x, mouseStruct->pt.y, timestamp);
            break;

        case WM_RBUTTONDOWN:
            if (g_mouseButtonCallback)
                g_mouseButtonCallback(1, true, mouseStruct->pt.x, mouseStruct->pt.y, timestamp);
            break;

        case WM_RBUTTONUP:
            if (g_mouseButtonCallback)
                g_mouseButtonCallback(1, false, mouseStruct->pt.x, mouseStruct->pt.y, timestamp);
            break;

        case WM_MBUTTONDOWN:
            if (g_mouseButtonCallback)
                g_mouseButtonCallback(2, true, mouseStruct->pt.x, mouseStruct->pt.y, timestamp);
            break;

        case WM_MBUTTONUP:
            if (g_mouseButtonCallback)
                g_mouseButtonCallback(2, false, mouseStruct->pt.x, mouseStruct->pt.y, timestamp);
            break;

        case WM_MOUSEWHEEL:
            if (g_mouseWheelCallback)
            {
                int delta = GET_WHEEL_DELTA_WPARAM(mouseStruct->mouseData);
                g_mouseWheelCallback(delta, mouseStruct->pt.x, mouseStruct->pt.y, timestamp);
            }
            break;
        }
    }

    return CallNextHookEx(g_mouseHook, nCode, wParam, lParam);
}

// Low-level keyboard hook procedure
LRESULT CALLBACK LowLevelKeyboardProc(int nCode, WPARAM wParam, LPARAM lParam)
{
    if (nCode >= 0)
    {
        KBDLLHOOKSTRUCT* keyboardStruct = (KBDLLHOOKSTRUCT*)lParam;
        double timestamp = GetCurrentTimestamp();

        if (g_keyboardCallback)
        {
            bool isDown = (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN);
            g_keyboardCallback(keyboardStruct->vkCode, isDown, timestamp);
        }
    }

    return CallNextHookEx(g_keyboardHook, nCode, wParam, lParam);
}

// Exported functions
extern "C"
{
    INPUTHOOK_API bool StartMouseHook(MouseMoveCallback moveCallback, MouseButtonCallback buttonCallback, MouseWheelCallback wheelCallback)
    {
        InitializeTimer();

        if (g_mouseHook != NULL)
            return false; // Already hooked

        g_mouseMoveCallback = moveCallback;
        g_mouseButtonCallback = buttonCallback;
        g_mouseWheelCallback = wheelCallback;

        g_mouseHook = SetWindowsHookEx(WH_MOUSE_LL, LowLevelMouseProc, GetModuleHandle(NULL), 0);
        return g_mouseHook != NULL;
    }

    INPUTHOOK_API bool StartKeyboardHook(KeyboardCallback keyCallback)
    {
        InitializeTimer();

        if (g_keyboardHook != NULL)
            return false; // Already hooked

        g_keyboardCallback = keyCallback;

        g_keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc, GetModuleHandle(NULL), 0);
        return g_keyboardHook != NULL;
    }

    INPUTHOOK_API void StopHooks()
    {
        if (g_mouseHook != NULL)
        {
            UnhookWindowsHookEx(g_mouseHook);
            g_mouseHook = NULL;
        }

        if (g_keyboardHook != NULL)
        {
            UnhookWindowsHookEx(g_keyboardHook);
            g_keyboardHook = NULL;
        }

        g_mouseMoveCallback = nullptr;
        g_mouseButtonCallback = nullptr;
        g_mouseWheelCallback = nullptr;
        g_keyboardCallback = nullptr;
    }

    INPUTHOOK_API bool IsHookActive()
    {
        return g_mouseHook != NULL || g_keyboardHook != NULL;
    }

    INPUTHOOK_API double GetTimestamp()
    {
        return GetCurrentTimestamp();
    }
}
