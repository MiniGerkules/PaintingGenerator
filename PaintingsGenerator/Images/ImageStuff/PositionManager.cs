using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

using PaintingsGenerator.MathStuff;

namespace PaintingsGenerator.Images.ImageStuff {
    internal class PositionManager {
        private readonly HashSet<Position> positions = new();
        public ImmutableHashSet<Position> StoredPositions => positions.ToImmutableHashSet();

        private Bounds bounds = new(0, 0, 0, 0);
        private LineFunc upperBoundFunc = new(0, 0);
        private LineFunc lowerBoundFunc = new(0, 0);

        public void StoreStrokePositions(Bounds bounds, StrokePivot start, StrokePivot end) {
            this.bounds = bounds;
            if (start.Position.X > end.Position.X) (start, end) = (end, start);

            InitBoundFuncs(start, end);

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

        public void StorePositionsAlongLine(StrokePivot left, StrokePivot right) {
            var centerBound = new LineFunc(left.Position, right.Position);
            int startX = new int[] { left.Position.X-(int)left.Radius, right.Position.X-(int)right.Radius }.Min();
            int endX = new int[] { left.Position.X+(int)left.Radius, right.Position.X+(int)right.Radius }.Max();

            LineFunc leftBound, rightBound, lowerBound, upperBound;
            if (Math.Abs(Math.Atan(centerBound.K)) > Math.PI/4) {
                (leftBound, rightBound, lowerBound, upperBound) = GetFuncBounds(
                    Math.Sign(centerBound.K) == 1, lowerBoundFunc,
                    centerBound.IsVertical() ? upperBoundFunc : centerBound,
                    lowerBoundFunc, left.Position, right.Position
                );
            } else {
                leftBound = lowerBoundFunc.GetPerp(left.Position);
                rightBound = lowerBoundFunc.GetPerp(right.Position);
                lowerBound = lowerBoundFunc;
                upperBound = centerBound;
            }
            int startY = new int[] { left.Position.Y-(int)left.Radius, right.Position.Y-(int)right.Radius }.Min();
            int endY = new int[] { left.Position.Y, right.Position.Y }.Max();
            StoreInBounds(new(startX, startY), new(endX, endY), leftBound.CountX,
                          rightBound.CountX, lowerBound.CountY, upperBound.CountY);

            if (Math.Abs(Math.Atan(centerBound.K)) > Math.PI/4) {
                (leftBound, rightBound, lowerBound, upperBound) = GetFuncBounds(
                    Math.Sign(centerBound.K) == 1,
                    centerBound.IsVertical() ? lowerBoundFunc : centerBound,
                    upperBoundFunc, upperBoundFunc, left.Position, right.Position
                );
            } else {
                leftBound = upperBoundFunc.GetPerp(left.Position);
                rightBound = upperBoundFunc.GetPerp(right.Position);
                lowerBound = centerBound;
                upperBound = upperBoundFunc;
            }
            startY = new int[] { left.Position.Y, right.Position.Y }.Min();
            endY = new int[] { left.Position.Y+(int)left.Radius, right.Position.Y+(int)right.Radius }.Max();
            StoreInBounds(new(startX, startY), new(endX, endY), leftBound.CountX,
                          rightBound.CountX, lowerBound.CountY, upperBound.CountY);
        }

        private static (LineFunc, LineFunc, LineFunc, LineFunc) GetFuncBounds(
                bool isRised, LineFunc lowerBoundFunc, LineFunc upperBoundFunc,
                LineFunc endpointsBoundIdentifier, Position left, Position right) {
            LineFunc leftBound, rightBound, lowerBound, upperBound;

            if (isRised) {
                leftBound = upperBoundFunc;
                rightBound = lowerBoundFunc;

                lowerBound = endpointsBoundIdentifier.GetPerp(left);
                upperBound = endpointsBoundIdentifier.GetPerp(right);
            } else {
                leftBound = lowerBoundFunc;
                rightBound = upperBoundFunc;

                lowerBound = endpointsBoundIdentifier.GetPerp(right);
                upperBound = endpointsBoundIdentifier.GetPerp(left);
            }

            return (leftBound, rightBound, lowerBound, upperBound);
        }

        private void StoreInBounds(Position start, Position end,
                                   Func<double, double> countLeftBoundX,
                                   Func<double, double> countRightBoundX,
                                   Func<double, double> countLowerBoundY,
                                   Func<double, double> countUpperBoundY) {
            for (int x = start.X; x <= end.X; ++x) {
                for (int y = start.Y; y <= end.Y; ++y) {
                    int leftX = (int)Math.Round(countLeftBoundX(y));
                    int rightX = (int)Math.Round(countRightBoundX(y));
                    int lowerY = (int)Math.Round(countLowerBoundY(x));
                    int upperY = (int)Math.Round(countUpperBoundY(x));

                    if (leftX <= x && x <= rightX && lowerY <= y && y <= upperY) {
                        var pos = new Position(x, y);
                        if (bounds.InBounds(pos)) positions.Add(pos);
                    }
                }
            }
        }

        private void StoreRoundPart(StrokePivot pivot) {
            for (int x = (int)-pivot.Radius; x <= pivot.Radius; ++x) {
                var curX = pivot.Position.X + x;
                if (!bounds.XInBounds(curX)) continue;

                for (int y = (int)-pivot.Radius; y <= pivot.Radius; ++y) {
                    var curY = pivot.Position.Y + y;
                    if (!bounds.YInBounds(curY)) continue;

                    if (x*x + y*y <= pivot.Radius*pivot.Radius)
                        positions.Add(new Position(curX, curY));
                }
            }
        }
    }
}
