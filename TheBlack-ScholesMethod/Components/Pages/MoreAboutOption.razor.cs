using Microsoft.AspNetCore.Components;
using Plotly.Blazor.Traces;
using Plotly.Blazor;
using Microsoft.JSInterop;

public class MoreAboutOptionModel : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery]
    public string? Option { get; set; }

    [Inject] protected IJSRuntime JsRuntime { get; set; }

    protected List<Greek> greeks = new();
    protected int selectedGreeksCount => greeks.Count(g => g.IsSelected);
    protected string OptionName => Option ?? string.Empty;

    protected CalculationsOnOptionsContext _Context = new CalculationsOnOptionsContext();
    protected FinancialOptionsSystemContext _Context_2 = new FinancialOptionsSystemContext();

    protected PlotlyChart chartCallOptionPrice;
    protected PlotlyChart chartPutOptionPrice;
	protected PlotlyChart chartDeltaOptionCall;
	protected PlotlyChart chartDeltaOprionPut;
	protected PlotlyChart chartGammaOptionCall;
	protected PlotlyChart chartGammaOprionPut;
	protected PlotlyChart chartVegaOptionCall;
	protected PlotlyChart chartVegaOprionPut;
	protected PlotlyChart chartThetaOptionCall;
	protected PlotlyChart chartThetaOprionPut;
	protected PlotlyChart chartRhoOptionCall;
	protected PlotlyChart chartRhoOprionPut;

	protected Config configCallOptionPrice = new Config();
    protected Config configPutOptionPrice = new Config();
	protected Config configDeltaOptionCall = new Config();
	protected Config configDeltaOptionPut = new Config();
	protected Config configGammaOptionCall = new Config();
	protected Config configGammaOptionPut = new Config();
	protected Config configVegaOptionCall = new Config();
	protected Config configVegaOptionPut = new Config();
	protected Config configThetaOptionCall = new Config();
	protected Config configThetaOptionPut = new Config();
	protected Config configRhoOptionCall = new Config();
	protected Config configRhoOptionPut = new Config();

	protected Layout layoutCallOptionPrice = new Layout();
    protected Layout layoutPutOptionPrice = new Layout();
	protected Layout layoutDeltaOptionCall = new Layout();
	protected Layout layoutDeltaOptionPut = new Layout();
	protected Layout layoutGammaOptionCall = new Layout();
	protected Layout layoutGammaOptionPut = new Layout();
	protected Layout layoutVegaOptionCall = new Layout();
	protected Layout layoutVegaOptionPut = new Layout();
	protected Layout layoutThetaOptionCall = new Layout();
	protected Layout layoutThetaOptionPut = new Layout();
	protected Layout layoutRhoOptionCall = new Layout();
	protected Layout layoutRhoOptionPut = new Layout();

	protected IList<ITrace> dataOptionCall = new List<ITrace>();
    protected IList<ITrace> dataOptionPut = new List<ITrace>();
	protected IList<ITrace> dataDeltaOptionCall = new List<ITrace>();
	protected IList<ITrace> dataDeltaOptionPut = new List<ITrace>();
	protected IList<ITrace> dataGammaOptionCall = new List<ITrace>();
	protected IList<ITrace> dataGammaOptionPut = new List<ITrace>();
	protected IList<ITrace> dataVegaOptionCall = new List<ITrace>();
	protected IList<ITrace> dataVegaOptionPut = new List<ITrace>();
	protected IList<ITrace> dataThetaOptionCall = new List<ITrace>();
	protected IList<ITrace> dataThetaOptionPut = new List<ITrace>();
	protected IList<ITrace> dataRhoOptionCall = new List<ITrace>();
	protected IList<ITrace> dataRhoOptionPut = new List<ITrace>();

    protected decimal currentAssetPrice;
    protected string typeActive = string.Empty;
    protected string valuteActive = string.Empty;
    protected decimal strikePrice;
    protected DateOnly expirationDate;
    protected decimal callVolatility;
    protected decimal putVolatility;
    protected long assetId;
    protected decimal currentCallPrice;
    protected decimal? currentPutPrice;

    private bool _shouldReload = true;
    private string _previousOption;

    /// <summary>
    /// Инициализация компонента при создании
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        _previousOption = Option;
    }

    /// <summary>
    /// Обработка изменения параметров компонента
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        if (_previousOption != Option)
        {
            _previousOption = Option;
            await LoadDataAsync();

            await ForcePageReload();
        }
    }

    /// <summary>
    /// Загрузка данных и построение графиков
    /// </summary>
    private async Task LoadDataAsync()
    {
        if (string.IsNullOrEmpty(OptionName)) return;

        try
        {
            _Context = new CalculationsOnOptionsContext();
            _Context_2 = new FinancialOptionsSystemContext();

            var infoOption = _Context_2.Options.FirstOrDefault(o => o.OptionId == Convert.ToInt64(OptionName));
            if (infoOption == null) return;

            var infoActive = _Context_2.Actives.FirstOrDefault(o => o.ActiveId == infoOption.ActiveId);
            if (infoActive == null) return;

            var infoCalculation = _Context.Results
                .Where(p => p.OptionName == Convert.ToInt64(OptionName))
                .OrderBy(p => p.DateOfFixation)
                .ToList();

            RequestingData(infoOption, infoActive, infoCalculation);
            InitializeGreeks();

            dataOptionCall = new List<ITrace>();
            dataOptionPut = new List<ITrace>();

            var scatter = new Scatter
            {
                Name = "PriceOptionCall",
                Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
				Line = new Plotly.Blazor.Traces.ScatterLib.Line
				{
					Color = "black",
					Width = 2
				},
				X = [.. infoCalculation.Select(p => p.DateOfFixation)],
                Y = [.. infoCalculation.Select(p => (object)p.CalculationCallOptionPrice)],
            };
            dataOptionCall.Add(scatter);

            var scatter1 = new Scatter
            {
                Name = "PriceOptionPut",
                Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
				Line = new Plotly.Blazor.Traces.ScatterLib.Line
				{
					Color = "black",
					Width = 2
				},
				X = [.. infoCalculation.Select(p => p.DateOfFixation)],
				Y = [.. infoCalculation.Select(p => (object)p.CalculationPutOptionPrice)],
            };
            dataOptionPut.Add(scatter1);

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            dataOptionCall = new List<ITrace>();
            dataOptionPut = new List<ITrace>();
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Принудительное обновление страницы
    /// </summary>
    private async Task ForcePageReload()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("location.reload", true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка перезагрузки страницы: {ex.Message}");
        }
    }

    /// <summary>
    /// Инициализация списка греков с описаниями
    /// </summary>
    protected void InitializeGreeks()
    {
        greeks = new List<Greek>
        {
            new Greek {
                Name = "дельта",
                Description = "Отражает, насколько чувствителен опционный контракт к изменениям цены акции.",
                IsSelected = false
            },
            new Greek {
                Name = "гамма",
                Description = "Показывает скорость изменения дельты при изменении цены базового актива.",
                IsSelected = false
            },
            new Greek {
                Name = "вега",
                Description = "Определяет чувствительность цены опциона к изменению волатильности рынка.",
                IsSelected = false
            },
            new Greek {
                Name = "тета",
                Description = "Измеряет изменение цены опциона с течением времени (временной распад).",
                IsSelected = false
            },
            new Greek {
                Name = "ро",
                Description = "Показывает чувствительность цены опциона к изменению процентной ставки.",
                IsSelected = false
            }
        };
    }

    protected void OnGreekSelectionChanged(Greek greek)
    {
        greek.IsSelected = !greek.IsSelected;
        UpdateGreekCharts(greek);
    }

    protected void UpdateGreekCharts(Greek greek)
    {
        switch (greek.Name)
        {
            case "дельта":
				GenerateGreekDeltaChartCallOption(greek);
				GenerateGreekDeltaChartPutOption(greek);
				break;
			case "гамма":
				GenerateGreekGammaChartCallOption(greek);
				GenerateGreekGammaChartPutOption(greek);
				break;
			case "вега":
				GenerateGreekVegaChartCallOption(greek);
				GenerateGreekVegaChartPutOption(greek);
				break;
			case "тета":
				GenerateGreekThetaChartCallOption(greek);
				GenerateGreekThetaChartPutOption(greek);
				break;
			case "ро":
				GenerateGreekRhoChartCallOption(greek);
				GenerateGreekRhoChartPutOption(greek);
				break;
		}
	}

    protected void GenerateGreekDeltaChartCallOption(Greek greek)
    {
		dataDeltaOptionCall =  new List<ITrace>();
		var scatter = new Scatter
        {
            Name = greek.Name,
            Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
			X = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.DateOfFixation)],
			Line = new Plotly.Blazor.Traces.ScatterLib.Line
			{
				Color = "black",
				Width = 2
			},
			Y = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.CalculationDeltaForCallOption)],
        };
        dataDeltaOptionCall.Add(scatter);
	}
	protected void GenerateGreekDeltaChartPutOption(Greek greek)
	{
		dataDeltaOptionPut = new List<ITrace>();
		var scatter = new Scatter
		{
			Name = greek.Name,
			Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
			Line = new Plotly.Blazor.Traces.ScatterLib.Line
			{
				Color = "black",
				Width = 2
			},
			X = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.DateOfFixation)],
			Y = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.CalculationDeltaForPutOption)],
		};
		dataDeltaOptionPut.Add(scatter);
	}

	protected void GenerateGreekGammaChartCallOption(Greek greek)
	{
		dataGammaOptionCall = new List<ITrace>();
		var scatter = new Scatter
		{
			Name = greek.Name,
			Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
			Line = new Plotly.Blazor.Traces.ScatterLib.Line
			{
				Color = "black",
				Width = 2
			},
			X = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.DateOfFixation)],
			Y = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.CalculationGammaForCallOption)],
		};
		dataGammaOptionCall.Add(scatter);
	}
	protected void GenerateGreekGammaChartPutOption(Greek greek)
	{
		dataGammaOptionPut = new List<ITrace>();
		var scatter = new Scatter
		{
			Name = greek.Name,
			Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
			Line = new Plotly.Blazor.Traces.ScatterLib.Line
			{
				Color = "black",
				Width = 2
			},
			X = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.DateOfFixation)],
			Y = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.CalculationGammaForPutOption)],
		};
		dataGammaOptionPut.Add(scatter);
	}
	protected void GenerateGreekVegaChartCallOption(Greek greek)
	{
		dataVegaOptionCall = new List<ITrace>();
		var scatter = new Scatter
		{
			Name = greek.Name,
			Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
			Line = new Plotly.Blazor.Traces.ScatterLib.Line
			{
				Color = "black",
				Width = 2
			},
			X = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.DateOfFixation)],
			Y = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.CalculationVegaForCallOption)],
		};
		dataVegaOptionCall.Add(scatter);
	}
	protected void GenerateGreekVegaChartPutOption(Greek greek)
	{
		dataVegaOptionPut = new List<ITrace>();
		var scatter = new Scatter
		{
			Name = greek.Name,
			Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
			Line = new Plotly.Blazor.Traces.ScatterLib.Line
			{
				Color = "black",
				Width = 2
			},
			X = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.DateOfFixation)],
			Y = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.CalculationVegaForPutOption)],
		};
		dataVegaOptionPut.Add(scatter);
	}

	protected void GenerateGreekThetaChartCallOption(Greek greek)
	{
		dataThetaOptionCall = new List<ITrace>();
		var scatter = new Scatter
		{
			Name = greek.Name,
			Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
			Line = new Plotly.Blazor.Traces.ScatterLib.Line
			{
				Color = "black",
				Width = 2
			},
			X = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.DateOfFixation)],
			Y = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.CalculationThetaForCallOption)],
		};
		dataThetaOptionCall.Add(scatter);
	}
	protected void GenerateGreekThetaChartPutOption(Greek greek)
	{
		dataThetaOptionPut = new List<ITrace>();
		var scatter = new Scatter
		{
			Name = greek.Name,
			Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
			Line = new Plotly.Blazor.Traces.ScatterLib.Line
			{
				Color = "black",
				Width = 2
			},
			X = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.DateOfFixation)],
			Y = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.CalculationThetaForPutOption)],
		};
		dataThetaOptionPut.Add(scatter);
	}
	protected void GenerateGreekRhoChartCallOption(Greek greek)
	{
		dataRhoOptionCall = new List<ITrace>();
		var scatter = new Scatter
		{
			Name = greek.Name,
			Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
			Line = new Plotly.Blazor.Traces.ScatterLib.Line
			{
				Color = "black",
				Width = 2
			},
			X = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.DateOfFixation)],
			Y = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.CalculationRhoForCallOption)],
		};
		dataRhoOptionCall.Add(scatter);
	}
	protected void GenerateGreekRhoChartPutOption(Greek greek)
	{
		dataRhoOptionPut = new List<ITrace>();
		var scatter = new Scatter
		{
			Name = greek.Name,
			Mode = Plotly.Blazor.Traces.ScatterLib.ModeFlag.Lines | Plotly.Blazor.Traces.ScatterLib.ModeFlag.Markers,
			Line = new Plotly.Blazor.Traces.ScatterLib.Line
			{
				Color = "black",
				Width = 2
			},
			X = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.DateOfFixation)],
			Y = [.. _Context.Results.Where(p => p.OptionName == Convert.ToInt64(OptionName)).Select(p => p.CalculationRhoForPutOption)],
		};
		dataRhoOptionPut.Add(scatter);
	}

	protected string GetGreekCssClass(string greekName)
    {
        return greekName switch
        {
            "дельта" => "delta-point",
            "гамма" => "gamma-point",
            "вега" => "vega-point",
            "тета" => "theta-point",
            "ро" => "rho-point",
            _ => ""
        };
    }

    private void RequestingData(Option infoOption, Active infoActive, List<Result> infoCalculation)
    {
        currentAssetPrice = infoActive.Price;
        typeActive = infoActive.ActiveStyle;
        valuteActive = infoActive.ActiveCurrency;
        strikePrice = infoOption.Strike;
        expirationDate = infoOption.ExpirationDate;
        callVolatility = infoOption.PredefinedIvForCall;
        putVolatility = infoOption.PredefinedIvForPut;
        assetId = infoActive.ActiveId;

        currentCallPrice = infoCalculation[0].CalculationCallOptionPrice;
        currentPutPrice = infoCalculation[0].CalculationPutOptionPrice;
    }
}