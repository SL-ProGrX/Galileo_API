namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdComiteResumenDto
    {
        public string? Cod_Comite { get; set; }
        public string? Descripcion { get; set; }
        public int? Cod_Director { get; set; }
        public string? Director { get; set; }
        public bool Activo { get; set; }
        public string? Unidad_Relacionada { get; set; }
        public string? Validacion_Unidad { get; set; }
        public bool Existe { get; set; }
    }

    public class AfCdComiteListaDto
    {
        public string? Codigo { get; set; }
        public string? Descripcion { get; set; }
    }

    public class AfCdComiteMiembroDto
    {
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public int? Cod_Puesto { get; set; }
        public string? Puesto { get; set; }
        public string? Notas { get; set; }
        public bool Activo { get; set; }
        public bool Apl_Desembolsos { get; set; }
        public string? Af_Email { get; set; }
        public string? Ut_Descripcion { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public string? Nombre_Jefe { get; set; }
        public string? Telefono_Jefe { get; set; }
        public string? Celular_Jefe { get; set; }
        public string? Correo_Jefe { get; set; }
        public string? Rango_Jefe { get; set; }
        public DateTime? Fecha_Eleccion { get; set; }
    }

    public class AfCdComiteMiembroHistorialDto
    {
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public int? Cod_Puesto { get; set; }
        public string? Puesto { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public bool Apl_Desembolsos { get; set; }
        public bool Activo { get; set; }
        public string? Cod_Comite { get; set; }
    }

    public class AfCdComiteLiquidacionDto
    {
        public string? Noperacion { get; set; }
        public string? Notas { get; set; }
        public decimal Monto { get; set; }
        public string? Tesoreria_Nsolicitud { get; set; }
        public DateTime? Liquida_Fecha { get; set; }
    }

    public class AfCdComiteMensajeDto
    {
        public string? Mensaje { get; set; }
        public DateTime? Fecha { get; set; }
        public DateTime? Vencimiento { get; set; }
        public string? Usuario { get; set; }
    }

    public class AfCdComiteDetalleDto
    {
        public AfCdComiteResumenDto? Comite { get; set; }
        public List<AfCdComiteListaDto> Unidades { get; set; } = new();
        public List<AfCdComiteListaDto> Actividades { get; set; } = new();
        public List<AfCdComiteListaDto> Ejecutivos { get; set; } = new();
        public List<AfCdComiteMiembroDto> Miembros { get; set; } = new();
        public List<AfCdComiteLiquidacionDto> Liquidaciones { get; set; } = new();
        public List<AfCdComiteLiquidacionDto> LiquidacionesHistorico { get; set; } = new();
        public List<AfCdComiteMensajeDto> Mensajes { get; set; } = new();
    }

    public class AfCdComiteGuardarRequest
    {
        public string? Cod_Comite { get; set; }
        public string? Descripcion { get; set; }
        public int? Cod_Director { get; set; }
        public bool? Activo { get; set; }
        public string? Usuario { get; set; }
    }

    public class AfCdComiteAsociacionRequest
    {
        public string? Cod_Comite { get; set; }
        public string? Codigo { get; set; }
        public string? Usuario { get; set; }
    }

    public class AfCdComiteMiembroGuardarRequest
    {
        public string? Cod_Comite { get; set; }
        public string? Cedula { get; set; }
        public int? Cod_Puesto { get; set; }
        public string? Notas { get; set; }
        public bool? Apl_Desembolsos { get; set; }
        public bool? Activo { get; set; }
        public string? Nombre_Jefe { get; set; }
        public string? Telefono_Jefe { get; set; }
        public string? Celular_Jefe { get; set; }
        public string? Correo_Jefe { get; set; }
        public string? Rango_Jefe { get; set; }
        public DateTime? Fecha_Eleccion { get; set; }
        public string? Usuario { get; set; }
    }
}
