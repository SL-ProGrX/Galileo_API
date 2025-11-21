namespace Galileo.Models.ProGrX.Bancos
{
    public class TesBancoDto
    {
        public int id_banco { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string desc_corta { get; set; } = string.Empty;
        public string? ctaconta { get; set; }
        public decimal? saldo { get; set; }
        public string? formato_transferencia { get; set; }
        public string? cta { get; set; }
        public decimal? firmas_desde { get; set; }
        public decimal? firmas_hasta { get; set; }
        public string? estado { get; set; }
        public DateTime fecha_envia { get; set; }
        public bool monitoreo { get; set; }
        public bool cta_regional { get; set; }
        public string? archivo_especial_ck { get; set; }
        public string? cod_grupo { get; set; }
        public bool? puente { get; set; }
        public bool? utiliza_formato_especial { get; set; }
        public string? archivo_cheques_firmas { get; set; }
        public string? archivo_cheques_sin_firmas { get; set; }
        public string? cod_divisa { get; set; }
        public string? lugar_emision { get; set; }
        public bool? supervision { get; set; }
        public int? supervision_dias { get; set; }
        public bool? sinpe_interna { get; set; }
        public string? sinpe_empresa { get; set; }
        public string? codigo_cliente { get; set; }
        public string? formato_transferencias_n2 { get; set; }
        public bool? utiliza_autogestion { get; set; }
        public decimal? concilia_ar_comision { get; set; }
        public string? concilia_ar_comision_cta { get; set; }
        public string? concilia_ar_unidad { get; set; }
        public string? concilia_ar_concepto { get; set; }
        public string? concilia_ar_centro { get; set; }
        public string? concilia_ar_centro_com { get; set; }
        public bool? utiliza_plan { get; set; }
        public string? grupox { get; set; }
        public string? formatoN1 { get; set; }
        public string? formatoN2 { get; set; }
        public string? divisadesc { get; set; }
        public string cod_cuenta { get; set; } = string.Empty;
        public string? cod_cuenta_desc { get; set; }
        public string cod_cuenta_con { get; set; } = string.Empty;
        public string? cod_cuenta_con_desc { get; set; }
        public string? unidad { get; set; }
        public string? unidad_desc { get; set; }
        public string? centro { get; set; }
        public string? centro_desc { get; set; }
        public string? centro_com { get; set; }
        public string? centro_com_desc { get; set; }
        public string? concepto { get; set; }
        public string? concepto_desc { get; set; }
        public bool? ilocalizable { get; set; }
        public bool? int_grupos_asociados { get; set; }
        public bool? int_requiere_cuenta_destino { get; set; }
    }

    public class DropDownListaDivisas
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class TesBancosCierres
    {
        public int idx { get; set; }
        public int id_banco { get; set; }
        public DateTime? inicio { get; set; }
        public DateTime? corte { get; set; }
        public decimal? saldo_inicial { get; set; }
        public decimal? total_bebitos { get; set; }
        public decimal? total_creditos { get; set; }
        public decimal? saldo_final { get; set; }
        public decimal? ajuste { get; set; }
        public DateTime? fecha { get; set; }
        public string? usuario { get; set; } = string.Empty;
        public decimal? saldo_minimo { get; set; }
    }

    public class ParametrosSaldoFecha
    {
        public int id_banco { get; set; } 
        public string desc_corta { get; set; } = string.Empty;
        public decimal saldo { get; set; }
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class ParametrosConciliacion
    {
        public int id_banco { get; set; }
        public string desc_corta { get; set; } = string.Empty;
        public decimal concilia_ar_comision { get; set; }
        public string cod_cuenta_con { get; set; } = string.Empty;
        public string unidad { get; set; } = string.Empty;
        public string centro { get; set; } = string.Empty;
        public string centro_com { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class TesBancosGruposAsgDto
    {
        public int? id_banco { get; set; }
        public string? cod_grupo { get; set; }
        public string? descripcion { get; set; }
    }

    public class BancoValidaCuenta
    {
        public string? cod_grupo { get; set; }
        public string? cta { get; set; }
        public string? cod_divisa { get; set; } = "COL";
        public bool int_grupos_asociados { get; set; }
    }

    public sealed class ArchivoDto
    {
        public string FileName { get; init; } = default!;
        public string ContentType { get; init; } = "application/octet-stream";
        public string FileContentsBase64 { get; set; } = default!;
    }

}