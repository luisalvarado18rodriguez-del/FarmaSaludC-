namespace FarmaSaludMVC.security
{
    public static class SecurityHelper
    {
        // Esta función convierte el texto plano (123) en un hash seguro
        public static string EncriptarPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Esta función compara el texto plano con el hash de la BD
        public static bool VerificarPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
