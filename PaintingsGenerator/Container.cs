using PaintingsGenerator.Images;
using System;

namespace PaintingsGenerator {
    public class Container<ElemType> {
        protected record class Proxy(Container<ElemType> Container,
                                     Position LeftUp, Position RightDown) {
            public int Width => RightDown.X - LeftUp.X;
            public int Height => RightDown.Y - LeftUp.Y;
            public ulong Size => (ulong)Width * (ulong)Height;

            public ElemType this[int i, int j] {
                get => Container[i + LeftUp.Y, j + LeftUp.X];
            }
        }

        private readonly ElemType[,] elems;

        public int Height => elems.GetLength(0);
        public int Width => elems.GetLength(1);

        public ElemType this[int i, int j] {
            get => elems[i, j];
            set => elems[i, j] = value;
        }

        public Container(ElemType[,] elems) {
            this.elems = elems;
        }

        /// <summary>
        /// Extract a part of image from `image` in position `pos` and size of
        /// side of square == 2*radius + 1
        /// </summary>
        /// <param name="image"> Image to extract part </param>
        /// <param name="pos"> Center of square </param>
        /// <param name="height"> Height from center square to its side </param>
        /// <returns> Part of image </returns>
        /// <exception cref="Exception"> If `pos` don't lie in image bounds </exception>
        protected Proxy GetPart(Position pos, uint height) {
            if (pos.X >= Width || pos.Y >= Height || pos.X < 0 || pos.Y < 0)
                throw new Exception("Can't get data from the required position!");

            var leftUp = new Position((int)Math.Max(0, pos.X - height),
                                      (int)Math.Max(0, pos.Y - height));
            var rightDown = new Position((int)Math.Min(Width - 1, pos.X + height),
                                         (int)Math.Min(Height - 1, pos.Y + height));

            return new(this, leftUp, rightDown);
        }
    }
}
