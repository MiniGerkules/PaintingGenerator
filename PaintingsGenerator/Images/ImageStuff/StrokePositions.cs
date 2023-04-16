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
    }
}
