using GameMarketIntel.Domain.Enums;

namespace GameMarketIntel.Domain.ValueObjects;

public sealed record ReleaseDateValue
{
    public ReleaseDateKind Kind { get; private set; }
    public int? Year { get; private set; }
    public int? Month { get; private set; }
    public int? Day { get;private set; }

    public int? Quarter { get; private set; }

    public const int MinimumYear = 1;
    public const int MaximumYear = 9999;
    public const int MinimumMonth = 1;
    public const int MaximumMonth = 12;
    public const int MinimumQuarter = 1;
    public const int MaximumQuarter = 4;

    private ReleaseDateValue() { }

private ReleaseDateValue(ReleaseDateKind kind, int? year, int? month, int? day, int? quarter)
{
    Kind = kind;
    Year = year;
    Month = month;
    Day = day;
    Quarter = quarter;

}

public static ReleaseDateValue ForDay (DateOnly date)=>
    new ReleaseDateValue(ReleaseDateKind.Day,date.Year,date.Month,date.Day,quarter:null);

public static ReleaseDateValue ForMonth(int year,int month)
{
    ValidateYear(year);
    ValidateMonth(month);
    return new ReleaseDateValue(ReleaseDateKind.Month, year,month,day:null,quarter:null);

}



    public static ReleaseDateValue ForYear(int year)
{
    ValidateYear(year);
    return new ReleaseDateValue(ReleaseDateKind.Year, year,month:null, day:null,quarter:null);

}
public static ReleaseDateValue ForQuarter(int year, int quarter)
{
    ValidateYear(year);
    ValidateQuarter(quarter);

   return new ReleaseDateValue(ReleaseDateKind.Quarter, year,month:null, day:null,quarter);
}




public static ReleaseDateValue ToBeDetermined()=>
    new ReleaseDateValue(ReleaseDateKind.ToBeDetermined, year:null,month:null, day:null, quarter:null);

private static void ValidateYear(int year)
{
   if(year is < MinimumYear or  > MaximumYear)
        throw new ArgumentOutOfRangeException(nameof(year),year, $"The release year must be between { MinimumYear } and { MaximumYear }");
}

private static void ValidateMonth(int month)
{
   if(month is < MinimumMonth or > MaximumMonth)
        throw new ArgumentOutOfRangeException(nameof(month),month, $"The release month must be between { MinimumMonth } and { MaximumMonth }");
}
private static void ValidateQuarter(int quarter)
{
    if(quarter is <MinimumQuarter or  > MaximumQuarter)
        throw new ArgumentOutOfRangeException(nameof(quarter), quarter, $"The release quarter must between { MinimumQuarter } and { MaximumQuarter }");
}


}