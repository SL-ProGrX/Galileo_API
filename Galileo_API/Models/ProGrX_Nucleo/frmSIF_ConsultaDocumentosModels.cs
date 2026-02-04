namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class SifConsultaDocsFormasDePagoData
    {
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string? cod_divisa { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; }
        public decimal importe_real { get; set; }
        public string? referencia { get; set; } = string.Empty;
        public string? notas { get; set; } = string.Empty;
    }
    public class SifConsultaDocSeguimientoData
    {

        public DateTime registro_fecha { get; set; }
        public string? registro_fechast { get; set; } = string.Empty;
        public string? registro_usuario { get; set; } = string.Empty;
        public DateTime traspaso_fecha { get; set; }
        public string? traspaso_fechast { get; set; } = string.Empty;
        public string? traspaso_usuario { get; set; } = string.Empty;
        public DateTime anulacion_fecha { get; set; }
        public string? anulacion_fechast { get; set; } = string.Empty;
        public string? anulacion_usuario { get; set; } = string.Empty;
    }
    public class SifConsultaDocCargaDocumentoData
    {         
        public string? tipo_documento { get; set; } = string.Empty; 
        public string? cod_transaccion { get; set; } = string.Empty;    
        public string? documentodesc { get; set; } = string.Empty;
        public string? identificacion { get; set; } = string.Empty;
        public string? nombre { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime registro_fecha { get; set; }
        public string? cod_Concepto { get; set; } = string.Empty;
        public string? registro_usuario { get; set; } = string.Empty;
        public string? concepto { get; set; } = string.Empty;
        public string? documento { get; set; } = string.Empty;
        public string? oficina { get; set; } = string.Empty;
        public string? caja { get; set; } = string.Empty;
        public string? estado { get; set; } = string.Empty;
        public string? linea1 { get; set; } = string.Empty;
        public string? linea2 { get; set; } = string.Empty;
        public string? linea3 { get; set; } = string.Empty;
        public string? linea4 { get; set; } = string.Empty;
        public string? linea5 { get; set; } = string.Empty;
        public string? linea6 { get; set; } = string.Empty;
        public string? linea7 { get; set; } = string.Empty;
        public string? linea8 { get; set; } = string.Empty;
        public string? linea9 { get; set; } = string.Empty;
        public string? linea10 { get; set; } = string.Empty;
        public string? linea11 { get; set; } = string.Empty;
        public string? detalle { get; set; } = string.Empty;
        public int bloqueo { get; set; }
        public int recibo_digital { get; set; }
    }


    public class SifConsultaDocCargaAsientoData
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string tipo_movimiento { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string cod_unidad { get; set; } = string.Empty;
        public string unidadx { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string ccx { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; }  
        public string referencia_01 { get; set; } = string.Empty;
        public string referencia_02 { get; set; } = string.Empty;
        public string referencia_03 { get; set; } = string.Empty;
        public decimal importe_real { get; set; }
        public string tipo_documento { get; set; } = string.Empty;
        public string cod_transaccion { get; set; } = string.Empty;

    }

    public class SifConsultaDocCuentasPorCobrarData
    {

        public string operacion { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public int crd_operacion { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string linea_desc { get; set; } = string.Empty;
        public string concepto_desc { get; set; } = string.Empty;
        public decimal? total { get; set; }
        public decimal? intcor { get; set; }
        public decimal? intmor { get; set; }
        public decimal? cargos { get; set; }
        public decimal? principal { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string tcon { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string documentodesc { get; set; } = string.Empty;

    }

    public class SifConsultaDocPatrimoniosData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int Cod_institucion { get; set; } 
        public string institucion { get; set; } = string.Empty; 
        public DateTime fecha { get; set; }         
        public decimal monto { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
        public string codconcepto { get; set; } = string.Empty;
        public decimal? fechaproc { get; set; }
        public string tcon { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string tipo_aporte { get; set; } = string.Empty;
        public string tipoaporteid { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public string codoficina { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public string fechaemision { get; set; } = string.Empty;
        public int idseq { get; set; }


    }
    public class SifConsultaDocFondosData
    {

        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int codInstitucion { get; set; }
        public string institucion { get; set; } = string.Empty;
        public int codoperadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public int cod_Contrato { get; set; }
        public DateTime? fecha { get; set; }
        public decimal? monto { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string codcaja { get; set; } = string.Empty;
        public string codconcepto { get; set; } = string.Empty;
        public int fechaproceso { get; set; }
        public string tcon { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public string plandesc { get; set; } = string.Empty;
        public string codoficina { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string fechaemision { get; set; } = string.Empty;
        public int? idseq { get; set; }

    }

    public class SifConsultaDocCreditosData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int cod_institucion { get; set; }
        public string institucion { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string lineaX { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public int id_solicitud { get; set; }
        public decimal? intCor { get; set; }
        public decimal intMor { get; set; } 
        public decimal cargo { get; set; }
        public decimal poliza { get; set; }
        public decimal? principal { get; set; }
        public string ncon { get; set; } = string.Empty;
        public string tcon { get; set; } = string.Empty;
        public int proceso { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string fuente { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public string cod_oficina_R { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public decimal? priDeduc { get; set; }
        public decimal? fecult { get; set; }
        public decimal? monto_Credito { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string cod_Caja { get; set; } = string.Empty;
        public string cod_Concepto { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string fecha_Emision { get; set; } = string.Empty;
        public int? id_Seq { get; set; }
        public int? ejecutivo_Id { get; set; }
        public decimal? total_Mov { get; set; }
        public string antiguedad { get; set; } = string.Empty;
        public decimal? tasa_Mov { get; set; }
        public decimal? tasa_Actual { get; set; }
        public decimal? iva { get; set; }
        public string garantia_Desc { get; set; } = string.Empty;

    }

    public class SifConsultaDocFiltros    {     
        public int? tipo_filtro { get; set; }
        public string? valor_filtro { get; set; } = string.Empty;
        public string? sesion { get; set; } = string.Empty;
        public string? valor_sesion { get; set; } = string.Empty;
        public string? lista_documentos { get; set; } = string.Empty;
        public string? lista_conceptos { get; set; } = string.Empty;
        public string? tipo_fecha { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte{ get; set; }
        public string? tipo_estado { get; set; } = string.Empty;
        public bool chk_documentos_bloqueados { get; set; }
        public string? usuario_registra { get; set; } = string.Empty;
        public string? no_transaccion { get; set; } = string.Empty;
        public string? no_documento { get; set; } = string.Empty;
        public string? referencia_01 { get; set; } = string.Empty;
        public string? referencia_02 { get; set; } = string.Empty;
        public string? referencia_03 { get; set; } = string.Empty;
        public string? caja { get; set; }
        public int? caja_apertura { get; set; }
        public string? usuarios { get; set; } = string.Empty;
        public string? cuenta { get; set; } = string.Empty;
        public string? forma_pago { get; set; } = string.Empty;
        public string? forma_pago_no_ref { get; set; } = string.Empty;
        public bool? chk_asientos_desbalanceados { get; set; }

        public string? filtro { get; set; } //filtro del buscar en tablas o buscador
        public int? pagina { get; set; } = 1;//pagina de la tabla
        public int? paginacion { get; set; } = 30; //paginacion de la tabla
        public int? sortOrder { get; set; } = 0; //0: sin orden, 1: ascendente, 2: descendente
        public string? sortField { get; set; } //campo por el cual se ordena

    }

    public class SifConsultaDocTrasaccionesData
    { 
        public string cod_transaccion { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string estado { get; set; } = string.Empty;
        public DateTime fecha_registro { get; set; }
        public string usuario { get; set; } = string.Empty; 
        public string cod_caja { get; set; } = string.Empty; 
        public int cod_apertura { get; set; }
        public int id_sesion { get; set; } 
        public string cod_oficina { get; set; } = string.Empty;
        public string cliente_identificacion { get; set; } = string.Empty;
        public string cliente_nombre { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    public class SifConsultaDocTrasaccionesDataLista
    {
        public SifConsultaDocTrasaccionesTotales totales { get; set; }
        public List<SifConsultaDocTrasaccionesData> lista { get; set; }
    }
    public class SifConsultaDocTrasaccionesTotales
    {
        public int total { get; set; }
        public decimal montototal { get; set; }
    }

}
