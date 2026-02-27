namespace mym.Models;

public static class UserProfileClaimTypes
{
    public const string Bio = "profile_bio";
    public const string EducationTitle = "profile_education_title";
    public const string DefaultEducationTitle = "Lisans";

    public static readonly string[] EducationTitles =
    {
        "Lise",
        "Ön Lisans",
        "Lisans",
        "Yuksek Lisans",
        "Doktora",
    };
}
