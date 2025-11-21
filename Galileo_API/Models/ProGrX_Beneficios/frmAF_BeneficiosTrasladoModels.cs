namespace Galileo.Models.AF
{
    public class AfiInformesTopFiltros
    {
        public string? filtro { get; set; }
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
    }

    public class AfiRemesasFiltros
    {
        public string? filtro { get; set; }
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
    }

    public class AfiBeneficiosRemesasDtoLista
    {
        public int Total { get; set; }
        public List<AfiBeneficiosRemesasDto> Beneficios { get; set; } = new List<AfiBeneficiosRemesasDto>();
    }

    public class AfiBeneficiosRemesasDto
    {
        public long cod_remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class AfiBeneTrasladoOpciones
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class AfiBeneficiosCargasDataLista
    {
        public int Total { get; set; }
        public List<AfiBeneficiosCargasData> Beneficios { get; set; } = new List<AfiBeneficiosCargasData>();
    }

    public class AfiBeneficiosCargasData
    {
        public string Cedula { get; set; } = string.Empty;
        public long Consec { get; set; }
        public string Cod_Beneficio { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public float Monto { get; set; }
        public int Cod_Banco { get; set; }
        public string Tipo_Emision { get; set; } = string.Empty;
        public string Cta_Bancaria { get; set; } = string.Empty;
        public string Tesoreria { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Envio_User { get; set; } = string.Empty;
        public DateTime? Envio_Fecha { get; set; }
        public string Tes_Supervision_Usuario { get; set; } = string.Empty;
        public DateTime? Tes_Supervision_Fecha { get; set; }
        public string Id_Token { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Estado_Persona { get; set; } = string.Empty;
        public string BancoDesc { get; set; } = string.Empty;
        public int id_beneficio { get; set; }
        public string Duplicado { get; set; } = string.Empty;
        public string? Valida_Beneficio { get; set; }
        public DateTime Registra_Fecha { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Beneficio_Desc { get; set; } = string.Empty;

        public string cod_categoria { get; set; } = string.Empty;
        public int id_pago { get; set; }
    }

    public class AfiFiltrosCargas
    {
        public long cod_remesa { get; set; }
        public Nullable<DateTime> fecha_inicio { get; set; }
        public Nullable<DateTime> fecha_corte { get; set; }
        public string estado { get; set; } = string.Empty;
        public string cod_banco { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool extraFiltros { get; set; }
        public string cod_oficina { get; set; } = string.Empty;
        public string cod_beneficio { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string vfiltro { get; set; } = string.Empty;
    }

    public class AfiCargasAplicar
    {
        public long cod_remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public List<CargaCasosData> casos { get; set; } = new List<CargaCasosData>();
    }

    public class CargaCasosData
    {
        public long consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string? justificacion { get; set; }
    }

    public class AfiBeneficiosTrasladoDto
    {
        public long cod_remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string fecha_inicio { get; set; } = string.Empty;
        public string fecha_corte { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string vfiltro { get; set; } = string.Empty;
    }

    public class AfiTrasladoAplicar
    {

        public long cod_remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public List<AfiBeneTrasladoAplciar> casos { get; set; } = new List<AfiBeneTrasladoAplciar>();
        public bool aplicaComision { get; set; }
        public string? token { get; set; }
    }

    public class AfiBeneTrasladoAplciar
    {
        public string cod_beneficio { get; set; } = string.Empty;
        public string tipo_emision { get; set; } = string.Empty;
        public long cod_banco { get; set; }
        public float monto { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string cta_bancaria { get; set; } = string.Empty;
        public long consec { get; set; }
        public int id_pago { get; set; }
    }

    public class AfiBeneficiosTraslado
    {
        public string descripcion { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
    }

    public class AfiBeneDatosCorreoPago
    {
        public string cedula { get; set; } = string.Empty;
        public string beneficio { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string expediente { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
    }

    public class CuboBeneficiosData
    {
        public string cod_remesa { get; set; } = string.Empty;
        public string expediente { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string estado_persona { get; set; } = string.Empty;
        public string estado_bene { get; set; } = string.Empty;
        public string estadolaboral { get; set; } = string.Empty;
        public string lugar_trabajo { get; set; } = string.Empty;
        public string provincia { get; set; } = string.Empty;
        public int? menbresia_meses { get; set; }
        public string categoria_desc { get; set; } = string.Empty;
        public string nombre_beneficiario { get; set; } = string.Empty;
        public string cod_banco { get; set; } = string.Empty;
        public string cta_bancaria { get; set; } = string.Empty;
        public int id_pago { get; set; }
        public string t_identificacion { get; set; } = string.Empty;
        public string t_beneficiario { get; set; } = string.Empty;
        public string tipo_emision { get; set; } = string.Empty;
        public string tesoreria { get; set; } = string.Empty;
        public string remesa_pago { get; set; } = string.Empty;
        public string beneficio_desc { get; set; } = string.Empty;
        public string cc { get; set; } = string.Empty;
        public string desc_cuenta { get; set; } = string.Empty;
        public float monto_autorizado { get; set; }
        public float monto_ejecutado { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cantidad { get; set; } = 0;
        public DateTime? registro_fecha { get; set; }
        public DateTime? envio_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string estado_pago { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string genero { get; set; } = string.Empty;
    }

    public class CuboParametros
    {
        public long cod_remesa { get; set; }
        public Nullable<DateTime> fecha_inicio { get; set; }
        public Nullable<DateTime> fecha_corte { get; set; }
        public string detalle { get; set; } = string.Empty;
    }
}