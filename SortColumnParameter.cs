using Microsoft.AspNetCore.Mvc;

namespace SearchSample;

public enum SortDirectionOption
{
    Asc,
    Desc
}

public interface ISortColumnParameter
{
    [HiddenInput(DisplayValue = false)]
    public string? Name { get; set; }

    [HiddenInput(DisplayValue = false)]
    public SortDirectionOption? Direction { get; set; }
}

public class SortColumnParameter : ISortColumnParameter
{
    //[HiddenInput(DisplayValue = false)]
    public string? Name { get; set; }

    //[HiddenInput(DisplayValue = false)]
    public SortDirectionOption? Direction { get; set; }
}