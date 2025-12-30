namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public static class MCajas
    {
        public static string FxStringCifrado(string input)
        {
            var x = new System.Text.StringBuilder();
            var y = new System.Text.StringBuilder();
            int sec = 0;

            foreach (var c in input)
            {
                x.Insert(0, ((int)c).ToString());
            }

            string s = x.ToString();
            for (int i = 0; i < s.Length; i += 3)
            {
                int len = Math.Min(3, s.Length - i);
                int b = int.Parse(s.Substring(i, len));

                b += sec switch
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
                y.Append(b.ToString());
            }

            return FxDepuraCadena(y.ToString());
        }

        private static string FxDepuraCadena(string cadena)
        {
            var res = new System.Text.StringBuilder();
            for (int i = 0; i < cadena.Length - 1; i++)
            {
                if (int.TryParse(cadena.Substring(i, 2), out int n) &&
                    n > 31 && n != 39 && n != 34)
                {
                    res.Insert(0, (char)n);
                }
            }
            return res.ToString();
        }
    }
}