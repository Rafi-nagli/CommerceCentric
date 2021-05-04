IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Items]') 
         AND name = 'Rank'
)
BEGIN
    ALTER TABLE Items 
    ADD Rank decimal(18, 2) NULL
END
