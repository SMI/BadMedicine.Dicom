using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using SynthEHR.Datasets;
using SynthEHR;

namespace BadMedicine.Dicom;

internal sealed class DicomDataGeneratorStats
{
    public static readonly DicomDataGeneratorStats Instance = new();


    /// <summary>
    /// Dictionary of Modality=>StudyDescription by frequency
    /// ^([a-z]+),"?([^"\n]+)"?,(\d+)$
    /// Only has StudyDescriptions
    /// </summary>
    public readonly Dictionary<string,BucketList<string>> TagValuesByModalityAndTag = new()
    {
        {"CR",new(){{814,"XR Chest"},{122,"XR Abdomen"},{113,"XR Pelvis"},{80,"XR Knee Rt"},{57,"XR Knee Lt"},{56,"XR Shoulder Rt"},{56,"XR Ankle Rt"},{50,"XR Foot Lt"},{45,"XR Hip Lt"},{43,"XR Knee Both"},{42,"XR Shoulder Lt"},{40,"XR Foot Rt"},{36,"XR Ankle Lt"},{34,"XR Lumbar spine"},{25,"XR Hip Rt"},{24,"XR Cervical spine"},{19,"XR Wrist Lt"},{19,"XR Elbow Lt"},{16,"XR Wrist Rt"},{15,"XR Femur Lt"},{14,"XR Foot Both"},{13,"XR Hand Lt"},{13,"XR Femur Rt"},{12,"TIB/FIB RIGHT"},{11,"XR Skeletal survey myeloma"},{10,"XR Scaphoid Rt"},{10,"XR Hand Rt"},{10,"Orthopaedic pinning lower limb Rt"},{9,"XR Humerus Rt"},{9,"XR Fingers Lt"},{8,"XR Wrist Both"},{8,"XR Thoracic spine"},{8,"XR Fingers Rt"},{8,"XR Ankle Both"},{8,"Orthopaedic pinning upper limb Rt"},{7,"XR Thumb Rt"},{7,"XR Thoracolumbar spine"},{7,"XR Humerus Lt"},{7,"TIB/FIB LEFT"},{7,"Orthopaedic pinning hip Lt"},{6,"XR Skull"},{5,"XR Thumb Lt"},{5,"XR Facial bones"},{1,"XR Toe great Rt"},{1,"XR Tibia & fibula Lt"},{1,"XR Sternum"},{1,"XR Shoulder Both"},{1,"XR Scaphoid Lt"},{1,"XR Patella Rt"},{1,"XR Orbit foreign body demonstration Both"},{1,"XR Neck soft tissue"},{1,"XR Mandible"},{1,"XR Hand Both"},{1,"XR Forefoot Lt"},{1,"XR Elbow Rt"},{1,"XR Calcaneus Rt"},{1,"Ureteric stent retrograde Rt"},{1,"Orthopaedic pinning upper limb Lt"},{1,"Orthopaedic pinning hip Rt"},{1,"Mobile image intensifier lower limb Rt"},{1,"HUMERUS AP"},{1,"FOREARM RIGHT"},{1,"FEMUR"}}},
{"CT",new(){{153,"CT Head"},{51,"CT Thorax & abdo & pel"},{51,"CT Abdomen & pelvis wi"},{32,"CT Angiogram pulmonary"},{28,"CT Chest high resolution"},{27,"CT Thorax & abdo & pelvis with contrast"},{17,"CT Urogram"},{12,"CT Renal Both"},{12,"CT Angiogram aorta"},{11,"PET FDG Whole body"},{11,"CT Abdomen & pelvis with contrast"},{10,"CT Angiogram lower limb Both"},{9,"CT Colonoscopy virtual"},{8,"CT Thorax & abdomen wi"},{6,"CT Thorax"},{6,"CT Renal with contrast Both"},{6,"CT Neck with contrast"},{5,"CT Head with contrast"},{1,"PET FDG Head and Neck"},{1,"CT Wrist Lt"},{1,"CT Thorax with contrast"},{1,"CT Thorax & abdomen with contrast"},{1,"CT Temporal bones"},{1,"CT Spine thoracic"},{1,"CT Spine cervical"},{1,"CT Sinuses"},{1,"CT Shoulder Rt"},{1,"CT Pelvis"},{1,"CT Pancreas with contrast"},{1,"CT Liver with contrast"},{1,"CT Knee Rt"},{1,"CT Knee Lt"},{1,"CT Hip Rt"},{1,"CT Hip Lt"},{1,"CT Hip Both"},{1,"CT Guided biopsy"},{1,"CT Guided ablation"},{1,"CT Foot Rt"},{1,"CT Cone beam mandible"},{1,"CT Cardiac angiogram coronary"},{1,"CT Angiogram intracranial"},{1,"CT Angio aortic arch &"},{1,"CT Adrenal Both"},{1,"CT Abdomen & pelvis"},{1,"CCHAPC"}}},
{"DX",new(){{305,"XR Chest"},{24,"XR Wrist Lt"},{14,"XR Ankle Rt"},{12,"XR Pelvis"},{12,"XR Abdomen"},{11,"XR Shoulder Rt"},{11,"XR Foot Rt"},{9,"XR Knee Rt"},{9,"XR Knee Lt"},{9,"XR Foot Lt"},{8,"XR Shoulder Lt"},{8,"XR Scaphoid Lt"},{6,"XR Hand Rt"},{5,"XR Hand Lt"},{1,"XR Wrist Rt"},{1,"XR Tibia and fibula Lt"},{1,"XR Thumb Rt"},{1,"XR Thumb Lt"},{1,"XR Thoracic spine"},{1,"XR Scaphoid Rt"},{1,"XR Pathological specimen"},{1,"XR Lumbar spine"},{1,"XR Hip Rt"},{1,"XR Femur Lt"},{1,"XR Facial bones"},{1,"XR Elbow Rt"},{1,"XR Elbow Lt"},{1,"XR Clavicle Lt"},{1,"XR Ankle Lt"}}},
{"IO",new(){{44,"XR Dental intraoral periapical"},{18,"XR Dental bitewing"},{11,"XR Dental intraoral incisor"},{8,"23 PERIAPICAL"},{5,"XR Dental intraoral molar"},{1,"XR Dental intraoral premolar"},{1,"XR Dental intra-oral"},{1,"XR Dental intra-oral molar"}}},
{"KO",new(){{1,"Primary PCI"}}},
{"MG",new(){{116,"XR Mammogram Both"},{14,"XR Mammogram Rt"},{13,"XR Mammogram Lt"},{1,"XR Mammogram compression view Lt"},{1,"Screening Mammography, Right"},{1,"Coned Compression, Right"}}},
{"MR",new(){{50,"MRI Head"},{17,"l-spine^General"},{16,"MRCP"},{13,"NINEWELLS^Lumbar Spine"},{10,"MRI Spine cervical"},{10,"MRI Internal auditory meatus Both"},{8,"MRI Heart"},{6,"MRI Pelvis prostate"},{1,"STRACATHRO^L SPINE"},{1,"NINEWELLS^Pelvis"},{1,"MRI Thorax"},{1,"MRI Spine whole"},{1,"MRI Spine thoracic"},{1,"MRI Scaphoid Right"},{1,"MRI Scaphoid Left"},{1,"MRI scan"},{1,"MRI Renal Both"},{1,"MRI Pituitary"},{1,"MRI Pelvis"},{1,"MRI Pelvis with contrast"},{1,"MRI Pelvis rectum"},{1,"MRI Pelvis gynaecological"},{1,"MRI Neck with contrast"},{1,"MRI Liver with contrast"},{1,"MRI Knee Rt"},{1,"MRI Knee Lt"},{1,"MRI Hip Lt"},{1,"MRI Head with contrast"},{1,"MRI Hand Rt"},{1,"MRI Forearm Rt"},{1,"MRI Foot Rt"},{1,"MRI Foot Lt"},{1,"MRI Cardiac myocardial viability"},{1,"MRI Breast with contrast Both"},{1,"MRI Ankle Lt"},{1,"MRA Lower limb Both"},{1,"hip^Routine"}}},
{"NM",new(){{34,"NM Bone whole body"},{12,"NM Bone dynamic"},{1,"NM Renogram with diuretic"},{1,"NM Parathyroid MIBI injection + Scan"},{1,"NM Lung ventilation scan V"},{1,"NM Lung perfusion scan Q"},{1,"NM Cardiac MUGA"},{1,"NM Brain Ioflupane DaTSCAN images"}}},
{"OT",new(){{391,"PET FDG Whole body"}}},
{"PR",new(){{13,"CT Thorax & abdo & pel"},{1,"XR Mammogram Rt"},{1,"XR Chest"},{1,"NINEWELLS^Lumbar Spine"},{1,"MRI Pelvis with contrast"},{1,"CT Renal with contrast Both"},{1,"CT Angiogram pulmonary"}}},
{"PT",new(){{2597,"PET FDG Whole body"},{42,"PET FDG Head and Neck"}}},
{"PX",new(){{7,"XR Orthopantomogram full"},{1,"XR Orthopantomogram Lt"}}},
{"RF",new(){{357,"Water soluble contrast swallow & meal"},{339,"Barium swallow"},{329,"Fistulogram"},{117,"Barium swallow & meal"},{32,"ERCP"},{18,"Tubogram"},{6,"Oesophageal stent insertion"},{1,"Video swallow"},{1,"Nasogastric feeding tube"},{1,"Endoscopic guided oesophageal stent"}}},
{"SC",new(){{1,"XR Dental intraoral periapical"}}},
{"SR",new(){{1037,"XR Chest"},{244,"CT Head"},{102,"XR Pelvis"},{72,"XR Abdomen"},{71,"US Abdomen"},{67,"CT Abdomen & pelvis wi"},{61,"CT Angiogram pulmonary"},{40,"XR Shoulder Rt"},{40,"XR Knee Rt"},{37,"CT Thorax & abdo & pel"},{34,"XR Ankle Rt"},{32,"XR Knee Lt"},{31,"XR Hip Lt"},{30,"XR Hip Rt"},{30,"CT Urogram"},{28,"XR Shoulder Lt"},{28,"XR Foot Lt"},{25,"CT Chest high resolution"},{24,"XR Mammogram Both"},{24,"NM Bone whole body"},{23,"XR Wrist Lt"},{23,"XR Foot Rt"},{22,"US Urinary tract"},{22,"CT Renal Both"},{20,"US Kidney Both"},{18,"XR Ankle Lt"},{17,"l-spine^General"},{16,"XR Lumbar spine"},{16,"XR Dental intraoral periapical"},{15,"MRI Head"},{14,"MRCP"},{13,"NINEWELLS^Lumbar Spine"},{12,"STRACATHRO^L SPINE"},{12,"Diagnostic Coronary Angiogram"},{11,"XR Knee Both"},{11,"XR Cervical spine"},{11,"CT Abdomen & pelvis with contrast"},{10,"XR Wrist Rt"},{10,"XR Hand Lt"},{10,"MRI Internal auditory meatus Both"},{10,"CT Thorax & abdomen wi"},{10,"CT Thorax & abdo & pelvis with contrast"},{9,"XR Elbow Lt"},{9,"CT Renal with contrast Both"},{9,"CT Abdomen & pelvis"},{8,"XR Thumb Rt"},{8,"US Pelvis transabdominal"},{8,"MRI Spine cervical"},{8,"CT Neck with contrast"},{7,"XR Orthopantomogram full"},{7,"XR Mammogram Lt"},{7,"XR Femur Lt"},{7,"US Testes"},{7,"US Neck"},{7,"US Abdomen & pelvis"},{7,"PICC Line insertion"},{7,"CT Thorax"},{7,"CT Thorax with contrast"},{6,"XR Thoracic spine"},{6,"XR Mammogram Rt"},{6,"XR Hand Rt"},{6,"XR Foot Both"},{6,"XR Fingers Lt"},{6,"US Soft Tissue"},{6,"NM Bone dynamic"},{6,"Diagnostic Angiogram +/- PCI"},{6,"CT Head with contrast"},{6,"CT Angiogram aorta"},{5,"XR Thumb Lt"},{5,"XR Femur Rt"},{5,"US Doppler lower limb veins Rt"},{5,"US Breast Lt"},{5,"MRI Heart"},{5,"CT Angiogram lower limb Both"},{1,"XR Wrist Both"},{1,"XR Toe great Rt"},{1,"XR Tibia and fibula Lt"},{1,"XR Tibia & fibula Lt"},{1,"XR Thoracolumbar spine"},{1,"XR Sternum"},{1,"XR Skull"},{1,"XR Skeletal survey myeloma"},{1,"XR Shoulder Both"},{1,"XR Scaphoid Rt"},{1,"XR Scaphoid Lt"},{1,"XR Orthopantomogram Lt"},{1,"XR Orbit foreign body demonstration Both"},{1,"XR Neck soft tissue"},{1,"XR Mandible"},{1,"XR Mammogram compression view Lt"},{1,"XR Humerus Rt"},{1,"XR Humerus Lt"},{1,"XR Hand Both"},{1,"XR Forefoot Lt"},{1,"XR Fingers Rt"},{1,"XR Facial bones"},{1,"XR Elbow Rt"},{1,"XR Dental intraoral molar"},{1,"XR Dental intraoral incisor"},{1,"XR Dental bitewing"},{1,"XR Clavicle Lt"},{1,"XR Calcaneus Rt"},{1,"XR Ankle Both"},{1,"Video swallow"},{1,"Venoplasty"},{1,"US Transplant kidney"},{1,"US Thyroid"},{1,"US Shoulder Rt"},{1,"US Salivary glands parotid"},{1,"US Pelvis transvaginal"},{1,"US Pelvis Transabdominal/Transvaginal"},{1,"US Liver"},{1,"US Knee Lt"},{1,"US Hand Lt"},{1,"US Guided skin marking"},{1,"US Guided core biopsy breast Lt"},{1,"US Guided core biopsy axilla Rt"},{1,"US Guided aspiration thyroid"},{1,"US Foot Rt"},{1,"US Foot Lt"},{1,"US Extremity"},{1,"US Elbow Rt"},{1,"US Doppler lower limb veins Lt"},{1,"US Breast Rt"},{1,"US Ankle Rt"},{1,"Ureteric stent retrograde Both"},{1,"Tunnelled central venous line insertion"},{1,"Tubogram"},{1,"Transthoracic Echocardiogram"},{1,"TIB/FIB RIGHT"},{1,"TIB/FIB LEFT"},{1,"Stent graft abdominal aorta"},{1,"Primary PCI"},{1,"Planned Multivessel Coronary PCI"},{1,"PET FDG Whole body"},{1,"Orthopaedic pinning upper limb Rt"},{1,"NM Renogram with diuretic"},{1,"NM Parathyroid MIBI injection + Scan"},{1,"NM Lung ventilation scan V"},{1,"NM Lung perfusion scan Q"},{1,"NM Cardiac MUGA"},{1,"NM Brain Ioflupane DaTSCAN images"},{1,"NINEWELLS^Pelvis"},{1,"Nephrostomy Rt"},{1,"Nephrostomy Lt"},{1,"Nephrostogram Rt"},{1,"Nephrostogram Lt"},{1,"Nasogastric feeding tube"},{1,"MRI Thorax"},{1,"MRI Spine whole"},{1,"MRI Spine thoracic"},{1,"MRI Scaphoid Right"},{1,"MRI Scaphoid Left"},{1,"MRI scan"},{1,"MRI Pituitary"},{1,"MRI Pelvis"},{1,"MRI Pelvis with contrast"},{1,"MRI Pelvis rectum"},{1,"MRI Pelvis prostate"},{1,"MRI Pelvis gynaecological"},{1,"MRI Neck with contrast"},{1,"MRI Liver with contrast"},{1,"MRI Knee Rt"},{1,"MRI Knee Lt"},{1,"MRI Hip Lt"},{1,"MRI Hand Rt"},{1,"MRI Forearm Rt"},{1,"MRI Foot Rt"},{1,"MRI Foot Lt"},{1,"MRI Ankle Lt"},{1,"hip^Routine"},{1,"Gastrostomy insertion"},{1,"FOREARM RIGHT"},{1,"Fluoroscopic guided injection hip Rt"},{1,"Diagnostic Angiogram +/- PCI with graft"},{1,"CT Wrist Lt"},{1,"CT Thorax & abdomen with contrast"},{1,"CT Temporal bones"},{1,"CT Spine thoracic"},{1,"CT Spine cervical"},{1,"CT Sinuses"},{1,"CT Shoulder Rt"},{1,"CT Pelvis"},{1,"CT Pancreas with contrast"},{1,"CT Knee Rt"},{1,"CT Knee Lt"},{1,"CT Hip Rt"},{1,"CT Hip Lt"},{1,"CT Hip Both"},{1,"CT Guided biopsy"},{1,"CT Foot Rt"},{1,"CT Cone beam mandible"},{1,"CT Colonoscopy virtual"},{1,"CT Cardiac angiogram coronary"},{1,"CT Angiogram intracranial"},{1,"Cateterismo y Angiocardiografía Izquierdos"},{1,"Barium swallow"},{1,"Barium swallow & meal"},{1,"Arterial stent renal Lt"},{1,"Arterial stent iliac Lt"},{1,"Arterial Stent Femoral Rt"},{1,"Angioplasty vein graft"},{1,"Angioplasty superficial femoral Rt"},{1,"Angioplasty superficial femoral Lt"},{1,"Angioplasty infrapopliteal Rt"},{1,"Angio cerebral"},{1,"Angio aortofemoral lower limb Both"}}},
{"US",new(){{1847,"US Abdomen"},{332,"US Kidney Both"},{310,"US Urinary tract"},{233,"US Abdomen & pelvis"},{151,"US Testes"},{103,"US Neck"},{102,"US Pelvis transabdominal"},{88,"US Doppler lower limb veins Rt"},{70,"US Doppler lower limb veins Lt"},{67,"US Soft Tissue"},{66,"US Breast Lt"},{65,"US Salivary glands parotid"},{47,"US Thyroid"},{39,"US Shoulder Rt"},{33,"US Liver"},{30,"US Foot Lt"},{30,"US Extremity"},{29,"US Guided aspiration thyroid"},{19,"US Hand Lt"},{19,"US Guided skin marking"},{19,"US Breast Rt"},{16,"US Pelvis transvaginal"},{15,"US Foot Rt"},{14,"US Pelvis Transabdominal/Transvaginal"},{14,"US Guided core biopsy axilla Rt"},{13,"US Ankle Rt"},{10,"US Transplant kidney"},{7,"US Elbow Rt"},{6,"US Knee Lt"},{5,"US Guided core biopsy breast Lt"},{1,"US guided guidewire Loc breast Rt"},{1,"US guided guidewire Loc breast Lt"}}},
{"XA",new(){{228,"Diagnostic Angiogram +/- PCI"},{197,"Diagnostic Coronary Angiogram"},{126,"Stent graft abdominal aorta"},{125,"Primary PCI"},{119,"Angioplasty superficial femoral Rt"},{101,"Planned Multivessel Coronary PCI"},{95,"Angio cerebral"},{75,"Planned Coronary PCI"},{68,"Angioplasty superficial femoral Lt"},{54,"Venoplasty"},{54,"Angioplasty infrapopliteal Rt"},{41,"Angioplasty vein graft"},{19,"Diagnostic Angiogram +/- PCI with graft"},{13,"Arterial Stent Femoral Rt"},{12,"Orthopaedic pinning lower limb Rt"},{11,"Diagnostic Coronary Angiogram + Grafts"},{11,"Cateterismo y Angiocardiografía Izquierdos"},{10,"Ureteric stent retrograde Both"},{9,"PICC Line insertion"},{9,"Orthopaedic pinning hip Lt"},{9,"Arterial stent iliac Lt"},{8,"Retrograde ureteropyelogram Both"},{8,"Orthopaedic pinning upper limb Rt"},{8,"Orthopaedic pinning hip Rt"},{8,"DDDR Pacemaker Implant"},{8,"Arterial stent renal Lt"},{7,"Gastrostomy insertion"},{7,"Angio aortofemoral lower limb Both"},{6,"VVIR Pacemaker Implant"},{6,"Retrograde ureteropyelogram Rt"},{6,"Nephrostomy Rt"},{5,"Tubogram"},{5,"Planned Coronary CTO PCI"},{5,"Mobile image intensifier lumbar spine"},{1,"Ureteric stent retrograde Rt"},{1,"Ureteric stent retrograde Lt"},{1,"Upgrade to CRT-D"},{1,"Tunnelled central venous line insertion"},{1,"Pacemaker Box Change"},{1,"Orthopaedic pinning lower limb Lt"},{1,"NULL"},{1,"Nephrostomy Lt"},{1,"Nephrostogram Rt"},{1,"Nephrostogram Lt"},{1,"Mobile image intensifier thoracic spine"},{1,"Mobile image intensifier cervical spine"},{1,"Fluoroscopic guided injection"},{1,"Fluoroscopic guided injection hip Rt"},{1,"Defibrillator Box Change"},{1,"CRT Pacemaker Implant"},{1,"Coronary^Diagnostic Coronary Catheterization"}}}
    };

