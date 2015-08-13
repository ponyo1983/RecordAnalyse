using System;
using System.Collections.Generic;
using System.Text;
using DataStorage.Analog;
using System.IO;
using System.Threading;
using System.Diagnostics;
using DataStorage.Curve;
using Common;



namespace DataStorage
{
    public class DatabaseModule 
    {


        static DatabaseModule module = null;

        private DatabaseModule()
        {
            try
            {
                string storeDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                if (Directory.Exists(storeDir) == false)
                {
                    Directory.CreateDirectory(storeDir);
                }
                string F1 = storeDir;
                if (Directory.Exists(F1) == false)
                {
                    Directory.CreateDirectory(F1);
                }
            
                string analogDir = Path.Combine(F1, "Analog");
                if (Directory.Exists(analogDir) == false)
                {
                    Directory.CreateDirectory(analogDir);
                }
               
                string curveDir = Path.Combine(F1, "Curve");
                if (Directory.Exists(curveDir) == false)
                {
                    Directory.CreateDirectory(curveDir);
                }

                CurveManager.GetInstance().StoreDir = curveDir;
            
                AnalogDataManager.GetInstance().StoreDir = analogDir;
   
            }
            catch(Exception ex)
            { 
            }
            
           

        }

        public static DatabaseModule GetInstance()
        {
            if (module == null)
            {
                module = new DatabaseModule();
                module.Start();
            }

            return module;
        }

        public  bool Start()
        {

        
            CurveManager.GetInstance().Start();
     
       
            AnalogDataManager.GetInstance().Start();
           
        
            return true;
        }


        public void Stop()
        {
            CurveManager.GetInstance().Stop();
            AnalogDataManager.GetInstance().Stop();
        }
      

   


    


   

    


        public void AddCurve(CurveGroup cg)
        {

            CurveManager manager = CurveManager.GetInstance();

            manager.AddCurve(cg);

        }

      

     

        public void AddAnalog(int type, int index, float analogVal, DateTime time)
        {
            AnalogDataManager.GetInstance().AddData(type, index, 0, analogVal, time);
        }


        public void AddCurve(int type, int index,DateTime time, int sampleRate, float[] data)
        {
            CurveGroup grp=new CurveGroup(type,index,time,sampleRate,data);

            CurveManager.GetInstance().AddCurve(grp);
        }


       
        public  bool Exit()
        {
            AnalogDataManager.GetInstance().Stop();

            CurveManager.GetInstance().Stop();
       

            return true;
        }

        #region IDataStoreModule 成员

     

        public DateTime QueryStartTime()
        {
            return DateTime.Now;
        }

    

     

        public List<AnalogRecordGroup> QueryAnalogHistory(int analogType, int analogIndex, DateTime beginTime, DateTime endTime)
        {
          
            List<List<AnalogPoint>> listAll = AnalogDataManager.GetInstance().GetAnalogPoint(analogType, analogIndex, beginTime, endTime);
        
            int listIndex = -1;
            if (listAll != null)
            {
                List<AnalogRecordGroup> listGrp = new List<AnalogRecordGroup>();
                for (int i = 0; i < listAll.Count; i++)
                {
                    List<AnalogPoint> listPoint = listAll[i];
                    if (listPoint != null)
                    {
                        listIndex++;
                        List<AnalogRecord> listRecord = new List<AnalogRecord>();
                        for (int j = 0; j < listPoint.Count; j++)
                        {
                            listRecord.Add(new AnalogRecord(listPoint[j].Time, listPoint[j].AnalogValue));
                            
                        }

                        listGrp.Add(new AnalogRecordGroup(listIndex, listRecord));
                    }
                }
                return listGrp;
            }

            return null;
        }

     

        public List<DateTime> QueryAnalogTimes(int analogType, int analogIndex)
        {
            return AnalogDataManager.GetInstance().QueryAnalogTimes(analogType, analogIndex);
        }

     


        Dictionary<string, AnalogReportType> reportTypeDict = new Dictionary<string, AnalogReportType>();

        public enum AnalogReportType
        {
            OTHER = 0,      //其他
            GD = 1,         //轨道电压
            XW = 2,         //轨道相位角
            DS = 3,         //灯丝继电器电流
            DM = 4,         //电码化电流
            DC = 5,         //道岔表示电压
            BZ = 6          //半自动闭塞
        }

        public static string GetModuleConfig()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly();

            string fileName = Path.GetFileNameWithoutExtension(assembly.Location) + ".ini";

            string tempPath = System.IO.Path.GetDirectoryName(assembly.Location);
            string configFileName = System.IO.Path.Combine(tempPath, fileName);

            return configFileName;

        }


       
   


  


  



    




        public List<DateTime> QueryCurveTimeList(int curveType, int index)
        {
            return CurveManager.GetInstance().QueryCurveTimeList(curveType, index);
        }

        public List<StationCurve> QueryCurveHistory(int curveType, int index, DateTime time)
        {
            return CurveManager.GetInstance().QueryCurveHistory(curveType, index,time);
        }


        #endregion

    }
}
