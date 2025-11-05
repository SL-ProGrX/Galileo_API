namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AjustesInstitucionAseccssDto
    {
        public int? codInstitucion { get; set; }
        public string? up { get; set; }
        public string? ut { get; set; }
        public string? ct { get; set; }
    }

    public class AjustesInstitucionDto
    {
        public int codInstitucion { get; set; }
        public string? codDepartamento { get; set; }
        public string? codSeccion { get; set; }
    }

    public class AfAjustePersonaDetalle
    {
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? provincia { get; set; }
        public string? canton { get; set; }
        public string? distrito { get; set; }
        public string? direccion { get; set; }
        public DateTime? fecha_nac { get; set; }
        public string? sexo { get; set; }
        public string? estadocivil { get; set; }
        public int? hijos { get; set; }
        public string? estadolaboral { get; set; }
        public DateTime? fechaingreso { get; set; }
        public string? estadoactual { get; set; }
        public string? af_email { get; set; }
        public string? notas { get; set; }
        public bool? ind_liquidacion { get; set; }
        public string? cod_banco { get; set; }
        public string? cuenta_ahorros { get; set; }
        public string? cod_departamento { get; set; }
        public int cod_institucion { get; set; }
        public string? cod_seccion { get; set; }
        public string? estado_persona_desc { get; set; }
        public string? estado_persona { get; set; }
        public string? descinst { get; set; }
        public string? descdept { get; set; }
        public string? centrodesc { get; set; }
        public string? descsec { get; set; }
        public string? tipoiddesc { get; set; }
        public int tipo_id { get; set; }
    }

    public class AFAjuste
    {
        public string cedula { get; set; } = string.Empty;
        public int nuevo_tipo_id { get; set; }
        public string nuevo_estado { get; set; } = string.Empty;
        public int cod_institucion { get; set; }
        public string cod_departamento { get; set; } = string.Empty;
        public string cod_seccion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}