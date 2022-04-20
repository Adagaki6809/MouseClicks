﻿using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FixMouse
{
    public class InterceptKeys
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201, WM_RBUTTONDOWN = 0x0204;
        private static readonly LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static readonly Stopwatch swLeftClick = new (), swRightClick = new ();
        private const string appName = "MouseClicks.exe";
        private static int countClicks = 0, time = 120;
        public static void Main()
        {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule?.ModuleName ?? appName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            switch((int)wParam)
            {
                case WM_LBUTTONDOWN:
                    return CountTimeBetweenClicks(swLeftClick, nCode, wParam, lParam);
                case WM_RBUTTONDOWN:
                    return CountTimeBetweenClicks(swRightClick, nCode, wParam, lParam);
                default:
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }
        }

        private static IntPtr CountTimeBetweenClicks(Stopwatch sw, int nCode, IntPtr wParam, IntPtr lParam)
        {
            Console.WriteLine($"[{countClicks++}] {((int)wParam == WM_LBUTTONDOWN ? "Left " : "Right")} Time: {sw.ElapsedMilliseconds}");
            if (sw.IsRunning)
            {
                sw.Stop();
            if (sw.ElapsedMilliseconds > time)
                {
                    sw.Reset();
                }
                else
                {   
                    sw.Reset();
                    sw.Start();
                    return new IntPtr(1);
                }
            }   
            if (!sw.IsRunning)
                sw.Start();
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}