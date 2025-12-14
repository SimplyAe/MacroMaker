#pragma once

#ifdef INPUTSIMULATOR_EXPORTS
#define INPUTSIMULATOR_API __declspec(dllexport)
#else
#define INPUTSIMULATOR_API __declspec(dllimport)
#endif

extern "C" {
    /// <summary>
    /// Moves the mouse cursor to the specified position
    /// </summary>
    INPUTSIMULATOR_API void MoveMouse(int x, int y);

    /// <summary>
    /// Moves the mouse cursor smoothly from current position to target with interpolation
    /// </summary>
    INPUTSIMULATOR_API void MoveMouseSmooth(int targetX, int targetY, int steps, int delayMs);

    /// <summary>
    /// Simulates a mouse button press
    /// </summary>
    INPUTSIMULATOR_API void MouseButtonDown(int button);

    /// <summary>
    /// Simulates a mouse button release
    /// </summary>
    INPUTSIMULATOR_API void MouseButtonUp(int button);

    /// <summary>
    /// Simulates a mouse click (down + up)
    /// </summary>
    INPUTSIMULATOR_API void MouseClick(int button);

    /// <summary>
    /// Simulates mouse wheel scroll
    /// </summary>
    INPUTSIMULATOR_API void MouseWheel(int delta);

    /// <summary>
    /// Simulates a key press
    /// </summary>
    INPUTSIMULATOR_API void KeyDown(int keyCode);

    /// <summary>
    /// Simulates a key release
    /// </summary>
    INPUTSIMULATOR_API void KeyUp(int keyCode);

    /// <summary>
    /// Simulates a key press and release
    /// </summary>
    INPUTSIMULATOR_API void KeyPress(int keyCode);

    /// <summary>
    /// High-precision sleep in milliseconds
    /// </summary>
    INPUTSIMULATOR_API void PreciseSleep(double milliseconds);
}
