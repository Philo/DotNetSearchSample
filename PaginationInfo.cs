namespace SearchSample
{
    public class PaginationInfo
    {
        public const int DefaultPageSize = 10;
        private int pageSize = DefaultPageSize;

        public int Page { get; set; } = 1;
        public int Size
        {
            get => pageSize;
            set
            {
                if (value < 0 || value > 50)
                {
                    pageSize = DefaultPageSize;
                }
                else
                {
                    pageSize = value;
                }
            }
        }
    }
}