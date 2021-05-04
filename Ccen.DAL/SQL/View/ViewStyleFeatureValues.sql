SELECT     s.Id, s.StyleID, fv.FeatureId, fv.Value, fv.ExtendedValue
FROM         dbo.Styles AS s INNER JOIN
                      dbo.StyleFeatureValues AS sfv ON s.Id = sfv.StyleId INNER JOIN
                      dbo.FeatureValues AS fv ON sfv.FeatureValueId = fv.Id