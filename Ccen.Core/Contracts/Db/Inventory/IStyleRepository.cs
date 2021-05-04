using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleRepository : IRepository<Style>
    {
        IQueryable<Style> GetAllActive();
        
        IQueryable<StyleEntireDto> GetAllActiveAsDto();
        IQueryable<StyleEntireDto> GetAllActiveAsDtoEx();
        IQueryable<ViewStyle> GetAllActual();
        IQueryable<StyleEntireDto> GetAllAsDto();
        IQueryable<StyleEntireDto> GetAllAsDtoEx();
        IQueryable<StyleEntireDto> GetAllAsDtoLite();
        IQueryable<ViewStyle> GetAllActualFiltered(string barcode);
        bool IsNewStyle(long? id, string styleId);
        void SetReSaveDate(long styleId, DateTime? when, long? by);
        List<StyleDTO> GetAllWithoutImage();
        Style Store(string styleString,
            int itemTypeId,
            string name, 
            string imageSource,
            DateTime when);


        StyleEntireDto GetActiveByStyleIdAsDto(string styleString);
        StyleEntireDto GetActiveByStyleIdAsDto(long styleId);
        StyleEntireDto GetByStyleIdAsDto(long styleId);
    }
}
