using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Colors;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    internal class RGBImage : Image<RGBColor> {
        public static readonly PixelFormat FORMAT = PixelFormats.Rgb24;
        public static readonly int BYTES_PER_PIXEL = (FORMAT.BitsPerPixel + 7) / 8;

        private Stroke<RGBColor>? lastStroke = null;

        #region Constructors
        private RGBImage(int width, int height) : base(new RGBColor[height, width]) {
        }

        public RGBImage(BitmapSource imageToHandle)
                : this(imageToHandle.PixelHeight, imageToHandle.PixelWidth) {
            if (imageToHandle.Format != FORMAT)
                imageToHandle = ConverFormat(imageToHandle);

            byte[] vals = new byte[BYTES_PER_PIXEL * imageToHandle.PixelWidth * imageToHandle.PixelHeight];
            imageToHandle.CopyPixels(vals, BYTES_PER_PIXEL * imageToHandle.PixelWidth, 0);

            for (int i = 0; i < Height; ++i) {
                for (int j = 0; j < Width; ++j) {
                    int blockStart = i * BYTES_PER_PIXEL * imageToHandle.PixelWidth + j * BYTES_PER_PIXEL;
                    this[i, j] = GetPixel(vals[blockStart..(blockStart + BYTES_PER_PIXEL)]);
                }
            }
        }

        public RGBImage(RGBImage other, Position leftUp, Position rightDown)
                : this(rightDown.X - leftUp.X + 1, rightDown.Y - leftUp.Y + 1) {
            for (int i = leftUp.Y; i <= rightDown.Y; ++i) {
                for (int j = leftUp.X; j <= rightDown.X; ++j)
                    this[i, j] = other[i, j];
            }
        }
        #endregion

        public BitmapSource ToBitmap() {
            int stride = Width * BYTES_PER_PIXEL;
            var pixels = new byte[Height, stride];

            if (FORMAT == PixelFormats.Rgb24) {
                for (int y = 0; y < Height; ++y) {
                    for (int x = 0; x < stride; x += 3) {
                        pixels[y, x + 0] = this[y, x].Red;
                        pixels[y, x + 1] = this[y, x].Green;
                        pixels[y, x + 2] = this[y, x].Blue;
                    }
                }
            } else {
                throw new Exception("Unsupported pixel encoding format!");
            }

            return BitmapSource.Create(
                Width, Height, 96, 96,
                FORMAT, null,
                pixels, stride
            );
        }

        public override void AddStroke(Stroke<RGBColor> stroke) {
            lastStroke = stroke;

            foreach (var pos in stroke.Positions) {
                int newRed = this[pos.Y, pos.X].Red + stroke.Color.Red;
                int newGreen = this[pos.Y, pos.X].Green + stroke.Color.Green;
                int newBlue = this[pos.Y, pos.X].Blue + stroke.Color.Blue;

                byte realRed = (byte)Math.Min(newRed, byte.MaxValue);
                byte realGreen = (byte)Math.Min(newGreen, byte.MaxValue);
                byte realBlue = (byte)Math.Min(newBlue, byte.MaxValue);

                this[pos.Y, pos.X] = new(realRed, realGreen, realBlue);
            }
        }

        public StrokePositions GetStroke(Gradient gradient, Position pos, uint height) {
            // Как получить градиент?
            // Как ему следовать?

            // 1) Получить мазок в соответствии с градиентом и максимальной допустимой ошибкой
            throw new NotImplementedException();
        }

        public RGBColor GetColor(StrokePositions stroke) {
            // 2) Получить цвет мазка
            throw new NotImplementedException();
        }

        public void RemoveLastStroke() {
            if (lastStroke != null) {
                // Тут нужно убрать цвета с пикселей, которые попали с командой AddStroke
                throw new NotImplementedException();
                lastStroke = null;
            }
        }

        #region StaticMethods
        public static RGBImage CreateEmpty(int width, int height) {
            var newImage = new RGBImage(width, height);

            for (int i = 0; i < height; ++i) {
                for (int j = 0; j < width; ++j)
                    newImage[i, j] = new RGBColor(byte.MaxValue, byte.MaxValue, byte.MaxValue);
            }

            return newImage;
        }

        #region Getters
        private static RGBColor GetPixel(byte[] pixelBytes) {
            if (FORMAT == PixelFormats.Rgb24)
                return new(pixelBytes[0], pixelBytes[1], pixelBytes[2]);
            else
                throw new Exception("Unsupported pixel encoding format!");
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
        public static RGBImage GetPart(RGBImage image, Position pos, uint height) {
            if (pos.X >= image.Width || pos.Y >= image.Height || pos.X < 0 || pos.Y < 0)
                throw new Exception("Can't get data from the required position!");

            Position leftUp = new(0, image.Width - 1);
            Position rightDown = new(0, image.Height - 1);

            if (pos.X > height) leftUp.X = pos.X - (int)height;
            if (pos.X < image.Width - 1 - height) rightDown.X = pos.X + (int)height;

            if (pos.Y > height) leftUp.Y = pos.Y - (int)height;
            if (pos.Y < image.Height - 1 - height) rightDown.Y = pos.X + (int)height;

            return new(image, leftUp, rightDown);
        }

        public static double GetDifference(RGBImage a, RGBImage b) {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new Exception("Images must be the same size!");

            ulong diff = 0;

            for (int y = 0; y < a.Height; ++y) {
                for (int x = 0; x < a.Width; ++x) {
                    diff += (ulong)Math.Abs(a[y, x].Red - b[y, x].Red);
                    diff += (ulong)Math.Abs(a[y, x].Green - b[y, x].Green);
                    diff += (ulong)Math.Abs(a[y, x].Blue - b[y, x].Blue);
                }
            }

            return (double)diff / (a.Height * a.Width);
        }
        #endregion

        private static BitmapSource ConverFormat(BitmapSource image) {
            var converter = new FormatConvertedBitmap();
            converter.BeginInit();
            converter.Source = image;
            converter.DestinationFormat = FORMAT;
            converter.EndInit();

            return converter;
        }
        #endregion
    }
}
