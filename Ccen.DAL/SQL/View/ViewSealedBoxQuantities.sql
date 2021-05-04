SELECT     sb.StyleId, sbi.BoxId, sb.BoxQuantity * sbi.bd AS Quantity, sb.PajamaPrice * sb.BoxQuantity * sbi.bd AS Price
FROM         dbo.SealedBoxes AS sb INNER JOIN
                          (SELECT     BoxId, SUM(BreakDown) AS bd
                            FROM          dbo.SealedBoxItems
                            GROUP BY BoxId) AS sbi ON sbi.BoxId = sb.Id
WHERE     (sb.Deleted = 0) AND (sb.Archived = 0)