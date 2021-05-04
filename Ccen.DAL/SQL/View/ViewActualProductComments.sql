SELECT     c.Id, c.ProductId, c.Message, c.Deleted, c.UpdateDate, c.CreateDate
FROM         dbo.ProductComments AS c INNER JOIN
                          (SELECT     ProductId, MAX(UpdateDate) AS MaxUpdateDate
                            FROM          dbo.ProductComments
                            WHERE      (Deleted = 0)
                            GROUP BY ProductId) AS lastC ON c.ProductId = lastC.ProductId AND c.UpdateDate = lastC.MaxUpdateDate