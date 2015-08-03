using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using fftwlib;

namespace RecordAnalyse.DSP
{
    public class FFTPlan : IDisposable
    {

        private bool disposed = false;
        IntPtr fplan1, fplan2;
        IntPtr pin, pout;
        int fftNum = 0;
        static object objLock = new object();
        public FFTPlan(int n)
        {
            lock (objLock)
            {
                fftNum = n;
                //create two unmanaged arrays, properly aligned
                pin = fftwf.malloc(n * 8);
                pout = fftwf.malloc(n * 8);


                fplan1 = fftwf.dft_1d(n, pin, pout, fftw_direction.Forward, fftw_flags.Estimate);
                fplan2 = fftwf.dft_1d(n, pin, pout, fftw_direction.Backward, fftw_flags.Estimate);
            }

        }

        /// <summary>
        /// 实现IDisposable中的Dispose方法
        /// </summary>
        public void Dispose()
        {
            //必须为true
            Dispose(true);
            //通知垃圾回收机制不再调用终结器（析构器）
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 不是必要的，提供一个Close方法仅仅是为了更符合其他语言（如C++）的规范
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// 必须，以备程序员忘记了显式调用Dispose方法
        /// </summary>
        ~FFTPlan()
        {
            //必须为false
            Dispose(false);
        }

        /// <summary>
        /// 非密封类修饰用protected virtual
        /// 密封类修饰用private
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                // 清理托管资源
            }
            // 清理非托管资源
            fftwf.free(pin);
            fftwf.free(pout);
            fftwf.destroy_plan(fplan1);
            fftwf.destroy_plan(fplan2);
            //让类型知道自己已经被释放
            disposed = true;
        }


        public int FFTNum
        {
            get { return fftNum; }
        }

        public void FFTForward(float[] fin, float[] fout)
        {
            if (disposed) return;
            if (fin == null || fin.Length < 2 * fftNum) return;
            if (fout == null || fout.Length < 2 * fftNum) return;

            Marshal.Copy(fin, 0, pin, fftNum * 2);

            fftwf.execute(fplan1);

            Marshal.Copy(pout, fout, 0, fftNum * 2);



        }

        public void FFTBackward(float[] fin, float fout)
        {
            if (disposed) return;

        }
    }
}
