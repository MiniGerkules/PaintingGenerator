using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace PaintingsGenerator.Images.ImageStuff {
    public class StrokePositions : IEnumerable<Position> {
        private List<Position> positions = new();
        public Position this[int i] => positions[i];
        public int Count => positions.Count;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<Position> GetEnumerator() {
            foreach (var pos in positions)
                yield return pos;
        }

        public void Add(Position position) => positions.Add(position);
        public void Add(StrokePositions strokePositions) => positions.AddRange(strokePositions);

        public void MakeUnique() {
            var unique = new HashSet<Position>(positions);
            positions = unique.ToList();
            positions.Sort((Position a, Position b) => a.X - b.X);
        }
    }
}
