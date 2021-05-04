using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.Core;
using Amazon.DTO;
using Amazon.Core.Contracts;

namespace Amazon.DAL.Repositories.Inventory
{
    public class StyleLocationRepository : Repository<StyleLocation>, IStyleLocationRepository
    {
        public StyleLocationRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }


        public void UpdateLocationsForStyle(IStyleHistoryService styleHistory,
            long styleId, 
            IList<StyleLocationDTO> locations,
            DateTime when,
            long? by)
        {
            var dbExistLocations = GetFiltered(l => l.StyleId == styleId).ToList();
            var newLocations = locations.Where(l => l.Id == 0).ToList();

            foreach (var dbLoc in dbExistLocations)
            {
                var existLoc = locations.FirstOrDefault(l => l.Id == dbLoc.Id);
                if (existLoc != null)
                {
                    bool hasChanges = dbLoc.Isle != existLoc.Isle
                                      || dbLoc.Section != existLoc.Section
                                      || dbLoc.Shelf != existLoc.Shelf
                                      || dbLoc.IsDefault != existLoc.IsDefault;

                    if (hasChanges)
                    {
                        styleHistory.AddRecord(styleId, StyleHistoryHelper.LocationKey,
                            dbLoc.Isle + "/" + dbLoc.Section + "/" + dbLoc.Shelf,
                            existLoc.Isle + "/" + existLoc.Section + "/" + existLoc.Shelf,
                            by);

                        dbLoc.Isle = existLoc.Isle;
                        dbLoc.Section = existLoc.Section;
                        dbLoc.Shelf = existLoc.Shelf;
                        dbLoc.IsDefault = existLoc.IsDefault;

                        dbLoc.SortIsle = existLoc.SortIsle;
                        dbLoc.SortSection = existLoc.SortSection;
                        dbLoc.SortShelf = existLoc.SortShelf;

                        dbLoc.UpdateDate = when;
                        dbLoc.UpdatedBy = by;
                    }
                }
                else
                {
                    styleHistory.AddRecord(styleId, StyleHistoryHelper.LocationKey,
                            dbLoc.Isle + "/" + dbLoc.Section + "/" + dbLoc.Shelf,
                            null,
                            by);
                    Remove(dbLoc);
                }
            }

            foreach (var newLoc in newLocations)
            {
                styleHistory.AddRecord(styleId, StyleHistoryHelper.LocationKey,
                            null,
                            newLoc.Isle + "/" + newLoc.Section + "/" + newLoc.Shelf,
                            by);
                
                Add(new StyleLocation()
                {
                    StyleId = styleId,
                    Isle = newLoc.Isle,
                    Section = newLoc.Section,
                    Shelf = newLoc.Shelf,
                    IsDefault = newLoc.IsDefault,

                    SortIsle = newLoc.SortIsle,
                    SortSection = newLoc.SortSection,
                    SortShelf = newLoc.SortShelf,

                    CreateDate = when,
                    CreatedBy = by
                });
            }

            unitOfWork.Commit();
        }

        public IList<StyleLocationDTO> GetByStyleId(long styleId)
        {
            var query = from sl in unitOfWork.GetSet<StyleLocation>()
                        where sl.StyleId == styleId
                        select sl;

            return AsDto(query).ToList();
        }

        public IList<StyleLocationDTO> GetByStyleIdsAsDto(IList<long> styleIds)
        {
            var query = from sl in unitOfWork.GetSet<StyleLocation>()
                        where styleIds.Contains(sl.StyleId)
                        select sl;

            return AsDto(query).ToList();
        }

        public IQueryable<StyleLocationDTO> GetAllAsDTO()
        {
            return AsDto(unitOfWork.GetSet<StyleLocation>());
        }

        private IQueryable<StyleLocationDTO> AsDto(IQueryable<StyleLocation> query)
        {
            return query.Select(sl => new StyleLocationDTO
            {
                Id = sl.Id,
                IsDefault = sl.IsDefault,
                Isle = sl.Isle,
                Section = sl.Section,
                Shelf = sl.Shelf,
                SortIsle = sl.SortIsle,
                SortSection = sl.SortSection,
                SortShelf = sl.SortShelf,
                StyleId = sl.StyleId
            });
        }
    }
}
