using System.Collections;
using System.Collections.Generic;

namespace PaintingsGenerator.Images.ImageStuff {
    public class StrokePositions : IEnumerable<Position> {
        private readonly List<Position> positions = new();
        public Position this[int i] => positions[i];
        public int Count => positions.Count;

        public void Add(Position position) => positions.Add(position);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<Position> GetEnumerator() {
            foreach (var pos in positions)
                yield return pos;
        }
    }
}
