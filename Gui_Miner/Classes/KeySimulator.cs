using System.Diagnostics;

namespace Gui_Miner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class GlobalKeyboardHook
    {
        private IntPtr hookId = IntPtr.Zero;
        private HookProc hookCallback;
        private bool disposed = false;
        private Form1 MainForm;
        public Keys[] StartKeys;
        public Keys[] StopKeys;


        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;

        private Queue<Keys> lastPressedKeys;

        public GlobalKeyboardHook()
        {
            hookCallback = new HookProc(HookCallbackProcedure);
            hookId = SetHook(hookCallback);
            
            // Circular buffer to store the last few keys
            lastPressedKeys = new Queue<Keys>(5);          
        }
        public void SetMainForm(Form1 MainForm)
        { this.MainForm = MainForm; }
        public void SetStartKeys(Keys[] keys)
        {
            StartKeys = keys;
            SetKeyHistoryBuffer();
        }
        public void SetStopKeys(Keys[] keys)
        {
            StopKeys = keys;
            SetKeyHistoryBuffer();
        }
        private void SetKeyHistoryBuffer()
        {
            int length = 0;

            if (StopKeys == null)
                length = StartKeys.Length;
            else if(StartKeys == null)
                length = StopKeys.Length;            
            else if (StopKeys.Length > StartKeys.Length)
                length = StopKeys.Length;
            else 
                length = StartKeys.Length;

            lastPressedKeys = new Queue<Keys>(length);
        }

        ~GlobalKeyboardHook()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                }

                UnhookWindowsHookEx(hookId);
                disposed = true;
            }
        }

        private IntPtr SetHook(HookProc hookProc)
        {
            using (Process currentProcess = Process.GetCurrentProcess())
            using (ProcessModule currentModule = currentProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, GetModuleHandle(currentModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallbackProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)0x00000104)
                {
                    KeyEventArgs args = new KeyEventArgs((Keys)vkCode);
                    KeyDown?.Invoke(this, args);

                    // Store the pressed key in the circular buffer
                    lastPressedKeys.Enqueue((Keys)vkCode);
                    if (lastPressedKeys.Count > StopKeys.Length)
                    {
                        lastPressedKeys.Dequeue(); // Remove the oldest key if the buffer exceeds length
                    }

                    var lastKeysPressed = lastPressedKeys.ToList();

                    if (StopKeys != null && lastKeysPressed.Count >= StopKeys.Count())
                    {
                        if(WereShortCutKeysPressed(StopKeys))
                        {
                            // Stop mining
                            MainForm.ClickStopButton();
                        }
                    }
                    if (StartKeys != null && lastKeysPressed.Count >= StartKeys.Count())
                    {
                        if (WereShortCutKeysPressed(StartKeys))
                        {
                            // Start mining
                            MainForm.ClickStartButton();
                        }
                    }
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    KeyEventArgs args = new KeyEventArgs((Keys)vkCode);
                    KeyUp?.Invoke(this, args);
                }
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private bool WereShortCutKeysPressed(Keys[] keys)
        {
            var lastKeysPressed = lastPressedKeys.ToList();
            int matches = 0;

            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i].ToString().ToLower();
                string lastKey = lastKeysPressed[i].ToString().ToLower();

                if (key.Contains("control") && lastKey.Contains("control"))
                {
                    matches++;
                }
                else if (key.Contains("menu") && lastKey.Contains("menu"))
                {
                    matches++;
                }
                else if (key.Contains("shift") && lastKey.Contains("shift"))
                {
                    matches++;
                }
                else if (key == lastKey)
                    matches++;
            }

            if (matches == keys.Length)
            {
                // The last keys pressed match keysArray in the same order
                return true;
            }
            
            return false;
        }

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }




}