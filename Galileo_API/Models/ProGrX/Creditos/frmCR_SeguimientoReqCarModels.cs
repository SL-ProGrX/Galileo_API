namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrSeguimientoReqCarCargaInicialRequest
    {
        public int operacion { get; set; } = 0;
        public string ventana { get; set; } = string.Empty;
    }

    public class CrSeguimientoReqCarRequisitosRequest
    {
        public int operacion { get; set; } = 0;
    }

    public class CrSeguimientoReqCarCargosRequest
    {
        public int operacion { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string tipo_consulta { get; set; } = string.Empty;
    }

    public class CrSeguimientoReqCarRequisitosGuardarRequest
    {
        public int operacion { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public List<CrSeguimientoReqCarRequisitoGuardarItem> requisitos { get; set; } = new();
    }

    public class CrSeguimientoReqCarRequisitoGuardarItem
    {
        public string cod_requisito { get; set; } = string.Empty;
        public int estado { get; set; } = 0;
        public int opcional { get; set; } = 0;
    }

    public class CrSeguimientoReqCarCargoAplicarRequest
    {
        public int operacion { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string cod_cargo { get; set; } = string.Empty;
        public bool checked_ind { get; set; } = false;
        public string tipo { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public decimal valor { get; set; } = 0;
        public string plazo_tipo { get; set; } = string.Empty;
        public int plazo_dias { get; set; } = 0;
        public bool diferido_cargo { get; set; } = false;
    }

    public class CrSeguimientoReqCarPrimaGuardarRequest
    {
        public int operacion { get; set; } = 0;
        public string cod_cargo { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrSeguimientoReqCarCargaInicialData
    {
        public int operacion { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string ventana { get; set; } = string.Empty;
        public string estado_solicitud { get; set; } = string.Empty;
        public bool editable { get; set; } = false;
    }

    public class CrSeguimientoReqCarRequisitoData
    {
        public int estado { get; set; } = 0;
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int opcional { get; set; } = 0;
    }

    public class CrSeguimientoReqCarCargoData
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string tipo { get; set; } = string.Empty;
        public string tipo_desc { get; set; } = string.Empty;
        public decimal valor { get; set; } = 0;
        public string plazo_tipo { get; set; } = string.Empty;
        public int plazo_dias { get; set; } = 0;
        public bool diferido_cargo { get; set; } = false;
        public bool checked_ind { get; set; } = false;
    }

    public class CrSeguimientoReqCarPrimaData
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
    }

    public class CrSeguimientoReqCarCargosData
    {
        public int operacion { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string tipo_consulta { get; set; } = string.Empty;
        public bool editable { get; set; } = false;
        public List<CrSeguimientoReqCarCargoData> cargos { get; set; } = new();
        public List<CrSeguimientoReqCarPrimaData> primas { get; set; } = new();
    }

    internal class CrSeguimientoReqCarOperacionData
    {
        public int operacion { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado_solicitud { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
    }
}