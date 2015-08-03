using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace UltraChart
{
    public class GDI32
    {
        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreatePen(int fnPenStyle, int width, int color);
        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(int crColor);
        [DllImport("Gdi32.dll")]
        public static extern int SetROP2(System.IntPtr hdc, int rop);
        [DllImport("Gdi32.dll")]
        public static extern int MoveToEx(IntPtr hdc, int x, int y, IntPtr lppoint);
        [DllImport("Gdi32.dll")]
        public static extern int LineTo(IntPtr hdc, int X, int Y);
        [DllImport("Gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);
        [DllImport("Gdi32.dll")]
        public static extern bool Rectangle(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);
    }
}
