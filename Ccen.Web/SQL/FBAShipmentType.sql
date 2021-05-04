IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[FBAPickLists]') 
         AND name = 'FBAPickListType'
)
BEGIN
    ALTER TABLE FBAPickLists 
    add FBAPickListType varchar(10) not null default 'FBA'
END