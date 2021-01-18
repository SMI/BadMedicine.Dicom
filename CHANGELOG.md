# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased]

### Dependencies

- Bump YamlDotNet from 9.1.1 to 9.1.4

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

[Unreleased]: https://github.com/HicServices/BadMedicine.Dicom/compare/v0.0.6...develop
[0.0.6]: https://github.com/HicServices/BadMedicine.Dicom/compare/v0.0.5...v0.0.6
[0.0.5]: https://github.com/HicServices/BadMedicine.Dicom/compare/v0.0.4...v0.0.5
[0.0.4]: https://github.com/HicServices/BadMedicine.Dicom/compare/v0.0.3...v0.0.4
[0.0.3]: https://github.com/HicServices/BadMedicine.Dicom/compare/5517d7e29aaf3742e91b86288b85f692a063dba4...v0.0.3
[0.0.2]: https://github.com/HicServices/BadMedicine.Dicom/compare/v0.0.1...5517d7e29aaf3742e91b86288b85f692a063dba4
[0.0.1]: https://github.com/HicServices/BadMedicine.Dicom/compare/bdea963df0337e47434c3e72bde7a16a111b99a8...v0.0.1
