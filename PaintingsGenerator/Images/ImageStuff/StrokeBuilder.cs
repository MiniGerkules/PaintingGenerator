using System;
using System.Linq;
using System.Collections.Generic;

using PaintingsGenerator.Colors;
using PaintingsGenerator.MathStuff;

namespace PaintingsGenerator.Images.ImageStuff {
    internal class StrokeBuilder {
        private static readonly Random rand = new();

        private readonly RGBImage image;
        private readonly Gradient gradient;

        public StrokeBuilder(RGBImage image, Gradient gradient) {
            this.image = image;
            this.gradient = gradient;
        }

        public StrokePositions GetStroke(Settings settings, DifferenceOfImages diff) {
            var bounds = new Bounds(0, image.Width - 1, 0, image.Height - 1);
            var start = GetStrokeStartByRand(diff, settings.StartBrushRadius);

            var peprVec = gradient.GetPerpVector(start.Position);
            var points = new StrokePositions { start };
            var errors = new List<double>() { image.GetColorError(start, image.GetColor(start)) };
            var maxRadius = start.Radius;

            while (true) {
                peprVec = gradient.GetPerpVector(points[^1].Position, peprVec);
                peprVec.Normalize();
                if (peprVec.IsPoint()) break;

                var newPos = GetNewPosition(points[^1].Position, peprVec, 2*points[^1].Radius + 1);
                if (!IsPositionNice(bounds, settings, newPos, points)) break;

                var (newPoint, _, error) = GetNewPivot(settings, newPos, points[^1].Radius);
                maxRadius = Math.Max(maxRadius, newPoint.Radius);
                errors.Add(error);

                var strokeErr = errors.Sum() / errors.Count;
                if (strokeErr > errors[0] * settings.MaxColorDiffInStrokeInTimes) break;

                points.Add(newPoint);

                if (points.GetLen() > (2*maxRadius + 1) * settings.RatioOfLenToWidthLargest) break;
            }

            return points;
        }

        private bool IsPositionNice(Bounds bounds, Settings settings,
                                    Position newPos, StrokePositions prevPos) {
            if (!bounds.InBounds(newPos)) return false;
            if (prevPos.Count < 2) return true;

            var prevSeg = new Vector2D(prevPos[^2].Position, prevPos[^1].Position);
            var curSeg = new Vector2D(prevPos[^1].Position, newPos);

            var scalProd = Vector2D.GetScalarProd(prevSeg, curSeg);
            var angle = Math.Acos(scalProd / (prevSeg.Length*curSeg.Length));
            var res = 180*angle / Math.PI < settings.MaxAngleOfBendingInDegrees;

            return res;
        }

        public static Position GetStrokeStartByDiff(DifferenceOfImages diff, uint radius) {
            uint startY = 2*radius, startX = startY;
            var posWithMaxDiff = new Position((int)startX, (int)startY);
            var maxDiff = diff.GetSumDifferenceAt(posWithMaxDiff, radius);

            for (uint y = startY, endY = (uint)(diff.Height - 2*radius); y < endY; y += radius) {
                for (uint x = startX, endX = (uint)(diff.Width - 2*radius); x < endX; x += radius) {
                    var curPos = new Position((int)x, (int)y);
                    var curDiff = diff.GetSumDifferenceAt(curPos, radius);

                    if (curDiff > maxDiff) {
                        maxDiff = curDiff;
                        posWithMaxDiff = curPos;
                    }
                }
            }

            return posWithMaxDiff;
        }

        public static StrokePivot GetStrokeStartByRand(DifferenceOfImages diff, uint radius) {
            var possibleStarts = new List<KeyValuePair<Position, ulong>>();
            for (int i = 0; i < 20; ++i) {
                var start = new Position(rand.Next(0, diff.Width), rand.Next(0, diff.Height));
                var difference = diff.GetSumDifferenceAt(start, radius);

                possibleStarts.Add(new(start, difference));
            }

            return new(possibleStarts.MaxBy((elem) => elem.Value).Key, radius);
        }

        private (StrokePivot, RGBColor, double) GetNewPivot(Settings settings, Position pos, uint prevRadius) {
            var minRadius = (uint)(prevRadius * (1-settings.MaxDiffOfBrushRadiusesInTimes));
            var maxRadius = (uint)(prevRadius * (1+settings.MaxDiffOfBrushRadiusesInTimes));

            var pivot = new StrokePivot(pos, minRadius);
            var color = image.GetColor(pivot);
            double startError = image.GetColorError(pivot, color), error = startError;
            for (uint radius = minRadius + 1; radius <= maxRadius; ++radius) {
                var curPivot = new StrokePivot(pos, radius);
                var curColor = image.GetColor(pivot);
                var curError = image.GetColorError(pivot, color);

                if (curError > startError * settings.MaxColorDiffInBrushInTimes) break;

                pivot = curPivot;
                color = curColor;
                error = curError;
            }

            return (pivot, color, error);
        }

        private static Position GetNewPosition(Position pos, Vector2D direction,
                                               uint step) {
            var newX = pos.X + direction.X*step;
            var newY = pos.Y + direction.Y*step;

            return new((int)newX, (int)newY);
        }
    }
}
