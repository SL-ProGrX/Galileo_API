namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CbrIncobrableMovimientos
    {
        public int Crd_Operacion { get; set; }
        public string Cod_Concepto { get; set; } = string.Empty;
        public int Operacion { get; set; }
        public decimal Saldo_Inicial { get; set; }
        public decimal Mov_Principal { get; set; }
        public decimal Saldo_Final { get; set; }
        public DateTime? Registro_fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public string Cod_Caja { get; set; } = string.Empty;
        public string Tipo_Documento { get; set; } = string.Empty;
        public string Num_Documento { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CbrIncobrableGeneral
    {
        public int COD_INCOBRABLE { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Id_Solicitud { get; set; }
        public int CxC_Operacion { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public decimal Recaudado { get; set; }
        public string Tipo_Documento { get; set; } = string.Empty;
        public string Cod_Transaccion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Genera_Documento { get; set; } = string.Empty;
        public DateTime? Mov_Fecha { get; set; }
        public decimal Mov_Monto { get; set; }
        public string Mov_Tipo_Desc { get; set; } = string.Empty;
        public DateTime? Corte { get; set; }
        public int Corte_Saldo { get; set; }
        public int Corte_Recaudo { get; set; }
        public DateTime? Registro_fecha { get; set; } 
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Modifica_fecha { get; set; }
        public string Modifica_Usuario { get; set; } = string.Empty;
        public string NOTAS_REGISTRO { get; set; } = string.Empty;
        public string NOTAS_REVERSION { get; set; } = string.Empty;
        public decimal IntCor { get; set; }
        public decimal IntMor { get; set; }
        public decimal Cargos { get; set; }
        public decimal Poliza { get; set; }

    }
    public class CbrIncobrableFiltros
    {
        public string Estado { get; set; } = string.Empty;
        public string? Usuario { get; set; } = string.Empty;
        public DateTime Inicio { get; set; }
        public DateTime Corte { get; set; }
        public string? Filtro { get; set; } = string.Empty;
        public DateTime Auxiliar { get; set; }
    }
        
}
