namespace Entities.RequestParameters
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

        public string SearchTerm { get; set; }
        public string OrderBy { get; set; } = "Name";
    }
}
