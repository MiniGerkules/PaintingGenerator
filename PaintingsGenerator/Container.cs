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

            var leftUp = new Position(GetPartLeftBound(pos, height),
                                      GetPartUpBound(pos, height));
            var rightDown = new Position(GetPartRightBound(pos, height),
                                         GetPartDownBound(pos, height));

            return new(this, leftUp, rightDown);
        }

        protected CircleProxy GetCirclePart(Position pos, uint radius) {
            if (!GetBounds().InBounds(pos))
                throw new Exception("Can't get data from the required position!");

            return new(this, pos, radius);
        }

        protected Bounds GetBounds() => new(0, Width - 1, 0, Height - 1);

        protected Bounds GetBounds(Position pos1, uint rad1, Position pos2, uint rad2) {
            int leftX = Math.Min(GetPartLeftBound(pos1, rad1),
                                 GetPartLeftBound(pos2, rad2));
            int rightX = Math.Max(GetPartRightBound(pos1, rad1),
                                  GetPartRightBound(pos2, rad2));

            int upY = Math.Min(GetPartUpBound(pos1, rad1),
                               GetPartUpBound(pos2, rad2));
            int downY = Math.Max(GetPartDownBound(pos1, rad1),
                                 GetPartDownBound(pos2, rad2));

            return new(leftX, rightX, upY, downY);
        }

        private int GetPartLeftBound(Position pos, uint height) => (int)Math.Max(0, pos.X - height);
        private int GetPartRightBound(Position pos, uint height) => (int)Math.Min(Width - 1, pos.X + height);
        private int GetPartUpBound(Position pos, uint height) => (int)Math.Max(0, pos.Y - height);
        private int GetPartDownBound(Position pos, uint height) => (int)Math.Min(Height - 1, pos.Y + height);
    }
}
