using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using NeoCortex;
using Similarity_Experiment.Constant;
using ConsoleTables;
using System.Linq;

namespace Similarity_Experiment
{
    class Similarity
    {
        /// <summary>
        /// This test do spatial pooling and save hamming distance, active columns 
        /// and speed of processing in text files in Output diyrectory.
        /// </summary>
        
        internal void SimilarityExperiment()
        {
            ///<param name="trainingImages">It extracts all the images stored in specific folder to binarise, vector genration and calculation of similarity </param>
            var trainingImages = GetFileFromPath(GlobalConstant.SimilarityExperimentGLobalConstant.trainingFolder);
            
            string outPutFolderip = CreateADirectory();

            ///<param name="newlistip">Stores the input vectors for the images given as an input</param>
            List<int[]> newlistip = new List<int[]>();  
            
            ///<param name="trainingImageip">The images stored 'trainingImages' are passed here</param>
            //Binarizes the images and stores them in the folder "Output of Similarity matrix"
            //input vectors are generated for the binarised images
            foreach (var trainingImageip in trainingImages)
            {
                int[] activeArray = new int[GlobalConstant.NeoCortrexGlobalConstant.numOfCols];
                FileInfo fII = new FileInfo(trainingImageip);

                string outputImage = $"{outPutFolderip}\\{GlobalConstant.SimilarityExperimentGLobalConstant.outputtPrefix}_cycle_{GlobalConstant.NeoCortrexGlobalConstant.counter}_{fII.Name}";
                string testName = $"{outPutFolderip}\\{GlobalConstant.SimilarityExperimentGLobalConstant.outputtPrefix}_{fII.Name}";
                string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{trainingImageip}", GlobalConstant.NeoCortrexGlobalConstant.imgSize, testName);
                int[] inputVector2 = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();
                newlistip.Add(ArrayUtils.IndexWhere(inputVector2,(el)=> el==1));
            }
            
            if (trainingImages.Length > 0)
            {
                bool isInStableState = false; 

                HtmConfig htmConfig = new HtmConfig() //Defines the parameters of the Spatial Pooler which will be used to generate stable SDRs
                {
                    InputDimensions = GlobalConstant.HtmConfigConstant.InputDimensions,
                    ColumnDimensions = GlobalConstant.HtmConfigConstant.ColumnDimensions,
                    PotentialRadius = GlobalConstant.HtmConfigConstant.PotentialRadius,
                    PotentialPct = GlobalConstant.HtmConfigConstant.PotentialPct,
                    GlobalInhibition = GlobalConstant.HtmConfigConstant.GlobalInhibition,
                    LocalAreaDensity = GlobalConstant.HtmConfigConstant.LocalAreaDensity,
                    NumActiveColumnsPerInhArea = GlobalConstant.HtmConfigConstant.NumActiveColumnsPerInhArea,
                    StimulusThreshold = GlobalConstant.HtmConfigConstant.StimulusThreshold,
                    SynPermInactiveDec = GlobalConstant.HtmConfigConstant.SynPermInactiveDec,
                    SynPermActiveInc = GlobalConstant.HtmConfigConstant.SynPermActiveInc,
                    SynPermConnected = GlobalConstant.HtmConfigConstant.SynPermConnected,
                    MinPctOverlapDutyCycles = GlobalConstant.HtmConfigConstant.MinPctOverlapDutyCycles,
                    MinPctActiveDutyCycles = GlobalConstant.HtmConfigConstant.MinPctActiveDutyCycles,
                    DutyCyclePeriod = GlobalConstant.HtmConfigConstant.DutyCyclePeriod,
                    MaxBoost = GlobalConstant.HtmConfigConstant.MaxBoost,
                    RandomGenSeed = GlobalConstant.HtmConfigConstant.RandomGenSeed,
                    Random = GlobalConstant.HtmConfigConstant.Random

                };

                Connections connections = new Connections(htmConfig); //A new instance is created for the parametrs of SP

                HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(connections, trainingImages.Length * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
                {
                    // Event should only be fired when entering the stable state.
                    // Ideal SP should never enter unstable state after stable state.
                    isInStableState = true;
                    Debug.WriteLine($"Entered STABLE state: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                });


                SpatialPooler spatialPooler = new SpatialPoolerMT(hpa);
                spatialPooler.Init(connections); //Initializes the Spatial Pooler and its parameters are passed
                string outPutFolder = CreateADirectory();
                if (outPutFolder.Length > 0)
                {

                    int cycle = 0;
                    ///<param name="allSDRs">An empty list to hold output vectors generated after the SP is trained to calculate output correlation </param>
                    List<int[]> allSDRs = new List<int[]>(); 

                
                    while (true)
                    {
                        //This loop extracts images passed as input, genrates input vector and passes it to SP
                        foreach (var trainingImage in trainingImages)
                        {
                            int[] activeArray = new int[GlobalConstant.NeoCortrexGlobalConstant.numOfCols];

                            FileInfo fI = new FileInfo(trainingImage);

                            string outputImage = $"{outPutFolder}\\{GlobalConstant.SimilarityExperimentGLobalConstant.outputtPrefix}_cycle_{GlobalConstant.NeoCortrexGlobalConstant.counter}_{fI.Name}";

                            string testName = $"{outPutFolder}\\{GlobalConstant.SimilarityExperimentGLobalConstant.outputtPrefix}_{fI.Name}";

                            string inputBinaryImageFile = NeoCortexUtils.BinarizeImage($"{trainingImage}", GlobalConstant.NeoCortrexGlobalConstant.imgSize, testName);

                            // Read input csv file into array
                            int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

                            int[] oldArray = new int[activeArray.Length];


                            //SP uses these vectors and from its default parameters and generates a stable SDR
                            //It compares the active array and generated input vectors of the Binarised image
                            spatialPooler.compute(inputVector, activeArray, true);

                            //Stores the indices of the active array of value 1
                            var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);

                            Debug.WriteLine($"Cycle: {cycle++} - Input: {trainingImage}");
                            Debug.WriteLine($"{Helpers.StringifyVector(activeCols)}\n");

                            //When SP reaches stable state, it calculates the similarity between the images
                            if (isInStableState)
                            {
                                if (allSDRs.Count == trainingImages.Length)
                                {
                                    GenerateResult(allSDRs, newlistip); //It takes output vectors and input vectors to find output and input similarity respectively
                                    return;
                                }

                                if (activeArray.Length > 0)
                                {
                                    allSDRs.Add(activeCols);
                                    
                                }
                                else
                                {
                                    throw new ExceptionHandler(ConstantOfCustomeException.Empty_ActiveArray_Exception);
                                }
                            }

                        }
                    }
                }
            }
        }


        /// <summary>
        /// 1. Correlation matrix.
        ///    It cross compares all input vectors and SDRs to find similarity and print the matrix.
        /// </summary>
        /// <param name="sdrs"> This List contains the stable SDR's</param>
        /// <param name="newlistip"> List conatining input vectors</param>
        /// The function takes list of stable SDRs and list of input vectors as arguments


        private void GenerateResult(List<int[]> sdrs, List<int[]> newlistip)
        {
            if (sdrs.Count == 0)
            {
                throw new ArgumentOutOfRangeException(ConstantOfCustomeException.No_SDR_Generated_Exception);
            }
            else
            {
                Debug.Write("Correlation matrix for the images"); //Prints the statement as correlation matrix is generated for input and output similarity
                Debug.WriteLine(" ");

                //Logic implemented to print the correlation matrix
                //The loop executes till all images in the folder are compared for similarity
                ///<param name="Mathhelpers.CalcArraySimilarity"> To find the correlation between the passed inputs</param>
                ///<param name="similarityValueofInputVector"> Calculates input similarity by comparing input vectors of images</param>
                ///<param name="similarityValueofSDR"> Calculates output similarity by comparing the stable SDR's</param>


                for (int i = 0; i < sdrs.Count; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        Debug.Write(" ");
                        double similarityValueofSDR = MathHelpers.CalcArraySimilarity(sdrs[i], sdrs[j]); 
                        double roundedSimilarity = Math.Round(similarityValueofSDR / 100, 2); // For decimation of output similarity value


                        double similarityValueofInputVector= MathHelpers.CalcArraySimilarity(newlistip[i], newlistip[j]);
              
                        double roundedSimilarityforip= Math.Round(similarityValueofInputVector / 100, 2);  //for decimation of input similarity value
                        Debug.Write(roundedSimilarityforip + "|" + roundedSimilarity);  //To represent the input and ouptut similarity separated by "|" for ease of understanding

                    }
                    Debug.WriteLine("");
                }

               
            }
        }

        /// <summary>
        /// This method is called to get files form the argument path.
        /// </summary>
        /// <param name="trainingFolder">The folder from where we get the images.</param>
        /// <returns></returns>
        private static string[] GetFileFromPath(string trainingFolder)
        {
            string[] directory = new string[] { };

            if (Directory.Exists(trainingFolder))
            {
                try
                {
                    directory = Directory.GetFiles(trainingFolder, "*.jpg"); //extracts all images with ".jpg" extension
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                finally
                {
                    if (directory.Length == 0) 
                    {
                        //throws an exception if the folder does not contain any images
                        Debug.WriteLine(string.Format("No images found in {0} folder", trainingFolder));
                        throw new ExceptionHandler(ConstantOfCustomeException.No_Images_Found_Exception);
                    }
                }

            }
            else
            {
                //Throws an exception if foldercontaining images is missing
                Debug.WriteLine(string.Format("{0} folder not found.", trainingFolder)); 
                throw new ExceptionHandler(ConstantOfCustomeException.No_Dictionary_Found_Exception);
            }

            return directory;
        }

        /// Creates an additional directory for the output folder
        
        private string CreateADirectory()  
        {
            string outFolder = "";
            try
            {
                outFolder = $"{GlobalConstant.SimilarityExperimentGLobalConstant.testOutputFolder}\\{GlobalConstant.SimilarityExperimentGLobalConstant.outputtPrefix}";
                Directory.CreateDirectory(outFolder);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return outFolder;

        }

    }
}