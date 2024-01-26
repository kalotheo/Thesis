using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Thesis.Model
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int PageLeft { get; private set; }
        public int PageRight { get; private set; }
        private static int StartRange { get; set; }
        private static int EndRange { get; set; }
        private static int TotalCount { get; set; }

        private static int SkipCount { get; set; }

        public int GetStartRange()
        {
            return StartRange;
        }

        public int GetEndRange()
        {
            return EndRange;
        }

        public int GetTotalCount()
        {
            return TotalCount;
        }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            PageLeft = 1;
            PageRight = TotalPages;

            if (TotalPages > 6)
            {
                if (pageIndex > 4)
                {

                    PageLeft = pageIndex - 3;

                    if (pageIndex + 3 <= TotalPages)
                    {
                        PageRight = pageIndex + 3;
                    }

                    else
                    {
                        PageLeft = TotalPages - 6;
                    }

                }

                else
                {
                    PageRight = 8 - PageLeft;
                }
            }

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, int pageIndex, int pageSize)
        {
            TotalCount = await source.CountAsync();
            SkipCount = (pageIndex - 1) * pageSize;
            var items = await source.Skip(SkipCount)
                .Take(pageSize).ToListAsync();
            StartRange = 1 + SkipCount;
            EndRange = items.Count() + SkipCount;
            return new PaginatedList<T>(items, TotalCount, pageIndex, pageSize);
        }

        public static PaginatedList<T> CreateICollection(
            ICollection<T> source, int pageIndex, int pageSize)
        {
            TotalCount = source.Count();
            SkipCount = (pageIndex - 1) * pageSize;
            var items = source.Skip(SkipCount)
                .Take(pageSize).ToList();
            StartRange = 1 + SkipCount;
            EndRange = items.Count() + SkipCount;
            return new PaginatedList<T>(items, TotalCount, pageIndex, pageSize);
        }
    }
}
