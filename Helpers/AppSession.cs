namespace LocAutoPlusApp.Helpers
{
    //internal class AppSession
    //{
    //}

    public static class AppSession
    {
        public static string? Token { get; set; }
        public static int EmployeId { get; set; }
        public static string? Nom { get; set; }
        public static string? Prenom { get; set; }
        public static string? Email { get; set; }
        public static string? Role { get; set; }

        public static bool EstAdmin => Role == "admin";

        public static void Clear()
        {
            Token = null;
            EmployeId = 0;
            Nom = null;
            Prenom = null;
            Email = null;
            Role = null;
        }
    }
}
