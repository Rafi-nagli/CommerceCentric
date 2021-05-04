SELECT     b.Id, b.Name, b.Archive, l.FileName, grOrders.HasPath AS AllPrinted, b.CreateDate, grOrders.Count, grOrders.ShippedCount
FROM         dbo.OrderBatches AS b INNER JOIN
                          (SELECT     BatchId, COUNT(Id) AS Count, MIN(HasPath) AS HasPath, SUM(HasPath) AS ShippedCount
                            FROM          (SELECT     BatchId, Id, CASE WHEN o.OrderStatus = 'Shipped' THEN 1 ELSE 0 END AS HasPath
                                                    FROM          dbo.Orders AS o
                                                    WHERE      (OrderStatus <> 'Canceled')) AS l_1
                            GROUP BY BatchId) AS grOrders ON grOrders.BatchId = b.Id LEFT OUTER JOIN
                      dbo.LabelPrintPacks AS l ON b.LablePrintPackId = l.Id