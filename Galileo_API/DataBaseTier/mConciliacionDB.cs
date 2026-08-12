namespace Galileo.DataBaseTier
{
    public static class MConciliacionDB
    {
        /// <summary>
        /// Convierte el número de un mes en su descripción en español.
        /// </summary>
        /// <param name="mes"></param>
        /// <returns></returns>
        public static string fxConvierteMES(int mes)
        {
            return mes switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => string.Empty
            };
        }
    }
}