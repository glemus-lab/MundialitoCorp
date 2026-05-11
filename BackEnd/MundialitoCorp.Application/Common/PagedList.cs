namespace MundialitoCorp.Application.Common
{
    public class PagedList<T>
    {
        public List<T> Data { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalRecords { get; }
        public int TotalPages { get; }

        public PagedList(List<T> data, int count, int pageNumber, int pageSize)
        {
            Data = data;
            TotalRecords = count;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }
    }
}
