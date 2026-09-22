namespace StaffManagementApp.Services;

public static class DateHelper
{
    public static int CalculateFullYears(DateTime start, DateTime end)
    {
        int years = end.Year - start.Year;
        if (start.Date > end.AddYears(-years).Date) years--;
        return Math.Max(0, years);
    }

    public static int CalculateAge(DateTime birthDate) =>
        CalculateFullYears(birthDate, DateTime.Today);

    public static int CalculateTenure(DateTime hireDate, DateTime? dismissDate = null) =>
        CalculateFullYears(hireDate, dismissDate?.Date ?? DateTime.Today);
}
