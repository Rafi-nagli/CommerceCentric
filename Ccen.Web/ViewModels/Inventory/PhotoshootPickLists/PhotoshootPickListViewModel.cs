using Amazon.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DTO.Inventory;
using Amazon.Web.ViewModels.Html;
using Amazon.Common.Helpers;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Web.ViewModels.ExcelToAmazon;
using Amazon.Core.Models.Items;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts;
using Amazon.DTO.Users;
using Amazon.Common.ExcelExport;
using System.IO;
using Amazon.Api.Exports;
using Amazon.Core.Views;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports.Types;
using Amazon.Web.Models.Exports;
using Amazon.Web.Models.SearchFilters;

namespace Amazon.Web.ViewModels.Inventory.FBAPickLists
{
    public class PhotoshootPickListViewModel
    {
        public long Id { get; set; }

        public bool IsLocked { get; set; }
        public DateTime? PhotoshootDate { get; set; }

        public bool Archived { get; set; }
        public DateTime CreateDate { get; set; }


        public string Name
        {
            get { return "Photoshoot-" + CreateDate.ToString("MM-dd-yyyy"); }
        }

        public IList<PhotoshootPickListEntryViewModel> Entries { get; set; }

        public PhotoshootPickListViewModel()
        {
            
        }

        public PhotoshootPickListViewModel(PhotoshootPickListDTO pickList)
        {
            Id = pickList.Id;
            Archived = pickList.Archived;
            PhotoshootDate = pickList.PhotoshootDate;
            IsLocked = pickList.IsLocked;
            CreateDate = pickList.CreateDate;
        }

        public static PhotoshootPickListViewModel Get(IUnitOfWork db, long? id)
        {
            var model = new PhotoshootPickListViewModel();
            model.Id = id ?? 0;
            if (id.HasValue)
            {
                var pickList = db.PhotoshootPickLists.Get(id.Value);
                var pickListEntries =
                    db.PhotoshootPickListEntries.GetAllAsDto().Where(fe => fe.PhotoshootPickListId == id.Value).ToList();
                model.CreateDate = pickList.CreateDate;
                model.PhotoshootDate = pickList.PhotoshootDate;
                model.IsLocked = pickList.IsLocked;
                model.Entries = pickListEntries.Select(e => new PhotoshootPickListEntryViewModel(e)).ToList();
            }
            else
            {
                model.Entries = new List<PhotoshootPickListEntryViewModel>();
            }
            return model;
        }

        public static IList<PhotoshootPickListViewModel> GetAll(IUnitOfWork db, PhotoshootPickListFilterViewModel filters)
        {
            var query = db.PhotoshootPickLists.GetAllAsDto();
            if (!filters.ShowArchived)  
                query = query.Where(f => !f.Archived);

            return query
                .OrderByDescending(f => f.CreateDate)
                .ToList()
                .Select(f => new PhotoshootPickListViewModel(f))
                .ToList();
        }

        public void Apply(IUnitOfWork db,
            DateTime when,
            long? by)
        {
            PhotoshootPickList pickList = null;            
            if (this.Id == 0) //New
            {
                pickList = new PhotoshootPickList()
                {
                    CreateDate = when,
                    CreatedBy = by
                };
                db.PhotoshootPickLists.Add(pickList);
            }
            else
            {
                pickList = db.PhotoshootPickLists.Get(this.Id);
            }
            pickList.PhotoshootDate = PhotoshootDate;
            db.Commit();
            
            var allPickListEntries = db.PhotoshootPickListEntries.GetAll().Where(p => p.PhotoshootPickListId == pickList.Id).ToList();
            var updatedEntries = new List<long>();
            foreach (var entry in Entries)
            {
                
                PhotoshootPickListEntry dbEntry = null;
                if (entry.Id == 0)
                {
                    dbEntry = new PhotoshootPickListEntry()
                    {
                        PhotoshootPickListId = pickList.Id,

                        CreateDate = when,
                        CreatedBy = by
                    };
                    db.PhotoshootPickListEntries.Add(dbEntry);
                }
                else
                {
                    dbEntry = allPickListEntries.FirstOrDefault(e => e.Id == entry.Id);
                }
                dbEntry.StyleId = entry.StyleId;
                dbEntry.StyleString = entry.StyleString;
                dbEntry.StyleItemId = entry.StyleItemId;
                dbEntry.TakenQuantity = 1;
                if (entry.Status == (int)PhotoshootEntryStatuses.Returned)
                    dbEntry.ReturnedQuantity = 1;
                else
                    dbEntry.ReturnedQuantity = 0;

                dbEntry.Status = entry.Status;
                if (dbEntry.Status != entry.Status)
                {
                    dbEntry.StatusDate = when;
                    dbEntry.StatusBy = by;
                }

                updatedEntries.Add(dbEntry.Id);
            }

            db.Commit();

            var toDeleteList = allPickListEntries.Where(e => !updatedEntries.Contains(e.Id)).ToList();
            foreach (var toDelete in toDeleteList)
            {
                db.PhotoshootPickListEntries.Remove(toDelete);
            }
            db.Commit();
        }

        public void GetForPickList(IUnitOfWork db)
        {
            
        }

        public static bool SetArchiveStatus(IUnitOfWork db, long id, bool newStatus)
        {
            var fbaPicklist = db.FBAPickLists.Get(id);
            fbaPicklist.Archived = newStatus;
            db.Commit();

            return fbaPicklist.Archived;
        }
    }
}