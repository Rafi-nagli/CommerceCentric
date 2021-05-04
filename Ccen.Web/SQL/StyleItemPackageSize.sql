IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[StyleItems]') 
         AND name = 'PackageLength'
)
BEGIN
    ALTER TABLE StyleItems 
    add PackageLength decimal(18, 2)
END

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[StyleItems]') 
         AND name = 'PackageWidth'
)
BEGIN
    ALTER TABLE StyleItems 
    add PackageWidth decimal(18, 2)
END

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[StyleItems]') 
         AND name = 'PackageHeight'
)
BEGIN
    ALTER TABLE StyleItems 
    add PackageHeight decimal(18, 2)
END


/* [ViewOrderItems] */

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewOrderItems'
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewOrderItems'
GO

/****** Object:  View [dbo].[ViewOrderItems]    Script Date: 18.10.2020 16:56:41 ******/
DROP VIEW [dbo].[ViewOrderItems]
GO

/****** Object:  View [dbo].[ViewOrderItems]    Script Date: 18.10.2020 16:56:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[ViewOrderItems]
AS
SELECT        oi.Id, oi.ListingId, oi.ItemPrice, oi.ShippingPrice, oi.ItemPriceCurrency, oi.ItemOrderIdentifier, oi.SourceItemOrderIdentifier, oi.QuantityOrdered, oi.QuantityShipped, oi.ReplaceType, s.StyleID AS StyleString, 
                         s.Id AS StyleId, s.Image AS StyleImage, si.Size AS StyleSize, si.Color AS StyleColor, si.Weight, si.Id AS StyleItemId, si.RestockDate, sCache.ItemStyle, sCache.ShippingSizeValue AS ShippingSize, 
                         si.PackageLength, si.PackageHeight, si.PackageWidth, sCache.InternationalPackageValue AS InternationalPackage, oi.CreateDate, oi.OrderId, oi.SourceListingId, oi.SourceStyleString, 
                         oi.SourceStyleItemId, oi.SourceStyleSize, oi.SourceStyleColor, oi.RecordNumber, oi.ItemStatus, oi.ShippingPriceCurrency, oi.ShippingPriceInUSD, oi.ShippingDiscount, oi.ShippingDiscountCurrency, 
                         oi.ShippingDiscountInUSD, oi.ItemPriceInUSD, oi.PromotionDiscountInUSD, oi.PromotionDiscountCurrency, oi.PromotionDiscount, sCache.ExcessiveShipmentValue AS ExcessiveShipment, oi.RefundItemPrice, 
                         oi.RefundItemPriceInUSD, oi.RefundShippingPrice, oi.RefundShippingPriceInUSD, s.MSRP, s.Cost, s.OriginalStyleID AS OriginalStyleString, oi.ShippingTax, oi.ItemGrandPrice, oi.ItemTax, oi.OverrideOrderDate, 
                         oi.ItemPaid, oi.ShippingPaid
FROM            dbo.OrderItems AS oi LEFT OUTER JOIN
                         dbo.Styles AS s ON oi.StyleId = s.Id LEFT OUTER JOIN
                         dbo.StyleCaches AS sCache ON s.Id = sCache.Id LEFT OUTER JOIN
                         dbo.StyleItems AS si ON si.Id = oi.StyleItemId
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "oi"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 279
            End
            DisplayFlags = 280
            TopColumn = 34
         End
         Begin Table = "s"
            Begin Extent = 
               Top = 6
               Left = 317
               Bottom = 136
               Right = 510
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "sCache"
            Begin Extent = 
               Top = 6
               Left = 548
               Bottom = 136
               Right = 777
            End
            DisplayFlags = 280
            TopColumn = 5
         End
         Begin Table = "si"
            Begin Extent = 
               Top = 138
               Left = 38
               Bottom = 268
               Right = 213
            End
            DisplayFlags = 280
            TopColumn = 12
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewOrderItems'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewOrderItems'
GO


/* ViewListings */

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewListings'
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewListings'
GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewListings'
GO

