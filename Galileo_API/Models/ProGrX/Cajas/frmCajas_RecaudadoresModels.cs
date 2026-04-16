namespace Galileo.Models.ProGrX.Cajas
{
    public class CajasRecaudadorData
    {
        public string cod_recaudador { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_desc { get; set; } = string.Empty;
        public string cod_cuenta_iv { get; set; } = string.Empty;
        public string cod_cuenta_iv_desc { get; set; } = string.Empty;
        public string cod_cuenta_comision { get; set; } = string.Empty;
        public string cod_cuenta_comision_desc { get; set; } = string.Empty;

        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
        public bool isNew { get; set; }
    }

    public class CajasRecaudadoresLista
    {
        public int total { get; set; }
        public List<CajasRecaudadorData>? lista { get; set; }

    }

    public class CajasRecaudadorContactoData
    {
        public string cod_recaudador { get; set; } = string.Empty;
        public int linea { get; set; }
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string tel_trabajo { get; set; } = string.Empty;
        public string tel_celular { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }

        public bool isNew { get; set; }
    }

    public class CajasRecaudadorServicioItem
    {
        public string cod_recaudador { get; set; } = string.Empty;
        public string cod_servicio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

}
