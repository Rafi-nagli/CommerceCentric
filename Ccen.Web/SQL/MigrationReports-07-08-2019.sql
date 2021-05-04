USE [pa_ccen]
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportFields]') 
         AND name = 'CustomReportPredefinedFieldId'
)
BEGIN
    ALTER TABLE CustomReportFields 
    add CustomReportPredefinedFieldId bigint not null
END

IF EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportFields]') 
         AND name = 'FieldEntity'
)
BEGIN
    ALTER TABLE CustomReportFields 
    drop column FieldEntity 
END



IF EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportFields]') 
         AND name = 'FieldDataType'
)
BEGIN
    ALTER TABLE CustomReportFields 
    drop column FieldDataType 
END




IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportPredefinedFields]') 
         AND name = 'Name'
)
BEGIN
    ALTER TABLE CustomReportPredefinedFields 
    add Name varchar(100) null
END

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportPredefinedFields]') 
         AND name = 'EntityName'
)
BEGIN
    ALTER TABLE CustomReportPredefinedFields 
    add EntityName varchar(100) null
END

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportPredefinedFields]') 
         AND name = 'ColumnName'
)
BEGIN
    ALTER TABLE CustomReportPredefinedFields 
    add ColumnName varchar(100) null
END

IF EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportPredefinedFields]') 
         AND name = 'DateType'
)
BEGIN
EXEC sp_rename '[dbo].[CustomReportPredefinedFields].DateType', 'DataType'; 
END

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportPredefinedFields]') 
         AND name = 'Width'
)
BEGIN
    ALTER TABLE CustomReportPredefinedFields 
    add Width int null
END

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportPredefinedFields]') 
         AND name = 'Title'
)
BEGIN
    ALTER TABLE CustomReportPredefinedFields 
    add Title varchar(100) null
END

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportFilters]') 
         AND name = 'CustomReportId'
)
BEGIN
ALTER TABLE CustomReportFilters     
    add CustomReportId bigint not null
END

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportFilters]') 
         AND name = 'CustomReportPredefinedFieldId'
)
BEGIN
ALTER TABLE CustomReportFilters     
    add CustomReportPredefinedFieldId bigint not null
END

IF EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CustomReportFilters]') 
         AND name = 'ReportFieldId'
)
BEGIN
ALTER TABLE CustomReportFilters 
    drop column ReportFieldId 
END


GO