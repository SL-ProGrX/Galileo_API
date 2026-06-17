namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrBitacoraEspecialSocioModel
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class CrBitacoraEspecialUsuarioModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CrBitacoraEspecialMovimientoModel
    {
        public string Movimiento { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CrBitacoraEspecialRegistrosObtenerRequest
    {
        public string Cedula { get; set; } = string.Empty;
        public bool ChkFechas { get; set; } = true;
        public DateTime? Fecha_Inicio { get; set; }
        public DateTime? Fecha_Corte { get; set; }
        public List<string> Movimientos { get; set; } = [];
        public bool ChkUsuarios { get; set; } = true;
        public string Usuario { get; set; } = string.Empty;
        public bool ChkRevision { get; set; } = false;
        public string Tipo { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
    }

    public class CrBitacoraEspecialRegistroModel
    {
        public long ID { get; set; } = 0;
        public long Id_Solicitud { get; set; } = 0;
        public string Movimiento { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public DateTime? Revisado_Fecha { get; set; }
        public string Revisado_Usuario { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string MovimientoDesc { get; set; } = string.Empty;
        public int Revisado { get; set; } = 0;
    }

    public class CrBitacoraEspecialAsignarRequest
    {
        public long Id_Credito_SuBit { get; set; } = 0;
        public string Revisado_Usuario { get; set; } = string.Empty;
    }
}
