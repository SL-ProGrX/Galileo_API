namespace Galileo_API.Models.ProGrX.Convenios
{
    public class CrConveniosRebajosInicialData
    {
        public DateTime fecha_servidor { get; set; }
        public DateTime cobro_inicio_min { get; set; }
        public DateTime cobro_inicio_max { get; set; }
    }

    public class CrConveniosRebajosCargoData
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrConveniosRebajosTransaccionBusquedaData
    {
        public int id_transac { get; set; } = 0;
        public string documento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public DateTime? registro_fecha { get; set; }
    }

    public class CrConveniosRebajosData
    {
        public int id_transac { get; set; } = 0;

        public string cod_convenio { get; set; } = string.Empty;

        public string convenio_desc { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnoreAttribute]
        public string ConvenioDesc
        {
            get => convenio_desc;
            set => convenio_desc = value ?? string.Empty;
        }

        public string cod_cargo { get; set; } = string.Empty;

        public string cargo_desc { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnoreAttribute]
        public string CargoDesc
        {
            get => cargo_desc;
            set => cargo_desc = value ?? string.Empty;
        }

        public string documento { get; set; } = string.Empty;

        public string detalle { get; set; } = string.Empty;

        public string notas { get; set; } = string.Empty;

        public decimal monto { get; set; } = 0;

        public decimal saldo { get; set; } = 0;

        public decimal recaudado { get; set; } = 0;

        public DateTime? cobro_inicio_fecha { get; set; }

        public int cobro_fracciones { get; set; } = 0;

        public int activo { get; set; } = 0;

        public string registro_usuario { get; set; } = string.Empty;

        public DateTime? registro_fecha { get; set; }

        public int numero_pagos { get; set; } = 0;
    }

    public class CrConveniosRebajosGuardarRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public int id_transac { get; set; } = 0;
        public int cobro_fracciones { get; set; } = 0;
        public string cod_cargo { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public DateTime? cobro_inicio_fecha { get; set; }
        public string documento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public int activo { get; set; } = 0;
    }

    public class CrConveniosRebajosEliminarRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public int id_transac { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrConveniosRebajosGuardarResult
    {
        public int transaccion_id { get; set; } = 0;
    }

    public class CrConveniosRebajosCobroData
    {
        public int id_transac { get; set; } = 0;
        public string cod_cargo { get; set; } = string.Empty;
        public string cargo_desc { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public decimal recaudado { get; set; } = 0;
        public DateTime? cobro_inicio_fecha { get; set; }
        public int cobro_fracciones { get; set; } = 0;
        public int activo { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
    }

    public class CrConveniosRebajosPagoData
    {
        public int id_fraccion { get; set; } = 0;
        public string cod_factura { get; set; } = string.Empty;
        public DateTime? fecha_corte { get; set; }
        public decimal monto { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
    }
}