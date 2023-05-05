using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    internal interface IImage : IBitmapConvertable {
        void AddStroke(Stroke stroke);
    }
}
