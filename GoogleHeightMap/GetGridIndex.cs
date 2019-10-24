using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleHeightMap
{
    class GetGridIndex
    {
        private double xMax = 0, xMin = 0, yMax = 0, yMin = 0;
        public Dictionary<int, List<int>> gridIndex = new Dictionary<int, List<int>>();
        private List<double> heightInfo = null;

        double xInterval = 0, yInterval = 0;
        int gridXCount = 100, gridYCount = 100;
        float buffer = 40;
        RWFiles m_r = null;
        public GetGridIndex(RWFiles r)
        {
            m_r = r;
            xMax = r.xMax;
            xMin = r.xMin;
            yMax = r.yMax;
            yMin = r.yMin;
            heightInfo = r.pointInfo;
        }

        public void createGrid()
        {
            double xLen = xMax - xMin;
            xInterval = xLen / gridXCount;

            double yLen = yMax - yMin;
            yInterval = yLen / gridYCount;

            int pointNum = heightInfo.Count / 3;
            for (int i = 0; i < pointNum; i++)
            {
                double xOffset = heightInfo[3 * i] - xMin;
                double yOffset = heightInfo[3 * i + 1] - yMin;

                int xGrid = Convert.ToInt32(Math.Floor(xOffset / xInterval));
                int yGrid = Convert.ToInt32(Math.Floor(yOffset / yInterval));

                //int xGeo = Convert.ToInt32(Math.Floor(xGrid * xInterval) + xMin);
                //int yGeo = Convert.ToInt32(Math.Floor(yGrid * yInterval) + yMin);

                int serial = gridXCount * yGrid + xGrid;

                {
                    List<int> dList = null;
                    if (!gridIndex.ContainsKey(serial))
                    {
                        dList = new List<int>();
                        gridIndex.Add(serial, dList);
                    }
                    else
                        dList = gridIndex[serial];
                    dList.Add(i);
                }

                int xGrid1 = (int)((xOffset-buffer) / xInterval);
                if (xGrid1 != xGrid)
                {
                    serial = gridXCount * yGrid + xGrid1;
                    List<int> dList = null;
                    if (!gridIndex.ContainsKey(serial))
                    {
                        dList = new List<int>();
                        gridIndex.Add(serial, dList);
                    }
                    else
                        dList = gridIndex[serial];
                    dList.Add(i);
                }

                 xGrid1 = (int)((xOffset + buffer) / xInterval);
                if (xGrid1 != xGrid)
                {
                    serial = gridXCount * yGrid + xGrid1;
                    List<int> dList = null;
                    if (!gridIndex.ContainsKey(serial))
                    {
                        dList = new List<int>();
                        gridIndex.Add(serial, dList);
                    }
                    else
                        dList = gridIndex[serial];
                    dList.Add(i);
                }


                int yGrid1 = (int)((yOffset-buffer) / yInterval);
                if (yGrid1 != yGrid)
                {
                    serial = gridXCount * yGrid1 + xGrid;
                    List<int> dList = null;
                    if (!gridIndex.ContainsKey(serial))
                    {
                        dList = new List<int>();
                        gridIndex.Add(serial, dList);
                    }
                    else
                        dList = gridIndex[serial];
                    dList.Add(i);
                }

                yGrid1 = (int)((yOffset + buffer) / yInterval);
                if (xGrid1 != xGrid)
                {
                    serial = gridXCount * yGrid1 + xGrid;
                    List<int> dList = null;
                    if (!gridIndex.ContainsKey(serial))
                    {
                        dList = new List<int>();
                        gridIndex.Add(serial, dList);
                    }
                    else
                        dList = gridIndex[serial];
                    dList.Add(i);
                }
                //dList.Add(heightInfo[i * 3]);
                //dList.Add(heightInfo[i * 3 + 1]);
                //dList.Add(heightInfo[i * 3 + 2]);

            }
        }


        public float getZvalueFromGrid(double[] geoXY)
        {

            //geoXY[0] -= 15000000;

            if (geoXY[0] < xMin || geoXY[0] > xMax || geoXY[1] < yMin || geoXY[1] > yMax)
            {
                return 0;
            }
            else
            {
                double xOffset = geoXY[0] - xMin;
                double yOffset = geoXY[1] - yMin;

                int xGrid = Convert.ToInt32(Math.Floor(xOffset / xInterval));
                int yGrid = Convert.ToInt32(Math.Floor(yOffset / yInterval));

                int tmpSerial = yGrid * gridXCount + xGrid;

                if (!gridIndex.ContainsKey(tmpSerial))
                {
                    return 0;
                }
                else
                {
                    List<int> dataIndexInGrid = gridIndex[tmpSerial];
                    int inPointCount = dataIndexInGrid.Count;
                    float minDis = float.MaxValue;
                    int minDisSerial = 0;
                    float tmpDis = 0;

                    for (int i = 0; i < inPointCount; i++)
                    {
                        int dataIdx = dataIndexInGrid[i];

                        tmpDis = disBetweenPoints(geoXY[0], geoXY[1], m_r.pointInfo[3 * dataIdx], m_r.pointInfo[3 * dataIdx + 1]);

                        if (minDis > tmpDis)
                        {
                            minDis = tmpDis;
                            minDisSerial = dataIdx;
                        }
                    }

                    return Convert.ToSingle(m_r.pointInfo[minDisSerial * 3 + 2]);
                }
            }
        }


        /*
    public float getZvalueFromGrid(double[] geoXY)
    {

        //geoXY[0] -= 15000000;

        if(geoXY[0]<xMin || geoXY[0]>xMax ||geoXY[1] <yMin || geoXY[1] > yMax)
        {
            return 0;
        }
        else
        {
            double xOffset = geoXY[0] - xMin;
            double yOffset = geoXY[1] - yMin;

            int xGrid = Convert.ToInt32(Math.Floor(xOffset / xInterval));
            int yGrid = Convert.ToInt32(Math.Floor(yOffset / yInterval));

            int tmpSerial = yGrid * gridXCount + xGrid;

            if (!gridIndex.ContainsKey(tmpSerial))
            {
                return 0;
            }
            else
            {
                List<int> dataIndexInGrid = gridIndex[tmpSerial];
                int inPointCount = dataIndexInGrid.Count ;
                //float minDis = float.MaxValue;
                //int minDisSerial = 0;
                float tmpDis = 0;

                float[] d4= new float[4];
                d4[0] = float.MaxValue; d4[1] = float.MaxValue; d4[2] = float.MaxValue; d4[3] = float.MaxValue;
                int[] s4 = new int[4]; // s1 = 0, s2 = 0, s3 = 0, s4=0;
                float[] z4 = new float[4]; 
                int dataIdx = 0;
                float sum = 0;
                float zValue = 0;
                int validNum = 0;

                //for (int i=0;i<inPointCount;i++)
                //{
                //    dataIdx = dataIndexInGrid[i];

                //    tmpDis = disBetweenPoints(geoXY[0], geoXY[1], m_r.pointInfo[3 * dataIdx], m_r.pointInfo[3 * dataIdx+1]);

                //    if(d4[0] > tmpDis)
                //    {
                //        d4[0] = tmpDis;
                //        s4[0] = dataIdx;
                //        validNum = 1;
                //    }
                //}

                //for (int i = 0; i < inPointCount; i++)
                //{
                //    dataIdx = dataIndexInGrid[i];

                //    tmpDis = disBetweenPoints(geoXY[0], geoXY[1], m_r.pointInfo[3 * dataIdx], m_r.pointInfo[3 * dataIdx + 1]);

                //    if(tmpDis > d4[0])
                //    {
                //        if(d4[1] > tmpDis)
                //        {
                //            d4[1] = tmpDis;
                //            s4[1] = dataIdx;
                //            validNum = 2;
                //        }
                //    }
                //}

                //for (int i = 0; i < inPointCount; i++)
                //{
                //    dataIdx = dataIndexInGrid[i];

                //    tmpDis = disBetweenPoints(geoXY[0], geoXY[1], m_r.pointInfo[3 * dataIdx], m_r.pointInfo[3 * dataIdx + 1]);

                //    if (tmpDis > d4[1])
                //    {
                //        if(d4[2] > tmpDis)
                //        {
                //            d4[2] = tmpDis;
                //            s4[2] = dataIdx;
                //            validNum = 3;
                //        }
                //    }
                //}

                //for (int i = 0; i < inPointCount; i++)
                //{
                //    dataIdx = dataIndexInGrid[i];

                //    tmpDis = disBetweenPoints(geoXY[0], geoXY[1], m_r.pointInfo[3 * dataIdx], m_r.pointInfo[3 * dataIdx + 1]);

                //    if (tmpDis > d4[2])
                //    {
                //        if (d4[3] > tmpDis)
                //        {
                //            d4[3] = tmpDis;
                //            s4[3] = dataIdx;
                //            validNum = 4;
                //        }
                //    }
                //}

                //if (validNum == 4)
                //{
                //    validNum += 0;
                //}
                sum = 0;
                if (validNum < 1)
                    return 0;
                else if (validNum < 2)
                    return z4[0];
                else
                {
                    for (int i = 0; i < validNum; i++)
                    {
                        sum += d4[i] * d4[i];
                        z4[i] = Convert.ToSingle(m_r.pointInfo[s4[i] * 3 + 2]);
                    }

                    zValue = 0;
                    for (int i = 0; i < validNum; i++)
                    {
                        zValue += (d4[validNum - 1 - i] * d4[validNum - 1 - i]) * z4[i] / sum;
                    }
                }

                return zValue;
            }
        }
    }
    */

        private float disBetweenPoints(double p0x,double p0y,double p1x,double p1y)
        {
            return Convert.ToSingle(Math.Sqrt(Math.Pow(p0x - p1x, 2) + Math.Pow(p0y - p1y, 2)));
        }

    }
}
