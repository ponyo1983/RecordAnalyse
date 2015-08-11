using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace ConfigManager
{
    public class IniDocument
    {

        string fileName;
        Dictionary<string, Section> fileCache = new Dictionary<string, Section>();

        public IniDocument(String fileName)
        {
            this.fileName = fileName;
        }
        public IniDocument()
        { 
        }

        public void Load(string fileName)
        {
            this.fileName = fileName;
            Load(this.fileName, ASCIIEncoding.GetEncoding("GB2312"));
        }

        public void Load()
        {
            Load(this.fileName, ASCIIEncoding.GetEncoding("GB2312"));
        }

        public void Save()
        {

            if (!File.Exists(this.fileName))
            {
                File.Create(this.fileName).Close();
            }
            FileInfo fi = new FileInfo(this.fileName);
            bool isRealOnly = fi.IsReadOnly;
            if (!Directory.Exists(fi.Directory.FullName))
            {
                Directory.CreateDirectory(fi.Directory.FullName);
            }
            if (isRealOnly && fi.Exists)
            {
                fi.IsReadOnly = false;
            }
            FileStream fs=File.Open(this.fileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
           
            using ( StreamWriter sw = new StreamWriter(fs, Encoding.Default))
            {
                foreach (Section seciton in this.fileCache.Values)
                {
                    sw.WriteLine("[" + seciton.Name+ "]");
                    seciton.WriteToStream(sw);
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("");
                }
            }
        }

        public void Load(string filename, Encoding encoding)
        {
            if (File.Exists(filename) == false) return;

            string[] fileLines = File.ReadAllLines(filename, encoding);
            Section dicSection = null;
            for (int i = 0; i < fileLines.Length; i++)
            {
                string line = fileLines[i].Trim();
                //去掉注释
                if (line == "") continue;
                if (line.StartsWith(";") || line.StartsWith("//")) continue;

                if (line.StartsWith("["))
                {
                    GetNewSection(ref dicSection, line);
                }
                else if (dicSection != null)
                {
                    GetNewValue(dicSection, line);
                }

            }

        }

        private void GetNewValue(Section dicSection, string line)
        {
            int index = line.IndexOf('=');
            if (index <= 0) return;

            string key = line.Substring(0, index).Trim();
            if (String.IsNullOrEmpty(key)) return;

            string value = line.Substring(index + 1).Trim();
            if (String.IsNullOrEmpty(value)) return;

            dicSection.Add(key, value);

        }

        private void GetNewSection(ref Section dicSection, string line)
        {
            int index = line.IndexOf(']');
            if (index < 2) return;

            string sectionName = line.Substring(1, index - 1).Trim();
            if (String.IsNullOrEmpty(sectionName)) return;

            if (fileCache.ContainsKey(sectionName))
            {
                dicSection = fileCache[sectionName];
            }
            else
            {
                dicSection = new Section(sectionName);
                fileCache.Add(sectionName, dicSection);
            }

        }
        public void AddSection(Section sec)
        {
            fileCache[sec.Name] = sec;
        }

        public Section GetSection(string sectionName)
        {

            if (fileCache.ContainsKey(sectionName))
            {
                return  fileCache[sectionName];
            }

            return null;
        }

   
        public string GetString(string section, string key)
        {
            if (!fileCache.ContainsKey(section)) return "";
            Section dicKey = fileCache[section];
            return dicKey.GetString(key);
        }

        public int GetInt(string section, string key, int defaultVal)
        {
            if (!fileCache.ContainsKey(section)) return defaultVal;
            Section dicKey = fileCache[section];
            return dicKey.GetInt(key,defaultVal);
        }

        public float GetFloat(string section, string key, float defaultVal)
        {
            if (!fileCache.ContainsKey(section)) return defaultVal;
            Section dicKey = fileCache[section];
            return dicKey.GetFloat(key, defaultVal);
        }

        public bool GetBool(string section, string key, bool defaultVal)
        {
            if (!fileCache.ContainsKey(section)) return defaultVal;
            Section dicKey = fileCache[section];
            return dicKey.GetBool(key, defaultVal);
        }

        public void SetValue(String secName, String key, String val)
        {
            Section sec=this.GetSection(secName);
            
            if (sec==null)
            {
                sec = new Section(secName);
                this.fileCache.Add(secName,sec);
            }
            sec.SetValue(key, val);
        }




        public ICollection<string> GetAllValues(string secName)
        {
            if(fileCache.ContainsKey(secName)==false)
            {
                return new List<String>();
            }
            return fileCache[secName].GetAllValues();
        }
    }

    public class Section
    {
        Dictionary<string, string> cache = new Dictionary<string, string>();
        private String name;

        public String Name
        {
            get { return name; }
        }

        public Section(String name)
        {
            this.name = name;
        }

        public bool Add(string key, string val)
        {
            if (cache.ContainsKey(key))
            {
                return false;
            }
            cache.Add(key, val);
            return true;

        }

        public int GetInt(string key, int defaultVal)
        {

            if (!cache.ContainsKey(key))
            {
                return defaultVal;
            }
            String str = cache[key];
            return StringHelper.ToInt(str, defaultVal);

        }

        public float GetFloat(string key, float defaultVal)
        {
            if (!cache.ContainsKey(key))
            {
                return defaultVal;
            }
            String str = cache[key];
            return StringHelper.ToFloat(str, defaultVal);
        }

        public bool GetBool(string key, bool defaultVal)
        {
            if (!cache.ContainsKey(key))
            {
                return defaultVal;
            }
            String str = cache[key];
            return StringHelper.ToBool(str, defaultVal);
        }

        public string GetString(string key)
        {
            if (!cache.ContainsKey(key))
            {
                return "";
            }
            return cache[key];
        }

        public Int32[] GetIntArr(string key,char splitChar,Int32 defaultVal)
        {
            String valStr = GetString(key);
            if (String.IsNullOrEmpty(valStr))
            {
                return new Int32[0];
            }
            String[] strArr = valStr.Split(new char[] { splitChar });
            Int32[] res=new Int32[strArr.Length];
            for(int i=0;i<strArr.Length;i++)
            {
               res[i] = StringHelper.ToInt(strArr[i], defaultVal);
            }
            return res;

        }

        internal void WriteToStream(StreamWriter sw)
        {
            foreach (KeyValuePair<string,string> pair in this.cache)
            {
                sw.WriteLine(pair.Key + "=" + pair.Value);
            }
        }


 

        public int[] GetIntArr(String key, int defaultVal)
        {
            return this.GetIntArr(key, ',', defaultVal);
        }

        public  void SetValue(string key, string val)
        {
            this.cache[key] = val;
        }

        public ICollection<string> GetAllValues()
        {
            return this.cache.Values;
        }
    }
}
