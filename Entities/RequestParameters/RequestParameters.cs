namespace Entities.Paging
{
    public abstract class RequestParameters
    {
        private const int MaxPagSize = 15;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPagSize ? MaxPagSize : value;
        }
    }
}
