namespace Galileo.Models.ProGrX.Clientes
{
    public class AfRemesasLiquidacionesLista
    {
        public int total { get; set; }
        public List<AfRemesaLiquidacionDto>? lista { get; set; }
    }

    public class AfRemesaLiquidacionDto
    {
        public int cod_remesa { get; set; }
        public DateTime fecha { get; set; }
        public string? usuario { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string? notas { get; set; }
        public string? estado { get; set; }
        public DateTime? microfilm_fecha { get; set; }
        public string? microfilm_usuario { get; set; }
    }

    public class AfRemesasLiquiCargaDatos
    {
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public List<DropDownListaGenericaModel>? cboOficinas { get; set; }
    }

    public class AfRemesasLiquiCargaLista
    {
        public int consec { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public DateTime fecLiq { get; set; }
    }
}