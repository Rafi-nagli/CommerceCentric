SELECT     StyleItemId AS Id, 
	MAX(StyleId) AS StyleId, 
	MAX(Size) AS Size, 
	MAX(InventoryQuantity) AS InventoryQuantity, 
	MAX(InventorySetDate) AS InventorySetDate, 
    SUM(SoldQuantity) AS SoldQuantity, 
	SUM(TotalSoldQuantity) AS TotalSoldQuantity
FROM        (
				SELECT     invQ.Id AS StyleItemId, invQ.Size, soldQ.StyleId, invQ.QuantitySetDate AS InventorySetDate, invQ.Quantity AS InventoryQuantity, 
                                              soldQ.Quantity AS TotalSoldQuantity, (CASE WHEN (invQ.QuantitySetDate IS NULL OR
                                              invQ.QuantitySetDate <= CreateDate) THEN soldQ.Quantity ELSE 0 END) AS SoldQuantity
                FROM          
					dbo.ViewInventoryQuantities AS invQ 
				INNER JOIN
					(SELECT     Id, StyleId, StyleItemId, CreateDate, Quantity
					FROM          dbo.QuantityChanges AS ch
					WHERE InActive = 0) AS soldQ 
				ON invQ.Id = soldQ.StyleItemId
			) AS allChanges

GROUP BY StyleItemId