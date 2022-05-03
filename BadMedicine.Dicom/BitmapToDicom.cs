using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.IO.Buffer;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BadMedicine.Dicom
{
    /// <summary>
    /// See https://gist.github.com/mdubey82/4030263
    /// </summary>
    public class BitmapToDicom
    {
        public static DicomPixelData ImportImage(Bitmap bitmap, DicomDataset dataset)
        {
            byte[] pixels = GetPixels(bitmap, out var rows, out var columns);
            MemoryByteBuffer buffer = new MemoryByteBuffer(pixels);

            dataset.AddOrUpdate(DicomTag.PhotometricInterpretation, PhotometricInterpretation.Rgb.Value);
            dataset.AddOrUpdate(DicomTag.Rows, (ushort)rows);
            dataset.AddOrUpdate(DicomTag.Columns, (ushort)columns);
            dataset.AddOrUpdate(DicomTag.BitsAllocated, (ushort)8);

            DicomPixelData pixelData = DicomPixelData.Create(dataset, true);

            pixelData.BitsStored = 8;
            pixelData.SamplesPerPixel = 3;
            pixelData.HighBit = 7;
            pixelData.PixelRepresentation = 0;
            pixelData.PlanarConfiguration = 0;
            pixelData.AddFrame(buffer);
            
            return pixelData;
        }

        /// <summary>
        /// Adds a second or third (or more) frame to <paramref name="pixelData"/>. 
        /// The size of <paramref name="bitmap"/> must match the original frame. Original
        /// frame can be configured with <see cref="ImportImage(Bitmap, DicomDataset)"/>
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="pixelData"></param>
        public static void AddFrame(Bitmap bitmap, DicomPixelData pixelData)
        {
            byte[] pixels = GetPixels(bitmap, out _, out _);
            MemoryByteBuffer buffer = new MemoryByteBuffer(pixels);

            pixelData.NumberOfFrames++;
            pixelData.AddFrame(buffer);
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
