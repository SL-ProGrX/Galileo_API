using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAHPrincipalDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrx;
        private const string validaCedula = "La cédula es requerida.";

        public FrmAHPrincipalDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrx = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene el resumen principal de patrimonio del afiliado y valida acceso restringido.
        /// </summary>
        public ErrorDto<FrmAhPrincipalConsultaResponse?> Ah_Principal_Consulta_Obtener(
            int codEmpresa,
            string cedula,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return DbHelper.CreateErrorResponse<FrmAhPrincipalConsultaResponse?>(validaCedula);

            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<FrmAhPrincipalConsultaResponse?>("El usuario es requerido.");

            try
            {
                var cedulaNormalizada = cedula.Trim();
                var usuarioNormalizado = usuario.Trim();

                var acceso = _mProGrx.fxSys_RA_Consulta(codEmpresa, cedulaNormalizada, usuarioNormalizado);
                if (acceso.Code == -1)
                    return DbHelper.CreateErrorResponse<FrmAhPrincipalConsultaResponse?>(acceso.Description!);

                if (!acceso.Result)
                {
                    return DbHelper.CreateErrorResponse<FrmAhPrincipalConsultaResponse?>(
                        "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorización para Consultar!",
                        -2);
                }

                const string sql = @"
select
    rtrim(Cedula) as cedula,
    rtrim(Nombre) as nombre,
    isnull(Obrero, 0) as obrero,
    isnull(Patronal, 0) as patronal,
    isnull(Custodia, 0) as custodia,
    isnull(capitaliza, 0) as capitaliza,
    rtrim(isnull(cod_divisa, '')) as cod_divisa,
    isnull(Obrero, 0) + isnull(Patronal, 0) + isnull(Custodia, 0) + isnull(capitaliza, 0) as total,
    isnull(dbo.fxPAT_Info_Aporte_Manual(CEDULA), 0) as aporte_cobro,
    fecAhorro as fec_ahorro,
    fecAporte as fec_aporte,
    fecCustodia as fec_custodia,
    fecCapitaliza as fec_capitaliza
from vPAT_Consolidado
where cedula = @cedula;";

                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = conn.QueryFirstOrDefault<FrmAhPrincipalConsultaResponse>(
                    sql,
                    new { cedula = cedulaNormalizada });

                if (result == null)
                {
                    return DbHelper.CreateErrorResponse<FrmAhPrincipalConsultaResponse?>(
                        "No se localizó la persona o sus registros de aportes, verifique...",
                        -2);
                }

                return DbHelper.CreateOkResponse<FrmAhPrincipalConsultaResponse?>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmAhPrincipalConsultaResponse?>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el detalle de movimientos de patrimonio filtrado por rubros seleccionados.
        /// </summary>
        public ErrorDto<List<FrmAhPrincipalDetallePatrimonioResponse>> Ah_Principal_DetallePatrimonio_Obtener(
            int codEmpresa,
            FrmAhPrincipalDetallePatrimonioRequest request)
        {
            if (request == null)
                return DbHelper.CreateErrorResponse<List<FrmAhPrincipalDetallePatrimonioResponse>>("La solicitud es requerida.");

            if (string.IsNullOrWhiteSpace(request.cedula))
                return DbHelper.CreateErrorResponse<List<FrmAhPrincipalDetallePatrimonioResponse>>(validaCedula);

            var tipos = new List<string>();

            if (request.incluir_obrero) tipos.Add("O");
            if (request.incluir_patronal) tipos.Add("P");
            if (request.incluir_capitalizacion) tipos.Add("C");
            if (request.incluir_custodia) tipos.Add("X");

            if (tipos.Count == 0)
            {
                return DbHelper.CreateErrorResponse<List<FrmAhPrincipalDetallePatrimonioResponse>>(
                    "Debe seleccionar al menos un rubro para consultar.",
                    -2);
            }

            

            const string sql = @"
select
    A.fecha as fecha,
    case
        when isnull(A.FechaProc, 0) = 0 then ''
        else stuff(right('000000' + convert(varchar(6), A.FechaProc), 6), 5, 0, '-')
    end as fecha_proceso,
    rtrim(isnull(A.Tipo, '')) as tipo,
    case rtrim(isnull(A.Tipo, ''))
        when 'P' then 'Patronal'
        when 'O' then 'Obrero'
        when 'C' then 'Capitalización'
        when 'X' then 'Custodia'
        else ''
    end as tipo_desc,
    isnull(A.Monto, 0) as monto,
    rtrim(isnull(D.descripcion, '')) as movimiento,
    rtrim(isnull(convert(varchar(50), A.NCon), '')) as mov_numero,
    rtrim(isnull(C.descripcion, '')) as mov_concepto,
    rtrim(isnull(A.Usuario, '')) as mov_usuario,
    cast(case when rtrim(isnull(A.Tcon, '')) in ('5', '8', 'ND', 'LIQ') then 1 else 0 end as bit) as resaltar_rojo,
    rtrim(isnull(A.Tcon, '')) as tcon
from ahorro_detallado A
left join SIF_Documentos D
    on A.Tcon = D.Tipo_Documento
left join SIF_Conceptos C
    on A.cod_Concepto = C.cod_Concepto
where A.cedula = @cedula
  and A.Tipo in @tipos
order by A.fecha desc, A.consec desc;";

            var response = DbHelper.ExecuteListQuery<FrmAhPrincipalDetallePatrimonioResponse>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    cedula = request.cedula.Trim(),
                    tipos
                });

            if (response.Code == -1 || response.Result == null)
                return response;

            foreach (var item in response.Result.Where(x => string.IsNullOrWhiteSpace(x.movimiento)))
            {
                item.movimiento = MCobroDb.fxTipoComprobante(
                    item.tcon,
                    item.mov_numero
                );
            }

            return response;
        }

        /// <summary>
        /// Obtiene el histórico de excedentes del afiliado.
        /// </summary>
        public ErrorDto<List<FrmAhPrincipalExcedentesResponse>> Ah_Principal_Excedentes_Obtener(
            int codEmpresa,
            string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return DbHelper.CreateErrorResponse<List<FrmAhPrincipalExcedentesResponse>>(validaCedula);

            const string sql = @"
select
    P.Inicio as inicio,
    P.CORTE as corte,
    isnull(E.excedente_bruto, 0) as excedente_bruto,
    isnull(E.capitalizado, 0) as capitalizado,
    isnull(E.renta, 0) as renta,
    isnull(E.excedente_final, 0) as excedente_final
from exc_cierre E
inner join EXC_PERIODOS P
    on E.ID_PERIODO = P.ID_PERIODO
where E.cedula = @cedula
order by P.CORTE desc, P.Inicio desc;";

            return DbHelper.ExecuteListQuery<FrmAhPrincipalExcedentesResponse>(
                _portalDb,
                codEmpresa,
                sql,
                new { cedula = cedula.Trim() });
        }

        /// <summary>
        /// Obtiene el histórico mensual de aportes del afiliado.
        /// </summary>
        public ErrorDto<List<FrmAhPrincipalHistoricoResponse>> Ah_Principal_Historico_Obtener(
            int codEmpresa,
            string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return DbHelper.CreateErrorResponse<List<FrmAhPrincipalHistoricoResponse>>(validaCedula);

            const string sql = @"
select
    isnull(A.Anio, 0) as anio,
    isnull(A.Mes, 0) as mes,
    rtrim(isnull(A.cod_Divisa, '')) as cod_divisa,
    isnull(A.ahorro, 0) as ahorro,
    isnull(A.Aporte, 0) as aporte,
    isnull(A.Custodia, 0) as custodia,
    isnull(A.capitaliza, 0) as capitaliza,
    rtrim(isnull(E.Descripcion, A.EstadoActual)) as estado_desc
from ase_per_aportes A
left join AFI_ESTADOS_PERSONA E
    on A.estadoactual = E.cod_Estado
where A.cedula = @cedula
order by A.anio desc, A.mes desc;";

            return DbHelper.ExecuteListQuery<FrmAhPrincipalHistoricoResponse>(
                _portalDb,
                codEmpresa,
                sql,
                new { cedula = cedula.Trim() });
        }

        /// <summary>
        /// Obtiene las liquidaciones pendientes registradas para el afiliado.
        /// </summary>
        public ErrorDto<List<FrmAhPrincipalLiquidacionesResponse>> Ah_Principal_Liquidaciones_Obtener(
            int codEmpresa,
            string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return DbHelper.CreateErrorResponse<List<FrmAhPrincipalLiquidacionesResponse>>(validaCedula);

            const string sql = @"
select
    isnull(Consec, 0) as consec,
    FecLiq as fec_liq,
    isnull(Ahorro_Liq, 0) as ahorro_liq,
    isnull(Aporte_Liq, 0) as aporte_liq,
    isnull(Capitalizado_Liq, 0) as capitalizado_liq,
    isnull(Extra_Liq, 0) as extra_liq,
    rtrim(isnull(Usuario, '')) as usuario
from liquidacion
where estado = 'P'
  and cedula = @cedula
order by FecLiq desc;";

            return DbHelper.ExecuteListQuery<FrmAhPrincipalLiquidacionesResponse>(
                _portalDb,
                codEmpresa,
                sql,
                new { cedula = cedula.Trim() });
        }
    }
}
