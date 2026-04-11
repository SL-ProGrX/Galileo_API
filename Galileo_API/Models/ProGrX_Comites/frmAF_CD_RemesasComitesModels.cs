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
        public int NOperacion { get; set; }
        public int Cod_Remesa { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? Notas { get; set; }
        public int TesoreriaId { get; set; } = 0;
    }
}
