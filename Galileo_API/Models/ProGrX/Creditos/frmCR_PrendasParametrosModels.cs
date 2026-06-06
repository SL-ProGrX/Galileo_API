namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrPrendasParametrosLista<T>
    {
        public int total { get; set; }
        public List<T> lista { get; set; } = new();
    }
    public class CrPrendasCatalogoData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public bool? isNew { get; set; }
    }

    public class CrPrendasCatalogoGuardarRequest
    {
        public string tipo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? activa { get; set; }
    }

    public class CrPrendasCatalogoEliminarRequest
    {
        public string tipo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
    }
    public class CrPrendasCoberturaData
    {
        public string id_cobertura { get; set; } = string.Empty;
        public string cod_poliza { get; set; } = string.Empty;
        public string cod_cobertura { get; set; } = string.Empty;
        public string cobertura { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public bool? isNew { get; set; }
    }

    public class CrPrendasCoberturaGuardarRequest
    {
        public string id_cobertura { get; set; } = string.Empty;
        public string cod_poliza { get; set; } = string.Empty;
        public string cod_cobertura { get; set; } = string.Empty;
        public string cobertura { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? activa { get; set; }
    }

    public class CrPrendasCoberturaEliminarRequest
    {
        public string id_cobertura { get; set; } = string.Empty;
    }

    public class CrPrendasPolizaF4Data
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
    public class CrPrendasComercializaListaData
    {
        public int id_comercio { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
    }

    public class CrPrendasComercializaData
    {
        public int id_comercio { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; }
        public int tipo_id { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public int id_banco { get; set; }
        public string banco_desc { get; set; } = string.Empty;
        public string tipo_id_desc { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string cod_banco_destino { get; set; } = string.Empty;
        public string cta_iban_destino { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? actualiza_fecha { get; set; }
        public string actualiza_usuario { get; set; } = string.Empty;
    }

    public class CrPrendasComercializaGuardarRequest
    {
        public int? id_comercio { get; set; }
        public int? tipo_id { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? activa { get; set; }
        public int? id_banco { get; set; }
        public string correo { get; set; } = string.Empty;
    }

    public class CrPrendasComercializaEliminarRequest
    {
        public int? id_comercio { get; set; }
        public int? tipo_id { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? activa { get; set; }
        public int? id_banco { get; set; }
        public string correo { get; set; } = string.Empty;
    }

    public class CrPrendasComercializaF4Data
    {
        public int id_comercio { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrPrendasTipoIdData
    {
        public int tipo_id { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int largo_minimo { get; set; }
    }

    public class CrPrendasBancoData
    {
        public int id_banco { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string desc_corta { get; set; } = string.Empty;
        public string cta { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string item { get; set; } = string.Empty;
    }

    public class CrPrendasCuentaData
    {
        public string cuenta { get; set; } = string.Empty;
        public string banco { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string divisa { get; set; } = string.Empty;
        public bool interbanca { get; set; }
        public string destino { get; set; } = string.Empty;
        public bool activa { get; set; }
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
    }
    public class CrPrendasUnidadData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool peso_apl { get; set; }
        public bool capacidad_apl { get; set; }
        public bool cilindraje_apl { get; set; }
        public bool activa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public bool? isNew { get; set; }
    }

    public class CrPrendasUnidadGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? peso_apl { get; set; }
        public bool? capacidad_apl { get; set; }
        public bool? cilindraje_apl { get; set; }
        public bool? activa { get; set; }
    }

    public class CrPrendasUnidadEliminarRequest
    {
        public string codigo { get; set; } = string.Empty;
    }

    public class CrPrendasParametroSpResult
    {
        public string movimiento { get; set; } = string.Empty;
        public int pass { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

}