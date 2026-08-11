using Microsoft.EntityFrameworkCore;
using SmartPharmacy.DAL.DTO.Response;

namespace SmartPharmacy.DAL.Extentions
{
    public static class PagenationExtention
    {
        public static async Task<PagenationResponse<T>> ApplyPagenation<T>(this IQueryable<T> query, int page, int limit)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;

            var totalCount = await query.CountAsync();
            var data = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return new PagenationResponse<T>
            {
                Data = data,
                TotalCount = totalCount,
                Page = page,
                Limit = limit,
            };
        }
    }
}
