namespace PeralAPI.Utilities
{
    public static class PasswordValidator
    {
        public static (bool IsValid, string Error) Validate(string password)
        {
            if (password.Length > 128)
                return (false, "Password must not exceed 128 characters.");

            if (password.Length < 8)
                return (false, "Password must be at least 8 characters.");

            if (!password.Any(char.IsUpper))
                return (false, "Password must contain at least one uppercase letter.");

            if (!password.Any(char.IsDigit))
                return (false, "Password must contain at least one number.");

            return (true, string.Empty);
        }
    }
}
