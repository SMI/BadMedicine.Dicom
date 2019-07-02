
--Any value less than 5 is exported as the string value "<5" and then we sub it for 1 locally
select StudyDescription,work.dbo.get_aggregate_value(count(*)) from CT_Godarts_StudyTable group by StudyDescription

select StudyDescription,work.dbo.get_aggregate_value(count(*)) from MR_Godarts_StudyTable group by StudyDescription
  
select Modality,StudyDescription,work.dbo.get_aggregate_value(count(*)) from OTHER_Godarts_ImageTable group by Modality,StudyDescription