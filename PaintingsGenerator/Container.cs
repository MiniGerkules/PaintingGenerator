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
                set => Container[i + LeftUp.Y, j + LeftUp.X] = value;
            }
        }

        protected record class Bounds {
            public int LeftX { get; }
            public int RightX { get; }
            public int UpY { get; }
            public int DownY { get; }

            public Bounds(int leftX, int rightX, int upY, int downY) {
                LeftX = leftX;
                RightX = rightX;
                UpY = upY;
                DownY = downY;
            }

            public bool XInBounds(int x) => LeftX <= x && x <= RightX;
            public bool YInBounds(int y) => UpY <= y && y <= DownY;
            public bool InBounds(Position pos) => XInBounds(pos.X) && YInBounds(pos.Y);
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

            var leftUp = new Position((int)GetPartLeftBound(pos, height),
                                      (int)GetPartUpBound(pos, height));
            var rightDown = new Position((int)GetPartRightBound(pos, height),
                                         (int)GetPartDownBound(pos, height));

            return new(this, leftUp, rightDown);
        }

        protected long GetPartLeftBound(Position pos, uint height) => Math.Max(0, pos.X - height);
        protected long GetPartRightBound(Position pos, uint height) => Math.Min(Width - 1, pos.X + height);
        protected long GetPartUpBound(Position pos, uint height) => Math.Max(0, pos.Y - height);
        protected long GetPartDownBound(Position pos, uint height) => Math.Min(Height - 1, pos.Y + height);

        protected Bounds GetBounds(Position start, Position end, uint height) {
            int leftX, rightX, upY, downY;

            if (start.X < end.X) {
                leftX = (int)GetPartLeftBound(start, height);
                rightX = (int)GetPartRightBound(end, height);
            } else {
                leftX = (int)GetPartLeftBound(end, height);
                rightX = (int)GetPartRightBound(start, height);
            }

            if (start.Y < end.Y) {
                upY = (int)GetPartUpBound(start, height);
                downY = (int)GetPartDownBound(end, height);
            } else {
                upY = (int)GetPartUpBound(end, height);
                downY = (int)GetPartDownBound(start, height);
            }

            return new(leftX, rightX, upY, downY);
        }
    }
}
