namespace Galileo.Models.ProGrX_Activos_Fijos
{
        public class ActivosReasignacionesBoleta
        {
            public string cod_traslado { get; set; } = "";
            public string num_placa { get; set; } = "";
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
        }
        public class ActivosReasignacionesCambioRequest
        {
            public string cod_traslado { get; set; } = "";        
            public string num_placa { get; set; } = "";           
            public string cod_motivo { get; set; } = "";     
            public string identificacion { get; set; } = ""; 
            public string identificacion_destino { get; set; } = "";
            public string notas { get; set; } = "";            
            public string usuario { get; set; } = "";            
            public string fecha_aplicacion { get; set; } = "";  
        }
        public class ActivosReasignacionesBoletaResult
        {
            public string cod_traslado { get; set; } = "";
        }
        public class ActivosReasignacionesBoletaHistorialItem
        {
            public string cod_traslado { get; set; } = "";
            public string num_placa { get; set; } = "";
            public string placa_alterna { get; set; } = "";
            public string descripcion { get; set; } = "";

            public string registro_fecha { get; set; } = "";
            public string registro_usuario { get; set; } = "";
            public string persona_origen { get; set; } = "";
            public string persona_destino { get; set; } = "";

            public string motivo { get; set; } = "";
            public string estado_desc { get; set; } = "";
        }
        public class ActivosReasignacionesBoletaHistorialLista
        {
            public int total { get; set; }
            public List<ActivosReasignacionesBoletaHistorialItem> lista { get; set; } = new();
        }
        public class ActivosReasignacionesActivo
        {
            public string num_placa { get; set; } = "";
            public string nombre { get; set; } = "";
            public string tipo_activo { get; set; } = "";
            public string cod_departamento { get; set; } = "";
            public string departamento { get; set; } = "";
            public string cod_seccion { get; set; } = "";
            public string seccion { get; set; } = "";
            public string identificacion { get; set; } = "";
            public string persona { get; set; } = "";
        }
        public class ActivosReasignacionesPersona
        {
            public string identificacion { get; set; } = "";
            public string persona { get; set; } = "";
            public string cod_departamento { get; set; } = "";
            public string departamento { get; set; } = "";
            public string cod_seccion { get; set; } = "";
            public string seccion { get; set; } = "";
        }
        public class ActivosReasignacionesBoletasFiltros : FiltrosLazyLoadData
        {
            public string? numPlaca { get; set; }
            public string? boletaInicio { get; set; }
            public string? boletaCorte { get; set; }
            public short todosActivos { get; set; } = 1;
            public string? fechaInicio { get; set; }
            public string? fechaCorte { get; set; }
        }
        public class ActivosReasignacionesActivoResumen
        {
            public string num_placa { get; set; } = "";
            public string placa_alterna { get; set; } = "";
            public string nombre { get; set; } = "";
        }
        public class ActivosReasignacionesActivosLista
        {
            public int total { get; set; }
            public List<ActivosReasignacionesActivoResumen> lista { get; set; } = new();
        }
        public class ActivosReasignacionesBoletasLoteRequest
        {
            public List<string> Boletas { get; set; } = new();
            public string? Usuario { get; set; }
        }
}
