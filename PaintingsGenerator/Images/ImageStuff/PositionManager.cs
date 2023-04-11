﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PaintingsGenerator.Images.ImageStuff {
    internal class PositionManager {
        private readonly HashSet<Position> positions = new();
        public ImmutableHashSet<Position> StoredPositions => positions.ToImmutableHashSet();

        public void StoreStrokePositions(Position start, Position end, uint radius) {
            var bounds = new Bounds(start, end, radius);
            var k_norm = (double)(end.Y-start.Y) / (end.X-start.X);

            if (double.IsInfinity(k_norm)) { // Vertical
                StorePositionsAlongVertical(bounds, start, end, k_norm, radius);
            } else if (Math.Abs(k_norm) <= 1e-5) { // Horizontal
                StorePositionsAlongHorizontal(bounds, start, end, k_norm, radius);
            } else { // With another angle
                StorePositionsAlongLine(bounds, start, end, k_norm, radius);
            }

            StoreRoundPart(bounds, start, radius);
            StoreRoundPart(bounds, end, radius);
        }

        private void StorePositionsAlongVertical(Bounds bounds, Position start,
                                                 Position end, double k, uint radius) {
            if (start.Y > end.Y) (start, end) = (end, start);

            var storePositions = (Bounds bounds, Position pos, uint radius) => {
                StorePositionsSymmetrically(bounds, pos, radius,
                                            (int additionX) => additionX,
                                            (int additionY) => 0);
            };

            for (int y = 0, endY = end.Y - start.Y + 1; y < endY; ++y) {
                int curY = y + start.Y;
                int curX = (int)(y/k) + start.X;

                storePositions(bounds, new(curX, curY), radius);
            }
        }

        private void StorePositionsAlongHorizontal(Bounds bounds, Position start,
                                                   Position end, double k, uint radius) {
            if (start.X > end.X) (start, end) = (end, start);

            var storePositions = (Bounds bounds, Position pos, uint radius) => {
                StorePositionsSymmetrically(bounds, pos, radius,
                                            (int additionX) => 0,
                                            (int additionY) => additionY);
            };

            for (int x = 0, endX = end.X - start.X + 1; x < endX; ++x) {
                int curX = x + start.X;
                int curY = (int)(k*x) + start.Y;

                storePositions(bounds, new(curX, curY), radius);
            }
        }

        private void StorePositionsSymmetrically(Bounds bounds, Position pos, uint radius,
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

        private void StorePositionsAlongLine(Bounds bounds, Position start, Position end,
                                             double k, uint radius) {
            if (start.X > end.X) (start, end) = (end, start);

            int biasX = (int)(radius * Math.Abs(Math.Sin(Math.Atan(k))));

            StorePositionsBetween(start, end, bounds, k, biasX, radius);

            int stepBack = (int)(radius * Math.Sin(Math.Atan(k))) * (k < 0 ? -1 : 1);
            int stepDown = (int)(radius * Math.Cos(Math.Atan(k))) * (k < 0 ? -1 : 1);

            int startX = start.X - stepBack;
            int startY = start.Y + stepDown;
            int maxY = Math.Max(end.Y + stepDown, start.Y + stepDown);
            var limitY1Y2 = (int y1, int y2) => (y1, Math.Min(y2, maxY));
            StoreEdgePositions(bounds, 0, biasX + stepBack, startX, startY, limitY1Y2, k);

            startX = end.X + stepBack;
            startY = end.Y - stepDown;
            int minY = Math.Min(end.Y - stepDown, start.Y - stepDown);
            limitY1Y2 = (int y1, int y2) => (Math.Max(y1, minY), y2);
            StoreEdgePositions(bounds, -(biasX + stepBack), 0, startX, startY, limitY1Y2, k);
        }

        private void StorePositionsBetween(Position start, Position end,
                                           Bounds bounds, double k, int biasX, uint radius) {
            int halfOfLen = (int)(radius / Math.Sin(Math.PI/2 - Math.Atan(k)));
            var getY1Y2 = (int x) => {
                int yBias = (int)(k*x) + start.Y;
                int y1 = yBias - halfOfLen;
                int y2 = yBias + halfOfLen;

                return (y1, y2);
            };

            StoreNewPositions(bounds, biasX + 1, end.X - start.X - biasX - 1, start.X, getY1Y2);
        }

        private void StoreEdgePositions(Bounds bounds, int startX, int endX, int biasX,
                                        int biasY, Func<int, int, (int, int)> limitY1Y2, double k) {
            double kPerp = -1 / k;

            var getY1Y2 = (int x) => {
                int y1 = (int)(k*x) + biasY;
                int y2 = (int)(kPerp*x) + biasY;
                if (y1 > y2) (y1, y2) = (y2, y1);

                (y1, y2) = limitY1Y2(y1, y2);
                return (y1, y2);
            };

            StoreNewPositions(bounds, startX, endX, biasX, getY1Y2);
        }

        private void StoreNewPositions(Bounds bounds, int startX, int endX, int biasX,
                                       Func<int, (int, int)> getY1Y2) {
            for (int x = startX; x <= endX; ++x) {
                var (y1, y2) = getY1Y2(x);

                for (int y = y1; y <= y2; ++y) {
                    var curPos = new Position(biasX + x, y);
                    if (bounds.InBounds(curPos)) positions.Add(curPos);
                }
            }
        }

        private void StoreRoundPart(Bounds bounds, Position pos, uint radius) {
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
