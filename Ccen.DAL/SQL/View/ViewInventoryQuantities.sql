SELECT     si.Id, 
			si.StyleId, 
			s.StyleID AS StyleString, 
			si.Size, 
			(case when si.Quantity IS NOT NULL then si.Quantity else boxesBySize.Quantity end) as Quantity,
			si.Quantity as DirectQuantity,
			boxesBySize.Quantity as BoxQuantity,
			si.Barcode, 
			(case when si.Quantity IS NOT NULL then si.QuantitySetDate else boxesBySize.MinCreateDate end) as QuantitySetDate,
			si.QuantitySetDate as DirectQuantitySetDate,
			boxesBySize.MinCreateDate as BoxQuantitySetDate

FROM        dbo.Styles AS s 
		INNER JOIN
            dbo.StyleItems AS si ON s.Id = si.StyleId 
		LEFT OUTER JOIN
			(SELECT     StyleId, Size, SUM(Quantity) AS Quantity, MIN(CreateDate) AS MinCreateDate
			FROM          (SELECT     oBox.StyleId, obItem.Size, oBox.BoxQuantity * obItem.Quantity AS Quantity, oBox.CreateDate
									FROM        dbo.OpenBoxes AS oBox 
									INNER JOIN
												dbo.OpenBoxItems AS obItem ON oBox.Id = obItem.BoxId
									WHERE      (oBox.Deleted = 0) AND (oBox.Archived = 0)
								UNION
									SELECT     sBox.StyleId, sbItem.Size, sBox.BoxQuantity * sbItem.BreakDown AS Quantity, sBox.CreateDate
									FROM        dbo.SealedBoxes AS sBox 
									INNER JOIN
												dbo.SealedBoxItems AS sbItem ON sBox.Id = sbItem.BoxId
									WHERE     (sBox.Deleted = 0) AND (sBox.Archived = 0)
							) AS AllBoxItems
			GROUP BY Size, StyleId) AS boxesBySize 
			ON si.StyleId = boxesBySize.StyleId AND si.Size = boxesBySize.Size
WHERE     (s.Deleted = 0)