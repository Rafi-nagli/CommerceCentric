CREATE OR ALTER VIEW dbo.[ViewOrdersSoldByDateAndMarkets]
AS
SELECT        TOP (100) PERCENT Date, Market, MarketplaceId, count(OrderId) AS Quantity
FROM            (SELECT     o.Id as OrderId,   CONVERT(date, o.OrderDate) AS Date, o.Market, o.MarketplaceId
                            FROM            dbo.Orders AS o 
                            WHERE    (o.FulfillmentChannel IS NULL OR
                                                    o.FulfillmentChannel <> 'AFN') AND (o.OrderStatus = 'Shipped' OR
                                                    o.OrderStatus = 'Pending' OR
                                                    o.OrderStatus = 'PartiallyShipped' OR
                                                    o.OrderStatus = 'Unshipped')) AS allOrders
GROUP BY Date, Market, MarketplaceId

GO