
namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosResponsablesCambioBoleta
    {
        public string cod_traslado { get; set; } = "";
        public string cod_motivo { get; set; } = "";
        public string motivo { get; set; } = "";
        public string notas { get; set; } = "";

        public string estado { get; set; } = "";
        public string estado_desc { get; set; } = "";

        public string registro_usuario { get; set; } = "";
        public string registro_fecha { get; set; } = "";
        public string? cerrado_usuario { get; set; }
        public string? cerrado_fecha { get; set; }
        public string? procesado_usuario { get; set; }
        public string? procesado_fecha { get; set; }

        public string fecha_aplicacion { get; set; } = "";

        public string identificacion { get; set; } = "";
        public string persona { get; set; } = "";
        public string cod_departamento { get; set; } = "";
        public string departamento { get; set; } = "";
        public string cod_seccion { get; set; } = "";
        public string seccion { get; set; } = "";
        public string identificacion_destino { get; set; } = "";
        public string persona_destino { get; set; } = "";
        public string cod_departamento_destino { get; set; } = "";
        public string departamento_destino { get; set; } = "";
        public string cod_seccion_destino { get; set; } = "";
        public string seccion_destino { get; set; } = "";
        public bool isNew { get; set; } = false;
    }
    public class ActivosResponsablesCambioPlaca
    {
        public short asignado { get; set; }
        public string num_placa { get; set; } = "";
        public string descripcion { get; set; } = "";
        public decimal depreciacion_ac { get; set; }
        public decimal depreciacion_mes { get; set; }
        public decimal valor_libros { get; set; }
    }
    public class ActivosResponsablesCambioPlacaGuardarRequest
    {
        public string cod_traslado { get; set; } = "";
        public string num_placa { get; set; } = "";
        public string usuario { get; set; } = "";
        public short primer_lote_bit { get; set; }
    }
    public class ActivosResponsablesCambioBoletaResult
    {
        public string cod_traslado { get; set; } = "";
    }
    public class ActivosResponsablesCambioBoletaResumen
    {
        public string cod_traslado { get; set; } = "";
        public string identificacion { get; set; } = "";
        public string persona { get; set; } = "";
        public string estado_desc { get; set; } = "";
        public string registro_fecha { get; set; } = "";
    }
    public class ActivosResponsablesCambioBoletaLista
    {
        public int total { get; set; }
        public List<ActivosResponsablesCambioBoletaResumen> lista { get; set; } = new();
    }
    public class ActivosResponsablesPersona
    {
        public string identificacion { get; set; } = "";
        public string persona { get; set; } = "";
        public string cod_departamento { get; set; } = "";
        public string departamento { get; set; } = "";
        public string cod_seccion { get; set; } = "";
        public string seccion { get; set; } = "";
    }

}
