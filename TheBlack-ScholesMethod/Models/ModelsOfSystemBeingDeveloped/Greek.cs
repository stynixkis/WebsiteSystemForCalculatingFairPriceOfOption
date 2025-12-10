/// <summary>
/// Класс, представляющий грек опциона
/// </summary>
public class Greek
{
    /// <summary>
    /// Название грека
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание грека
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Флаг выбора грека для отображения
    /// </summary>
    public bool IsSelected { get; set; }
}