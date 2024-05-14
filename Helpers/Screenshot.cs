
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Extensions.Logging;

namespace TikTokDetection.Helpers
{
    internal static class Screenshot
    {
        private static readonly ILogger logger = LoggerInitializer.CreateLogger();

        public static double Change(string oldFileName, string newFileName)
        {
            try
            {
                Mat img1 = CvInvoke.Imread(oldFileName, ImreadModes.Color);
                Mat img2 = CvInvoke.Imread(newFileName, ImreadModes.Color);

                // Initialize ORB detector
                var orb = new ORB();

                // Detect keypoints and descriptors
                VectorOfKeyPoint keypoints1 = new VectorOfKeyPoint();
                VectorOfKeyPoint keypoints2 = new VectorOfKeyPoint();
                Mat descriptors1 = new Mat();
                Mat descriptors2 = new Mat();

                orb.DetectAndCompute(img1, null, keypoints1, descriptors1, false);
                orb.DetectAndCompute(img2, null, keypoints2, descriptors2, false);

                // Create a matcher
                BFMatcher matcher = new BFMatcher(DistanceType.Hamming);

                // Match descriptors
                VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
                matcher.KnnMatch(descriptors1, descriptors2, matches, 2);

                const double ratioTestThreshold = 0.75;
                List<MDMatch> goodMatches = new List<MDMatch>();

                for (int i = 0; i < matches.Size; i++)
                {
                    VectorOfDMatch match = matches[i];
                    if (match.Size > 1)
                    {
                        MDMatch[] inds = match.ToArray();
                        if (inds[0].Distance < ratioTestThreshold * inds[1].Distance)
                        {
                            goodMatches.Add(inds[0]);
                        }
                    }
                }

                // Calculate similarity percentage
                int totalKeypoints = keypoints1.Size + keypoints2.Size;
                if (totalKeypoints > 0)
                {
                    // Multiply by 2 to account for matches being counted once per image
                    double similarity = (double)goodMatches.Count * 2 / totalKeypoints;
                    similarity *= 100; // Convert to percentage

                    logger.LogInformation($"Similarity: {similarity:0.00}%");
                    return similarity;
                }
                else
                {
                    logger.LogError("No keypoints detected in one or both images");
                    return 999;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in detecting changes: {ex.Message}");
                return 999;
            }
        }
    }
}
