namespace PgxAPI.Models
{

    public class CntUnidadDTO
    {
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CntCentroCostosDTO
    {
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CntPeriodosDTO
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Cod_Contabilidad { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Cierre_Fecha { get; set; }
        public string Cierre_Usuario { get; set; } = string.Empty;
        public DateTime Periodo_Corte { get; set; }


    }

    public class CntDescripCuentaDTO
    {
        public string Descripcion { get; set; } = string.Empty;
    }


    public class CntValidaDTO
    {
        public int Existe { get; set; }
    }

    public class CntDescripTipoAsientoDTO
    {
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CntContabilidadesDTO
    {
        public int Nivel1 { get; set; }
        public int Nivel2 { get; set; }
        public int Nivel3 { get; set; }
        public int Nivel4 { get; set; }
        public int Nivel5 { get; set; }
        public int Nivel6 { get; set; }
        public int Nivel7 { get; set; }
        public int Nivel8 { get; set; }

    }

    public class DefMascarasDTO
    {
        public int gMascaraTChar { get; set; }
        public string gstrNiveles { get; set; } = string.Empty;
        public string gstrMascara { get; set; } = string.Empty;
        public int gEnlace { get; set; }
    }

    public class SifEmpresaDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cod_Empresa_Enlace { get; set; }
    }

    public class ErrormCntLink
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }



}
