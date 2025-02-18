using System.Text.RegularExpressions;

namespace FastBite.Validators;

public class RegexPatterns
{
    public const string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    public const string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,}$";
    public const string phoneNumberPattern = @"^\+994\d{9}$";
    public const string namePattern = @"^[a-zA-Zа-яА-Я]+$";
    public const string surnamePattern = @"^[a-zA-Zа-яА-Я]+$";
}