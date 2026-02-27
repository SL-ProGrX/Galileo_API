namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXContabilidadData
    {
        public int cod_contabilidad { get; set; } = 0;
        public string nombre { get; set; } = string.Empty;
        public string cedula_juridica { get; set; } = string.Empty;
        public string contacto { get; set; } = string.Empty;

        public int? nivel1 { get; set; }
        public int? nivel2 { get; set; }
        public int? nivel3 { get; set; }
        public int? nivel4 { get; set; }
        public int? nivel5 { get; set; }
        public int? nivel6 { get; set; }
        public int? nivel7 { get; set; }
        public int? nivel8 { get; set; }

        public string tel_central { get; set; } = string.Empty;
        public string tel_fax { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;

        public bool? expareas { get; set; }
        public bool? expasientos { get; set; }
        public bool? expcuentas { get; set; }
        public bool? expdiferidos { get; set; }
        public bool? expmantenimiento { get; set; }
        public bool? expplanfijo { get; set; }
        public bool? expplanrate { get; set; }
        public bool? exppresupuesto { get; set; }

        public bool? filtra_ctas_bancos { get; set; }
        public bool? filtra_ctas_contabilidad { get; set; }
        public bool? filtra_ctas_inversiones { get; set; }
        public bool? filtra_ctas_operaciones { get; set; }
        public bool? filtra_ctas_rrhh { get; set; }

        public string razonsocial { get; set; } = string.Empty;
        public string razonsocial_desc { get; set; } = string.Empty;

        public bool consolida_ind { get; set; } = false;
        public long? contabase_id { get; set; }
        public string? contabase_desc { get; set; }
        public string? unidad_id { get; set; }
        public string? unidad_desc { get; set; }

        public bool crear_predeterminados { get; set; } = false;
    }

    public class DropDownConsolidaListaData
    {
        public object? idx { get; set; }
        public string itmx { get; set; } = string.Empty;
    }
}
