using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace GoogleHeightMap
{
    class RWFiles
    {
        private string directoryPath = null;
        public List<double> pointInfo = new List<double>();

        public double xMax = double.MinValue;
        public double xMin = double.MaxValue;
        public double yMax = double.MinValue;
        public double yMin = double.MaxValue;
        public double hMax = double.MinValue;
        public double hMin = double.MaxValue;

        //private int zoneNum = 15000000d;
        private double zoneNum = 18000000d;
        private string inPath = null;

        public RWFiles(string directoryPath, string inPath)
        {
            this.directoryPath = directoryPath;
            this.inPath = inPath;

            //double[] tmpGeo = new double[2] { 18545416.54, 3325177.3 };
            //TileSystem.geoXYTolatLong(tmpGeo);
        }

        public void getHeightInfo()
        {
            //readSR("*.R");
            //readSR("*.S");
            readDEM();
        }

        private void readDEM()
        {
            using (FileStream fs = new FileStream(inPath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {                 
                    string[] tmpLine = null;
                    string line = null;

                    while ((line = sr.ReadLine()) != null)
                    {
                        tmpLine = line.Split(',');

                        double.TryParse(tmpLine[0], out double x);
                        double.TryParse(tmpLine[1], out double y);
                        double.TryParse(tmpLine[2], out double h);

                        pointInfo.Add(x);
                        pointInfo.Add(y);
                        pointInfo.Add(h);

                        if (xMax < x) xMax = x;
                        if (xMin > x) xMin = x;
                        if (yMax < y) yMax = y;
                        if (yMin > y) yMin = y;
                        if (hMax < h) hMax = h;
                        if (hMin > h) hMin = h;


                    }

                    sr.Close();
                    fs.Close();
                }
            }
        }




        private void readSR(string fileType)
        {
            try
            {
                string[] files = Directory.GetFiles(this.directoryPath, fileType); //???

                foreach (string s in files)
                {
                    using (StreamReader srFile = new StreamReader(s))
                    {
                        string line;
                        while ((line = srFile.ReadLine()) != null)
                        {
                            if (line.StartsWith("H"))
                                continue;

                            if (line.Length <= 72)
                                continue;

                            if (!double.TryParse(line.Substring(47, 8), out double x))
                                continue;
                            x += zoneNum;
                            if (!double.TryParse(line.Substring(56, 9), out double y))
                                continue;
                            if (!double.TryParse(line.Substring(66, 6), out double h))
                                continue;

                            pointInfo.Add(x);
                            pointInfo.Add(y);
                            pointInfo.Add(h);

                            if (xMax < x) xMax = x;
                            if (xMin > x) xMin = x;
                            if (yMax < y) yMax = y;
                            if (yMin > y) yMin = y;
                            if (hMax < h) hMax = h;
                            if (hMin > h) hMin = h;
                        }
                        srFile.Close();
                    }
                    
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public void writeZValue(string path,byte[] zBuffer)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        //int zLen = zBuffer.Length;
                        ////double hStep = hMax - hMin;
                        //byte tmpZ = 0;

                        //for (int i = 0; i < zLen; i++)
                        //{
                        //    tmpZ = Convert.ToByte(((zBuffer[i] - hMin) / hStep) * 256);
                        //    bw.Write(tmpZ);
                        //}
                        bw.Write(zBuffer);

                        bw.Close();
                        fs.Close();
                    }
                }
            }
            catch(Exception e)
            {

            }
            
        }
    }
}
