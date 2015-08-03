using System;
using System.Collections.Generic;
using System.Text;

namespace RecordAnalyse.DSP
{
    interface IFilter
    {
        float[] Process(float[] origSignal, int offset, int length);
    }
}
