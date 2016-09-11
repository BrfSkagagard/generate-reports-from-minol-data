using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace MinolReportsCreator
{
    class Program
    {
        const double FlatRate = 90;
        static void Main(string[] args)
        {
            string gitFolder = @"C:\Users\Mattias\Documents\GitHub\";
            try
            {
                if (args != null && args.Length > 0)
                {
                    gitFolder = args[0];
                }

                var folderBoard = gitFolder + "brfskagagard-styrelsen" + Path.DirectorySeparatorChar;
                var folderBoardExists = Directory.Exists(folderBoard);

                var json = new DataContractJsonSerializer(typeof(MinoWebLogin));
                var apartments = GetApartments(gitFolder);

                Console.WriteLine("Number of apartments: " + apartments.Count);

                foreach (Apartment apartment in apartments)
                {
                    var report = new ApartmentReport();
                    report.Number = apartment.Number;
                    var folder = gitFolder + "brfskagagard-lgh" + apartment.Number + Path.DirectorySeparatorChar;

                    using (
                        var stream =
                            File.OpenRead(folder + Path.DirectorySeparatorChar + "minol-login.json"))
                    {
                        stream.Position = 0;
                        var login = json.ReadObject(stream) as MinoWebLogin;
                        if (login != null)
                        {
                            report.LoginInfo = login;
                        }
                    }

                    var heatMeasurement = apartment.GetLastMonthHeatMeasure();
                    var heatLastYearMeasurement = apartment.GetMeasurmentForSameMonthLastYear(heatMeasurement);
                    var warmWaterMeasurement = apartment.GetLastMonthWarmWaterMeasure();
                    var warmWaterLastYearMeasurement = apartment.GetMeasurmentForSameMonthLastYear(warmWaterMeasurement);
                    var similarApartments = apartments.Where(a => a.Size == apartment.Size && a.Number != apartment.Number).GroupBy(a => a.Size).OrderBy(g => g.Key).FirstOrDefault().ToList();
                    var buildingApartments = apartments.Where(a => a.Building == apartment.Building && a.Number != apartment.Number).GroupBy(a => a.Building).OrderBy(g => g.Key).FirstOrDefault().ToList();


                    report.TopHeader = FirstLetterUpperCase(heatMeasurement.Period);
                    report.TopHeat = new PieInformation
                    {
                        Text = heatMeasurement.Consumption.ToString("0.00") // + " kWh"
                    };
                    AddPieInformation(report.TopHeat, heatMeasurement.Consumption, heatLastYearMeasurement.Consumption);
                    report.OwnHeat = report.TopHeat;

                    report.TopWarmwater = new PieInformation
                    {
                        Text = warmWaterMeasurement.Consumption.ToString("0.00") // + " m³"
                    };
                    AddPieInformation(report.TopWarmwater, warmWaterMeasurement.Consumption, warmWaterLastYearMeasurement.Consumption);
                    report.OwnWarmwater = report.TopWarmwater;

                    var currentCost = (heatMeasurement.Cost + warmWaterMeasurement.Cost);
                    var flatRateYearlyCost = (FlatRate * apartment.Size);
                    var flatRateMonthlyCost = Math.Ceiling(flatRateYearlyCost / 12);

                    report.TopCost = new CostPieInformation
                    {
                        Text = currentCost.ToString("0.00")// + " kr"
                    };

                    if (currentCost > flatRateMonthlyCost)
                    {
                        // we have used the flat rate.
                        report.TopCost.IsOver = true;
                    }
                    AddPieInformation(report.TopCost, currentCost, flatRateMonthlyCost);

                    report.SimilarHeat = SumApartments(MeasurmentTypes.Heat, similarApartments);
                    report.SimilarWarmwater = SumApartments(MeasurmentTypes.Warmwater, similarApartments);

                    report.BuildingHeat = SumApartments(MeasurmentTypes.Heat, buildingApartments);
                    report.BuildingWarmwater = SumApartments(MeasurmentTypes.Warmwater, buildingApartments);

                    var jsonReporter = new DataContractJsonSerializer(typeof(ApartmentReport));

                    Console.WriteLine("\t# " + apartment.Number);

                    using (
                        var stream =
                            File.Create(folder +
                                           Path.DirectorySeparatorChar + "minol-apartment-report-last-month.json"))
                    {
                        jsonReporter.WriteObject(stream, report);
                        stream.Flush();
                    }

                    // We only want to update repositories that we know about (read: that we have created)
                    if (folderBoardExists)
                    {
                        using (
                            var stream =
                                File.Create(folderBoard + "minol-apartment-report-" + report.Number + "-last-month.json"))
                        {
                            jsonReporter.WriteObject(stream, report);
                            stream.Flush();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                using (var stream = File.CreateText(gitFolder + "generate-reports-last-error.txt"))
                {
                    stream.Write(ex.ToString());
                    stream.Flush();
                }

                throw;
            }
        }

        private static PieInformation SumApartments(MeasurmentTypes measurementType, List<Apartment> apartments)
        {
            var pie = new PieInformation();
            // Populate WarmWater
            var lastMonthAverage = apartments.Sum(a => (measurementType == MeasurmentTypes.Warmwater ? a.GetLastMonthWarmWaterMeasure() : a.GetLastMonthHeatMeasure()).Consumption) / apartments.Count;
            var lastYearAverage = apartments.Sum(a => a.GetMeasurmentForSameMonthLastYear((measurementType == MeasurmentTypes.Warmwater ? a.GetLastMonthWarmWaterMeasure() : a.GetLastMonthHeatMeasure())).Consumption) / apartments.Count;
            AddPieInformation(pie, lastMonthAverage, lastYearAverage);
            switch (measurementType)
            {
                case MeasurmentTypes.Warmwater:
                    pie.Text = lastMonthAverage.ToString("0.00"); // + " m³";
                    break;
                case MeasurmentTypes.Heat:
                    pie.Text = lastMonthAverage.ToString("0.00"); // + " kWh";
                    break;
                default:
                    pie.Text = "no info";
                    break;
            }
            return pie;
        }


        private static void AddPieInformation(PieInformation pie, double currentValue,
            double lastYearValue)
        {
            if (lastYearValue == 0 && currentValue == 0)
            {
                pie.Rotation = 180;
                pie.IsBig = true;
            }
            else
            {
                // Populate WarmWater
                double percent = currentValue /
                                 (lastYearValue + currentValue);
                var degrees = (int)(percent * 360);
                if (degrees > 180)
                {
                    pie.Rotation = 360 - degrees;
                    pie.IsBig = true;
                }
                else
                {
                    pie.Rotation = degrees;
                    pie.IsBig = false;
                }
            }
        }

        private static string FirstLetterUpperCase(string text)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(text);
        }

        private static List<Apartment> GetApartments(string gitFolder)
        {
            var apartments = new List<Apartment>();
            var folders = Directory.GetDirectories(gitFolder, "brfskagagard-lgh*");
            var json = new DataContractJsonSerializer(typeof(Apartment));
            foreach (string folder in folders)
            {
                using (
                    var stream =
                        File.OpenRead(folder + Path.DirectorySeparatorChar + "minol-apartment-measurement.json"))
                {
                    stream.Position = 0;
                    var apartment = json.ReadObject(stream) as Apartment;
                    if (apartment == null)
                    {
                        continue;
                    }

                    apartments.Add(apartment);
                }
            }
            return apartments;
        }
    }
}
