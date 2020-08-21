using OpenToolkit.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VoxelCraft
{
    public static class Debug
    {
        public static void Log(object toLog)
        {
            ChangeConsoleColor(ConsoleColor.Gray);
            StackFrame frame = new StackFrame(1, true);

            // Weird behaviour can happen if an attempt to log is done when the application
            // is closing, this prevents that issue. A better solution should be implemented
            // though.
            if (frame == null || frame.GetFileName() == null)
            {
                return;
            }

            string[] splitFilePath = frame.GetFileName().Split('\\');
            string fileName = splitFilePath[splitFilePath.Length - 1];
            string callInfo = fileName + " : " + frame.GetFileLineNumber();

            Console.WriteLine($"< {callInfo} - {toLog}");
        }


        private static DebugProc DebugCallbackProc;
        public static void EnableOpenGLDebug()
        {
            DebugCallbackProc = DebugCallback;

            GCHandle.Alloc(DebugCallbackProc);

            GL.DebugMessageCallback(DebugCallbackProc, IntPtr.Zero);

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
        }

        private static void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            string messageString = Marshal.PtrToStringAnsi(message, length);

            Console.WriteLine($"{severity} {type} | {messageString}");

            if (type == DebugType.DebugTypeError)
            {
                throw new Exception(messageString);
            }
        }

        private static void ChangeConsoleColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }
    }

    public class RollingAverageDebug<T>
    {
        private T[] KeptData;
        private int CurrentIndex;

        public RollingAverageDebug(int entryCount)
        {
            KeptData = new T[entryCount];
        }

        public void AddData(T data)
        {
            if(CurrentIndex >= KeptData.Length)
            {
                CurrentIndex = 0;
            }

            KeptData[CurrentIndex] = data;

            CurrentIndex++;
        }

        public T[] GetData()
        {
            return KeptData;
        }
    }
}
