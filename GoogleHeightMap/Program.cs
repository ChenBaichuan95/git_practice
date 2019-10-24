using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleHeightMap
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = "D:\\data\\SPS测试数据\\吐哈盆地火焰山西段三维SPS";
           



            string path = "D:\\data\\SPS测试数据\\gaoshi1\\SPS";
            //string fielPath = "D:\\data\\lwm10.xyz";
            //string fielPath = "D:\\data\\LWM_0905_高程\\lwm1017.xyz";
            //string fielPath = "D:\\data\\长庆测试_高程\\cq1021.xyz";
            //string fielPath = "D:\\data\\高程扩大\\cqkd1022.xyz";
            string fielPath = "D:\\data\\高程扩大\\cqXian80.xyz";

            //string path = "D:\\data\\GaoSTsps";
            RWFiles r = new RWFiles(path, fielPath);
            r.getHeightInfo();

            GetGridIndex g = new GetGridIndex(r);
            g.createGrid();


            //xMax = 18535000;
            //xMin = 18583000;
            //yMax = 3369000;
            //yMin = 3337000;
           // GetZValue getValue = new GetZValue(r, g, 18583000, 18535000, 3369000, 3337000);
            //GetZValue getValue = new GetZValue(r, g, 19149387, 19117713, 4197723, 4162654);
            GetZValue getValue = new GetZValue(r, g, 19149399, 19117734, 4197720, 4162630);
            //GetZValue getValue = new GetZValue(r, g, 18678311, 18647743, 4190693, 4156671);
            
            getValue.ZInterpolation();

            //path = "D:\\data\\lwm1021.lay";
            //path = "D:\\data\\长庆测试_高程\\cq1021.lay";
            path = "D:\\data\\高程扩大\\cqXian1024.lay";
            r.writeZValue(path, getValue.outByte);
            
        }
    }
}
