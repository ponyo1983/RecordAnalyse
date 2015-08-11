using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace ConfigManager
{
    public class StringHelper
    {
        #region
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hexStr"></param>
        /// <param name="outBytes"></param>
        /// <returns></returns>
        public static bool HexStr2ByteArray(String hexStr, out byte[] outBytes)
        {

            String[] tempStrArr;
            outBytes = null;
            tempStrArr = hexStr.Split(new char[] { ',', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (tempStrArr == null) return false;
            if (tempStrArr.Length == 0) return false;

            outBytes = new byte[tempStrArr.Length];

            for (int i = 0; i < tempStrArr.Length; i++)
            {
                UInt16 tempUint16;
                if (!UInt16.TryParse(tempStrArr[i], System.Globalization.NumberStyles.HexNumber, null, out tempUint16))
                {

                    outBytes = null;
                    return false;
                }
                else
                {
                    outBytes[i] = (byte)tempUint16;

                }
            }

            return true;


        }
        /// <summary>
        /// 数组转换为字符串 以空格分割
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string ByteArr2HexString(byte[] buffer)
        {
            string str = ByteArr2HexString(buffer,0, (int)buffer.Length, " ");
            return str;
        }

        /// <summary>
        /// 将数组转换为字符串使用指定的分隔符
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string ByteArr2HexString(byte[] buffer,String spliter)
        {
            string str = ByteArr2HexString(buffer,0, (int)buffer.Length, spliter);
            return str;
        }

        /// <summary>
        /// 将数组转换为字符串使用指定的分隔符
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="len"></param>
        /// <param name="spliter"></param>
        /// <returns></returns>
        public static string ByteArr2HexString(byte[] buffer, int startIndex, int len, String spliter)
        {
            
            StringBuilder sb = new StringBuilder("");
            if (buffer == null || buffer.Length <= startIndex || startIndex + len <= startIndex) return sb.ToString();
            int i = startIndex;
            //最后一个字符不需要加分割符号,所以单独处理
            for (i = 0; i < startIndex+len - 1 && i < buffer.Length - 1; i++)
            {
                sb.Append(buffer[i].ToString("X2"));
                if(spliter!=null) sb.Append(spliter);
            }
            sb.Append(buffer[i].ToString("X2"));
     
            return sb.ToString();
        }

        public static bool TryToInt(string val, ref int retVal)
        {
            if (string.IsNullOrEmpty(val)) return false;
            if (val.StartsWith("0x") || val.StartsWith("0X"))
            {
                return int.TryParse(val.Substring(2, val.Length - 2), System.Globalization.NumberStyles.HexNumber, null, out retVal);
            }
            return int.TryParse(val, out retVal);

        }


        public static bool TryToLong(string val, ref long retVal)
        {
            if (string.IsNullOrEmpty(val)) return false;
            if (val.StartsWith("0x") || val.StartsWith("0X"))
            {
                return long.TryParse(val.Substring(2, val.Length - 2), System.Globalization.NumberStyles.HexNumber, null, out retVal);
            }
            return long.TryParse(val, out retVal);

        }
        public static string GetTimeString(DateTime time)
        {
            string str = time.ToString("yyyy-MM-dd HH:mm:ss");
            return str;
        }
        public static bool TryToDateTime(string dateTimeStr, ref DateTime retVal)
        {
            return DateTime.TryParseExact(dateTimeStr, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out retVal);

        }

        public static bool TryToFloat(string val, ref float retVal)
        {
            bool ret = false;
            ret = float.TryParse(val, out retVal);
            if (ret == false)
            {
                retVal = float.NaN;
            }
            return ret;
        }


        public static float ToFloat(string val, float defVal)
        {
            if (val == "非数字")
            {
                return float.NaN;
            }
            try
            {
                return Convert.ToSingle(val);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(string.Format("数据转换格式错误：val: {0}  {1}", val, e.Message));
                return defVal;
            }
        }

        public static int ToInt(string val, int defVal)
        {
            if (string.IsNullOrEmpty(val))
            {
                return defVal;
            }
            try
            {
                if (val.StartsWith("0x") || val.StartsWith("0X"))
                {
                    return int.Parse(val.Substring(2, val.Length - 2), System.Globalization.NumberStyles.HexNumber);
                }
                return int.Parse(val);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(string.Format("数据转换格式错误：val: {0}  {1}", val, e.Message));
                return defVal;
            }
        }

        public static byte[] BinaryFromString(string str)
        {
            byte[] retBuf = null;
            HexStr2ByteArray(str, out retBuf);
            return retBuf;
        }
        public static DateTime ToDateTime(string dateTimeStr)
        {
            return DateTime.ParseExact(dateTimeStr, "yyyy-MM-dd HH:mm:ss", null);
        }
        public static string BinaryToString(byte[] buf, int maxLen)
        {
            if (buf == null)
            {
                return "";
            }
            int count = Math.Min(buf.Length, maxLen);
            bool bOmit = count < buf.Length;
            StringBuilder sb = new StringBuilder((count * 2) + 30);
            for (int i = 0; i < count; i++)
            {
                sb.Append(buf[i].ToString("X2"));
                sb.Append(" ");
            }
            if (bOmit)
            {
                sb.Append(string.Format(" ... Total {0} Bytes", buf.Length));
            }
            return sb.ToString();
        }

        public static long ToLong(string val, long defVal)
        {
            if (string.IsNullOrEmpty(val))
            {
                return defVal;
            }
            try
            {
                if (val.StartsWith("0x") || val.StartsWith("0X"))
                {
                    return long.Parse(val.Substring(2, val.Length - 2), System.Globalization.NumberStyles.HexNumber);
                }
                return long.Parse(val);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(string.Format("数据转换格式错误：val: {0}  {1}", val, e.Message));
                return defVal;
            }
        }
        #endregion

        public static string[] ToUpper(string[] strs)
        {
            if (strs == null) return strs;
            for (int i = 0; i < strs.Length; i++)
            {
                if (strs[i] == null) continue;
                strs[i] = strs[i].ToUpper();
            }
            return strs;
        }

        public static string[] SplitTrimString(string str)
        {
            return SplitTrimString(str, new char[] { ',', (char)0xff0c }, false, true);
        }

        public static string[] SplitTrimString(string str, bool bToUpper)
        {
            return SplitTrimString(str, new char[] { ',', (char)0xff0c }, bToUpper, true);
        }

        public static string[] SplitTrimString(string str, bool bToUpper, bool removeEmpty)
        {
            return SplitTrimString(str, new char[] { ',', (char)0xff0c }, bToUpper, removeEmpty);
        }

        public static string[] SplitTrimString(string str, char[] splitChars, bool bToUpper, bool removeEmpty)
        {
            if (string.IsNullOrEmpty(str)) return null;
            if (splitChars == null) splitChars = new char[] { ',', (char)0xff0c };
            string[] ss = null;
            if (removeEmpty) ss = str.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            else ss = str.Split(splitChars);
            if (bToUpper) return ToUpper(ss);
            return ss;
        }

        public static string AnsiStrToUniCode(BinaryReader br, int nMaxBytes)
        {
            string str = "";
            if ((br != null) && (nMaxBytes > 0))
            {
                byte[] ba = new byte[nMaxBytes];
                int nReadBytes = 0;
                int nReadHz = 0;
                int i = 0;
                for (i = 0; i < nMaxBytes; i++)
                {
                    ba[i] = br.ReadByte();
                    if (ba[i] == 0)
                    {
                        break;
                    }
                    nReadBytes++;
                    if ((ba[i] & 0x80) != 0)
                    {
                        nReadHz++;
                    }
                }
                if (((nReadHz % 2) == 0) && (nReadBytes >= 1))
                {
                    BinaryReader br2 = new BinaryReader(new MemoryStream(ba), Encoding.Default);
                    str = new string(br2.ReadChars(nReadBytes - (nReadHz / 2)));
                    br2.Close();
                }
            }
            return str;
        }

        public static int CopyByteArrayToString(byte[] ba, int nIndex, ref string str)
        {
            str = "";
            if ((ba == null) || (ba.Length <= nIndex))
            {
                return 0;
            }
            int nReadBytes = 0;
            int nReadHz = 0;
            for (int i = nIndex; i < ba.Length; i++)
            {
                if (ba[i] == 0)
                {
                    break;
                }
                nReadBytes++;
                if ((ba[i] & 0x80) != 0)
                {
                    nReadHz++;
                }
            }
            if ((nReadHz % 2) == 0)
            {
                BinaryReader br = new BinaryReader(new MemoryStream(ba), Encoding.Default);
                br.ReadBytes(nIndex);
                char[] chars = br.ReadChars(nReadBytes - (nReadHz / 2));
                str = new string(chars);
                br.Close();
            }
            return (nReadBytes + 1);
        }

        public static int CopyStringToByteArray(string str, BinaryWriter bw)
        {
            Debug.Assert(bw != null);
            Debug.Assert(str != null);
            if ((str == null) || (bw == null))
            {
                return 0;
            }
            if (str == "")
            {
                bw.Write((byte)0);
                return 1;
            }
            int nBegin = (int)bw.Seek(0, SeekOrigin.Current);
            bw.Write(str.ToCharArray());
            bw.Write((byte)0);
            return (((int)bw.Seek(0, SeekOrigin.Current)) - nBegin);
        }

        public static int CopyStringToByteArray(string str, byte[] ba, int nIndex)
        {
            Debug.Assert(str != null);
            if (str == null)
            {
                return 0;
            }
            Debug.Assert(ba != null);
            Debug.Assert(nIndex >= 0);
            if ((ba == null) || (nIndex < 0))
            {
                return 0;
            }
            if (str == "")
            {
                ba[nIndex] = 0;
                return 1;
            }
            BinaryWriter bw = new BinaryWriter(new MemoryStream(ba), Encoding.Default);
            bw.Seek(nIndex, SeekOrigin.Begin);
            bw.Write(str.ToCharArray());
            bw.Write((byte)0);
            int nRet = ((int)bw.Seek(0, SeekOrigin.Current)) - nIndex;
            bw.Close();
            return nRet;
        }

        public static int GetStrLengthInByte(string str)
        {
            if (str == null)
            {
                return 0;
            }
            int nLength = 0;
            ushort nCode = 0;
            for (int i = 0; i < str.Length; i++)
            {
                nCode = str[i];
                if (nCode > 0xff)
                {
                    nLength += 2;
                }
                else
                {
                    nLength++;
                }
            }
            return (nLength + 1);
        }

        public static string ToDBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == '　')
                {
                    c[i] = ' ';
                }
                else if ((c[i] > 0xff00) && (c[i] < 0xff5f))
                {
                    c[i] = (char)(c[i] - 0xfee0);
                }
            }
            return new string(c);
        }

        public static bool ToBool(string str, bool defaultVal)
        {
            str = str.ToUpper();
            if (str == "是" || str == "YES" || str == "1" || str == "TRUE")
            {
                return true;
            }
            else if (str == "否" || str == "NO" || str == "0" || str == "FALSE")
            {
                return false;
            }
            else
            {
                return defaultVal;
            }
        }

        public static Encoding CP936 = Encoding.GetEncoding(936);

        /// <summary>
        /// 右对齐字符串
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <param name="rightPadLen">右侧固定空格的个数</param>
        /// <param name="width">固定长度</param>
        /// <returns></returns>
        public static String GetLeftPadding(String str, int rightPadLen, int width)
        {
            int strLen = CP936.GetByteCount(str);
            if (strLen >= width)
            {
                return str;
            }
            int leftPadCount = width - strLen - rightPadLen;
            StringBuilder sb = new StringBuilder();
            if (leftPadCount > 0)
            {
                AddRepeat(sb, " ", leftPadCount);
            }
            sb.Append(str);
            AddRepeat(sb, " ", rightPadLen);
            return sb.ToString();
           

        }

        /// <summary>
        /// 右对齐字符串
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <param name="rightPadLen">右侧固定空格的个数</param>
        /// <param name="width">固定长度</param>
        /// <returns></returns>
        public static String GetRightPadding(String str, int leftPadLen, int width)
        {
            int strLen = CP936.GetByteCount(str);
        
            int rightPadCount = width - strLen - leftPadLen;
            StringBuilder sb = new StringBuilder();
            AddRepeat(sb, " ", leftPadLen);
            sb.Append(str);
         
            if (rightPadCount > 0)
            {
                AddRepeat(sb, " ", rightPadCount);
            }
            return sb.ToString();

        }

        private static void AddRepeat(StringBuilder sb, string str, int count)
        {
            for (int i = 0; i < count; i++)
            {
                sb.Append(str);
            }
        }

        static readonly String[] ChineseNumTable = new String[] 
        {"〇", "一","二","三","四","五","六","七","八","九","十"};
        public static String GetNumChineseChar(int val)
        {
            if (val < 0 || val > 9)
            {
                return "不合法的数字,GetNumChineseChar只支持0-9的数字";
            }
            return ChineseNumTable[val];
        }
    }

}
