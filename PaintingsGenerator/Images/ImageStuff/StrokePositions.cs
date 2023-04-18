using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace PaintingsGenerator.Images.ImageStuff {
    public class StrokePositions : IEnumerable<StrokePivot> {
        private List<StrokePivot> positions = new();
        public StrokePivot this[int i] => positions[i];
        public int Count => positions.Count;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<StrokePivot> GetEnumerator() {
            foreach (var pos in positions)
                yield return pos;
        }

        public void Add(StrokePivot position) => positions.Add(position);
        public void Add(StrokePositions strokePositions) => positions.AddRange(strokePositions);

        public double GetLen() {
            double len = 0;

            for (int i = 0, end = positions.Count - 1; i < end; ++i) {
                var cur = positions[i].Position;
                var next = positions[i + 1].Position;

                len += Math.Sqrt(Math.Pow(next.X - cur.X, 2) + Math.Pow(next.Y - cur.Y, 2));
            }

            return len;
        }

        internal uint MaxWidht() => positions.MaxBy(pivot => pivot.Radius)!.Radius;
    }
}
