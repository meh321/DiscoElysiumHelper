using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscoElysiumHelper
{
    internal static class Helper
    {
        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern Int32 CloseHandle(IntPtr hProcess);

        private static IntPtr Begin(ref IntPtr baseAddr)
        {
            var proc = System.Diagnostics.Process.GetProcessesByName("Disco Elysium");
            if (proc.Length != 1)
                return IntPtr.Zero;

            bool found = false;
            foreach(System.Diagnostics.ProcessModule m in proc[0].Modules)
            {
                if(m.ModuleName == "GameAssembly.dll")
                {
                    found = true;
                    baseAddr = m.BaseAddress;
                    break;
                }
            }

            if (!found)
            {
                baseAddr = new IntPtr(1);
                return IntPtr.Zero;
            }

            return OpenProcess(ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite | ProcessAccessFlags.VMOperation, false, proc[0].Id);
        }

        private static void End(IntPtr handle)
        {
            if(handle != IntPtr.Zero)
                CloseHandle(handle);
        }

        private static byte[] ParseBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return new byte[0];

            hex = hex.Replace(" ", "").Replace("-", "");

            if ((hex.Length % 2) != 0)
                throw new ArgumentException();

            byte[] arr = new byte[hex.Length / 2];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = byte.Parse(hex.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);

            return arr;
        }

        private static byte?[] ParseNullableBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return new byte?[0];

            hex = hex.Replace(" ", "").Replace("-", "");

            if ((hex.Length % 2) != 0)
                throw new ArgumentException();

            byte?[] arr = new byte?[hex.Length / 2];
            for (int i = 0; i < arr.Length; i++)
            {
                string p = hex.Substring(i * 2, 2);
                if (p == "??")
                    arr[i] = null;
                else
                    arr[i] = byte.Parse(p, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
            }

            return arr;
        }

        private static bool VerifyBytes(IntPtr handle, IntPtr addr, string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return true;

            var parsed = ParseNullableBytes(hex);
            byte[] actual = null;
            string error = ReadImpl(handle, addr, parsed.Length, ref actual);
            if(error != null)
                return false;

            for(int i = 0; i < parsed.Length; i++)
            {
                if (!parsed[i].HasValue)
                    continue;

                if (actual[i] != parsed[i].Value)
                    return false;
            }

            return true;
        }

        private static string WriteImpl(IntPtr handle, IntPtr addr, byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            int did = 0;
            WriteProcessMemory(handle, addr, data, (uint)data.Length, out did);

            if (did != data.Length)
                return "Failed to write all bytes!";

            return null;
        }

        private static string ReadImpl(IntPtr handle, IntPtr addr, int len, ref byte[] data)
        {
            if(len <= 0)
            {
                data = new byte[0];
                return null;
            }

            if(len > 1024 * 1024)
                return "Trying to read too many bytes at once!";

            data = new byte[len];
            int did = 0;
            ReadProcessMemory(handle, addr, data, (uint)len, ref did);

            if (did != len)
                return "Failed to read all bytes!";

            return null;
        }

        internal static void Write(long offset, string what, string expect)
        {
            var baseAddr = IntPtr.Zero;
            var handle = Begin(ref baseAddr);
            if(handle == IntPtr.Zero)
            {
                if(baseAddr == new IntPtr(1))
                    Error("Failed to find module in process!");
                else
                    Error("Failed to get handle for game process!");
                return;
            }

            string error = null;
            try
            {
                var addr = new IntPtr(unchecked(baseAddr.ToInt64() + offset));

                if (!string.IsNullOrEmpty(expect))
                {
                    if (!VerifyBytes(handle, addr, expect))
                        error = "Byte pattern couldn't be verified!";
                }

                if (error == null)
                {
                    var data = ParseBytes(what);

                    error = WriteImpl(handle, addr, data);
                }
            }
            catch
            {
                error = "Unhandled exception!";
            }
            finally
            {
                End(handle);
            }

            if (error != null)
                Error(error);
        }

        internal static byte[] Read(long offset, int len)
        {
            var baseAddr = IntPtr.Zero;
            var handle = Begin(ref baseAddr);
            if (handle == IntPtr.Zero)
            {
                if (baseAddr == new IntPtr(1))
                    Error("Failed to find module in process!");
                else
                    Error("Failed to get handle for game process!");
                return null;
            }

            byte[] result = null;
            string error = null;
            try
            {
                var addr = new IntPtr(unchecked(baseAddr.ToInt64() + offset));
                
                error = ReadImpl(handle, addr, len, ref result);
            }
            catch
            {
                error = "Unhandled exception!";
            }
            finally
            {
                End(handle);
            }

            if (error != null)
            {
                Error(error);
                return null;
            }

            return result;
        }

        private static void Error(string message)
        {
            System.Windows.Forms.MessageBox.Show(message);
            throw new InvalidOperationException();
        }
    }

    internal sealed class App
    {
        internal App(long offset, string change, string original)
        {
            this.Offset = offset;
            this.Change = change;
            this.Original = original;
        }

        internal readonly long Offset;

        internal readonly string Change;

        internal readonly string Original;

        internal bool IsApplied
        {
            get;
            private set;
        }

        internal void Apply()
        {
            if (this.IsApplied)
                return;

            all.Add(this);
            this.IsApplied = true;

            Helper.Write(this.Offset, this.Change, this.Original);
        }

        internal void Unapply()
        {
            if (!this.IsApplied)
                return;

            all.Remove(this);
            this.IsApplied = false;

            Helper.Write(this.Offset, this.Original, this.Change);
        }

        private static readonly List<App> all = new List<App>();

        internal static IReadOnlyList<App> All
        {
            get
            {
                return all;
            }
        }

        internal static void RemoveAll()
        {
            while (All.Count != 0)
                All[All.Count - 1].Unapply();
        }
    }
}
