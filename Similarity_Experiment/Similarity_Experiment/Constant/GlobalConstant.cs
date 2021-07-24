using NeoCortexApi;
using System;


namespace Similarity_Experiment.Constant
{

    struct GlobalConstant
    {
        /// <summary>
        /// This constans are for Neocortrex API
        /// </summary>
        public struct NeoCortrexGlobalConstant
        {
            public static int outImgSize = 1024;

            public static int[] colDims = new int[] { 64, 64 };

            public static int numOfCols = 64 * 64;
            public static int imgSize = 28;
            public static int counter = 0;
        }
        /// <summary>
        /// This constant set for images which will use for training and for output folder.
        /// </summary>
        public struct SimilarityExperimentGLobalConstant
        {
            public static string outputtPrefix = "OutputMatrix";
            public static string testOutputFolder = $"OutputOfSimilarityExperiment";
            public static string trainingFolder = "five";
        }

        public struct HtmConfigConstant
        {
            public static int[] InputDimensions = new int[] { GlobalConstant.NeoCortrexGlobalConstant.imgSize, GlobalConstant.NeoCortrexGlobalConstant.imgSize };
            public static int[] ColumnDimensions = new int[] { 64, 64 };
            public static int PotentialRadius = (int)(0.8 * GlobalConstant.NeoCortrexGlobalConstant.imgSize * GlobalConstant.NeoCortrexGlobalConstant.imgSize);
            public static double PotentialPct = 0.5;
            public static bool GlobalInhibition = true;
            public static double LocalAreaDensity = -1.0;
            public static double NumActiveColumnsPerInhArea = 0.02 * GlobalConstant.NeoCortrexGlobalConstant.numOfCols;
            public static double StimulusThreshold = 0.0;
            public static double SynPermInactiveDec = 0.008;
            public static double SynPermActiveInc = 0.05;
            public static double SynPermConnected = 0.10;
            public static double MinPctOverlapDutyCycles = 1.0;
            public static double MinPctActiveDutyCycles = 0.001;
            public static int DutyCyclePeriod = 50;
            public static double MaxBoost = 10.0;
            public static int RandomGenSeed = 42;
            public static Random Random = new ThreadSafeRandom(42);
        }
    }
}