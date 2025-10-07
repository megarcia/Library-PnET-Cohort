using System;

namespace Landis.Library.PnETCohorts
{
    public class BiomassUtil
    {
        private float[] biomassData;
        private int biomassNum;
        private float biomassThreshold;

        public float BiomassThreshold
        {
            get
            {
                return biomassThreshold;
            }
            set
            {
                biomassThreshold = value;
            }
        }

        public BiomassUtil()
        {
        }

        public float GetBiomassData(int i, int j)
        {
            if (i > biomassNum || j < 1 || j > 2)
                throw new Exception("index error at GetBiomass");
            return biomassData[(i - 1) * 2 + j - 1];
        }

        public void SetBiomassData(int i, int j, float value)
        {
            if (i > biomassNum || j < 1 || j > 2)
                throw new Exception("index error at SetBiomass");
            biomassData[(i - 1) * 2 + j - 1] = value;
        }

        public void SetBiomassNum(int num)
        {
            biomassNum = num;
            biomassData = null;
            biomassData = new float[num * 2];
        }
    }
}
