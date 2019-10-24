using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleHeightMap
{
    class GetZValue
    {
        private Dictionary<int, List<int>> gridIndex = null;
        private double xMax = 0, xMin = 0, yMax = 0, yMin = 0;
        public float[] zBuffer = null;
        private GetGridIndex gdi = null;
        public byte[] outByte = null;
        private RWFiles r = null;

        private int mapMaxLevel = 13;
        //private int mapMinLevel = 19;
        private int mapMinLevel = 18;
        private int[] tileInfo = null;
        private int[] pointLevelInfo = null;
        private int pointCount = 0;
        private int[] samplingInfo = null;

        //private float[] gR = new float[7] { 19.1093 , 9.5546 , 4.7773 }

        public GetZValue(RWFiles r ,GetGridIndex gdi, double waXmax, double waXmin, double waYmax, double waYmin)
        {
            gridIndex = gdi.gridIndex;
            xMax = r.xMax;
            xMin = r.xMin;
            yMax = r.yMax;
            yMin = r.yMin;
            this.gdi = gdi;
            this.r = r;

            // SetWorkAreaBounding(waXmax, waXmin, waYmax, waYmin);

            xMax = waXmax;
            xMin = waXmin;
            yMax = waYmax;
            yMin = waYmin;
        }

      
        public void ZInterpolation()
        {
            int VertexXCount = 0, VertexYCount = 0;

            GetHeaderInfo();

            int mapLevelCount = mapMinLevel - mapMaxLevel + 1;
            int SamplingNum = 3;
            int SubDivisions = SamplingNum - 1;

            //samplingInfo = new int[7] { 17, 15, 15 , 15, 15, 13, 9 };   //minLevel ->19
            samplingInfo = new int[6] { 17, 15, 15 , 15, 15, 13};   //minLevel ->18

            int tileXNum = 0, tileYNum = 0;
            for (int i = 0; i < mapLevelCount; i++)
            {
                SamplingNum = samplingInfo[i];
                SubDivisions = SamplingNum - 1;

                //每级点数
                tileXNum = Math.Abs(tileInfo[i * 4 + 2] - tileInfo[i * 4]) + 1;
                tileYNum = Math.Abs(tileInfo[i * 4 + 3] - tileInfo[i * 4 + 1]) + 1;

                VertexXCount = tileXNum * SamplingNum - tileXNum + 1;
                VertexYCount = tileYNum * SamplingNum - tileYNum + 1;

                pointLevelInfo[i] = VertexXCount * VertexYCount;
                pointCount += pointLevelInfo[i];

                //每级的采样个数
                //samplingInfo[i] = 3;
            }

            ///////////////////
            //maxLevel->minLevel 2   || each Level minTileX minTileY XTileNum YTileNum || each Level PointCount || each Level SamplingNum || maxH && minH || zValue
            //int byteCount = 2 * 4 + mapLevelCount * 4 * 4 + mapLevelCount * 4 + mapLevelCount * 4 + pointCount;
            int byteCount = 2 * 4 + mapLevelCount * 4 * 4 + mapLevelCount * 4 + mapLevelCount * 4 + 8 + pointCount;
            outByte = new byte[byteCount];

            int count = 0;
            //maxLevel->minLevel 2 
            Array.Copy(BitConverter.GetBytes(mapMaxLevel), 0, outByte, count, 4);
            count += 4;
            Array.Copy(BitConverter.GetBytes(mapMinLevel), 0, outByte, count, 4);
            count += 4;

            //each Level minTileX minTileY XTileNum YTileNum
            int tileXCount = 0, tileYCount = 0;
            for (int i = 0; i < mapLevelCount; i++)
            {
                Array.Copy(BitConverter.GetBytes(tileInfo[i * 4]), 0, outByte, count, 4);
                count += 4;

                Array.Copy(BitConverter.GetBytes(tileInfo[i * 4 + 1]), 0, outByte, count, 4);
                count += 4;

                tileXCount = tileInfo[i * 4 + 2] - tileInfo[i * 4] + 1;
                Array.Copy(BitConverter.GetBytes(tileXCount), 0, outByte, count, 4);
                count += 4;

                tileYCount = tileInfo[i * 4 + 3] - tileInfo[i * 4 + 1] + 1;
                Array.Copy(BitConverter.GetBytes(tileYCount), 0, outByte, count, 4);
                count += 4;
            }

            //each Level PointCount
            for (int i = 0; i < mapLevelCount; i++)
            {
                Array.Copy(BitConverter.GetBytes(pointLevelInfo[i]), 0, outByte, count, 4);
                count += 4;
            }

            // each Level SamplingNum
            for (int i = 0; i < mapLevelCount; i++)
            {
                Array.Copy(BitConverter.GetBytes(samplingInfo[i]), 0, outByte, count, 4);
                count += 4;
            }

            Array.Copy(BitConverter.GetBytes(Convert.ToSingle(r.hMax)), 0, outByte, count, 4);
            count += 4;

            Array.Copy(BitConverter.GetBytes(Convert.ToSingle(r.hMin)), 0, outByte, count, 4);
            count += 4;

            float[] tmpTileXY = new float[2];
            double[] tmpGeoXY = new double[2];
            double hStep = r.hMax - r.hMin;
            float zValue = 0;
            int tmpLevel = 0;
            int tmpMinTileX = 0, tmpMinTileY = 0;

            //zValue
            for (int i = 0; i < mapLevelCount; i++)
            {
                tileXNum = tileInfo[i * 4 + 2] - tileInfo[i * 4] + 1;
                tileYNum = tileInfo[i * 4 + 3] - tileInfo[i * 4 + 1] + 1;

                tmpMinTileX = tileInfo[i * 4];
                tmpMinTileY = tileInfo[i * 4 + 1];
                tmpLevel = i + mapMaxLevel;

                SamplingNum = samplingInfo[i];
                SubDivisions = SamplingNum - 1;

                VertexXCount = tileXNum * SamplingNum - tileXNum + 1;
                VertexYCount = tileYNum * SamplingNum - tileYNum + 1;

                for (int y = 0; y < VertexYCount; y++)
                {
                    //tmpTileXY[1] = y / SubDivisions + (y % SubDivisions) / (float)(SamplingNum - 1) + tmpMinTileY;
                   
                    tmpTileXY[1] = y / SubDivisions + (y % SubDivisions) / (float)(SamplingNum - 1) + tmpMinTileY;

                    for (int x = 0; x < VertexXCount; x++)
                    {
                        tmpTileXY[0] = x / SubDivisions + (x % SubDivisions) / (float)(SamplingNum - 1) + tmpMinTileX;
                        TileSystem.tileXYToGeoXY(tmpTileXY, tmpGeoXY, tmpLevel);
                        zValue = gdi.getZvalueFromGrid(tmpGeoXY);
                        
                        byte hOffset = (byte)(255*(zValue - r.hMin) / hStep  );
                        outByte[count] = hOffset;
                        count++;
                    }
                }
            }
        }

        /*
        private void GetHeaderInfo()
        {
            int tXMax = 0, tXMin = 0, tYMax = 0, tYMin = 0;

            double[] minXY = new double[2];
            double[] minXY1 = new double[2];
            double[] maxXY = new double[2];
            double[] maxXY1 = new double[2];

            int[] tileXY = new int[2];

            int mapLevelCount = mapMinLevel - mapMaxLevel + 1;

            tileInfo = new int[mapLevelCount * 4];
            pointLevelInfo = new int[mapLevelCount];
           
            for (int i = 0; i < mapLevelCount; i++)
            {
                tXMax = int.MinValue;
                tXMin = int.MaxValue;
                tYMax = int.MinValue;
                tYMin = int.MaxValue;

                minXY[0] = xMin; minXY[1] = yMax;
                minXY1[0] = xMax; minXY1[1] = yMax;
                maxXY[0] = xMax; maxXY[1] = yMin;
                maxXY1[0] = xMin; maxXY1[1] = yMin;

                //int[] ttileXY = new int[2] { 6499, 1363 };
                //double[] tgeoXY = new double[2];
                //TileSystem.tileXYToGeoXY(ttileXY, tgeoXY, 13);
                //TileSystem.geoXYTolatLong(tgeoXY);


                //TileSystem.geoXYTolatLong(minXY);
                //TileSystem.geoXYTolatLong(minXY1);
                //TileSystem.geoXYTolatLong(maxXY);
                //TileSystem.geoXYTolatLong(maxXY1);

                tileXY[0] = 0; tileXY[1] = 0;
                TileSystem.geoXYToTileXY(minXY, tileXY, i + mapMaxLevel);
                if (tXMax < tileXY[0]) tXMax = tileXY[0];
                if (tXMin > tileXY[0]) tXMin = tileXY[0];
                if (tYMax < tileXY[1]) tYMax = tileXY[1];
                if (tYMin > tileXY[1]) tYMin = tileXY[1];

                tileXY[0] = 0;tileXY[1] = 0;

                TileSystem.geoXYToTileXY(minXY1, tileXY, i + mapMaxLevel);
                if (tXMax < tileXY[0]) tXMax = tileXY[0];
                if (tXMin > tileXY[0]) tXMin = tileXY[0];
                if (tYMax < tileXY[1]) tYMax = tileXY[1];
                if (tYMin > tileXY[1]) tYMin = tileXY[1];
                tileXY[0] = 0; tileXY[1] = 0;

                TileSystem.geoXYToTileXY(maxXY, tileXY, i + mapMaxLevel);
                if (tXMax < tileXY[0]) tXMax = tileXY[0];
                if (tXMin > tileXY[0]) tXMin = tileXY[0];
                if (tYMax < tileXY[1]) tYMax = tileXY[1];
                if (tYMin > tileXY[1]) tYMin = tileXY[1];
                tileXY[0] = 0; tileXY[1] = 0;

                TileSystem.geoXYToTileXY(maxXY1, tileXY, i + mapMaxLevel);
                if (tXMax < tileXY[0]) tXMax = tileXY[0];
                if (tXMin > tileXY[0]) tXMin = tileXY[0];
                if (tYMax < tileXY[1]) tYMax = tileXY[1];
                if (tYMin > tileXY[1]) tYMin = tileXY[1];

                tileInfo[i * 4] = tXMin;
                tileInfo[i * 4 + 1] = tYMin;
                tileInfo[i * 4 + 2] = tXMax;
                tileInfo[i * 4 + 3] = tYMax;



                if (i == 0)
                {
                    int[] tXY = { tXMin, tYMax + 1 };
                    double[] gXY = new double[2];
                    TileSystem.tileXYToGeoXY(tXY, gXY, mapMaxLevel);
                    xMin = gXY[0]; yMin = gXY[1];

                    tXY[0] = tXMax + 1; tXY[1] = tYMin;
                    TileSystem.tileXYToGeoXY(tXY, gXY, mapMaxLevel);
                    xMax = gXY[0];yMax = gXY[1];
                }




            }
        }
        */

        private void GetHeaderInfo()
        {
            int tXMax = 0, tXMin = 0, tYMax = 0, tYMin = 0;

            double[] minXY = new double[2];
            double[] minXY1 = new double[2];
            double[] maxXY = new double[2];
            double[] maxXY1 = new double[2];

            int[] tileXY = new int[2];
            int mapLevelCount = mapMinLevel - mapMaxLevel + 1;

            tileInfo = new int[mapLevelCount * 4];
            pointLevelInfo = new int[mapLevelCount];

            tXMax = int.MinValue;
            tXMin = int.MaxValue;
            tYMax = int.MinValue;
            tYMin = int.MaxValue;

            minXY[0] = xMin; minXY[1] = yMax;
            minXY1[0] = xMax; minXY1[1] = yMax;
            maxXY[0] = xMax; maxXY[1] = yMin;
            maxXY1[0] = xMin; maxXY1[1] = yMin;

            //int testPX = 0, testPY = 0;
            ////double testLa = 0, testLong = 0;
            ////TileSystem.TileXYToPixelXY(6492, 3371, out testPX, out testPY);
            ////TileSystem.PixelXYToLatLong(testPX, testPY, 13,out testLa,out testLong);

            //int testTx = 0, testTy = 0;
            //TileSystem.LatLongToPixelXY( 37.806027, 106.684280, 13, out testPX, out testPY);
            //TileSystem.PixelXYToTileXY(testPX, testPY, out testTx, out testTy);


            //TileSystem.latlng2xy(37.806027, 106.684280, out testPX, out testPY);
            //TileSystem.PixelXYToTileXY(testPX, testPY, out testTx, out testTy);

            tileXY[0] = 0; tileXY[1] = 0;
            TileSystem.geoXYToTileXY(minXY, tileXY, mapMaxLevel);
            if (tXMax < tileXY[0]) tXMax = tileXY[0];
            if (tXMin > tileXY[0]) tXMin = tileXY[0];
            if (tYMax < tileXY[1]) tYMax = tileXY[1];
            if (tYMin > tileXY[1]) tYMin = tileXY[1];

            tileXY[0] = 0; tileXY[1] = 0;

            TileSystem.geoXYToTileXY(minXY1, tileXY, mapMaxLevel);
            if (tXMax < tileXY[0]) tXMax = tileXY[0];
            if (tXMin > tileXY[0]) tXMin = tileXY[0];
            if (tYMax < tileXY[1]) tYMax = tileXY[1];
            if (tYMin > tileXY[1]) tYMin = tileXY[1];
            tileXY[0] = 0; tileXY[1] = 0;

            TileSystem.geoXYToTileXY(maxXY, tileXY, mapMaxLevel);
            if (tXMax < tileXY[0]) tXMax = tileXY[0];
            if (tXMin > tileXY[0]) tXMin = tileXY[0];
            if (tYMax < tileXY[1]) tYMax = tileXY[1];
            if (tYMin > tileXY[1]) tYMin = tileXY[1];
            tileXY[0] = 0; tileXY[1] = 0;

            TileSystem.geoXYToTileXY(maxXY1, tileXY, mapMaxLevel);
            if (tXMax < tileXY[0]) tXMax = tileXY[0];
            if (tXMin > tileXY[0]) tXMin = tileXY[0];
            if (tYMax < tileXY[1]) tYMax = tileXY[1];
            if (tYMin > tileXY[1]) tYMin = tileXY[1];

            tileInfo[0] = tXMin;
            tileInfo[1] = tYMin;
            tileInfo[2] = tXMax;
            tileInfo[3] = tYMax;

            string leftTopStr, rightBottomStr;
            leftTopStr = TileSystem.TileXYToQuadKey(tXMin, tYMin, mapMaxLevel);
            rightBottomStr = TileSystem.TileXYToQuadKey(tXMax, tYMax, mapMaxLevel);

            int tmpLevel = 0, tmpMinX = 0, tmpMinY = 0, tmpMaxX = 0, tmpMaxY = 0;
            for (int i = 1; i < mapLevelCount; i++)
            {
                leftTopStr += '0';
                rightBottomStr += '3';

                tmpLevel = i + mapMaxLevel;
                TileSystem.QuadKeyToTileXY(leftTopStr,out tmpMinX, out tmpMinY, out tmpLevel);
                TileSystem.QuadKeyToTileXY(rightBottomStr, out tmpMaxX, out tmpMaxY, out tmpLevel);

                tileInfo[i * 4] = tmpMinX;
                tileInfo[i * 4 + 1] = tmpMinY;
                tileInfo[i * 4 + 2] = tmpMaxX;
                tileInfo[i * 4 + 3] = tmpMaxY;
            }

        }
    }
}