/****** Object:  View [dbo].[ViewListings]    Script Date: 18.10.2020 17:01:33 ******/
DROP VIEW [dbo].[ViewListings]
GO

/****** Object:  View [dbo].[ViewListings]    Script Date: 18.10.2020 17:01:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[ViewListings]
AS
SELECT        l.Id, l.Market, l.MarketplaceId, l.ListingId, l.SKU, l.IsFBA, l.IsRemoved, l.ItemId, l.CurrentPrice, l.CurrentPriceInUSD, si.Weight, si.Id AS StyleItemId, si.Size AS StyleSize, si.Color AS StyleColor, 
                         l.RealQuantity AS Quantity, s.StyleID AS StyleString, s.Id AS StyleId, i.ASIN, i.Size, i.Color, i.ParentASIN, i.Title, i.PrimaryImage AS ItemPicture, sCache.ItemStyle, sCache.ShippingSizeValue AS ShippingSize, 
                         si.PackageLength, si.PackageHeight, si.PackageWidth, sCache.InternationalPackageValue AS InternationalPackage, l.RestockDate, p.Rank, CASE WHEN p.ImageSource IS NULL OR
                         p.ImageSource = '' THEN s.Image ELSE p.ImageSource END AS Picture, l.OpenDate, l.CreateDate, p.OnHold AS OnHoldParent, i.ColorVariation, i.SourceMarketUrl, i.SourceMarketId, s.Image AS StyleImage, 
                         s.Type AS StyleType, p.RankUpdateDate, s.DropShipperId, s.Name AS StyleName, sic.PreOrderExpReceiptDate, si.FulfillDate, p.SourceMarketId AS ParentSourceMarketId, i.UseStyleImage, l.IsPrime, 
                         i.Barcode
FROM            dbo.Listings AS l INNER JOIN
                         dbo.Items AS i ON l.ItemId = i.Id LEFT OUTER JOIN
                         dbo.ParentItems AS p ON i.ParentASIN = p.ASIN AND i.Market = p.Market AND ISNULL(i.MarketplaceId, N'') = ISNULL(p.MarketplaceId, N'') LEFT OUTER JOIN
                         dbo.Styles AS s ON i.StyleId = s.Id LEFT OUTER JOIN
                         dbo.StyleCaches AS sCache ON s.Id = sCache.Id LEFT OUTER JOIN
                         dbo.StyleItems AS si ON si.Id = i.StyleItemId LEFT OUTER JOIN
                         dbo.StyleItemCaches AS sic ON sic.Id = si.Id
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[43] 4[10] 2[29] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "l"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 247
            End
            DisplayFlags = 280
            TopColumn = 53
         End
         Begin Table = "i"
            Begin Extent = 
               Top = 6
               Left = 285
               Bottom = 114
               Right = 482
            End
            DisplayFlags = 280
            TopColumn = 25
         End
         Begin Table = "p"
            Begin Extent = 
               Top = 6
               Left = 520
               Bottom = 114
               Right = 717
            End
            DisplayFlags = 280
            TopColumn = 27
         End
         Begin Table = "s"
            Begin Extent = 
               Top = 114
               Left = 38
               Bottom = 222
               Right = 200
            End
            DisplayFlags = 280
            TopColumn = 10
         End
         Begin Table = "sCache"
            Begin Extent = 
               Top = 114
               Left = 238
               Bottom = 222
               Right = 446
            End
            DisplayFlags = 280
            TopColumn = 3
         End
         Begin Table = "si"
            Begin Extent = 
               Top = 114
               Left = 484
               Bottom = 222
               Right = 645
            End
            DisplayFlags = 280
            TopColumn = 13
         End
         Begin Table = "sic"
            Begin Extent = 
               Top = 6
               Left = 755
               Bottom = 136
               Right = 1052
            End
            DisplayFlags = 280
            TopColumn = 0
   ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewListings'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'      End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewListings'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewListings'
GO








