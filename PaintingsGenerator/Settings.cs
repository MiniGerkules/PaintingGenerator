namespace PaintingsGenerator {
    public record class Settings {
        public double MaxColorDiffInBrushInTimes { get; init; } = 1.5;
        public double MaxColorDiffInStrokeInTimes { get; init; } = 1.3;

        public double MaxDiffOfBrushRadiusesInTimes { get; init; } = 0.2;

        public double RatioOfLenToWidthShortest { get; init; } = 153.0 / 115.0;
        public double RatioOfLenToWidthLargest { get; init; } = 467.0 / 45.0;

        public double DiffWithTemplateToStopInPercent { get; init; } = 20;

        public uint StartBrushRadius { get; init; } = 6;
    }
}
