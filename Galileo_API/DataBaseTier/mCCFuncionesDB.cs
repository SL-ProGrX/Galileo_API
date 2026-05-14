namespace Galileo_API.DataBaseTier
{
    public static class MccFuncionesDb
    { 
        public static string ObtenerNombreMes(int mes)
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
                9 => "Setiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => mes.ToString()
            };
        }

        public static int? ObtenerNumeroMes(string mes)
        {
            return mes?.Trim() switch
            {
                "Enero" => 1,
                "Febrero" => 2,
                "Marzo" => 3,
                "Abril" => 4,
                "Mayo" => 5,
                "Junio" => 6,
                "Julio" => 7,
                "Agosto" => 8,
                "Setiembre" or "Septiembre" => 9,
                "Octubre" => 10,
                "Noviembre" => 11,
                "Diciembre" => 12,
                _ => null
            };
        }
    }
}
