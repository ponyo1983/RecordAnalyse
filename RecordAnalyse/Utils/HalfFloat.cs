using System;
using System.Collections.Generic;
using System.Text;

namespace RecordAnalyse.Utils
{
    public class HalfFloat
    {

        public static int Halfp2Singles( float[] target,  UInt16[] source, int numel)
        {
            
            UInt16[] hp = source; // Type pun input as an unsigned 16-bit int
            UInt32[] xp = new UInt32[numel];// target; // Type pun output as an unsigned 32-bit int
            for (int i = 0; i < numel; i++)
            {
                target[i] = 0;
                xp[i] = BitConverter.ToUInt32(BitConverter.GetBytes(target[i]),0);
            }
            

         
            UInt16 h, hs, he, hm;
            UInt32 xs, xe, xm;
            Int32 xes;
            int e;
            int next;  // Little Endian adjustment
            int checkieee = 1;  // Flag to check for IEEE754, Endian, and word size
            //double one = 1.0; // Used for checking IEEE754 floating point format

            if (checkieee != 0)
            { // 1st call, so check for IEEE754, Endian, and word size
                byte[] tmp = BitConverter.GetBytes(1.0);
                UInt32 t = BitConverter.ToUInt32(tmp, 0);
                if (t != 0)
                { // If Big Endian, then no adjustment
                    next = 0;
                }
                else
                { // If Little Endian, then adjustment will be necessary
                    next = 1;
                    t = BitConverter.ToUInt32(tmp, 4);
                }
                if (t != 0x3FF00000u)
                { // Check for exact IEEE 754 bit pattern of 1.0
                    return 1;  // Floating point bit pattern is not IEEE 754
                }
              
                checkieee = 0; // Everything checks out OK
            }

            //if( source == 0 || target == 0 ) // Nothing to convert (e.g., imag part of pure real)
            //    return 0;
            int hpIndex = 0;
            int xpIndex = 0;
            while (numel != 0)
            {
                numel--;
                h = hp[hpIndex++];
                if ((h & 0x7FFFu) == 0)
                {  // Signed zero
                    xp[xpIndex++] = ((UInt32)h) << 16;  // Return the signed zero
                }
                else
                { // Not zero
                    hs = (UInt16)(h & 0x8000u);  // Pick off sign bit
                    he = (UInt16)(h & 0x7C00u);  // Pick off exponent bits
                    hm = (UInt16)(h & 0x03FFu);  // Pick off mantissa bits
                    if (he == 0)
                    {  // Denormal will convert to normalized
                        e = -1; // The following loop figures out how much extra to adjust the exponent
                        do
                        {
                            e++;
                            hm <<= 1;
                        } while ((hm & 0x0400u) == 0); // Shift until leading bit overflows into exponent bit
                        xs = ((UInt32)hs) << 16; // Sign bit
                        xes = ((Int32)(he >> 10)) - 15 + 127 - e; // Exponent unbias the halfp, then bias the single
                        xe = (UInt32)(xes << 23); // Exponent
                        xm = ((UInt32)(hm & 0x03FFu)) << 13; // Mantissa
                        xp[xpIndex++] = (xs | xe | xm); // Combine sign bit, exponent bits, and mantissa bits\

                    }
                    else if (he == 0x7C00u)
                    {  // Inf or NaN (all the exponent bits are set)
                        if (hm == 0)
                        { // If mantissa is zero ...
                            xp[xpIndex++] = (((UInt32)hs) << 16) | ((UInt32)0x7F800000u); // Signed Inf
                        }
                        else
                        {
                            xp[xpIndex++] = (UInt32)0xFFC00000u; // NaN, only 1st mantissa bit set
                        }
                    }
                    else
                    { // Normalized number
                        xs = ((UInt32)hs) << 16; // Sign bit
                        xes = ((Int32)(he >> 10)) - 15 + 127; // Exponent unbias the halfp, then bias the single
                        xe = (UInt32)(xes << 23); // Exponent
                        xm = ((UInt32)hm) << 13; // Mantissa
                        xp[xpIndex] = (xs | xe | xm); // Combine sign bit, exponent bits, and mantissa bits
                        target[xpIndex] = BitConverter.ToSingle(BitConverter.GetBytes(xp[xpIndex]),0);
                        xpIndex++;
                    }
                }
            }
            return 0;
        }


