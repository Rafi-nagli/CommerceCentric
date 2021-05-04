SELECT     Id, 
			MAX(StyleId) as StyleId, 
			MAX(Size) as Size,
			MAX(InventoryQuantity) as InventoryQuantity,
			MAX(InventorySetDate) AS InventorySetDate,
            SUM(SoldQuantity) AS SoldQuantity, 
			SUM(TotalSoldQuantity) AS TotalSoldQuantity,
			MIN(OrderDate) AS MaxOrderDate, 
			MAX(OrderDate) AS MinOrderDate, 
			COUNT(OrderDate) AS OrderCount
FROM   
(
	SELECT
		invQ.Id,
		soldQ.StyleId,
		soldQ.Size,
		soldQ.OrderDate,
		invQ.QuantitySetDate as InventorySetDate,
		invQ.Quantity as InventoryQuantity,
		soldQ.Quantity as TotalSoldQuantity,
		(case when (invQ.QuantitySetDate IS NULL OR invQ.QuantitySetDate <= OrderDate)
			then soldQ.Quantity
			else 0 end) as SoldQuantity
	FROM
		dbo.ViewInventoryQuantities AS invQ 

		INNER JOIN

		 (SELECT     siEx.Id, siEx.StyleId, siEx.Size, m.Quantity, o.OrderDate
                FROM          Inventory_dev.dbo.Orders AS o INNER JOIN
                                        Inventory_dev.dbo.ItemOrderMappings AS m ON m.OrderId = o.Id INNER JOIN
                                        Inventory_dev.dbo.Items AS i ON m.ItemId = i.Id INNER JOIN
                                            (SELECT     si.Id, si.StyleId, si.Size, sib.Barcode
                                                FROM          dbo.StyleItems AS si INNER JOIN
                                                                    dbo.StyleItemBarcodes AS sib ON si.Id = sib.StyleItemId
                                                WHERE      (sib.Barcode IS NOT NULL) AND (sib.Barcode <> '')) AS siEx ON siEx.Barcode = i.Barcode
                WHERE      (m.Quantity < 1000)) AS soldQ

		ON invQ.Id = soldQ.Id
) as allOrders

GROUP BY Id