using System;
using System.Collections.Generic;
using System.Linq;

namespace MinolReportsCreator.Repositories
{
    public class ApartmentReportRepository
    {
        public static List<ApartmentReport> CreateApartmentReports(List<Apartment> apartments, List<MinoWebLogin> logins)
        {
            var list = new List<ApartmentReport>();

            foreach (Apartment apartment in apartments)
            {
                var report = new ApartmentReport();
                report.Number = apartment.Number;

                report.LoginInfo = logins.FirstOrDefault(l => l.Number == apartment.Number);

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
                var flatRateYearlyCost = (Program.FlatRate * apartment.Size);
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
                list.Add(report);
            }

            return list;
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
            System.Globalization.TextInfo textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(text);
        }

    }
}
