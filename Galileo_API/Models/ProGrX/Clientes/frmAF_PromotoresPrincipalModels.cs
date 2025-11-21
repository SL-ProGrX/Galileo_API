namespace Galileo.Models.ProGrX.Clientes
{
    public class AfPromotoresPrincipalDto
    {
        public int id_promotor { get; set; }
        public string nombre { get; set; } = string.Empty;
        public DateTime? fechaing { get; set; }
        public int estado { get; set; }
        public int cod_banco { get; set; }
        public string banco { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public string cod_comision { get; set; } = string.Empty;
        public string? user_referencia { get; set; } = string.Empty;
        public string? observacion { get; set; } = string.Empty;
        public string? tipo { get; set; } = string.Empty;
        public string? direccion { get; set; } = string.Empty;
        public string? aptopostal { get; set; } = string.Empty;
        public string? email { get; set; } = string.Empty;
        public string? telefono { get; set; } = string.Empty;
        public string? telefono_ext { get; set; } = string.Empty;
        public string? fax { get; set; } = string.Empty;
        public string? fax_ext { get; set; } = string.Empty;
        public string? nombre_contacto { get; set; } = string.Empty;
        public bool comite { get; set; }
        public bool apl_comision { get; set; }
        public string? usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
    }

    public class AfPromotoresPrincipalLista
    {
        public int total { get; set; }
        public List<AfPromotoresPrincipalDto> lista { get; set; } = new List<AfPromotoresPrincipalDto>();
    }

    public class AfPromotoresCuentasDto
    {
        public string banco { get; set; } = string.Empty;
        public string tipoDesc { get; set; } = string.Empty;
        public string cod_Divisa { get; set; } = string.Empty;
        public string cuenta_Interna { get; set; } = string.Empty;
        public string cuenta_Interbanca { get; set; } = string.Empty;
        public bool activa { get; set; }
        public string destino { get; set; } = string.Empty;
        public DateTime registro_Fecha { get; set; }
        public string registro_Usuario { get; set; } = string.Empty;
    }

    public class AfPromotoresBancoDto
    {
        public int id_banco { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }
}