using FellowOakDicom;
using System.Drawing;
using System.Drawing.Imaging;

namespace BadMedicine.Dicom
{
    /// <summary>
    /// Handles drawing image data directly into the dicom file
    /// </summary>
    internal class PixelDrawer
    {
        readonly SolidBrush _blackBrush = new(Color.Black);
        readonly SolidBrush _whiteBrush = new(Color.White);

        internal Bitmap DrawBlackBoxWithWhiteText(int width, int height, string msg)
        {
            var bitmap = new Bitmap(500, 500,PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(_blackBrush, 0, 0, width, height);
                using var font = new Font(FontFamily.GenericMonospace, 12);
                g.DrawString(msg, font, _whiteBrush,new RectangleF(0,0,500,500));
            }

            return bitmap;
        }
    }
}