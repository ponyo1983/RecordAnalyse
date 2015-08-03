using System;
using System.Collections.Generic;
using System.Text;

namespace DataStorage
{
  public  interface IDataProcessBlock
    {
        bool Process();
        bool Wait(int milliSec);
    }
}
