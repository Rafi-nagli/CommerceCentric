using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;

namespace Amazon.DAL.Repositories
{
    public class LabelPrintPackRepository : Repository<LabelPrintPack>, ILabelPrintPackRepository
    {
        public LabelPrintPackRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        //public IList<LabelPrintPackDTO> GetAllDTOs()
        //{
        //    var query = from p in GetAll()
        //        join info in unitOfWork.OrderShippingInfos.GetAll() on p.Id equals info.LabelPrintPackId

        //        select new LabelPrintPackDTO
        //        {
        //            Id = p.Id,
        //            FileName = p.FileName,
        //            CreateDate = p.CreateDate
        //        };


        //}
    }
}
