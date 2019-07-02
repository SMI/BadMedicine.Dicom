select count(*) from [CT_Godarts_ImageTable]
  select count(*) from [MR_Godarts_ImageTable]
  select Modality, work.dbo.get_aggregate_value(count(*)) from [OTHER_Godarts_ImageTable] Group By Modality

