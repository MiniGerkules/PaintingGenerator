using System;
using System.Collections;
using System.Collections.Generic;

namespace PaintingsGenerator {
    public class Container<ElemType> {
        protected record class RectProxy(Container<ElemType> Container,
                                         Position LeftUp, Position RightDown) {
            public int Width => RightDown.X - LeftUp.X;
            public int Height => RightDown.Y - LeftUp.Y;
            public ulong Size => (ulong)Width * (ulong)Height;

            public ElemType this[int i, int j] {
                get => Container[i + LeftUp.Y, j + LeftUp.X];
                set => Container[i + LeftUp.Y, j + LeftUp.X] = value;
            }
        }

        protected record class CircleProxy : IEnumerable<ElemType> {
            private Container<ElemType> Container { get; init; }
            private Position Pos { get; init; }
            private int Radius { get; init; }

            private readonly int startX, endX;
            private readonly int startY, endY;

            public CircleProxy(Container<ElemType> container,
                               Position pos, uint radius) {
                Container = container;
                Pos = pos;
                Radius = (int)radius;

                var bounds = Container.GetBounds();

                var checker = (int x) => bounds.XInBounds(x + pos.X);
                (startX, endX) = GetBounds(checker);

                checker = (int y) => bounds.YInBounds(y + pos.Y);
                (startY, endY) = GetBounds(checker);
            }

            private (int, int) GetBounds(Func<int, bool> checker) {
                int start = 0, end = 0;

                for (int i = -Radius; i < 0; ++i) {
                    if (checker(i)) {
                        start = i;
                        break;
                    }
                }
                for (int i = Radius; i > 0; --i) {
                    if (checker(i)) {
                        end = i;
                        break;
                    }
                }

                return (start, end);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public IEnumerator<ElemType> GetEnumerator() {
                for (int y = startY; y <= endY; ++y) {
                    for (int x = startX; x <= endX; ++x) {
                        if (x*x + y*y <= Radius*Radius)
                            yield return Container[y + Pos.Y, x + Pos.X];
                    }
                }
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
        public ulong Size => (ulong)Width * (ulong)Height;

        virtual public ElemType this[int i, int j] {
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
        protected RectProxy GetRectPart(Position pos, uint height) {
            if (!GetBounds().InBounds(pos))
                throw new Exception("Can't get data from the required position!");

            var leftUp = new Position((int)GetPartLeftBound(pos, height),
                                      (int)GetPartUpBound(pos, height));
            var rightDown = new Position((int)GetPartRightBound(pos, height),
                                         (int)GetPartDownBound(pos, height));

            return new(this, leftUp, rightDown);
        }

        protected CircleProxy GetCirclePart(Position pos, uint radius) {
            if (!GetBounds().InBounds(pos))
                throw new Exception("Can't get data from the required position!");

            return new(this, pos, radius);
        }

        protected long GetPartLeftBound(Position pos, uint height) => Math.Max(0, pos.X - height);
        protected long GetPartRightBound(Position pos, uint height) => Math.Min(Width - 1, pos.X + height);
        protected long GetPartUpBound(Position pos, uint height) => Math.Max(0, pos.Y - height);
        protected long GetPartDownBound(Position pos, uint height) => Math.Min(Height - 1, pos.Y + height);

        protected Bounds GetBounds() => new(0, Width - 1, 0, Height - 1);

        protected Bounds GetBounds(Position angle1, Position angle2, uint height) {
            int leftX, rightX, upY, downY;

            if (angle1.X < angle2.X) {
                leftX = (int)GetPartLeftBound(angle1, height);
                rightX = (int)GetPartRightBound(angle2, height);
            } else {
                leftX = (int)GetPartLeftBound(angle2, height);
                rightX = (int)GetPartRightBound(angle1, height);
            }

            if (angle1.Y < angle2.Y) {
                upY = (int)GetPartUpBound(angle1, height);
                downY = (int)GetPartDownBound(angle2, height);
            } else {
                upY = (int)GetPartUpBound(angle2, height);
                downY = (int)GetPartDownBound(angle1, height);
            }

            return new(leftX, rightX, upY, downY);
        }
    }
}
