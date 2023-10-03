# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased]

...

## [0.0.16] - 2023-10-04

### Dependencies

- Bump HIC.BadMedicine from 1.1.0 to 1.1.1
- Bump SixLabors.ImageSharp.Drawing from 1.0.0-beta15 to 2.0.0
- Bump SixLabors.ImageSharp from 2.1.3 to 3.0.2

## [0.0.15] - 2022-10-31

### Dependencies

- Bump YamlDotNet from 11.2.1 to 12.0.2
- Bump HIC.BadMedicine from 1.1.0 to 1.1.1
- Bump SixLabors.ImageSharp.Drawing from 1.0.0-beta14 to 1.0.0-beta15

## [0.0.14] - 2022-07-11

### Dependencies

- Bump HIC.DicomTypeTranslation from 4.0.0 to 4.0.1
- Bump HIC.BadMedicine from 1.0.1 to 1.1.0
- Bump SixLabors.ImageSharp from 2.1.2 to 2.1.3

## [0.0.13] - 2022-06-02

- Fixed SpiralPitchFactor illegal value of 0.0 [#107](https://github.com/SMI/BadMedicine.Dicom/issues/107)
- Added support for specifying explicit UIDs to use when generating images
- Added linked statistics for frequency of StudyDescription, SeriesDescription and BodyPartExamined for CT
  - Adds SeriesDescription and BodyPartExamined as new tags now modelled
  - Changes StudyDescriptions to more accurately match real DICOM CT data (includes some blank fields)

## [0.0.12] - 2022-05-18

- Fixed memory leaks generating pixel data when running in linux
- Updated to using ImageSharp for pixel data generation instead of libgdiplus

## [0.0.11] - 2022-03-29

### Dependencies

- Built on .Net 6.0 now
- Update fo-dicom to 5.0.2
- Bump HIC.DicomTypeTranslation from 3.0.0 to 4.0.0 
- Bump NUnit from 3.13.2 to 3.13.3
- Bump Microsoft.NET.Test.Sdk from 17.0.0 to 17.1.0


## [0.0.10] - 2022-02-17

### Fixed

- Fixed non zero exit code when using `--help` on command line

### Dependencies

- Bump NunitXml.TestLogger from 3.0.91 to 3.0.117
- Bump XunitXml.TestLogger from 3.0.62 to 3.0.70
- Bump YamlDotNet from 9.1.4 to 11.2.1
- Bump Microsoft.NET.Test.Sdk from 16.9.1 to 17.0.0
- Bump NUnit from 3.13.1 to 3.13.2
- Bump NUnit3TestAdapter from 3.17.0 to 4.2.0
- Bump HIC.DicomTypeTranslation from 2.3.2 to 3.0.0 
- Bump fo-dicom.NetCore from 4.0.7 to 4.0.8

## [0.0.9] - 2021-03-03

### Dependencies

- Remove spurious MathNet.Numerics dependency from nuspec as it caused conflicts with BadMedicine
- Bump HIC.DicomTypeTranslation from 2.3.1 to 2.3.2
- Bump System.Drawing.Common from 5.0.1 to 5.0.2

## [0.0.8] - 2021-03-02

### Changes

- Now built and released via Github Actions

### Dependencies

- Bump YamlDotNet from 9.1.1 to 9.1.4
- Bump XunitXml.TestLogger from 2.1.26 to 2.1.45
- Bump System.Drawing.Common from 5.0.0 to 5.0.1
- Bump HIC.BadMedicine from 0.1.6 to 1.0.0
- Bump Microsoft.NET.Test.Sdk from 16.8.3 to 16.9.1

## [0.0.7] - 2020-08-18

- Enable SourceLink package
- Bump fo-dicom.NetCore from 4.0.5 to 4.0.6
- Bump HIC.DicomTypeTranslation from 2.2.2 to 2.3.1
- Bump YamlDotNet from 8.1.1 to 8.1.2
- Clean remaining LGTM alerts

## [0.0.6] - 2020-05-20

### Changed

- Updated dependencies to latest versions (see [Packages.md](./Packages.md))


## [0.0.5] - 2020-01-30

## Added

- Added direct to database mode for CLI application (`BadDicom` only i.e. not part of the nuget package).

## Changed

- Changed number of images per series to 100 for CT

## [0.0.4] - 2019-10-28

## Changed

- Updated dependencies to latest fo Dicom library 4.0.3

## Fixed

- Fixed dodgy tags in test data generated which results in broken images with fo Dicom 4.0.3

## [0.0.3] - 2019-10-28

### Added 

 - Added Csv mode where by dicom tags are output in full to series/study/instance level CSV files

## [0.0.2] - 2019-07-16

### Added 
 
- Xml Documentation now built/shipped with API

## [0.0.1] - 2019-07-03

### Added 

- Command Line Executable
- Support for CT dicom image generation
- Support for PatientAge, Modality, Address, UIDs, StudyDate/Time
- Support for pixel data / NoPixels flag

[Unreleased]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.15...develop
[0.0.15]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.14...v0.0.15
[0.0.14]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.13...v0.0.14
[0.0.13]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.12...v0.0.13
[0.0.12]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.11...v0.0.12
[0.0.11]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.10...v0.0.11
[0.0.10]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.9...v0.0.10
[0.0.9]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.8...v0.0.9
[0.0.8]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.7...v0.0.8
[0.0.7]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.6...v0.0.7
[0.0.6]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.5...v0.0.6
[0.0.5]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.4...v0.0.5
[0.0.4]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.3...v0.0.4
[0.0.3]: https://github.com/SMI/BadMedicine.Dicom/compare/5517d7e29aaf3742e91b86288b85f692a063dba4...v0.0.3
[0.0.2]: https://github.com/SMI/BadMedicine.Dicom/compare/v0.0.1...5517d7e29aaf3742e91b86288b85f692a063dba4
[0.0.1]: https://github.com/SMI/BadMedicine.Dicom/compare/bdea963df0337e47434c3e72bde7a16a111b99a8...v0.0.1
