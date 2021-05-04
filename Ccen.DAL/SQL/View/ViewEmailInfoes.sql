SELECT     o.Id, o.AmazonIdentifier AS OrderId, o.PersonName AS BuyerName, o.AmazonEmail AS Email, o.BuyerEmail, p.ProductName, o.ActualDeliveryDate, 
                      sh.ShippingDate
FROM         dbo.Orders AS o INNER JOIN
                      dbo.OrderShippingInfoes AS sh ON o.Id = sh.OrderId INNER JOIN
                      dbo.ItemOrderMappings AS m ON sh.Id = m.ShippingInfo_Id INNER JOIN
                      dbo.Listings AS l ON m.Listing_Id = l.Id INNER JOIN
                      dbo.Items AS i ON l.ItemId = i.Id INNER JOIN
                      dbo.ParentItems AS p ON i.ParentASIN = p.ASIN