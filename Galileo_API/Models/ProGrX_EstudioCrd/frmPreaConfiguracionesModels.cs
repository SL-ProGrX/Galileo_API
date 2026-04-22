namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class CrPreaConfiguracionesComiteMaxListaResult
    {
        public int total { get; set; }
        public List<CrPreaConfiguracionesComiteMaxListaData> lista { get; set; } = new();
    }
    public class CrPreaConfiguracionesComiteMaxListaData
    {
        public int id_comite { get; set; }
        public string comite { get; set; } = string.Empty;
        public decimal monto_max_ahorro { get; set; }
        public decimal monto_max_pagare { get; set; }
        public decimal monto_max_hipotecario { get; set; }
        public decimal monto_max_prendario { get; set; }
        public decimal monto_max_fiduciario { get; set; }
    }
    public class CrPreaConfiguracionesComiteMaxGuardarRequest
    {
        public int? id_comite { get; set; }
        public decimal? monto_max_ahorro { get; set; }
        public decimal? monto_max_pagare { get; set; }
        public decimal? monto_max_hipotecario { get; set; }
        public decimal? monto_max_prendario { get; set; }
        public decimal? monto_max_fiduciario { get; set; }
    }

    public class CrPreaConfiguracionesComiteLineasListaResult
    {
        public int total { get; set; }
        public List<CrPreaConfiguracionesComiteLineasListaData> lista { get; set; } = new();
    }
    public class CrPreaConfiguracionesComiteLineasListaData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool ind_monto_max { get; set; }
    }
    public class CrPreaConfiguracionesComiteLineasGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public bool? ind_monto_max { get; set; }
    }

    public class CrPreaConfiguracionesComiteAdjuntosListaResult
    {
        public int total { get; set; }
        public List<CrPreaConfiguracionesComiteAdjuntosListaData> lista { get; set; } = new();
    }
    public class CrPreaConfiguracionesComiteAdjuntosListaData
    {
        public int id_comite { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool adjunto_obligatorio { get; set; }
    }
    public class CrPreaConfiguracionesComiteAdjuntosGuardarRequest
    {
        public int? id_comite { get; set; }
        public bool? adjunto_obligatorio { get; set; }
    }

    public class CrPreaConfiguracionesGarantiaLiquidezListaResult
    {
        public int total { get; set; }
        public List<CrPreaConfiguracionesGarantiaLiquidezListaData> lista { get; set; } = new();
    }
    public class CrPreaConfiguracionesGarantiaLiquidezListaData
    {
        public string garantia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto_liquidez_minima { get; set; }
    }
    public class CrPreaConfiguracionesGarantiaLiquidezGuardarRequest
    {
        public string garantia { get; set; } = string.Empty;
        public decimal? monto_liquidez_minima { get; set; }
    }

    public class CrPreaConfiguracionesGarantiaRefundeListaResult
    {
        public int total { get; set; }
        public List<CrPreaConfiguracionesGarantiaRefundeListaData> lista { get; set; } = new();
    }
    public class CrPreaConfiguracionesGarantiaRefundeListaData
    {
        public string garantia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool refunde_ahorro { get; set; }
        public bool refunde_prendario { get; set; }
        public bool refunde_hipotecario { get; set; }
        public bool refunde_fiduciario { get; set; }
        public bool refunde_pagare { get; set; }
        public bool refunde_excedente { get; set; }
    }
    public class CrPreaConfiguracionesGarantiaRefundeGuardarRequest
    {
        public string garantia { get; set; } = string.Empty;
        public bool? refunde_ahorro { get; set; }
        public bool? refunde_prendario { get; set; }
        public bool? refunde_hipotecario { get; set; }
        public bool? refunde_fiduciario { get; set; }
        public bool? refunde_pagare { get; set; }
        public bool? refunde_excedente { get; set; }
    }

    public class CrPreaConfiguracionesCambioEstadoListaResult
    {
        public int total { get; set; }
        public List<CrPreaConfiguracionesCambioEstadoListaData> lista { get; set; } = new();
    }
    public class CrPreaConfiguracionesCambioEstadoListaData
    {
        public int id_motivo { get; set; }
        public string motivo { get; set; } = string.Empty;
        public bool estado { get; set; }
        public DateTime? fec_registro { get; set; }
        public string usu_registro { get; set; } = string.Empty;
        public DateTime? fec_modifica { get; set; }
        public string usu_modifica { get; set; } = string.Empty;
        public bool isNew { get; set; }
    }
    public class CrPreaConfiguracionesCambioEstadoGuardarRequest
    {
        public int? id_motivo { get; set; }
        public string motivo { get; set; } = string.Empty;
        public bool? estado { get; set; }
    }

    public class CrPreaConfiguracionesEdadPensionListaResult
    {
        public int total { get; set; }
        public List<CrPreaConfiguracionesEdadPensionListaData> lista { get; set; } = new();
    }
    public class CrPreaConfiguracionesEdadPensionListaData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool ind_edad_pension { get; set; }
        public bool ind_edad_pension_for { get; set; }
        public string garantias { get; set; } = string.Empty;
        public string comites { get; set; } = string.Empty;
    }
    public class CrPreaConfiguracionesEdadPensionGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string garantias { get; set; } = string.Empty;
        public string comites { get; set; } = string.Empty;
        public bool? ind_edad_pension { get; set; }
        public bool? ind_edad_pension_for { get; set; }
    }

    public class CrPreaConfiguracionesLineaDropdownDto
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}