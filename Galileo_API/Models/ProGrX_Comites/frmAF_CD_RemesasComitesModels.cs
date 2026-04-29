namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdRemesaTesDto
    {
        public int Cod_Remesa { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Usuario { get; set; }
        public DateTime? Fecha_Inicio { get; set; }
        public DateTime? Fecha_Corte { get; set; }
        public string? Notas { get; set; }
        public string? Estado { get; set; }
    }

    public class AfCdRemesaTesSaveDto
    {
        public required int Cod_Remesa { get; set; } // 0 para insertar, >0 para actualizar
        public string Usuario { get; set; } = string.Empty;
        public DateTime? Fecha_Inicio { get; set; }
        public DateTime? Fecha_Corte { get; set; }
        public string? Notas { get; set; }
    }

    public class AfCdRemesaTesFechasDto
    {
        public DateTime? Fecha_Inicio { get; set; }
        public DateTime? Fecha_Corte { get; set; }
    }

    public class AfCdBancoDto
    {
        public int Id_Banco { get; set; }
        public string? Descripcion { get; set; }
    }

    public class AfCdCuentaOperacionDto
    {
        public int NOperacion { get; set; }
        public int Cod_Comite { get; set; }
        public string? Descripcion { get; set; }
        public string? Cedula { get; set; }
        public string? Asociado { get; set; }
        public string? Cuenta { get; set; }
        public string? Registro_Usuario { get; set; }
        public string? Tipo { get; set; }
    }

    public class AfCdCuentaActividadDto
    {
        public string Cod_Actividad { get; set; } = string.Empty;
        public int NOperacion { get; set; }
        public decimal? Monto { get; set; }
    }

    public class AfCdRemesaEstadoDto
    {
        public string? Estado { get; set; }
    }

    public class AfCdCuentaRemesaDto
    {
        public int Cod_Remesa { get; set; }
    }

    public class AfCdCuentaRemesaSpParams
    {
        public int? NOperacion { get; set; }
        public int? Cod_Remesa { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? Notas { get; set; }
        public int TesoreriaId { get; set; } = 0;
    }

    public class AfCdRemesaTesFiltroParams
    {
        public int Cantidad { get; set; } = 15;
        public string? Estado { get; set; } // 'A', 'C', 'T' o null para todos
    }

    public class AfCdRemesaComiteDetalleDto
    {
        public int Cod_Remesa { get; set; }
        public int NOperacion { get; set; }
        public int NSolicitud { get; set; }
        public DateTime? Tesoreria_Fecha { get; set; }
        public string Comite { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Cod_Comite { get; set; } = string.Empty;
    }

    public class AfCdRemesaComiteDetalleParams
    {
        public string Comite { get; set; } = string.Empty;
        public required DateTime FechaInicio { get; set; }
        public required DateTime FechaCorte { get; set; }
    }

    public class AfCdRemesaResumenDto
    {
        public int Cod_Remesa { get; set; }
        public DateTime? Fecha { get; set; }
        public DateTime? Fecha_Inicio { get; set; }
        public DateTime? Fecha_Corte { get; set; }
        public string? Usuario { get; set; }
        public decimal Monto { get; set; }
        public int Casos { get; set; }
    }

    public class AfCdRemesaDetalleDto
    {
        public int NOperacion { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Id_Banco { get; set; }
        public string Banco { get; set; } = string.Empty;
        public string Comite { get; set; } = string.Empty;
    }

    public class TesTokenConsecDto
    {
        public int Consec { get; set; }
    }

    public class TesTokenInsertDto
    {
        public string Id_Token { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class AfCdRemesaDesembolsoParams
    {
        public required int Remesa { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }
}
