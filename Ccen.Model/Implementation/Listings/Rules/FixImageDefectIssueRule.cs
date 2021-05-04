using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.Model.Implementation.Listings.Rules
{
    //public class FixImageDefectIssueRule
    //{
    //    private IDbFactory _dbFactory;
    //    private ILogService _log;
    //    private ITime _time;
    //    private ISystemActionService _actionService;

    //    public FixImageDefectIssueRule(IDbFactory dbFactory,
    //        ISystemActionService actionService,
    //        ILogService log,
    //        ITime time)
    //    {
    //        _dbFactory = dbFactory;
    //        _log = log;
    //        _time = time;
    //        _actionService = actionService;
    //    }

    //    public void Apply(IList<ListingDefectDTO> defects)
    //    {
    //        var imageDefects = defects
    //            .Where(a => a.FieldName == "Image"
    //                && a.AlertType == "Missing"
    //                && a.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
    //            .ToList();

    //        if (!imageDefects.Any())
    //            return;

    //        var asinList = defects.Select(d => d.ASIN).Distinct().ToList();

    //        using (var db = _dbFactory.GetRWDb())
    //        {
    //            var allItemIds = db.Items.GetAll().Where(i => asinList.Contains(i.ASIN)
    //                && i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
    //                .Select(i => i.Id)
    //                .ToList();

    //            foreach (var itemId in allItemIds)
    //            {
    //                _log.Info("Add Update Product Image action for ItemId=" + itemId);
                         
    //                var newAction = new SystemActionDTO()
    //                {
    //                    ParentId = null,
    //                    Status = (int)SystemActionStatus.None,
    //                    Type = (int)SystemActionType.UpdateOnMarketProductImage,
    //                    Tag = itemId.ToString(),
    //                    InputData = null,

    //                    CreateDate = _time.GetUtcTime(),
    //                    CreatedBy = null,
    //                };
    //                db.SystemActions.AddAction(newAction);
    //            }

    //            db.Commit();
    //        }
    //    }
    //}
}
