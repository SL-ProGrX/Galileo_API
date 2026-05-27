namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class FrmCntXConCierreData
    {
        public int Cod_Consolida { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
    public class FrmCntXConCierrePortalData
    {
        public int Cod_Portal { get; set; }
        public int Cod_Contabilidad { get; set; }
        public string Por_User { get; set; } = string.Empty;
        public string Por_Password { get; set; } = string.Empty;
        public string Por_Server { get; set; } = string.Empty;
        public string Por_Database { get; set; } = string.Empty;
    }

    public class FrmCntXConCierrePortalLista
    {
        public int Total { get; set; }
        public List<FrmCntXConCierrePortalData> Lista { get; set; } = [];
    }

    public class FrmCntXConCierreLista
    {
        public int Total { get; set; }
        public List<FrmCntXConCierreData> Lista { get; set; } = [];
    }

    public class FrmCntXConCierreDefinicionData
    {
        public int Cod_Contabilidad { get; set; }
        public string Nivel { get; set; } = string.Empty;
    }

    public class FrmCntXConCierreDefinicionLista
    {
        public int Total { get; set; }
        public List<FrmCntXConCierreDefinicionData> Lista { get; set; } = [];
    }
    public class FrmCntXConCierreValidaPeriodoRequest
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public int CodContabilidad { get; set; }
        public bool SoloAbierto { get; set; }
    }

    public class FrmCntXConCierreCuentaMovData
    {
        public string Cod_Cuenta { get; set; } = string.Empty;
        public decimal? SI { get; set; }
        public decimal? TD { get; set; }
        public decimal? TC { get; set; }
    }
    public class FrmCntXConCierreCuentaMovLista
    {
        public int Total { get; set; }
        public List<FrmCntXConCierreCuentaMovData> Lista { get; set; } = [];
    }
    public class FrmCntXConCierreActualizarMovimientoRequest : FrmCntXConCierreCuentaMovData
    {
        public int? Cod_Consolida { get; set; }
        public int? Cod_Contabilidad { get; set; }
        public int? Anio { get; set; }
        public int? Mes { get; set; }
    }

    public class FrmCntXConCierreContabilidadPortalData
    {
        public int Cod_Contabilidad { get; set; }
    }
    public class FrmCntXConCierreContabilidadPortalLista
    {
        public int Total { get; set; }
        public List<FrmCntXConCierreContabilidadPortalData> Lista { get; set; } = [];
    }
    public class FrmCntXConCierreExisteMovimientoRequest
    {
        public int? CodConsolida { get; set; }
        public int? CodContabilidad { get; set; }
        public int? Anio { get; set; }
        public int? Mes { get; set; }
        public string? CodCuenta { get; set; } = string.Empty;
    }
    public class FrmCntXConCierreInsertarMovimientoRequest : FrmCntXConCierreCuentaMovData
    {
        public int? CodConsolida { get; set; }
        public int? CodContabilidad { get; set; }
        public int? Anio { get; set; }
        public int? Mes { get; set; }
    }
}
