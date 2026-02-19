namespace Galileo_API.Models.ProGrX_Contabilidad
{
        public class ArfAcreedorDto
        {
            public int cod_acreedor { get; set; }

            public string? descripcion { get; set; }

            public string? identificacion { get; set; }

            public string? telefono_01 { get; set; }

            public string? telefono_02 { get; set; }

            public string? website { get; set; }

            public string? email_01 { get; set; }

            public string? email_02 { get; set; }

            public string? apto_postal { get; set; }

            public string? direccion { get; set; }

            public string? provincia { get; set; }

            public string? canton { get; set; }

            public string? distrito { get; set; }

            public string? contacto_nombre { get; set; }

            public bool activo { get; set; }

            public int? cod_banco { get; set; }

            public string? cod_cuenta { get; set; }

            public int? cod_proveedor { get; set; }

            public int? tipo_id { get; set; }

            public string? proveedor_desc { get; set; }
            public string? cod_cuenta_mask { get; set; }
            public string? cuenta_desc { get; set; }
    }


        public class ProvinciaAcreedorDto
        {
            public string? provincia { get; set; }

            public string? descripcion { get; set; }
        }

  
        public class CantonAcreedorDto
        {
            public string? canton { get; set; }

            public string? descripcion { get; set; }
        }

 
        public class DistritoAcreedorDto
        {
            public string? provincia { get; set; }

            public string? canton { get; set; }

            public string? distrito { get; set; }

            public string? descripcion { get; set; }
        }

    public class CuentaBancariaAcreedorDto
    {
        public string? cuenta { get; set; }

        public string? banco { get; set; }

        public string? tipo { get; set; }

        public string? divisa { get; set; }

        public string? interbanca { get; set; }

        public string? destino { get; set; }

        public string? estado { get; set; }

        public DateTime? registro_fecha { get; set; }

        public string? registro_usuario { get; set; }
    }



}



