using Dicom;
using System;
using System.Collections.Generic;
using System.Text;

namespace BadMedicine.Dicom
{
    public class SeriesWritter
    {
        private readonly SeriesWritterArgs args;

        /// <summary>
        /// Creates a new series
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="numberToWrite"></param>
        public SeriesWritter(DicomDataGenerator parent, SeriesWritterArgs args)
        {
            this.args = args;
        }

        /// <summary>
        /// Generates all datasets for the series
        /// </summary>
        /// <returns></returns>
        public DicomDataset[] Generate()
        {
            return null;
        }

    }
}