    public readonly BucketList<ModalityStats> ModalityFrequency = InitializeModalityFrequency(new Random());

    public readonly ReadOnlyDictionary<string,BucketList<DescBodyPart>> DescBodyPartsByModality = DescBodyPart.d;

    /// <summary>
    /// Distribution of time of day (in hours only) that tests were taken
    /// select DATEPART(HOUR,StudyTime),work.dbo.get_aggregate_value(count(*)) from CT_GoDARTS_StudyTable group by DATEPART(HOUR,StudyTime)
    /// </summary>
    private readonly BucketList<int> _hourOfDay = new()
    {
        { 57, 9 },
        { 55, 13 },
        { 54, 14 },
        { 51, 12 },
        { 44, 16 },
        { 42, 17 },
        { 42, 15 },
        { 41, 11 },
        { 36, 10 },
        { 33, 18 },
        { 15, 8 },
        { 8, 22 },
        { 7, 20 },
        { 5, 21 },
        { 1, 6 },
        { 1, 5 },
        { 1, 4 },
        { 1, 19 },
        { 1, 1 }
    };

    /// <summary>
    /// CT Image Type
    /// </summary>
    private readonly BucketList<string> _imageType = new()
    {
        { 96, @"ORIGINAL\PRIMARY\AXIAL" },
        { 3, @"DERIVED\SECONDARY" },
        { 1, @"ORIGINAL\PRIMARY\LOCALIZER" }
    };


