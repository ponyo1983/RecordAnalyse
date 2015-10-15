using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace RecordAnalyse.Utils
{

    

    

   public class DiskUtil
    {
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint OPEN_EXISTING = 3;

        [DllImport("kernel32.dll", SetLastError = true)]

        private static extern SafeFileHandle CreateFileA(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        private System.IO.FileStream _DriverStream;

        private SafeFileHandle _DriverHandle;





        /// <summary>
        /// 获取扇区信息
        /// </summary>
        /// <param name="DriverName">G:</param>
        public DiskUtil(string DriverName)
        {

            try
            {

              
                if (DriverName == null && DriverName.Trim().Length == 0) return;
                this.DriveName = DriverName;

                if (DriverName.Length >= 3)
                {
                    _DriverStream = new FileStream(DriverName, FileMode.Open, FileAccess.Read);
                }
                else
                {
                    _DriverHandle = CreateFileA("\\\\.\\" + this.DriveName.Trim(), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

                    _DriverStream = new System.IO.FileStream(_DriverHandle, System.IO.FileAccess.Read);
                }
               

      
            }

            catch (Exception)
            {

            }

        }



        public void Close()
        {

            if (_DriverStream != null)
            {
                _DriverStream.Close();
            }
            if (_DriverHandle!=null && (_DriverHandle.IsClosed==false))
            {
                _DriverHandle.Close();
            }
        }

        public string DriveName
        {
            get;
            private set;
        }


        public int Read(byte[] data,int offset,int length)
        {

            int rdLen=0;
            try
            {
                rdLen = _DriverStream.Read(data, offset, length);

            }
            catch (Exception)
            {
                rdLen = 0;
            }

            return rdLen;
                

            
        }


        public long Position
        {
            get
            {
                return _DriverStream.Position;
            }
            set
            {
                _DriverStream.Position = value;
         
            }

        }


    }
}
