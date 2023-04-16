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
        private LineFunc upperBoundFunc = new(0, 0);
        private LineFunc lowerBoundFunc = new(0, 0);

        public void StoreStrokePositions(Bounds bounds, StrokePivot start, StrokePivot end) {
            this.bounds = bounds;
            if (start.Position.X > end.Position.X) (start, end) = (end, start);

            if (lowerBoundFunc.IsVertical() || upperBoundFunc.IsVertical()) {
                StorePositionsAlongVertical(start, end);
            } else if (lowerBoundFunc.IsHorizontal() || upperBoundFunc.IsHorizontal()) {
                StorePositionsAlongHorizontal(start, end);
            } else {
                StorePositionsAlongLine(start, end);
            }

            StoreRoundPart(start);
            StoreRoundPart(end);
        }

        private void InitBoundFuncs(StrokePivot left, StrokePivot right) {
            var radius = Math.Abs((long)left.Radius - right.Radius);

            if (radius == 0)
                InitBoundFuncsSameRadius(left, right);
            else
                InitBoundFuncsDiffRadius(left, right, (uint)radius);
        }

        private void InitBoundFuncsSameRadius(StrokePivot left, StrokePivot right) {
            var centerK = LineFunc.CountK(left.Position, right.Position);
            var angle = Math.Atan(centerK);

            var stepX = left.Radius * Math.Sin(angle);
            var stepY = left.Radius * Math.Cos(angle);

            var lowerBoundX = left.Position.X + stepX;
            var lowerBoundY = left.Position.Y - stepY;
            var upperBoundX = left.Position.X - stepX;
            var upperBoundY = left.Position.Y + stepY;

            var bLowBound = lowerBoundY - centerK*lowerBoundX;
            var bUpBound = upperBoundY - centerK*upperBoundX;

            lowerBoundFunc = new(centerK, bLowBound);
            upperBoundFunc = new(centerK, bUpBound);
        }

        private void InitBoundFuncsDiffRadius(StrokePivot left, StrokePivot right, uint radius) {
            var lenBetweenCenters = Math.Sqrt(Math.Pow(right.Position.X - left.Position.X, 2) +
                                              Math.Pow(right.Position.Y - left.Position.Y, 2));
            var angleToTangent = Math.Acos(radius / lenBetweenCenters);
            var centerLineAngle = Math.Atan(LineFunc.CountK(left.Position, right.Position));
            var angle1 = Math.PI/2 - angleToTangent + centerLineAngle;
            var angle2 = angleToTangent + centerLineAngle - Math.PI/2;

            Func<double, (double, double)> getLowerStep, getUpperStep;
            if (left.Radius > right.Radius && centerLineAngle > 0 ||
                    left.Radius < right.Radius && centerLineAngle < 0) {
                getLowerStep = (double radius) => (radius * Math.Sin(angle1), radius * Math.Cos(angle1));
                getUpperStep = (double radius) => (radius * Math.Sin(angle2), radius * Math.Cos(angle2));
            } else {
                getLowerStep = (double radius) => (radius * Math.Sin(angle2), radius * Math.Cos(angle2));
                getUpperStep = (double radius) => (radius * Math.Sin(angle1), radius * Math.Cos(angle1));
            }

            var (stepXLeftLower, stepYLeftLower) = getLowerStep(left.Radius);
            var (stepXRightLower, stepYRightLower) = getLowerStep(right.Radius);

            var (stepXLeftUpper, stepYLeftUpper) = getUpperStep(left.Radius);
            var (stepXRightUpper, stepYRightUpper) = getUpperStep(right.Radius);

            var upperBoundLeftX = left.Position.X - stepXLeftUpper;
            var upperBoundRightX = right.Position.X - stepXRightUpper;
            var upperBoundLeftY = left.Position.Y + stepYLeftUpper;
            var upperBoundRightY = right.Position.Y + stepYRightUpper;

            var lowerBoundLeftX = left.Position.X + stepXLeftLower;
            var lowerBoundRightX = right.Position.X + stepXRightLower;
            var lowerBoundLeftY = left.Position.Y - stepYLeftLower;
            var lowerBoundRightY = right.Position.Y - stepYRightLower;

            lowerBoundFunc = new(lowerBoundLeftX, lowerBoundLeftY, lowerBoundRightX, lowerBoundRightY);
            upperBoundFunc = new(upperBoundLeftX, upperBoundLeftY, upperBoundRightX, upperBoundRightY);
        }

        private void StorePositionsAlongVertical(StrokePivot below, StrokePivot above) {
            if (below.Position.Y > above.Position.Y) (below, above) = (above, below);

            if (below.Radius == above.Radius) {
                for (int y = below.Position.Y; y <= above.Position.Y; ++y) {
                    StorePositionsSymmetrically(new(below.Position.X, y), below.Radius,
                                                (int additionX) => additionX, (int additionY) => 0);
                }
            } else {
                int verticalX;
                Func<double, double> xCounter;
                if (upperBoundFunc.IsVertical()) {
                    verticalX = below.Position.X - (int)below.Radius*Math.Sign(upperBoundFunc.K);
                    xCounter = lowerBoundFunc.CountX;
                } else {
                    verticalX = below.Position.X + (int)below.Radius*Math.Sign(lowerBoundFunc.K);
                    xCounter = upperBoundFunc.CountX;
                }

                for (int y = below.Position.Y; y <= above.Position.Y; ++y) {
                    int nonVertX = (int)xCounter(y);
                    int minX = Math.Min(verticalX, nonVertX), maxX = Math.Max(verticalX, nonVertX);

                    for (int x = minX; x <= maxX; ++x) {
                        var pos = new Position(x, y);
                        if (bounds.InBounds(pos)) positions.Add(pos);
                    }
                }
            }
        }

        private void StorePositionsAlongHorizontal(StrokePivot left, StrokePivot right) {
            if (left.Radius == right.Radius) {
                for (int x = left.Position.X; x <= right.Position.X; ++x) {
                    StorePositionsSymmetrically(new(x, left.Position.Y), left.Radius,
                                                (int additionX) => 0,
                                                (int additionY) => additionY);
                }
            } else {
                for (int x = left.Position.X; x <= right.Position.X; ++x) {
                    int yMin = (int)lowerBoundFunc.CountY(x);
                    int yMax = (int)upperBoundFunc.CountY(x);

                    for (int y = yMin; y <= yMax; ++y) {
                        var pos = new Position(x, y);
                        if (bounds.InBounds(pos)) positions.Add(pos);
                    }
                }
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
