public class Currency
{
    public static string MdlCode = "MDL";
    public static string UsdCode = "USD";
    public static string EurCode = "EUR";
    public static string RonCode = "RON";
    public static string RubCode = "RUB";

    public string Name { get; set; }

    public string Cod { get; set; }

    public int Nominal { get; set; }

    public decimal Value { get; set; }

    public override string ToString()
    {
        return Cod;
    }
}