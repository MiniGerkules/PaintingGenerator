using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace PaintingsGenerator.Images.ImageStuff {
    public class StrokePositions : IEnumerable<StrokePivot> {
        private readonly List<StrokePivot> positions = new();
        private ulong sumOfRadiuses = 0;

        public StrokePivot this[int i] => positions[i];
        public int Count => positions.Count;

        public double Length { get; private set; } = 0;
        public double AvgRadius => (double)sumOfRadiuses / (positions.Count + (positions.Count == 0 ? 1 : 0));

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<StrokePivot> GetEnumerator() {
            foreach (var pos in positions)
                yield return pos;
        }

        public void Add(StrokePivot position) {
            Length += position.Radius;
            sumOfRadiuses += position.Radius;

            if (positions.Count == 0) {
                Length += position.Radius;
            } else {
                var cur = positions[^1].Position;
                var next = position.Position;

                Length += Math.Sqrt(Math.Pow(next.X - cur.X, 2) + Math.Pow(next.Y - cur.Y, 2));
                Length -= positions[^1].Radius;
            }

            positions.Add(position);
        }

        public void Add(StrokePositions strokePositions) {
            Length += strokePositions.Length;
            sumOfRadiuses += strokePositions.sumOfRadiuses;

            if (positions.Count != 0)
                Length -= positions[^1].Radius + strokePositions[0].Radius;

            positions.AddRange(strokePositions);
        }

        public uint MaxWidht() => positions.MaxBy(pivot => pivot.Radius)!.Radius;
    }
}
