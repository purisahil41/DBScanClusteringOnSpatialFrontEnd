using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.Services;
using System.Web.Script.Services;
using System.Globalization;
using System.Diagnostics;

namespace ClusteringTool
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        public class CrimeEntryOutput
        {
            public CrimeDataEntry crimeEntry = null;
            public double totalCost = 0.0f;
            public double timeCost = 0.0f;
            public double dateCost = 0.0f;
            public double locationCost = 0.0f;
            public int frequency = 0;
        }

        public static List<Cluster> clusterList = new List<Cluster>();
        public static Dictionary<CrimeEntryOutput, double> listCrimeEntryOutputAspx = new Dictionary<CrimeEntryOutput, double>();
        public static Dictionary<CrimeEntryOutput, double> listCrimeEntryOutput = new Dictionary<CrimeEntryOutput, double>();
        public static int crimeIndex = 0;
        public const int centroidDistance = 7;
        public const int pointDistance = 2;
        public static int numOfPointsCompared = 0;

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public static string GetCrime(string longitudeLatitude)
        {
            numOfPointsCompared = 0;
            Stopwatch stpWatch = new Stopwatch();
            stpWatch.Start();
            string[] longLat = longitudeLatitude.Split(new string[] { "#@#" }, StringSplitOptions.RemoveEmptyEntries);

            string returnString = getAnticipatedCrime(longLat);
           
            stpWatch.Stop();

            double secondsElapsed = stpWatch.Elapsed.TotalSeconds;

            return returnString;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            clusterList = new List<Cluster>();
            using (StreamReader sr = new StreamReader(@"F:\University Of Waterloo\CS846_SoftwareEngineeringForBigData\Final_Project\FInal_Data\DBScan_Extended\Final_Data_output_e2_1000.txt"))
            {
                // Read the stream to a string, and write the string to the console.
                String line = sr.ReadLine();
                Cluster cluster = new Cluster();

                while (!String.IsNullOrEmpty(line))
                {
                    if (line == "*******************************")
                    {
                        clusterList.Add(cluster);
                        cluster = new Cluster();
                    }

                    String[] stringArr = line.Split(new char[] { ',' });
                    if (stringArr.Length > 2)
                    {
                        string[] latLong = stringArr[0].Split(new char[] { ':' });
                        if (latLong.Length >= 2)
                        {
                            //string latLongVal = latLong[1];
                            //string[] latLongDiff = latLongVal.Split(new char[] { '|' });
                            //allLatLong.Add("{lat: " + latLongDiff[0] + ", lng: " + latLongDiff[1] + "}");
                            cluster.crimeEntries.Add(new CrimeDataEntry(stringArr));

                        }

                    }
                    else if (stringArr.Length == 2)
                    {
                        cluster.clusterCentroid = getClusterCentroid(line);
                    }

                    line = sr.ReadLine();
                }
            }
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public static string GetRelatedPoints()
        {
            string returnString = "";
            
            foreach (CrimeEntryOutput cr in listCrimeEntryOutputAspx.Keys)
            {
                returnString += "|||" + cr.crimeEntry.point.latitude + "@@" + cr.crimeEntry.point.longitude + "@@" + cr.crimeEntry.primaryType + " - " + cr.crimeEntry.description; 
            }
            if (!String.IsNullOrEmpty(returnString)) { returnString = returnString.Substring(3); }
            return returnString;
        }

        private static string getAnticipatedCrime(string[] longLat)
        {
            double[] userCoordinates = new double[] { double.Parse(longLat[0]), double.Parse(longLat[1]) };
            List<Cluster> nearClusters = getNearClusters(userCoordinates);

            List<CrimeDataEntry> nearCrimeEntries = getNearCrimeEntries(userCoordinates, nearClusters);
            numOfPointsCompared = nearCrimeEntries.Count;
            string anticipatedCrime = getCrimeDescription(nearCrimeEntries, userCoordinates);

            return anticipatedCrime;
        }

        private static string getCrimeDescription(List<CrimeDataEntry> nearCrimeEntries, double[] userCoordinates)
        {
            string htmlDescription = "";

            double maxCost = -10000.0f;
            CrimeDataEntry maxCrimeEntry = null;

            listCrimeEntryOutput = new Dictionary<CrimeEntryOutput, double>();
            List<string> crimeTypesAdded  = new List<string>();
            foreach (CrimeDataEntry crimeEntry in nearCrimeEntries)
            {
                CrimeEntryOutput crimeEntryOutput = computeCost(crimeEntry, userCoordinates, nearCrimeEntries);
                double computedCost = 0.35 * -1000 * crimeEntryOutput.locationCost - 0.15 * Math.Abs(crimeEntryOutput.timeCost) - 0.35 * Math.Abs(crimeEntryOutput.dateCost) * 180 + 0.15 * crimeEntryOutput.frequency * 1000;
                listCrimeEntryOutput.Add(crimeEntryOutput, computedCost);
                //if(computedCost > maxCost)
                //{
                //    maxCost = computedCost;
                //    maxCrimeEntry = crimeEntry;
                //}

                //if (!crimeTypesAdded.Contains(crimeEntry.primaryType))
                //{
                //    listCrimeEntryOutput.Add(crimeEntryOutput, computedCost);
                //    crimeTypesAdded.Add(crimeEntry.primaryType);
                //}               
            }

            var sortedDicitonary = from entry in listCrimeEntryOutput orderby entry.Value descending select entry;
            listCrimeEntryOutputAspx = new Dictionary<CrimeEntryOutput, double>();
            string primaryTypeValue = "";
            for (int i = 0; i < sortedDicitonary.Count(); i++)
            {
                maxCrimeEntry = sortedDicitonary.ElementAt(i).Key.crimeEntry;
                double[] pointLoc = new double[] { maxCrimeEntry.point.longitude, maxCrimeEntry.point.latitude };
                double distanceFar = compute(userCoordinates, pointLoc);

                if (!crimeTypesAdded.Contains(maxCrimeEntry.primaryType))
                {
                    crimeTypesAdded.Add(maxCrimeEntry.primaryType);
                    if (i == 0)
                    {
                        htmlDescription += "<div class=\"content displayCrime\" >";
                        primaryTypeValue = sortedDicitonary.ElementAt(i).Key.crimeEntry.primaryType;


                    }
                    else
                    {
                        htmlDescription += "<div class=\"content hideCrime\" >";
                    }
                    htmlDescription += "<div id=\"siteNotice\">" +
                       "</div>" +
                       "<h1 id=\"firstHeading\" class=\"firstHeading\">" + maxCrimeEntry.primaryType + "</h1>" +
                       "<div id=\"bodyContent\">" +
                       "<p>Be aware of <b>" + maxCrimeEntry.primaryType + "</b> <br/>" +
                       "<p>Specifically for <b>" + maxCrimeEntry.description + "</b> <br/>" +
                       "which happened <b>" + distanceFar + " kms </b> far <br/>" +
                       "at <b>" + maxCrimeEntry.block + "</b><br/>" +
                       "on  <b>" + maxCrimeEntry.dateTime + "</b>.</p> <br/>" +
                       "The total number of times this crime has happened in the neighbourhood is <b>" + sortedDicitonary.ElementAt(i).Key.frequency + "</b>.</p> <br/>" +
                       "</div>" +
                       "</div>";
                }


                
                if (sortedDicitonary.ElementAt(i).Key.crimeEntry.primaryType == primaryTypeValue)
                {
                    listCrimeEntryOutputAspx.Add(sortedDicitonary.ElementAt(i).Key, sortedDicitonary.ElementAt(i).Value);
                } 
            }
                     

            htmlDescription += "<div id=\"nextCrime\"> <a  href=\"#\"><u>Next</u></a></div> ";


            return htmlDescription;
        }

        private static CrimeEntryOutput computeCost(CrimeDataEntry crimeEntry, double[] userCoordinates, List<CrimeDataEntry> crimeEntries)
        {
            CrimeEntryOutput crimeOutput = new CrimeEntryOutput();
            crimeOutput.crimeEntry = crimeEntry;
            // Calculation distance between the two points
            double[] pointLoc = new double[] { crimeEntry.point.longitude, crimeEntry.point.latitude };
            double locationCost = compute(pointLoc, userCoordinates);

            // Difference of Time
            TimeSpan timeCurrent = DateTime.Now.TimeOfDay;
            TimeSpan timeCrime = crimeEntry.dateTime.TimeOfDay;
            TimeSpan timeDiff = timeCurrent - timeCrime;

            // Difference of Date
            DateTime dateCurrent = DateTime.Now.Date;
            DateTime dateCrime = crimeEntry.dateTime.Date;
            TimeSpan dateDiff = dateCrime - dateCurrent;

            // Check Frequency
            int frequency = 0;
            foreach (CrimeDataEntry crimeEntryIterator in crimeEntries)
            {
                if (crimeEntry.primaryType == crimeEntryIterator.primaryType) frequency++;
            }

            crimeOutput.dateCost = dateDiff.Days;
            crimeOutput.timeCost = timeDiff.Minutes;
            crimeOutput.locationCost = locationCost;
            crimeOutput.frequency = frequency;
            return crimeOutput;

        }


        private static List<CrimeDataEntry> getNearCrimeEntries(double[] userCoordinates, List<Cluster> nearClusters)
        {
            List<CrimeDataEntry> nearCrimeEntries = new List<CrimeDataEntry>();
            foreach (Cluster clus in nearClusters)
            {
                foreach (CrimeDataEntry crimeEntry in clus.crimeEntries)
                {
                    double[] crimeCoord = new double[] { crimeEntry.point.longitude, crimeEntry.point.latitude };
                    if (compute(userCoordinates, crimeCoord) < pointDistance)
                    {
                        nearCrimeEntries.Add(crimeEntry);
                    }
                }
            }
            return nearCrimeEntries;
        }

        private static List<Cluster> getNearClusters(double[] userCoordinates)
        {
            List<Cluster> nearClusters = new List<Cluster>();
            double minDistance = 100.0f;
            foreach (Cluster clus in clusterList)
            {
                double[] clusCentroid = new double[] { clus.clusterCentroid.longitude, clus.clusterCentroid.latitude };
                double distance = compute(userCoordinates, clusCentroid);
                if (distance < centroidDistance)
                {
                    nearClusters.Add(clus);
                }

                if (distance < minDistance) minDistance = distance;

            }
            return nearClusters;
        }


        public static double compute(double[] arg0, double[] arg1)
        {
            int R = 6371; // Radious of the earth
            Double lat1 = arg0[0];
            Double lon1 = arg0[1];
            Double lat2 = arg1[0];
            Double lon2 = arg1[1];
            Double latDistance = toRad(lat2 - lat1);
            Double lonDistance = toRad(lon2 - lon1);
            Double a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2) +
                       Math.Cos(toRad(lat1)) * Math.Cos(toRad(lat2)) *
                       Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);
            Double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            Double distance = R * c;
            return distance;

        }


        private static Double toRad(Double value)
        {
            return value * Math.PI / 180;
        }

        
        private Point getClusterCentroid(string line)
        {
            string[] latLong = line.Split(new char[] { ':' });

            string latLongVal = latLong[1];
            string[] latLongDiff = latLongVal.Split(new char[] { ',' });
            return new Point(double.Parse(latLongDiff[0]), double.Parse(latLongDiff[1]));

        }

        public string getPointValues(Point point)
        {
            return "{lat: " + point.longitude + ", lng: " + point.latitude + "}";
        }

        public string getPointData(CrimeDataEntry crimeEntryData)
        {
            return crimeEntryData.primaryType + " - " + crimeEntryData.description;
        }
    }



    public class Cluster
    {
        public List<CrimeDataEntry> crimeEntries = new List<CrimeDataEntry>();
        public Point clusterCentroid = new Point();
    }

    public class CrimeDataEntry
    {
        public DateTime dateTime = DateTime.Now;
        public string block = "";
        public string primaryType = "";
        public string description = "";
        public string localDescription = "";
        public Point point = null;
        public int year = 0;

        public CrimeDataEntry(String[] strInput)
        {
            string[] latLong = strInput[0].Split(new char[] { ':' });
            if (latLong.Length >= 2)
            {
                string latLongVal = latLong[1];
                string[] latLongDiff = latLongVal.Split(new char[] { '|' });
                this.point = new Point(double.Parse(latLongDiff[0]), double.Parse(latLongDiff[1]));
            }
            string dateTimeValue = "";
            string[] dateTimeArr = strInput[1].Split(new char[] { ':' });
            for (int i = 0; i < dateTimeArr.Length; i++)
            {
                if (i != 0)
                {
                    dateTimeValue += ":" + dateTimeArr[i];

                }
            }
            dateTimeValue = dateTimeValue.Substring(2);
            this.dateTime = DateTime.ParseExact(dateTimeValue, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None);
            this.block = strInput[2].Split(new char[] { ':' })[1];
            this.primaryType = strInput[3].Split(new char[] { ':' })[1];
            this.description = strInput[4].Split(new char[] { ':' })[1];
            this.localDescription = strInput[5].Split(new char[] { ':' })[1];
        }

    }

    public class Point
    {
        public double longitude = 0.0f;
        public double latitude = 0.0f;

        public Point()
        {

        }

        public Point(double longitude, double latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }
}