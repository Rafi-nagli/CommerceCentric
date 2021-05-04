IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[ShippingMethods]') 
         AND name = 'RequiredPackageSize'
)
BEGIN
    ALTER TABLE ShippingMethods 
    add RequiredPackageSize bit not null default(0)
END

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[ShippingMethods]') 
         AND name = 'ShortName'
)
BEGIN
    ALTER TABLE ShippingMethods 
    add ShortName nvarchar(100)
END

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[ShipmentProviders]') 
         AND name = 'ShortName'
)
BEGIN
    ALTER TABLE ShipmentProviders 
    add ShortName nvarchar(100)
END
