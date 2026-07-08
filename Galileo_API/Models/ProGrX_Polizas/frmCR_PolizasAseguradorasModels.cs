namespace Galileo_API.Models.ProGrX_Polizas
{

    public class PolizaAseguradoraDto
    {
        public string cod_aseguradora { get; set; } = "";
        public string nombre { get; set; } = "";
        public string cedula_juridica { get; set; } = "";

        public string? telefono_01 { get; set; }
        public string? telefono_02 { get; set; }
        public string? tel_fax { get; set; }

        public string? sitio_web { get; set; }
        public string? email_01 { get; set; }
        public string? email_02 { get; set; }
        public string? apto_postal { get; set; }

        public string? direccion { get; set; }
        public string? provincia { get; set; }
        public string? canton { get; set; }
        public string? distrito { get; set; }

        public string? nombre_contacto { get; set; }

        public bool? activo { get; set; }

        public string? codigo_retencion { get; set; }
        public string? retencion_desc { get; set; }
        public string? formato_tramas { get; set; }

        public string? cod_cuenta { get; set; }
        public string? cuenta_desc { get; set; }

        public string? cod_cuenta_comision { get; set; }
        public string? cuenta_comision_desc { get; set; }

        public string? proveedor_desc { get; set; }
        public int? cod_proveedor { get; set; }
        public int? cod_banco { get; set; }
        public string? cod_banco_desc { get; set; }
    }

    public class CuentaBancariaDto
    {
        public string cuenta { get; set; } = "";
        public string cod_banco { get; set; } = "";
        public string banco { get; set; } = "";
        public string tipo { get; set; } = "";
        public string divisa { get; set; } = "";
        public string interbanca { get; set; } = "";
        public string destino { get; set; } = "";
        public string estado { get; set; } = "";
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
    }


    public class ProvinciaaseguradoraDto
    {
        public string provincia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CantonaseguradoraDto
    {
        public string provincia { get; set; } = string.Empty;
        public string canton { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class DistritoaseguradoraDto
    {
        public string provincia { get; set; } = string.Empty;
        public string canton { get; set; } = string.Empty;
        public string distrito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

}