        public static int Singles2Halfp(  UInt16[] target,  float[] source, int numel)
        {
            UInt16[] hp = (UInt16[])target; // Type pun output as an unsigned 16-bit int
            UInt32[] xp = new UInt32[numel]; // Type pun input as an unsigned 32-bit int

            for (int i = 0; i < numel; i++)
            {
                xp[i] = BitConverter.ToUInt32(BitConverter.GetBytes(source[i]),0);
            }

            UInt16 hs, he, hm;
            UInt32 x, xs, xe, xm;
            int hes;
            int next;  // Little Endian adjustment
            int checkieee = 1;  // Flag to check for IEEE754, Endian, and word size
            double one = 1.0; // Used for checking IEEE754 floating point format
            UInt32 ip; // Used for checking IEEE754 floating point format

            if (checkieee != 0)
            { // 1st call, so check for IEEE754, Endian, and word size
                byte[] tmp = BitConverter.GetBytes(1.0);
                ip = BitConverter.ToUInt32(tmp, 0);
                if (ip != 0)
                { // If Big Endian, then no adjustment
                    next = 0;
                }
                else
                { // If Little Endian, then adjustment will be necessary
                    next = 1;
                    ip = BitConverter.ToUInt32(tmp, 4);
                }
                if (ip != 0x3FF00000u)
                { // Check for exact IEEE 754 bit pattern of 1.0
                    return 1;  // Floating point bit pattern is not IEEE 754
                }
             
                checkieee = 0; // Everything checks out OK
            }

            //if( source == NULL || target == NULL ) { // Nothing to convert (e.g., imag part of pure real)
            //    return 0;
            //}

            int xpIndex = 0;
            int hpIndex = 0;
            while (numel != 0)
            {
                numel--;
                x = xp[xpIndex++];
                if ((x & 0x7FFFFFFFu) == 0)
                {  // Signed zero
                    hp[hpIndex++] = (UInt16)(x >> 16);  // Return the signed zero
                }
                else
                { // Not zero
                    xs = x & 0x80000000u;  // Pick off sign bit
                    xe = x & 0x7F800000u;  // Pick off exponent bits
                    xm = x & 0x007FFFFFu;  // Pick off mantissa bits
                    if (xe == 0)
                    {  // Denormal will underflow, return a signed zero
                        hp[hpIndex++] = (UInt16)(xs >> 16);
                    }
                    else if (xe == 0x7F800000u)
                    {  // Inf or NaN (all the exponent bits are set)
                        if (xm == 0)
                        { // If mantissa is zero ...
                            hp[hpIndex++] = (UInt16)((xs >> 16) | 0x7C00u); // Signed Inf
                        }
                        else
                        {
                            hp[hpIndex++] = (UInt16)0xFE00u; // NaN, only 1st mantissa bit set
                        }
                    }
                    else
                    { // Normalized number
                        hs = (UInt16)(xs >> 16); // Sign bit
                        hes = ((int)(xe >> 23)) - 127 + 15; // Exponent unbias the single, then bias the halfp
                        if (hes >= 0x1F)
                        {  // Overflow
                            hp[hpIndex++] = (UInt16)((xs >> 16) | 0x7C00u); // Signed Inf
                        }
                        else if (hes <= 0)
                        {  // Underflow
                            if ((14 - hes) > 24)
                            {  // Mantissa shifted all the way off & no rounding possibility
                                hm = (UInt16)0u;  // Set mantissa to zero
                            }
                            else
                            {
                                xm |= 0x00800000u;  // Add the hidden leading bit
                                hm = (UInt16)(xm >> (14 - hes)); // Mantissa
                                if (((xm >> (13 - hes)) & 0x00000001u) != 0) // Check for rounding
                                    hm += (UInt16)1u; // Round, might overflow into exp bit, but this is OK
                            }
                            hp[hpIndex++] = (ushort)(hs | hm); // Combine sign bit and mantissa bits, biased exponent is zero
                        }
                        else
                        {
                            he = (UInt16)(hes << 10); // Exponent
                            hm = (UInt16)(xm >> 13); // Mantissa
                            if ((xm & 0x00001000u) != 0) // Check for rounding
                                hp[hpIndex++] = (UInt16)((hs | he | hm) + (UInt16)1u); // Round, might overflow to inf, this is OK
                            else
                                hp[hpIndex++] = (UInt16)(hs | he | hm);  // No rounding
                        }
                    }
                }
            }
            return 0;
        }


    }
}