    /// <summary>
    /// Generates a random time of day with a frequency that matches the times when the most images are captured (e.g. more images are
    /// taken at 1pm than at 8pm
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    public TimeSpan GetRandomTimeOfDay(Random r)
    {
        var ts = new TimeSpan(0,_hourOfDay.GetRandom(r),r.Next(60),r.Next(60),0);

        // JS 2024-05-21: This is a leftover from an old bug where probabilities and hours were confused,
        // leading to scans timed at 44 o'clock. I'm not sure if this is still necessary.
        //ts = ts.Subtract(new TimeSpan(ts.Days,0,0,0));

        if (ts.Days != 0)
            throw new Exception("What!");

        return ts;
    }

    public string GetRandomImageType(Random r) => _imageType.GetRandom(r);

    /// <summary>
    /// returns a random string e.g. T101H12451352 where the first letter indicates Tayside and 5th letter indicates Hospital
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    public static string GetRandomAccessionNumber(Random r) => $"T{r.Next(4)}{r.Next(2)}{r.Next(5)}H{r.Next(9999999)}";

    private static BucketList<ModalityStats> InitializeModalityFrequency(Random r) =>
        new()
        {
            { 182846, new ModalityStats("CT", 2, 0, 100, 0, r) },
            { 53161, new ModalityStats("MR", 1, 0, 100, 10, r) },
            { 3805, new ModalityStats("US", 1, 0, 100, 10, r) },
            { 2639, new ModalityStats("PT", 1, 0, 100, 10, r) },
            { 1949, new ModalityStats("CR", 1, 0, 100, 10, r) },
            { 1511, new ModalityStats("XA", 1, 0, 100, 10, r) },
            { 1206, new ModalityStats("RF", 1, 0, 100, 10, r) },
            { 485, new ModalityStats("DX", 1, 0, 100, 10, r) },
            { 391, new ModalityStats("OT", 1, 0, 100, 10, r) },
            { 149, new ModalityStats("MG", 1, 0, 100, 10, r) },
            { 92, new ModalityStats("IO", 1, 0, 100, 10, r) },
            { 58, new ModalityStats("NM", 1, 0, 100, 10, r) },
            { 23, new ModalityStats("PR", 1, 0, 100, 10, r) },
            { 8, new ModalityStats("PX", 1, 0, 100, 10, r) },
            { 1, new ModalityStats("SC", 1, 0, 100, 10, r) },
            { 1, new ModalityStats("KO", 1, 0, 100, 10, r) }
        };

    /// <summary>
    /// Returns the existing stats for tag popularity, modality frequencies etc.
    /// </summary>
    /// <returns></returns>
    public static DicomDataGeneratorStats GetInstance()
    {
        return Instance;
    }
}