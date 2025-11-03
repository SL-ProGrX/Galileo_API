namespace PgxAPI.Models.ProGrX.Cajas
{
    public class GestionEstadoDTO
    {
        public int Gestion_Id { get; set; }
        public string Gestion_Estado { get; set; }
    }

    public class FondosAporteAplicarDTO
    {
        public string cedula { get; set; }
        public string plan { get; set; }
        public int contrato { get; set; }
        public decimal aporte { get; set; }
        public string tipodoc { get; set; }
        public string usuario { get; set; }
        public string caja { get; set; }
        public int apertura { get; set; }
        public int sesionid { get; set; }
        public string tiquete { get; set; }
        public string nombre { get; set; }
        public string cod_divisa { get; set; }

    }

    public class fondosRequiereAutorizacionDTO
    {
        public bool requiere { get; set; }
        public decimal montomaximo { get; set; }
    }

    public class fondosGestionRegistroAddDTO
    {
        public string cedula { get; set; }
        public string tipo { get; set; }
        public string operadora { get; set; }
        public string plan { get; set; }
        public int contrato { get; set; }
        public decimal montoautorizado { get; set; }
        public decimal aporte { get; set; }
        public string usuario { get; set; }
    }

    public class fondosGestionRegistroDTO
    {
        public int gestion_id { get; set; }
        public string gestion_estado { get; set; }
    }

    public class FndSubCuentasDTO
    {
        public int idx { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal valorfijo { get; set; } = 0;
    }



}

