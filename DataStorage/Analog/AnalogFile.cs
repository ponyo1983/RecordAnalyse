using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections;


namespace DataStorage.Analog
{
    class AnalogFile
    {

        const int BlockNum = 2;

        AnalogDataManager dataManager;

        AnalogRecordFile fileData;
        AnalogIndexFile fileIndex;


        ManualResetEvent blockFinishedEvent = new ManualResetEvent(false);
        Queue<AnalogStoreBlock> queueBlock = new Queue<AnalogStoreBlock>();
        AnalogStoreBlock currentBlock = null;

        int reserveCount = 0;

       
        public AnalogFile(AnalogDataManager manager, int type,int index)
        {
            this.dataManager = manager;
            this.Type = type;
            this.Index = index;
            if (type == 0x42) //半自动电压和电流
            {
                this.MaxFileSize =100 * 10 * 1024 * 1024; //每天大约10M文件 按100天计算
            }
            else
            {
                this.MaxFileSize = 100 * 1024 * 1024; //每天大约1M文件
            }
            fileIndex = new AnalogIndexFile(this);
            fileData = new AnalogRecordFile(this);

            for (int i = 0; i < BlockNum; i++)
            {
                queueBlock.Enqueue(new AnalogStoreBlock(i, this));
            }
          
            Random rnd = new Random();

            this.reserveCount = rnd.Next(AnalogStoreBlock.MaxPoint);
            currentBlock = NewBlock(null, false);
            currentBlock.AddPrevNULL();

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

        public int MaxFileSize
        {
            get;
            private set;
        }

        public AnalogDataManager DataManager
        {
            get { return this.dataManager; }
        }


        public void AddAnalog(DateTime time, float value, byte digit)
        {
            if (currentBlock == null)
            {
                currentBlock = NewBlock(null,false);
            }
            int pointNum = currentBlock.AddAnalog(time, value, digit);
            if (pointNum >= 0)
            {
                if (pointNum + reserveCount >= AnalogStoreBlock.MaxPoint)
                {
                    Flush(false);
                }
            }
            else
            {
                this.dataManager.PutDataBlock(currentBlock);
                currentBlock = NewBlock(currentBlock, true);
                currentBlock.IndexRecord.Type = (pointNum == -2) ? FallbackType.Fallback : FallbackType.Normal;
                currentBlock.AddAnalog(time, value, digit);
            }

            
        }

        public AnalogRecordFile DataFile
        {
            get { return fileData; }
        }
        public AnalogIndexFile IndexFile
        {
            get { return fileIndex; }
        }


        public void PutBlock(AnalogStoreBlock block)
        {
            lock (((ICollection)queueBlock).SyncRoot)
            {
                queueBlock.Enqueue(block);
                blockFinishedEvent.Set();
            }
            
        }

        private AnalogStoreBlock GetBlock(int timeout)
        {
            AnalogStoreBlock block = null;
            lock (((ICollection)queueBlock).SyncRoot)
            {
                if (queueBlock.Count > 0)
                {
                    block = queueBlock.Dequeue();
                }
                else {
                    blockFinishedEvent.Reset();
                }
            }
            if (block != null || timeout == 0) return block;

            if (timeout > 0)
            {
                blockFinishedEvent.WaitOne(timeout, false);
            }
            else
            {
                blockFinishedEvent.WaitOne();
            }
            return GetBlock(0);
        }

        private AnalogStoreBlock NewBlock(AnalogStoreBlock oldBlock,bool newIndex)
        {
            AnalogStoreBlock newBlock = GetBlock(-1);
            if ((oldBlock == null) && (!newIndex))
            {
                newBlock.IndexRecord = fileIndex.LastIndex;
            }
            else if (newIndex)
            {
                newBlock.IndexRecord = fileIndex.NewIndex();
            }
            else
            {
                newBlock.IndexRecord = oldBlock.IndexRecord;
            }
            newBlock.Clear();
            return newBlock;
            
        }

        public void Flush(bool newOrder)
        {
            this.dataManager.PutDataBlock(currentBlock);
            currentBlock = NewBlock(currentBlock, false);
            reserveCount = 0;
            if (newOrder)
            {
                Random rand = new Random();
                reserveCount = rand.Next(AnalogStoreBlock.MaxPoint);
            }
        }
      

        public List<List<AnalogPoint>> GetAnalogPoint(DateTime timeBegin, DateTime timeEnd)
        {

            if (currentBlock != null && currentBlock.ConflictWith(timeBegin,timeEnd))
            {
                Flush(true);
            }
            AnalogRequestBlock block = new AnalogRequestBlock(this, timeBegin, timeEnd);

            dataManager.PutDataBlock(block);

            block.Wait(5000);

            return block.Result;

            
        }

        public List<DateTime> QueryAnalogTimes()
        {
            if (fileIndex != null)
            {
                return fileIndex.GetAnalogTimes();
            }
            return new List<DateTime>();
        }

    }
}
