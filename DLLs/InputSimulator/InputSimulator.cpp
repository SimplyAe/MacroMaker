#include "InputSimulator.h"
#include <Windows.h>
#include <thread>
#include <chrono>
#include <cmath>

// Helper function to send input
void SendMouseInput(DWORD flags, int x = 0, int y = 0, DWORD data = 0)
{
    INPUT input = { 0 };
    input.type = INPUT_MOUSE;
    input.mi.dwFlags = flags;
    input.mi.dx = x;
    input.mi.dy = y;
    input.mi.mouseData = data;
    SendInput(1, &input, sizeof(INPUT));
}

void SendKeyboardInput(WORD keyCode, DWORD flags)
{
    INPUT input = { 0 };
    input.type = INPUT_KEYBOARD;
    input.ki.wVk = keyCode;
    input.ki.dwFlags = flags;
    SendInput(1, &input, sizeof(INPUT));
}

// Convert screen coordinates to absolute coordinates for SendInput
void ScreenToAbsolute(int x, int y, int& absX, int& absY)
{
    int screenWidth = GetSystemMetrics(SM_CXSCREEN);
    int screenHeight = GetSystemMetrics(SM_CYSCREEN);
    absX = (x * 65535) / screenWidth;
    absY = (y * 65535) / screenHeight;
}

// Exported functions
extern "C"
{
    INPUTSIMULATOR_API void MoveMouse(int x, int y)
    {
        int absX, absY;
        ScreenToAbsolute(x, y, absX, absY);
        SendMouseInput(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, absX, absY);
    }

    INPUTSIMULATOR_API void MoveMouseSmooth(int targetX, int targetY, int steps, int delayMs)
    {
        POINT currentPos;
        GetCursorPos(&currentPos);

        int startX = currentPos.x;
        int startY = currentPos.y;

        for (int i = 1; i <= steps; i++)
        {
            // Linear interpolation
            double t = (double)i / steps;
            int x = (int)(startX + (targetX - startX) * t);
            int y = (int)(startY + (targetY - startY) * t);

            MoveMouse(x, y);

            if (delayMs > 0 && i < steps)
                PreciseSleep(delayMs);
        }
    }

    INPUTSIMULATOR_API void MouseButtonDown(int button)
    {
        DWORD flags = 0;
        switch (button)
        {
        case 0: flags = MOUSEEVENTF_LEFTDOWN; break;
        case 1: flags = MOUSEEVENTF_RIGHTDOWN; break;
        case 2: flags = MOUSEEVENTF_MIDDLEDOWN; break;
        default: return;
        }
        SendMouseInput(flags);
    }

    INPUTSIMULATOR_API void MouseButtonUp(int button)
    {
        DWORD flags = 0;
        switch (button)
        {
        case 0: flags = MOUSEEVENTF_LEFTUP; break;
        case 1: flags = MOUSEEVENTF_RIGHTUP; break;
        case 2: flags = MOUSEEVENTF_MIDDLEUP; break;
        default: return;
        }
        SendMouseInput(flags);
    }

    INPUTSIMULATOR_API void MouseClick(int button)
    {
        MouseButtonDown(button);
        PreciseSleep(10); // Small delay between down and up
        MouseButtonUp(button);
    }

    INPUTSIMULATOR_API void MouseWheel(int delta)
    {
        SendMouseInput(MOUSEEVENTF_WHEEL, 0, 0, delta);
    }

    INPUTSIMULATOR_API void KeyDown(int keyCode)
    {
        SendKeyboardInput((WORD)keyCode, 0);
    }

    INPUTSIMULATOR_API void KeyUp(int keyCode)
    {
        SendKeyboardInput((WORD)keyCode, KEYEVENTF_KEYUP);
    }

    INPUTSIMULATOR_API void KeyPress(int keyCode)
    {
        KeyDown(keyCode);
        PreciseSleep(10);
        KeyUp(keyCode);
    }

    INPUTSIMULATOR_API void PreciseSleep(double milliseconds)
    {
        if (milliseconds <= 0)
            return;

        auto start = std::chrono::high_resolution_clock::now();
        auto duration = std::chrono::duration<double, std::milli>(milliseconds);
        auto end = start + duration;

        // Spin-wait for the last millisecond for better precision
        if (milliseconds > 1.0)
        {
            std::this_thread::sleep_for(std::chrono::duration<double, std::milli>(milliseconds - 1.0));
        }

        while (std::chrono::high_resolution_clock::now() < end)
        {
            // Busy wait for maximum precision
        }
    }
}
