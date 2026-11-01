using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OneHealth
{
    /// <summary>
    /// Esc as a LIFO stack (M3): every open modal (tutorial tip, pause menu, settings, confirm, scenario select)
    /// registers here, and only the topmost one may react to Esc. A modal that handles Esc consumes it for the
    /// rest of the frame, so closing a tip or a sub-dialog never also opens or closes what lies beneath it,
    /// whatever order the scripts update in. (Event.current.Use() is IMGUI and does not apply to the Input System.)
    /// </summary>
    public static class ModalStack
    {
        static readonly List<Object> stack = new List<Object>();
        static int consumedFrame = -1;
        static int simulatedFrame = -1;

        /// <summary>Puts the modal on top (or moves it there if it is already open).</summary>
        public static void Push(Object owner)
        {
            stack.Remove(owner);
            stack.Add(owner);
        }

        public static void Remove(Object owner) { stack.Remove(owner); }

        /// <summary>The topmost open modal, or null. Modals destroyed with their scene are dropped.</summary>
        public static Object Top
        {
            get
            {
                stack.RemoveAll(o => o == null);
                return stack.Count > 0 ? stack[stack.Count - 1] : null;
            }
        }

        public static bool IsTop(Object owner) { return owner != null && Top == owner; }

        /// <summary>Esc went down this frame and nothing has handled it yet.</summary>
        public static bool EscapePressed
        {
            get
            {
                if (consumedFrame == Time.frameCount) return false;
                if (simulatedFrame == Time.frameCount) return true;
#if ENABLE_INPUT_SYSTEM
                return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
                return Input.GetKeyDown(KeyCode.Escape);
#endif
            }
        }

        /// <summary>Automated QA: behave as if Esc is pressed on the next frame (keyboard input is not delivered to an unfocused editor).</summary>
        public static void SimulateEscape() { simulatedFrame = Time.frameCount + 1; }

        /// <summary>Marks this frame's Esc as handled.</summary>
        public static void ConsumeEscape() { consumedFrame = Time.frameCount; }

        /// <summary>True (and Esc consumed) when Esc was pressed this frame and the owner is the topmost modal.</summary>
        public static bool TryConsumeEscape(Object owner)
        {
            if (!EscapePressed || !IsTop(owner)) return false;
            ConsumeEscape();
            return true;
        }
    }
}
