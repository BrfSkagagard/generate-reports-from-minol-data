using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinolReportsCreator.Repositories
{
    public class StyrelseReportRepository
    {
        [Flags]
        public enum StyrelseWarningType
        {
            Heat,
            WarmWater,
            Cost
        }

        public class StyrelseWarning
        {
            public int ApartmentNumber { get; set; }
            public StyrelseWarningType Type { get; set; }
            public string Text { get; set; }
            public double Order { get; set; }
        }

        public static List<StyrelseWarning> CreateWarnings(List<Apartment> apartments)
        {
            var warnings = new List<StyrelseWarning>();
            foreach (var apartment in apartments)
            {
                var heat = apartment.GetLastMonthHeatMeasure();
                // It be something wrong IF it is a winter month and normal living
                if (heat.Consumption < 1.0)
                {
                    warnings.Add(new StyrelseWarning
                    {
                        ApartmentNumber = apartment.Number,
                        Type = StyrelseWarningType.Heat,
                        Text = "Lägenhet " + apartment.Number + " - " + heat.Consumption.ToString("0.00") + " kWh",
                        Order = heat.Consumption
                    });
                }

                var warmwater = apartment.GetLastMonthWarmWaterMeasure();
                if (warmwater.Consumption < 0.2)
                {
                    warnings.Add(new StyrelseWarning
                    {
                        ApartmentNumber = apartment.Number,
                        Type = StyrelseWarningType.WarmWater,
                        Text = "Lägenhet " + apartment.Number + " - " + warmwater.Consumption.ToString("0.00") + " m³",
                        Order = warmwater.Consumption
                    });
                }

                var cost = heat.Cost + heat.Cost;
                var currentCost = (heat.Cost + heat.Cost);
                var flatRateYearlyCost = (Program.FlatRate * apartment.Size);
                var flatRateMonthlyCost = Math.Ceiling(flatRateYearlyCost / 12);
                var overConsumption = (flatRateMonthlyCost - currentCost);
                if (currentCost > flatRateMonthlyCost)
                {
                    warnings.Add(new StyrelseWarning
                    {
                        ApartmentNumber = apartment.Number,
                        Type = StyrelseWarningType.Cost,
                        Text = "Lägenhet " + apartment.Number + " - " + overConsumption.ToString("0.00") + " kr",
                        Order = overConsumption
                    });
                }
            }

            return warnings;
        }
    }
}
