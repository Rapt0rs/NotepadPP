using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NotepadPP.Master;

namespace NotepadPP.Memory {
    #region Enums
    enum ProcessAccessFlags : uint {
        QueryInformation = 0x00000400,
        VirtualMemoryRead = 0x00000010,
        VirtualMemoryWrite = 0x00000020
    }
    #endregion
    
    class Mem {
        #region Imports
        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
             ProcessAccessFlags processAccess,
             bool bInheritHandle,
             int processId
        );
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten
        );
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead
        );
        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private static IntPtr Open(Process proc) {
            return OpenProcess(ProcessAccessFlags.VirtualMemoryRead | ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryWrite, false, proc.Id);
        }
        #endregion

        #region Public
        #endregion
        #region Private
        private IntPtr hProcess;
        private Client c;
        #endregion

        #region Public
        public Mem(Process game, Client cl) {
            c = cl;
            hProcess = Open(game);
        }
        public int ReadInt32(IntPtr address) {
            return BitConverter.ToInt32(Read(address, sizeof(int)), 0);
        }
        public long ReadLong(IntPtr address) {
            return BitConverter.ToInt64(Read(address, sizeof(long)), 0);
        }
        public float ReadFloat(IntPtr address) {
            return BitConverter.ToSingle(Read(address, sizeof(float)), 0);
        }
        public IntPtr ReadPtr(IntPtr address) {
            return new IntPtr(ReadInt32(address));
        }

        public bool WriteFloat(IntPtr address, float value) {
            byte[] bytes = BitConverter.GetBytes(value);
            return Write(address, bytes);
        }
        public bool WriteBool(IntPtr address, bool value) {
            byte[] bytes = BitConverter.GetBytes(value);
            return Write(address, bytes);
        }

        #endregion
        #region Private
        private bool Write(IntPtr address,byte[] value) {
            if (c.Master.getRenderEngine() != null) {
                while (!c.Master.getRenderEngine().isGameActive()) {
                    Thread.Sleep(100);
                }
            }

            if (address.ToInt32() == 0) {
                return false;
            }
            IntPtr ignore = new IntPtr(0);
            return WriteProcessMemory(hProcess, address, value, value.Length, out ignore);
        }
        private byte[] Read(IntPtr address, int length) {
            if (c.Master.getRenderEngine() != null) {
                while (!c.Master.getRenderEngine().isGameActive()) {
                    Thread.Sleep(100);
                }
            }

            if (address.ToInt32() == 0) {
                return BitConverter.GetBytes(0);
            }
            byte[] buffer = new byte[length];
            IntPtr ignore = new IntPtr(0);
            if (!ReadProcessMemory(hProcess, address, buffer, length, out ignore)) {
                CloseHandle(hProcess);
                hProcess = IntPtr.Zero;
                System.Windows.Forms.MessageBox.Show("A fatal error has occured!\nThis is most likely because you started the exe to soon.\nPlease restart the exe.", "Uh Oh!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            return buffer;
        }
        #endregion
    }
}
