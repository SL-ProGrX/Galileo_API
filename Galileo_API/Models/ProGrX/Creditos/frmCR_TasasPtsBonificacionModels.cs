namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrTasasPtsBonificacionPlanData
    {
        public string cod_tasa_bono { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class CrTasasPtsBonificacionDefinicionData
    {
        public string cod_tasa_bono { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
    }

    public class CrTasasPtsBonificacionDefinicionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public bool editar { get; set; } = false;
        public string codigo_original { get; set; } = string.Empty;
        public CrTasasPtsBonificacionDefinicionData definicion { get; set; } = new();
    }

    public class CrTasasPtsBonificacionDefinicionEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_tasa_bono { get; set; } = string.Empty;
    }

    public class CrTasasPtsBonificacionMembresiaData
    {
        public int linea { get; set; } = 0;
        public decimal inicio { get; set; } = 0;
        public decimal corte { get; set; } = 0;
        public decimal tasa_bono { get; set; } = 0;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
    }

    public class CrTasasPtsBonificacionMembresiaGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_tasa_bono { get; set; } = string.Empty;
        public CrTasasPtsBonificacionMembresiaData membresia { get; set; } = new();
    }

    public class CrTasasPtsBonificacionLineaEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_tasa_bono { get; set; } = string.Empty;
        public int linea { get; set; } = 0;
    }

    public class CrTasasPtsBonificacionDestinoData
    {
        public int linea { get; set; } = 0;
        public string cod_destino { get; set; } = string.Empty;
        public string destino_desc { get; set; } = string.Empty;
        public decimal plazo_inicio { get; set; } = 0;
        public decimal plazo_corte { get; set; } = 0;
        public decimal tasa_bono { get; set; } = 0;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
    }

    public class CrTasasPtsBonificacionDestinoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_tasa_bono { get; set; } = string.Empty;
        public CrTasasPtsBonificacionDestinoData destino { get; set; } = new();
    }

    public class CrTasasPtsBonificacionLiquidezData
    {
        public int linea { get; set; } = 0;
        public decimal cap_inicial { get; set; } = 0;
        public decimal cap_final { get; set; } = 0;
        public decimal tasa_bono { get; set; } = 0;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
    }

    public class CrTasasPtsBonificacionLiquidezGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_tasa_bono { get; set; } = string.Empty;
        public CrTasasPtsBonificacionLiquidezData liquidez { get; set; } = new();
    }

    public class CrTasasPtsBonificacionAsignacionLineaData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public List<CrTasasPtsBonificacionAsignacionGarantiaData> garantias { get; set; } = [];
    }

    public class CrTasasPtsBonificacionAsignacionGarantiaData
    {
        public string codigo { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }

    public class CrTasasPtsBonificacionAsignacionPlanData
    {
        public string cod_tasa_bono { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }

    public class CrTasasPtsBonificacionAsignacionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_tasa_bono { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }

    public class CrTasasPtsBonificacionDestinoCatalogoData
    {
        public string cod_destino { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}
