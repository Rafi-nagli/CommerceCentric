using Amazon.DTO;
using Amazon.DTO.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Listings.Rules
{
    public interface IItemAutoFixRule
    {
        void Apply(ItemDTO item, IList<ItemAdditionDTO> additions);
    }
}
