namespace Galileo.Models.ProGrX.Cajas
{
    public class FiltrosSaldosFavorTransito : FiltrosLazyLoadData
    {
        public string Estado { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string TipoDocumento { get; set; } = "";
        public string NumeroDocumento { get; set; } = "";
        public string UsuarioRegistro { get; set; } = "";
        public string EntidadPagadora { get; set; } = "";
        public string OrigenRecursos { get; set; } = "";
        public decimal MontoDesde { get; set; } = 0;
        public decimal MontoHasta { get; set; } = 999999999999.99m;

        public bool TodasLasFechas { get; set; } = false;
        public string FechaInicio { get; set; } = "";
        public string FechaCorte { get; set; } = "";
    }
    public class CajasSaldosFavorLista
    {
        public int Total { get; set; }
        public List<CajasSaldosFavorItem>? Lista { get; set; }
    }
    
    public class CajasSaldosFavorItem
    {
        public int Linea { get; set; }
        public required string Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? Doc_Tipo { get; set; }
        public string? Doc_Numero { get; set; }

        public decimal Monto { get; set; }
        public decimal Saldo { get; set; }
        public string? Cod_Divisa { get; set; }

        public string? Registro_Fecha_Format { get; set; }
        public string? Registro_Usuario { get; set; }

        public string? Liq_Fecha { get; set; }
        public string? Liq_Usuario { get; set; }
        public decimal? Liq_Monto { get; set; }
        public string? Liq_NSolicitud { get; set; }
        public string? Liq_Plan { get; set; }
        public string? Liq_Contrato { get; set; }
        public string? Liq_Tipo_Doc { get; set; }
        public string? Liq_Num_Doc { get; set; }

        public string? BancoDesc { get; set; }
        public string? EntidadPagoDesc { get; set; }
        public string? OrigenRecursoDesc { get; set; }

        public string? Autoriza_Estado_Desc { get; set; }
        public string? Valida_Usuario { get; set; }
        public string? Valida_Fecha { get; set; }
        public string? Valida_Notas { get; set; }
    }
    
    public class CajasSaldosFavorAutorizaRequest
    {
        public List<int> SaldoFavorIds { get; set; } = new();
        public string Estado { get; set; } = "";
        public string Usuario { get; set; } = "";
        public string Notas { get; set; } = "";
    }
    
    public class CajasEmpresaInfoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Cedula_Juridica { get; set; } = string.Empty;
    }

}
