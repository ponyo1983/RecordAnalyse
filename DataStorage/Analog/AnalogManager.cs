using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.IO;


namespace DataStorage.Analog
{
    class AnalogDataManager
    {

        Dictionary<int, AnalogFile> dicFile = new Dictionary<int, AnalogFile>();

        Queue<IDataProcessBlock> queueDataBlock = new Queue<IDataProcessBlock>(); //

        ManualResetEvent blockReceivedEvent = new ManualResetEvent(false); //

        Queue<DataUnit> queueData = new Queue<DataUnit>();
        AutoResetEvent dataReceivedEvent = new AutoResetEvent(false);

        private AnalogDataManager() { }


        private static AnalogDataManager manager = null;


        public static AnalogDataManager GetInstance()
        {
            if (manager == null)
            {
                manager = new AnalogDataManager();
                manager.IsRunning = false;
                manager.Ready = false;
            }
            return manager;
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
        public void Start()
        {
            if (IsRunning) return;
            if (string.IsNullOrEmpty(this.StoreDir)) return;
            if (Directory.Exists(this.StoreDir) == false) return;
            
            Thread threadAddPoint = new Thread(new ThreadStart(ProcAddData));
            threadAddPoint.IsBackground = true;
            threadAddPoint.Start();

            Thread threadStore = new Thread(new ThreadStart(ProcStore));
            threadStore.IsBackground = true;
            threadStore.Start();

            IsRunning = true;
        }


        public void Stop()
        {
            FinishAll();

            AnalogStoreBlock blockStop = new AnalogStoreBlock();

            PutDataBlock(blockStop);

            blockStop.Wait(10000);
        }

        public List<List<AnalogPoint>> GetAnalogPoint(int type, int index, DateTime timeBegin, DateTime timeEnd)
        {
            int analogIndex = type * 10000 + index;
            if (dicFile.ContainsKey(analogIndex) == false)
            {
                AnalogFile fileAnalog = new AnalogFile(this, type, index);
                dicFile.Add(analogIndex, fileAnalog);
            }
          return  dicFile[analogIndex].GetAnalogPoint(timeBegin, timeEnd);
        }


        public List<DateTime> QueryAnalogTimes(int type, int index)
        {
            int analogIndex = type * 10000 + index;

            if (dicFile.ContainsKey(analogIndex) == false)
            {
                AnalogFile fileAnalog = new AnalogFile(this, type, index);
                dicFile.Add(analogIndex, fileAnalog);
            }
            
            return dicFile[analogIndex].QueryAnalogTimes();
            

        }

    
        /// <summary>
        /// 寻找现在已有的文件记录
        /// </summary>
        private void ProcAddData()
        {

      

            while (true)
            {
                
                DataUnit[] datas = null;
                lock (((ICollection)queueData).SyncRoot)
                {
                 datas=   queueData.ToArray();
                 queueData.Clear();
                }

                if (datas != null && datas.Length > 0)
                {
                    lock (dicFile)
                    {
                        for (int i = 0; i < datas.Length; i++)
                        {
                            int analogIndex = datas[i].Type * 10000 + datas[i].Index;
                            if (dicFile.ContainsKey(analogIndex) == false)
                            {

                                AnalogFile fileAnalog = new AnalogFile(this, datas[i].Type, datas[i].Index);
                                dicFile.Add(analogIndex, fileAnalog);
                                dicFile[analogIndex].AddAnalog(datas[i].Time, float.NaN, (byte)0x0f); //开始的第一个点加入无效数据
                            }
                            dicFile[analogIndex].AddAnalog(datas[i].Time, datas[i].Analog, datas[i].Digit);

                        }
                    }
                }
                else
                {
                    dataReceivedEvent.WaitOne();
                }

            }
        }

        public bool Ready
        {
            get;
            private set;
        }

        
        public void AddData(int type, int index, byte digit, float analog, DateTime time)
        {
            DataUnit unit = new DataUnit(type, index, digit, analog, time);
            lock (((ICollection)queueData).SyncRoot)
            {
                queueData.Enqueue(unit);
                dataReceivedEvent.Set();
            }
           

        }

        public void PutDataBlock(IDataProcessBlock block)
        {
            lock (((ICollection)queueDataBlock).SyncRoot)
            {
                queueDataBlock.Enqueue(block);
                blockReceivedEvent.Set();
            }
        }

        /// <summary>
        /// 存储过程
        /// </summary>
        private void ProcStore()
        {
            while (true)
            {
                if (blockReceivedEvent.WaitOne())
                {
                    IDataProcessBlock block = null;
                    lock (((ICollection)queueDataBlock).SyncRoot)
                    {
                        if (this.queueDataBlock.Count > 0)
                        {
                            block = queueDataBlock.Dequeue();
                        }
                        else
                        {
                            blockReceivedEvent.Reset();
                        }
                    }
                    if (block != null)
                    {
                       bool goon= block.Process();
                       if (!goon) break;
                    }
                }
            }
        }


        public void FinishAll()
        {
            foreach (AnalogFile file in dicFile.Values)
            {
                file.Flush(false);
            }
        }

    }


    class DataUnit
    {
        public DataUnit(int type, int index, byte digit, float analog, DateTime time)
        {
            this.Type = type;
            this.Index = index;
            this.Digit = digit;
            this.Analog = analog;
            this.Time = time;
        }

        public int Type
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }

        public byte Digit
        {
            get;
            private set;
        }

        public float Analog
        {
            get;
            private set;
        }

        public DateTime Time
        {
            get;
            private set;
        }

    }
}
