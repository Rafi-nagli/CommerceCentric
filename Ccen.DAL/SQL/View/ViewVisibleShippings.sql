SELECT     Id, OrderId, ShippingNumber, ShippingMethodId, LabelPath, LabelPrintPackId, StampsShippingCost, InsuranceCost, TrackingNumber, IsFeedSubmitted, IsFulfilled, 
                      FeedId, MessageIdentifier, IsActive, IsVisible, ShippingDate, LabelPurchaseResult, LabelPurchaseMessage, LabelPurchaseDate, IntegratorTxIdentifier, StampsTxId, 
                      CreateDate, UpdateDate, SignConfirmationCost
FROM         dbo.OrderShippingInfoes
WHERE     (IsVisible = 1)