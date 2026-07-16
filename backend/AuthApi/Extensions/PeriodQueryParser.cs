namespace AuthApi.Extensions;

public static class PeriodQueryParser
{
    public static bool TryResolveDateRange(
        string? periodType,
        string? startDate,
        string? endDate,
        int? year,
        int? month,
        bool requirePeriodType,
        out DateTime rangeStart,
        out DateTime rangeEndExclusive,
        out string? error)
    {
        rangeStart = default;
        rangeEndExclusive = default;
        error = null;

        if (string.IsNullOrWhiteSpace(periodType))
        {
            if (!requirePeriodType)
                return true;

            error = "Parâmetro periodType é obrigatório. Valores permitidos: range, monthly, yearly.";
            return false;
        }

        var normalizedPeriodType = periodType.Trim().ToLowerInvariant();

        switch (normalizedPeriodType)
        {
            case "range":
                if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    error = "startDate e endDate são obrigatórios quando periodType=range.";
                    return false;
                }

                if (!DateOnly.TryParseExact(
                        startDate,
                        "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var parsedStartDate))
                {
                    error = "startDate inválido. Use o formato YYYY-MM-DD.";
                    return false;
                }

                if (!DateOnly.TryParseExact(
                        endDate,
                        "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var parsedEndDate))
                {
                    error = "endDate inválido. Use o formato YYYY-MM-DD.";
                    return false;
                }

                if (parsedStartDate > parsedEndDate)
                {
                    error = "startDate não pode ser maior que endDate.";
                    return false;
                }

                rangeStart = DateTime.SpecifyKind(parsedStartDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                rangeEndExclusive = DateTime.SpecifyKind(parsedEndDate.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                return true;

            case "monthly":
                if (!year.HasValue || !month.HasValue)
                {
                    error = "year e month são obrigatórios quando periodType=monthly.";
                    return false;
                }

                if (month is < 1 or > 12)
                {
                    error = "month deve estar entre 1 e 12.";
                    return false;
                }

                rangeStart = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                rangeEndExclusive = rangeStart.AddMonths(1);
                return true;

            case "yearly":
                if (!year.HasValue)
                {
                    error = "year é obrigatório quando periodType=yearly.";
                    return false;
                }

                rangeStart = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                rangeEndExclusive = rangeStart.AddYears(1);
                return true;

            default:
                error = "periodType inválido. Valores permitidos: range, monthly, yearly.";
                return false;
        }
    }
}
