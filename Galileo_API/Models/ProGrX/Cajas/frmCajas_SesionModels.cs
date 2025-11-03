namespace PgxAPI.Models.ProGrX.Cajas
{


    public class CajaSesionDTO
    {
        public int SesionId { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; }
    }

    public class CajasSesionFinalizaRequestDTO
    {
        public int SesionId { get; set; }
        public string Usuario { get; set; }
    }

    public sealed class CajasSesionDTO
    {
        public int id_sesion { get; set; }
        public string cod_usuario { get; set; }
        public string estado { get; set; }
        public string identificacion { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_finalizacion { get; set; }
        public string cod_caja { get; set; }
        public int cod_apertura { get; set; }

    }

    public sealed class CajasSesionMovimientosDTO
    {
        public string tipo_documento { get; set; }
        public string cod_transaccion { get; set; }
        public string cod_concepto { get; set; }
        public int cod_apertura { get; set; }
        public string cod_caja { get; set; }
        public string cliente_identificacion { get; set; }
        public string cliente_nombre { get; set; }
        public decimal monto { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; }
        public string documento_desc { get; set; }
        public string concepto_desc { get; set; }
        public string referencia_01 { get; set; }
        public string referencia_02 { get; set; }
        public string referencia_03 { get; set; }
    }


}
