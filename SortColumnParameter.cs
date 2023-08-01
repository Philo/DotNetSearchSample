using Microsoft.AspNetCore.Mvc;

namespace SearchSample
{
    public class SortColumnParameter
    {
        [HiddenInput(DisplayValue = false)]
        public string? Name { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string? Direction { get; set; }
    }
}