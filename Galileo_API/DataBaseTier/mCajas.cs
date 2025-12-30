namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public static class MCajas
    {
        public static string FxStringCifrado(string input)
        {
            var asciiReversed = string.Concat(
                input
                    .Select(c => ((int)c).ToString())
                    .Reverse()
            );

            var resultBuilder = new System.Text.StringBuilder();
            int sec = 0;

            for (int i = 0; i < asciiReversed.Length; i += 3)
            {
                int len = Math.Min(3, asciiReversed.Length - i);
                int block = int.Parse(asciiReversed.Substring(i, len));

                block += sec switch
                {
                    0 => 1,
                    1 => -5,
                    2 => 7,
                    3 => -13,
                    4 => -2,
                    5 => 3,
                    _ => 0
                };

                sec = (sec + 1) % 6;

                resultBuilder.Append(block);
            }

            return FxDepuraCadena(resultBuilder.ToString());
        }

        private static string FxDepuraCadena(string cadena)
        {
            var finalBuilder = new System.Text.StringBuilder();

            for (int i = 0; i < cadena.Length - 1; i++)
            {
                if (int.TryParse(cadena.Substring(i, 2), out int n) &&
                    n > 31 && n != 39 && n != 34)
                {
                    finalBuilder.Insert(0, (char)n);
                }
            }
            return finalBuilder.ToString();
        }
    }
}