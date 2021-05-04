SELECT     s.Id, s.StyleID, fv.FeatureId, f.Name, fv.Id AS FeatureValueId, fv.Value, fv.ExtendedValue, sfv.CreateDate, sfv.UpdateDate
FROM         dbo.Styles AS s INNER JOIN
                      dbo.StyleFeatureValues AS sfv ON s.Id = sfv.StyleId INNER JOIN
                      dbo.FeatureValues AS fv ON sfv.FeatureValueId = fv.Id INNER JOIN
                      dbo.Features AS f ON f.Id = fv.FeatureId