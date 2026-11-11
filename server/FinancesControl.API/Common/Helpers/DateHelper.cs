namespace FinancesControl.API.Common;

public static class DateHelper
{
    public static DateTime GetNextOccurrence(int dayOfMonth)
    {
        var today = DateTime.UtcNow;
        DateTime nextDate;

        if (today.Day <= dayOfMonth)
        {
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            var adjustedDay = Math.Min(dayOfMonth, daysInMonth);
            nextDate = new DateTime(today.Year, today.Month, adjustedDay);
        }
        else
        {
            var nextMonth = today.AddMonths(1);
            var lastDayOfMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
            var adjustedDay = Math.Min(dayOfMonth, lastDayOfMonth);
            nextDate = new DateTime(nextMonth.Year, nextMonth.Month, adjustedDay);
        }

        return nextDate;
    }
}
