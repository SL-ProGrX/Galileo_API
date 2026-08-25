namespace Galileo.Models.ProGrX.ControlTramites
{
    public class AfSeguimientoRevisionesTagAfiliacionesListaResult
    {
        public int total { get; set; }
        public List<AfSeguimientoRevisionesTagAfiliacionData> lista { get; set; } = new();
    }

    public class AfSeguimientoRevisionesTagAfiliacionData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario_registra { get; set; } = string.Empty;
        public long? numero_remesa { get; set; }
        public string usuario_remesa { get; set; } = string.Empty;
        public long consecutivo { get; set; }
    }
    public class AfSeguimientoRevisionesTagSeleccionRequest
    {
        public string? cedula { get; set; }
        public long? consecutivo { get; set; }
        public int? id_error { get; set; }
        public long? linea_err { get; set; }
        public bool? seleccionado { get; set; }
    }
    public class AfSeguimientoRevisionesTagDetalleData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public long consecutivo { get; set; }
        public long numero_boleta { get; set; }

        public string estado_actual { get; set; } = string.Empty;
        public string estado_descripcion { get; set; } = string.Empty;

        public DateTime? fecha_ingreso { get; set; }
        public DateTime? fecha_nacimiento { get; set; }

        public string sexo { get; set; } = string.Empty;
        public string sexo_descripcion { get; set; } = string.Empty;

        public string estado_civil { get; set; } = string.Empty;
        public string estado_civil_descripcion { get; set; } = string.Empty;

        public string provincia { get; set; } = string.Empty;
        public string provincia_descripcion { get; set; } = string.Empty;

        public string canton { get; set; } = string.Empty;
        public string canton_descripcion { get; set; } = string.Empty;

        public string distrito { get; set; } = string.Empty;
        public string distrito_descripcion { get; set; } = string.Empty;

        public string direccion { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public string apartado { get; set; } = string.Empty;

        public string nombramiento { get; set; } = string.Empty;
        public DateTime? fecha_nombramiento { get; set; }
        public short anios_servicio { get; set; }

        public string promotor { get; set; } = string.Empty;
        public string promotor_descripcion { get; set; } = string.Empty;

        public string notificaciones { get; set; } = string.Empty;

        public string institucion { get; set; } = string.Empty;
        public string institucion_descripcion { get; set; } = string.Empty;

        public string profesion { get; set; } = string.Empty;
        public string profesion_descripcion { get; set; } = string.Empty;

        public string sector { get; set; } = string.Empty;
        public string sector_descripcion { get; set; } = string.Empty;

        public string departamento { get; set; } = string.Empty;
        public string departamento_descripcion { get; set; } = string.Empty;

        public string seccion { get; set; } = string.Empty;
        public string seccion_descripcion { get; set; } = string.Empty;

        public string unidad_programatica { get; set; } = string.Empty;
        public string unidad_programatica_descripcion { get; set; } = string.Empty;

        public string unidad_trabajo { get; set; } = string.Empty;
        public string unidad_trabajo_descripcion { get; set; } = string.Empty;

        public string centro_trabajo { get; set; } = string.Empty;
        public string centro_trabajo_descripcion { get; set; } = string.Empty;

        public string oficina { get; set; } = string.Empty;
        public string oficina_descripcion { get; set; } = string.Empty;

        public string tipo_identificacion { get; set; } = string.Empty;
        public string tipo_identificacion_descripcion { get; set; } = string.Empty;

        public string tipo_sociedad { get; set; } = string.Empty;
        public string tipo_sociedad_descripcion { get; set; } = string.Empty;

        public string actividad_economica { get; set; } = string.Empty;
        public string actividad_economica_descripcion { get; set; } = string.Empty;

        public int hijos { get; set; }
        public int numero_pagos { get; set; }
    }

    public class AfSeguimientoRevisionesTagSeguimientoData
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string tag_descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class AfSeguimientoRevisionesTagEtiquetaData
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string tag_descripcion { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    public class AfSeguimientoRevisionesTagRevisionData
    {
        public int id_error { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool seleccionado { get; set; }
        public bool aplicado { get; set; }
        public string mensaje { get; set; } = string.Empty;
        public long? linea_err { get; set; }
    }
    public class AfSeguimientoRevisionesTagAplicarRequest
    {
        public string? cedula { get; set; }
        public long? consecutivo { get; set; }
        public string? tag_codigo { get; set; }
        public string? observacion { get; set; }
        public List<AfSeguimientoRevisionesTagAplicarRevisionRequest> revisiones { get; set; } = new();
    }
    public class AfSeguimientoRevisionesTagAplicarRevisionRequest
    {
        public int? id_error { get; set; }
        public bool? seleccionado { get; set; }
    }
}