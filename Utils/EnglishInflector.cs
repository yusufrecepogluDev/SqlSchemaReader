namespace DbModelGenerator.Utils
{
    public static class EnglishInflector
    {
        public static string ToSingular(string word)
        {
            if (word.EndsWith("ies"))
                return word.Substring(0, word.Length - 3) + "y"; 
            if (word.EndsWith("s") && !word.EndsWith("ss"))
                return word.Substring(0, word.Length - 1);        
            return word;                                         
        }

        public static string ToPlural(string word)
        {
            if (word.EndsWith("y") && !"aeiou".Contains(char.ToLower(word[word.Length - 2])))
                return word.Substring(0, word.Length - 1) + "ies"; 
            if (word.EndsWith("s") || word.EndsWith("x") || word.EndsWith("z") || word.EndsWith("ch") || word.EndsWith("sh"))
                return word + "es";                                 
            return word + "s";                                     
        }
    }

}
