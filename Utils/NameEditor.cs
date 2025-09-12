using System;

namespace DbModelGenerator.Utils
{
    internal class NameEditor
    {
        public static string PascalCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input
                .Replace('ç', 'c').Replace('Ç', 'C')
                .Replace('ğ', 'g').Replace('Ğ', 'G')
                .Replace('ı', 'i').Replace('İ', 'I')
                .Replace('ö', 'o').Replace('Ö', 'O')
                .Replace('ş', 's').Replace('Ş', 'S')
                .Replace('ü', 'u').Replace('Ü', 'U');

            string[] parts = input.Split(new[] { '_', ' ', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);

            return string.Concat(parts.Select(p => char.ToUpper(p[0]) + p.Substring(1).ToLower()));
        }


        public static string GetAbbreviation(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var parts = input
                .Where(c => char.IsUpper(c))
                .Select(c => char.ToLower(c))
                .ToArray();

            return new string(parts)
                .Replace('ç', 'c')
                .Replace('ğ', 'g')
                .Replace('ı', 'i')
                .Replace('ö', 'o')
                .Replace('ş', 's')
                .Replace('ü', 'u');
        }


    }
}
