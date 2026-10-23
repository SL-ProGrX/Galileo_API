namespace Galileo_API.Models.ProGrX_RRHH
{
    /// <summary>
    /// Información principal de una entidad relacionada.
    /// </summary>
    public class RhCatEntidadData
    {
        public string cod_er { get; set; } = string.Empty;

        public string identificacion { get; set; } = string.Empty;

        public string nombre { get; set; } = string.Empty;

        public string nombre_corto { get; set; } = string.Empty;

        public string estado { get; set; } = string.Empty;

        public bool? asoc_solidarista { get; set; }

        public string emitir { get; set; } = string.Empty;

        public int? cod_banco { get; set; }

        public string banco { get; set; } = string.Empty;

        public string contacto_nombre { get; set; } = string.Empty;

        public string contacto_email { get; set; } = string.Empty;

        public string contacto_movil { get; set; } = string.Empty;

        public string contacto_telefono { get; set; } = string.Empty;

        public DateTime? registro_fecha { get; set; }

        public string registro_usuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para consultar una entidad por código.
    /// </summary>
    public class RhCatEntidadObtenerRequest
    {
        public string cod_er { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para navegación anterior/siguiente de entidades.
    /// direccion:
    /// 0 = anterior
    /// 1 = siguiente
    /// </summary>
    public class RhCatEntidadScrollRequest
    {
        public string? cod_er { get; set; } = string.Empty;

        public int? direccion { get; set; }
    }

    /// <summary>
    /// Request para registrar o modificar una entidad.
    /// </summary>
    public class RhCatEntidadGuardarRequest
    {
        public RhCatEntidadData entidad { get; set; } = new();
    }
    /// <summary>
    /// Registro utilizado por el buscador F4 de entidades.
    /// </summary>
    public class RhCatEntidadBusquedaData
    {
        public string cod_er { get; set; } = string.Empty;

        public string identificacion { get; set; } = string.Empty;

        public string nombre { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para buscador F4 de entidades.
    /// </summary>
    public class RhCatEntidadBusquedaRequest
    {
        public string? texto { get; set; } = string.Empty;
    }

    /// <summary>
    /// Banco disponible para la entidad según el usuario.
    /// Información proveniente de spCrd_SGT_Bancos.
    /// </summary>
    public class RhCatEntidadBancoData
    {
        public int id_banco { get; set; }

        public string descripcion { get; set; } =
            string.Empty;

        public string item_descripcion { get; set; } =
            string.Empty;
    }

    /// <summary>
    /// Cuenta bancaria vinculada a la identificación de la entidad.
    /// La información se obtiene de SYS_CUENTAS_BANCARIAS
    /// y TES_BANCOS_GRUPOS.
    /// </summary>
    public class RhCatEntidadCuentaData
    {
        public string cuenta_interna { get; set; } = string.Empty;

        public string banco { get; set; } = string.Empty;

        public string tipo { get; set; } = string.Empty;

        public string cod_divisa { get; set; } = string.Empty;

        public bool cuenta_interbanca { get; set; }

        public string destino { get; set; } = string.Empty;

        public bool activa { get; set; }

        public DateTime? registro_fecha { get; set; }

        public string registro_usuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lista completa de cuentas bancarias de una entidad.
    /// La paginación, filtrado y ordenamiento se manejan en frontend.
    /// </summary>
    public class RhCatEntidadCuentasLista
    {
        public int total { get; set; }

        public List<RhCatEntidadCuentaData> lista { get; set; } = new();
    }

    /// <summary>
    /// Concepto de nómina disponible/asignado a una entidad relacionada.
    /// </summary>
    public class RhCatEntidadConceptoData
    {
        public string cod_concepto { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;

        public bool asignado { get; set; }
    }

    /// <summary>
    /// Lista completa de conceptos disponibles para la entidad.
    /// Incluye los asignados a la entidad y los conceptos sin COD_ER.
    /// Sort, filtro y paginación se realizan en frontend.
    /// </summary>
    public class RhCatEntidadConceptosLista
    {
        public int total { get; set; }

        public List<RhCatEntidadConceptoData> lista { get; set; } = new();
    }

    /// <summary>
    /// Request para asignar o desasignar inmediatamente
    /// un concepto de nómina a una entidad.
    /// </summary>
    public class RhCatEntidadConceptoActualizarRequest
    {
        public string cod_er { get; set; } = string.Empty;

        public string cod_concepto { get; set; } = string.Empty;

        public bool? asignar { get; set; }
    }
}