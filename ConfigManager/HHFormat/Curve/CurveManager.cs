using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConfigManager.HHFormat.Curve
{
   public class CurveManager
    {
         Dictionary<string, CurveGroup> dicCurveGrp = new Dictionary<string, CurveGroup>();


         private CurveManager() { }


         private static CurveManager manager = null;
         public static CurveManager GetInstance()
        {
            if (manager == null)
            {
                manager = new CurveManager();
                string fieName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\记录曲线.rhhcfg");
                manager.Load(fieName);
            }
            return manager;
        }

         private void Load(string fileName)
         {
             if (File.Exists(fileName) == false) return;
             IniDocument ini = new IniDocument();
             ini.Load(fileName);
             int num = ini.GetInt("记录曲线类型", "数目", 0);
             for (int i = 0; i < num; i++)
             {
                 CurveGroup grp = new CurveGroup(ini, i);
                 if (grp.IsValid == false) continue;

                 if (dicCurveGrp.ContainsKey(grp.Name) == false)
                 {
                     dicCurveGrp.Add(grp.Name, grp);
                 }
             }


         }

         public List<DevCurve> GetCurves(string src, string devName)
         {
             if (dicCurveGrp.ContainsKey(src))
             {
                 return dicCurveGrp[src].GetCurves(devName);
             }

             return null;
         }
    }
}
