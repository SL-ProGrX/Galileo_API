namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrCatalogoCargoData
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool automatico { get; set; } = false;
        public bool aumenta_base_crd { get; set; } = false;
        public string base_calculo { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public decimal valor { get; set; } = 0;
        public string cod_cuenta { get; set; } = string.Empty;
        public string tipo_deduccion { get; set; } = string.Empty;
        public string plazo_tipo { get; set; } = string.Empty;
        public int plazo_dias { get; set; } = 0;
        public decimal monto_inicio { get; set; } = 0;
        public decimal monto_corte { get; set; } = 0;
        public bool diferido_cargo { get; set; } = false;
        public string diferido_cod_cuenta { get; set; } = string.Empty;
        public decimal iva_porcentaje { get; set; } = 0;
        public bool activo { get; set; } = false;
    }

    public class CrCatalogoCargoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public CrCatalogoCargoData cargo { get; set; } = new();
    }

    public class CrCatalogoCargoEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_cargo { get; set; } = string.Empty;
    }

    public class CrCatalogoCargoAsignacionData
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public decimal valor { get; set; } = 0;
        public bool existe { get; set; } = false;
    }

    public class CrCatalogoCargoArbolData
    {
        public string key { get; set; } = string.Empty;
        public string parent_key { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public bool leaf { get; set; } = false;
        public string codigo { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string nivel { get; set; } = string.Empty;
    }

    public class CrCatalogoCargoAsignacionObtenerRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
    }

    public class CrCatalogoCargoAsignacionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_cargo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public bool isChecked { get; set; } = false;
    }

    public class CrCatalogoCargoTablaAplicacionData
    {
        public int id_tabla { get; set; } = 0;
        public decimal monto_inicio { get; set; } = 0;
        public decimal monto_corte { get; set; } = 0;
        public int plazo_inicio { get; set; } = 0;
        public int plazo_corte { get; set; } = 0;
        public string tipo { get; set; } = string.Empty;
        public decimal apl_valor { get; set; } = 0;
    }

    public class CrCatalogoCargoTablaAplicacionObtenerRequest
    {
        public string cod_cargo { get; set; } = string.Empty;
    }

    public class CrCatalogoCargoTablaAplicacionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_cargo { get; set; } = string.Empty;
        public int id_tabla { get; set; } = 0;
        public decimal monto_inicio { get; set; } = 0;
        public decimal monto_corte { get; set; } = 0;
        public int plazo_inicio { get; set; } = 0;
        public int plazo_corte { get; set; } = 0;
        public string tipo { get; set; } = string.Empty;
        public decimal apl_valor { get; set; } = 0;
    }

    public class CrCatalogoCargoTablaAplicacionEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_cargo { get; set; } = string.Empty;
        public int id_tabla { get; set; } = 0;
    }
}