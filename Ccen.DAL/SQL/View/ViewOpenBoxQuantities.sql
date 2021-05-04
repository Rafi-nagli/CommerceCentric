SELECT     ob.StyleId, obi.BoxId, obi.q AS Quantity, ob.Price * obi.q AS Price
FROM         dbo.OpenBoxes AS ob INNER JOIN
                          (SELECT     BoxId, SUM(Quantity) AS q
                            FROM          dbo.OpenBoxItems
                            GROUP BY BoxId) AS obi ON obi.BoxId = ob.Id
WHERE     (ob.Deleted = 0) AND (ob.Archived = 0)