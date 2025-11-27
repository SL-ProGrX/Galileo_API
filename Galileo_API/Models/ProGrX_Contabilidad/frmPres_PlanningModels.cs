namespace Galileo.Models.PRES
{
    public class PresVistaPresupuestoData
    {
        public long cod_cuenta { get; set; }
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal real_mes { get; set; }
        public decimal mensual { get; set; }
        public decimal diferencia_mes { get; set; }
        public decimal real_acumulado { get; set; }
        public decimal acumulado { get; set; }
        public decimal diferencia_acumulada { get; set; }
        public decimal pres_total { get; set; }
        public decimal diferencia_total { get; set; }
        public decimal ejecutado_mes { get; set; }
        public decimal ejecutado_acumulado { get; set; }
        public decimal ejecutado_total { get; set; }
        public bool acepta_movimientos { get; set; }
    }

    public class PreVistaPresupuestoCuentaData
    {
        public long cod_cuenta { get; set; }
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal real_mes { get; set; }
        public decimal mensual { get; set; }
        public decimal diferencia_mes { get; set; }
        public decimal real_acumulado { get; set; }
        public decimal acumulado { get; set; }
        public decimal diferencia_acumulada { get; set; }
        public decimal pres_total { get; set; }
        public decimal diferencia_total { get; set; }
        public decimal ejecutado_mes { get; set; }
        public decimal ejecutado_acumulado { get; set; }
        public decimal ejecutado_total { get; set; }
        public bool acepta_movimientos { get; set; }
        public int anio { get; set; }
        public int mes { get; set; }
        public DateTime periodo { get; set; }
        public decimal pre_mensual_inicial { get; set; }
        public decimal ajuste_positivo { get; set; }
        public decimal ajuste_negativo { get; set; }

        //ajustes
        public int linea { get; set; }
        public string? cod_modelo { get; set; }
        public int cod_contabilidad { get; set; }
        public string? cod_ajuste { get; set; }
        public decimal acumulado_inicial { get; set; }
        public decimal mensual_inicial { get; set; }
        public decimal ajuste_monto { get; set; }
        public decimal mensual_final { get; set; }
        public decimal acumulado_final { get; set; }
        public string? notas { get; set; }
        public DateTime registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public decimal descarga_monto { get; set; }
        public decimal decarga_linea { get; set; }
        public decimal inicial { get; set; }
        public decimal final { get; set; }
    }

    public class PresVistaPresCuentaRealHistoricoData
    {
        public DateTime periodo { get; set; }
        public decimal neto_mes { get; set; }
        public decimal saldo_final { get; set; }
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
    }

    public class PresVistaPresupuestoBuscar
    {
        public long cod_conta { get; set; }
        public string cod_modelo { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string centro_costo { get; set; } = string.Empty;
        public int anio { get; set; }
        public int mes { get; set; }
        public string tipo_vista { get; set; } = string.Empty;
        public bool ctaMov { get; set; }
        public bool traReal  { get; set; }
        public string? cuenta { get; set; } = string.Empty;
        public string? periodo { get; set; } = string.Empty;
    }

    public class PresVistaPresupuestoCuentaBuscar
    {
        public long cod_conta { get; set; }
        public string cod_modelo { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string centro_costo { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string tipo_vista { get; set; } = string.Empty;
        public string? periodo { get; set; }
    }

    public class PresPresCuentaRealBuscar
    {
        public long cod_conta { get; set; }
        public string cod_modelo { get; set; } = string.Empty;
        public int mes { get; set; }
        public string cod_unidad { get; set; } = string.Empty;
        public string centro_costo { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string tipo_vista { get; set; } = string.Empty;
    }

    public class PresAjustesGuarda
    {
        public long? cod_conta { get; set; }
        public string cod_modelo { get; set; } = string.Empty;
        public int? anio { get; set; }
        public int? mes { get; set; }
        public string cuenta { get; set; } = string.Empty;
        public decimal? mensual_nuevo { get; set; }
        public decimal? mnt_ajuste { get; set; }
        public string cod_unidad { get; set; } = string.Empty;
        public string centro_costo { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string ajuste_id { get; set; } = string.Empty;
    }

    public class PresTiposAjustes
    {
        public int cod_ajuste { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int ajuste_libre_positivo { get; set; }
        public int ajuste_libre_negativo { get; set; }
        public bool activo { get; set; }
    }

    public class PresModelisLista
    {
        public string idX { get; set; } = string.Empty;
        public string itmX { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public int inicio_anio { get; set; } = 0;
    }

    public class PresCargaMasivaModel
    {
        public string cuenta { get; set; } = string.Empty;
        public string nombre_cuenta { get; set; } = string.Empty;
        public string unidad { get; set; } = string.Empty;
        public string? nombre_unidad { get; set; } = string.Empty;
        public string cc { get; set; } = string.Empty;
        public string? nombre_cc { get; set; } = string.Empty;
        public string movimiento { get; set; } = string.Empty;
        public string? nombre_movimiento { get; set; } = string.Empty;
        public string? divisa { get; set; } = string.Empty;
        public decimal? tipoCambio { get; set; } = 0;
        public decimal valor { get; set; } = 0;
    }
}