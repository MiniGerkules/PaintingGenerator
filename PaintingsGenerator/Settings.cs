namespace PaintingsGenerator {
    public record class Settings {
        public double MaxColorDiffInBrushInTimes { get; init; } = 1.5;
        public double MaxColorDiffInStrokeInTimes { get; init; } = 1.5;

        public double MaxDiffOfBrushRadInTimes { get; init; } = 0.2;

        public double DiffWithTemplateToStop { get; init; } = 50;

        public uint StartBrushRadius { get; init; } = 6;
    }
}
