using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PaintingsGenerator.Images.ImageStuff {
    internal class PositionManager {
        private record class LineFunc {
            public double K { get; init; }
            public double B { get; init; }

            public LineFunc(double k, double b) {
                K = k;
                B = b;
            }

            public LineFunc(double xStart, double yStart, double xEnd, double yEnd) {
                K = (double)(yEnd-yStart) / (xEnd-xStart);
                B = CountB(K, xStart, yStart);
            }

            public LineFunc(Position pos1, Position pos2) : this(pos1.X, pos1.Y, pos2.X, pos2.Y) {
            }

            public bool IsVertical() => double.IsInfinity(K);
            public bool IsHorizontal() => Math.Abs(K) <= 1e-5;

            public double CountX(double y) => (y-B) / K;
            public double CountY(double x) => K*x + B;
            public LineFunc GetPerp(Position goThrough) => new(-1.0/K, CountB(-1.0/K, goThrough.X, goThrough.Y));

            public static double CountK(Position pos1, Position pos2) => (double)(pos2.Y-pos1.Y) / (pos2.X - pos1.X);
            public static double CountB(double k, double xThrought, double yThrough) => yThrough - k*xThrought;
        }

        private readonly HashSet<Position> positions = new();
        public ImmutableHashSet<Position> StoredPositions => positions.ToImmutableHashSet();
        
        private Bounds bounds = new(0, 0, 0, 0);
        private double k = 0;

        public void StoreStrokePositions(Bounds bounds, Position start, Position end, uint radius) {
            this.bounds = bounds;
            k = (double)(end.Y-start.Y) / (end.X-start.X);

            if (double.IsInfinity(k)) { // Vertical
                StorePositionsAlongVertical(start, end, radius);
            } else if (Math.Abs(k) <= 1e-5) { // Horizontal
                StorePositionsAlongHorizontal(start, end, radius);
            } else { // With another angle
                StorePositionsAlongLine(start, end, radius);
            }

            StoreRoundPart(start, radius);
            StoreRoundPart(end, radius);
        }

        private void StorePositionsAlongVertical(Position start, Position end, uint radius) {
            if (start.Y > end.Y) (start, end) = (end, start);

            var storePositions = (Position pos, uint radius) => {
                StorePositionsSymmetrically(pos, radius, (int additionX) => additionX,
                                            (int additionY) => 0);
            };

            for (int y = 0, endY = end.Y - start.Y + 1; y < endY; ++y) {
                int curY = y + start.Y;
                int curX = (int)(y/k) + start.X;

                storePositions(new(curX, curY), radius);
            }
        }

        private void StorePositionsAlongHorizontal(Position start, Position end, uint radius) {
            if (start.X > end.X) (start, end) = (end, start);

            var storePositions = (Position pos, uint radius) => {
                StorePositionsSymmetrically(pos, radius, (int additionX) => 0,
                                            (int additionY) => additionY);
            };

            for (int x = 0, endX = end.X - start.X + 1; x < endX; ++x) {
                int curX = x + start.X;
                int curY = (int)(k*x) + start.Y;

                storePositions(new(curX, curY), radius);
            }
        }

        private void StorePositionsSymmetrically(Position pos, uint radius,
                                                 Func<int, int> additionX,
                                                 Func<int, int> additionY) {
            positions.Add(pos);

            for (int i = 1; i <= radius; ++i) {
                var posPositive = new Position(pos.X + additionX(i), pos.Y + additionY(i));
                var posNegative = new Position(pos.X - additionX(i), pos.Y - additionY(i));

                if (bounds.InBounds(posPositive)) positions.Add(posPositive);
                if (bounds.InBounds(posNegative)) positions.Add(posNegative);
            }
        }

        private void StorePositionsAlongLine(Position start, Position end, uint radius) {
            if (start.X > end.X) (start, end) = (end, start);

            int biasX = (int)(radius * Math.Abs(Math.Sin(Math.Atan(k))));
            StorePositionsBetween(start, end, biasX, radius);

            int stepBack = (int)(radius * Math.Sin(Math.Atan(k))) * (k < 0 ? -1 : 1);
            int stepDown = (int)(radius * Math.Cos(Math.Atan(k))) * (k < 0 ? -1 : 1);

            int startX = start.X - stepBack;
            int startY = start.Y + stepDown;
            int maxY = Math.Max(end.Y + stepDown, start.Y + stepDown);
            var limitY1Y2 = (int y1, int y2) => (y1, Math.Min(y2, maxY));
            StoreEdgePositions(0, biasX + stepBack, startX, startY, limitY1Y2);

            startX = end.X + stepBack;
            startY = end.Y - stepDown;
            int minY = Math.Min(end.Y - stepDown, start.Y - stepDown);
            limitY1Y2 = (int y1, int y2) => (Math.Max(y1, minY), y2);
            StoreEdgePositions(-(biasX + stepBack), 0, startX, startY, limitY1Y2);
        }

        private void StorePositionsBetween(Position start, Position end, int biasX, uint radius) {
            int halfOfLen = (int)(radius / Math.Sin(Math.PI/2 - Math.Atan(k)));
            var getY1Y2 = (int x) => {
                int yBias = (int)(k*x) + start.Y;
                int y1 = yBias - halfOfLen;
                int y2 = yBias + halfOfLen;

                return (y1, y2);
            };

            StoreNewPositions(biasX + 1, end.X - start.X - biasX - 1, start.X, getY1Y2);
        }

        private void StoreEdgePositions(int startX, int endX, int biasX, int biasY,
                                        Func<int, int, (int, int)> limitY1Y2) {
            double kPerp = -1.0 / k;

            var getY1Y2 = (int x) => {
                int y1 = (int)(k*x) + biasY;
                int y2 = (int)(kPerp*x) + biasY;
                if (y1 > y2) (y1, y2) = (y2, y1);

                (y1, y2) = limitY1Y2(y1, y2);
                return (y1, y2);
            };

            StoreNewPositions(startX, endX, biasX, getY1Y2);
        }

        private void StoreNewPositions(int startX, int endX, int biasX,
                                       Func<int, (int, int)> getY1Y2) {
            for (int x = startX; x <= endX; ++x) {
                var (y1, y2) = getY1Y2(x);

                for (int y = y1; y <= y2; ++y) {
                    var curPos = new Position(biasX + x, y);
                    if (bounds.InBounds(curPos)) positions.Add(curPos);
                }
            }
        }

        private void StoreRoundPart(Position pos, uint radius) {
            for (int x = (int)-radius; x <= radius; ++x) {
                var curX = pos.X + x;
                if (!bounds.XInBounds(curX)) continue;

                for (int y = (int)-radius; y <= radius; ++y) {
                    var curY = pos.Y + y;
                    if (!bounds.YInBounds(curY)) continue;

                    if (x*x + y*y <= radius*radius)
                        positions.Add(new Position(curX, curY));
                }
            }
        }
    }
}
