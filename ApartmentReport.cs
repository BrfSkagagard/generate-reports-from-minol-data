namespace MinolReportsCreator
{
    public class ApartmentReport
    {
        public int Number { get; set; }

        public MinoWebLogin LoginInfo { get; set; }

        // Top
        public string TopHeader { get; set; }
        public CostPieInformation TopCost { get; set; }
        public PieInformation TopHeat { get; set; }
        public PieInformation TopWarmwater { get; set; }

        // Heat
        public PieInformation OwnHeat { get; set; }
        public PieInformation SimilarHeat { get; set; }
        public PieInformation BuildingHeat { get; set; }

        // Warmwater
        public PieInformation OwnWarmwater { get; set; }
        public PieInformation SimilarWarmwater { get; set; }
        public PieInformation BuildingWarmwater { get; set; }
    }

    public class PieInformation
    {
        public string Text { get; set; }
        public int Rotation { get; set; }
        public bool IsBig { get; set; }
    }

    public class CostPieInformation : PieInformation
    {
        public bool IsOver { get; set; }
    }
}
