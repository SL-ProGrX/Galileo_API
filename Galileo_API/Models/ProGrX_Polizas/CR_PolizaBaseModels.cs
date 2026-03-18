namespace Galileo_API.Models.ProGrX_Polizas
{
    public abstract class PolizaBeneficiariosB1B6Base
    {
        public int? b1_tipo_id { get; set; }
        public string b1_cedula { get; set; } = string.Empty;
        public string b1_nombre { get; set; } = string.Empty;
        public string b1_parentesco { get; set; } = string.Empty;
        public decimal? b1_porcentaje { get; set; }

        public int? b2_tipo_id { get; set; }
        public string b2_cedula { get; set; } = string.Empty;
        public string b2_nombre { get; set; } = string.Empty;
        public string b2_parentesco { get; set; } = string.Empty;
        public decimal? b2_porcentaje { get; set; }

        public int? b3_tipo_id { get; set; }
        public string b3_cedula { get; set; } = string.Empty;
        public string b3_nombre { get; set; } = string.Empty;
        public string b3_parentesco { get; set; } = string.Empty;
        public decimal? b3_porcentaje { get; set; }

        public int? b4_tipo_id { get; set; }
        public string b4_cedula { get; set; } = string.Empty;
        public string b4_nombre { get; set; } = string.Empty;
        public string b4_parentesco { get; set; } = string.Empty;
        public decimal? b4_porcentaje { get; set; }

        public int? b5_tipo_id { get; set; }
        public string b5_cedula { get; set; } = string.Empty;
        public string b5_nombre { get; set; } = string.Empty;
        public string b5_parentesco { get; set; } = string.Empty;
        public decimal? b5_porcentaje { get; set; }

        public int? b6_tipo_id { get; set; }
        public string b6_cedula { get; set; } = string.Empty;
        public string b6_nombre { get; set; } = string.Empty;
        public string b6_parentesco { get; set; } = string.Empty;
        public decimal? b6_porcentaje { get; set; }
    }

    public abstract class PolizaRecepcionRowBase
    {
        public short numero_linea { get; set; }
        public string documento { get; set; } = string.Empty;
        public int fecha_proceso { get; set; }
        public int cod_operadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public int cod_contrato { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal fondos { get; set; }
        public short cod_institucion { get; set; }
        public short existe_persona { get; set; }
        public short existe_contrato { get; set; }
        public short procesado { get; set; }
    }

    
}
