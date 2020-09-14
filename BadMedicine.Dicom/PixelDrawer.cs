using Dicom;
using Dicom.Imaging;
using Dicom.IO.Buffer;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BadMedicine.Dicom
{
    /// <summary>
    /// Handles drawing image data directly into the dicom file
    /// </summary>
    internal class PixelDrawer
    {
        readonly SolidBrush _blackBrush = new SolidBrush(Color.Black);
        readonly SolidBrush _whiteBrush = new SolidBrush(Color.White);

        internal void DrawBlackBoxWithWhiteText(DicomDataset ds, int width, int height, string msg)
        {
            using (var bitmap = new Bitmap(500, 500))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.FillRectangle(_blackBrush, 0, 0, width, height);
                    using (var font = new Font(FontFamily.GenericMonospace, 12))
                        g.DrawString(msg, font, _whiteBrush, 250, 100);
                }

                byte[] pixels = GetPixels(bitmap, out int rows, out int columns);
                MemoryByteBuffer buffer = new MemoryByteBuffer(pixels);

                ds.Add(DicomTag.PhotometricInterpretation, PhotometricInterpretation.Rgb.Value);
                ds.Add(DicomTag.Rows, (ushort)rows);
                ds.Add(DicomTag.Columns, (ushort)columns);
                ds.Add(DicomTag.BitsAllocated, (ushort)8);

                DicomPixelData pixelData = DicomPixelData.Create(ds, true);
                pixelData.BitsStored = 8;
                pixelData.SamplesPerPixel = 3;
                pixelData.HighBit = 7;
                pixelData.PixelRepresentation = 0;
                pixelData.PlanarConfiguration = 0;
                pixelData.AddFrame(buffer);
            }
        }

        private static byte[] GetPixels(Bitmap image, out int rows, out int columns)
        {
            rows = image.Height;
            columns = image.Width;

            if (rows % 2 != 0 && columns % 2 != 0)
                --columns;

            BitmapData data = image.LockBits(new Rectangle(0, 0, columns, rows), ImageLockMode.ReadOnly, image.PixelFormat);
            IntPtr bmpData = data.Scan0;
            try
            {
                int stride = columns * 3;
                int size = rows * stride;
                byte[] pixelData = new byte[size];
                for (int i = 0; i < rows; ++i)
                    Marshal.Copy(new IntPtr(bmpData.ToInt64() + i * data.Stride), pixelData, i * stride, stride);

                //swap BGR to RGB
                SwapRedBlue(pixelData);
                return pixelData;
            }
            finally
            {
                image.UnlockBits(data);
            }
        }
        private static void SwapRedBlue(byte[] pixels)
        {
            for (int i = 0; i < pixels.Length; i += 3)
            {
                byte temp = pixels[i];
                pixels[i] = pixels[i + 2];
                pixels[i + 2] = temp;
            }
        }
    }
}