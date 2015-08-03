using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using Common;


namespace DataStorage.Curve
{
    /// <summary>
    /// 记录曲线的存放
    /// </summary>
    class CurveManager
    {


        CurveFile curveFile;
        

        private static CurveManager singleton = null;

        Queue<IDataProcessBlock> queueBlock = new Queue<IDataProcessBlock>();
        AutoResetEvent eventBlk = new AutoResetEvent(false);

        Thread threadBlock;

        public static CurveManager GetInstance()
        {
            if (singleton == null) { singleton = new CurveManager(); }
            return singleton;
        }

        private CurveManager() { 
        
        }



        public string StoreDir
        {
            get;
            set;
        }

        public bool IsRunning
        {
            get;
            private set;
        }
        public void Start() {

            if (this.IsRunning) return;
            threadBlock = new Thread(new ThreadStart(ProcessBlock));
            threadBlock.IsBackground = true;
            threadBlock.Start();

            this.IsRunning = true;
        }

        public void Stop() {
        
        }


        public void AddCurve(CurveGroup scf)
        {
            if (scf == null) return;

            curveFile.AddCurves(scf);
        }


        public void AddBlock(IDataProcessBlock blk)
        {
            lock (((ICollection)queueBlock).SyncRoot)
            {
                if (queueBlock.Count > 1000)
                {
                    queueBlock.Dequeue();
                }
                queueBlock.Enqueue(blk);
                eventBlk.Set();
            }
        }

        private void ProcessBlock()
        {
            try
            {
                curveFile = new CurveFile(this);

                while (true)
                {
                    if (eventBlk.WaitOne(5000, false))
                    {
                        IDataProcessBlock[] blks = null;
                        lock (((ICollection)queueBlock).SyncRoot)
                        {
                            if (queueBlock.Count > 0)
                            {
                                blks = queueBlock.ToArray();
                                queueBlock.Clear();
                            }
                        }
                        if (blks != null)
                        {
                            for (int i = 0; i < blks.Length; i++)
                            {
                                if (blks[i].Process() == false) return;
                            }
                        }
                       
                    }
                }
            }
            catch (Exception) { 
            
            }
        }


        public List<DateTime> QueryCurveTimeList(int curveType, int index)
        {
            return curveFile.GetCurveTimeList(curveType, index);
        }

        public List<StationCurve> QueryCurveHistory(int curveType, int index, DateTime time)
        {
            CurveRequestBlock blk = new CurveRequestBlock(curveFile, curveType, index, time);

            AddBlock(blk);

            if (blk.Wait(2000))
            {
                return blk.Result;
            }
            return null;
        }

    
      



    }
}
