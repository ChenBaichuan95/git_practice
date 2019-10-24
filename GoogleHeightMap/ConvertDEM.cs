using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GoogleHeightMap
{
    class ConvertDEM
    {
        string inPath = null;

        double[] lat = null;
        double[] lon = null;
        double[] h = null;

        public ConvertDEM(string fPath)
        {
            this.inPath = fPath;   
        }

        int lineCount = 0;

        public void convert()
        {
            

            return;
        }


        private void readFile()
        {
            using (FileStream fs = new FileStream(inPath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (sr.ReadLine() != null)
                    {
                        lineCount++;
                    }

                    fs.Position = 0;
                    lat = new double[lineCount];
                    lon = new double[lineCount];
                    h = new double[lineCount];

                    lineCount = 0;

                    string[] tmpLine = null;
                    string line = null;

                    while ((line = sr.ReadLine()) != null)
                    {
                        tmpLine = line.Split(',');

                        double.TryParse(tmpLine[0], out double tlat);
                        lat[lineCount] = tlat;

                        double.TryParse(tmpLine[1], out double tLon);
                        lon[lineCount] = tLon;

                        double.TryParse(tmpLine[2], out double th);
                        h[lineCount] = th;
                    }

                    sr.Close();
                    fs.Close();
                }
            }
        }


        public void writeFile()
        {
            using (FileStream fs = new FileStream(inPath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (sr.ReadLine() != null)
                    {
                        lineCount++;
                    }

                    fs.Position = 0;
                    lat = new double[lineCount];
                    lon = new double[lineCount];
                    h = new double[lineCount];

                    lineCount = 0;

                    string[] tmpLine = null;
                    string line = null;

                    while ((line = sr.ReadLine()) != null)
                    {
                        tmpLine = line.Split(',');

                        double.TryParse(tmpLine[0], out double tlat);
                        lat[lineCount] = tlat;

                        double.TryParse(tmpLine[1], out double tLon);
                        lon[lineCount] = tLon;

                        double.TryParse(tmpLine[2], out double th);
                        h[lineCount] = th;
                    }

                    sr.Close();
                    fs.Close();
                }
            }
        }

    }
}
