using System.Collections.Generic;

namespace BethanysPieShop.Models
{
    interface ICategoryRepository
    {
        IEnumerable<Category> Categories { get; }
    }
}
