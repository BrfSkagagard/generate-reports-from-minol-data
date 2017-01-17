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
            Cost,
            Period
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
            string period = null;
            var hasInvalidPeriod = false;

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
                        Text = heat.Consumption.ToString("0.00"),
                        Order = heat.Consumption
                    });
                }

                // Warn if consumption is less then 0.2 (as it is seemed as unnatural)
                var warmwater = apartment.GetLastMonthWarmWaterMeasure();
                if (warmwater.Consumption < 0.2)
                {
                    warnings.Add(new StyrelseWarning
                    {
                        ApartmentNumber = apartment.Number,
                        Type = StyrelseWarningType.WarmWater,
                        Text = warmwater.Consumption.ToString("0.00"),
                        Order = warmwater.Consumption
                    });
                }

                // Warn if cost is more then paid for
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
                        Text = overConsumption.ToString("0.00"),
                        Order = overConsumption
                    });
                }

                if (heat.Period != warmwater.Period)
                {
                    warnings.Add(new StyrelseWarning
                    {
                        ApartmentNumber = apartment.Number,
                        Type = StyrelseWarningType.Period,
                        Text = $"Heat and warmwater measurement is not from same period (Heat:{heat.Period}, Warmwater: {warmwater.Period})",
                        Order = apartment.Number
                    });
                }

                // TODO: Check if all apartments have the same last period

                if (period == null)
                {
                    period = heat.Period;
                }

                if (!hasInvalidPeriod && heat.Period != period)
                {
                    warnings.Add(new StyrelseWarning
                    {
                        ApartmentNumber = -1,
                        Type = StyrelseWarningType.Period,
                        Text = "Last measured period is not the same for all apartments",
                        Order = -1
                    });
                    hasInvalidPeriod = true;
                }

            }

            return warnings;
        }
    }
}
