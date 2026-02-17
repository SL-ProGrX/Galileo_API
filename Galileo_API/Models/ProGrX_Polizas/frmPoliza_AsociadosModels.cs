namespace Galileo_API.Models.ProGrX_Polizas
{
    public sealed class PolizaAsociadoCorteSpDto
    {
        public DateTime corte { get; init; }
        public string cedula { get; init; } = "";
        public string id_alterno { get; init; } = "";
        public string apellido_1 { get; init; } = "";
        public string apellido_2 { get; init; } = "";
        public string nombre_1 { get; init; } = "";
        public string nombre_2 { get; init; } = "";
        public string email_01 { get; init; } = "";
        public string email_02 { get; init; } = "";
        public DateTime? fecha_nacimiento { get; init; }
        public string genero { get; init; } = "";
        public string nacionalidad { get; init; } = "";
        public string provincia { get; init; } = "";
        public string canton { get; init; } = "";
        public string distrito { get; init; } = "";
        public string direccion { get; init; } = "";
        public string tel_tipo { get; init; } = "";
        public string tel_numero { get; init; } = "";
        public string movimiento { get; init; } = "";
    }

    public sealed class PolizaBeneficiariosSpDto
    {
        public string cedula { get; init; } = "";
        public string nombre { get; init; } = "";

        public int b1_tipo_id { get; init; }
        public string b1_cedula { get; init; } = "";
        public string b1_nombre { get; init; } = "";
        public string b1_parentesco { get; init; } = "";
        public decimal? b1_porcentaje { get; init; }

        public int b2_tipo_id { get; init; }
        public string b2_cedula { get; init; } = "";
        public string b2_nombre { get; init; } = "";
        public string b2_parentesco { get; init; } = "";
        public decimal? b2_porcentaje { get; init; }

        public int b3_tipo_id { get; init; }
        public string b3_cedula { get; init; } = "";
        public string b3_nombre { get; init; } = "";
        public string b3_parentesco { get; init; } = "";
        public decimal? b3_porcentaje { get; init; }

        public int b4_tipo_id { get; init; }
        public string b4_cedula { get; init; } = "";
        public string b4_nombre { get; init; } = "";
        public string b4_parentesco { get; init; } = "";
        public decimal? b4_porcentaje { get; init; }

        public int b5_tipo_id { get; init; }
        public string b5_cedula { get; init; } = "";
        public string b5_nombre { get; init; } = "";
        public string b5_parentesco { get; init; } = "";
        public decimal? b5_porcentaje { get; init; }

        public int b6_tipo_id { get; init; }
        public string b6_cedula { get; init; } = "";
        public string b6_nombre { get; init; } = "";
        public string b6_parentesco { get; init; } = "";
        public decimal? b6_porcentaje { get; init; }
    }
}
