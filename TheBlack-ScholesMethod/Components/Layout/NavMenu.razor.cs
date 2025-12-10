using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

public class NavMenuModel : ComponentBase
{
    protected static CalculationsOnOptionsContext _Context = new CalculationsOnOptionsContext();

    protected long[] originalOptionCodes = _Context.Results.Select(p => p.OptionName).Distinct().OrderBy(p => p).ToArray();

    protected long[] filteredOptionCodes = Array.Empty<long>();
    protected string searchText = string.Empty;

    protected override void OnInitialized()
    {
        filteredOptionCodes = originalOptionCodes;
    }

    protected string GetHref(long optionCode)
    {
		return $"/more?option={Uri.EscapeDataString(optionCode.ToString())}";
    }

    protected void SearchProducts()
    {
        Console.WriteLine($"ПОИСК НАЧАЛСЯ: '{searchText}'");

        if (string.IsNullOrWhiteSpace(searchText))
        {
            filteredOptionCodes = originalOptionCodes;
        }
        else
        {
            filteredOptionCodes = originalOptionCodes
                .Where(p => p == Convert.ToInt64(searchText))
                .OrderBy(p => p)
                .ToArray();
        }

        Console.WriteLine($"Найдено элементов: {filteredOptionCodes.Length}");
        StateHasChanged();
    }
    protected void OnSearchValueChanged()
    {
        var digitsOnly = new string(searchText.Where(char.IsDigit).ToArray());

        if (digitsOnly != searchText)
        {
            searchText = digitsOnly;
        }

        if (string.IsNullOrEmpty(searchText))
        {
            SearchProducts();
        }
    }

    protected void OnSearchKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            SearchProducts();
        }
    }
}