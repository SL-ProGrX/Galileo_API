using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PdfSharp.Pdf.Filters;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesAutorizacionDb
    {
        private readonly VerificadorCoreFactory _factory;
        private readonly MTesoreria _mTesoreria;
        private readonly PortalDB _portalDB;

        public FrmTesAutorizacionDb(IConfiguration config)
        {
            _mTesoreria = new MTesoreria(config);
            _portalDB = new PortalDB(config);
            _factory = new VerificadorCoreFactory(config);
        }

        /// <summary>
        /// Obtener solicitudes pendientes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesSolicitudesLista> TES_SolicitudesPendientes_Obtener(int CodEmpresa, string filtros)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var filtro = DbHelper.DeserializeOrNew<TesAutorizacionFiltros>(filtros);
                var response = new TesSolicitudesLista
                {
                    solicitudes = new List<Galileo.Models.TES.TesSolicitudesData>(),
                    total = 0
                };

                // Rango de fecha (inicio del día / fin del día)
                var fechaInicio = filtro.fecha_inicio.Date;
                var fechaCorte = filtro.fecha_corte.Date.AddDays(1).AddTicks(-1);
                // 1) Ajustar rangos de montos por usuario (si existen)
                AjustarRangosPorUsuario(conn, filtro);

                // 2) Supervisión banco e interbancaria
                var lenInter = GetInterbancariaLength(conn, filtro.id_banco);

                // 3) Revisión automática (SP)
                EjecutarRevisionAutomatica(conn, filtro.id_banco);

                // 4) Conteo total
                response!.total = GetConteoPendientes(conn, filtro.id_banco, filtro.tipo_doc);

                // 5) Construcción de query dinámica
                var baseQuery = FrmTesAutorizacionSql.SP_TRANSACCIONES_PENDIENTES;
                var (query, sqlParams) = BuildFinalQueryAndParams(
                    conn, baseQuery, filtro, fechaInicio, fechaCorte, lenInter);

                // 6) Ejecución
                response.solicitudes = conn
                    .Query<Galileo.Models.TES.TesSolicitudesData>(query, sqlParams)
                    .ToList();

                if (filtro.tipo_doc == "TS" && filtro.activaCuentaSinpe)
                {
                    foreach (var solicitud in response.solicitudes)
                    {
                        var valida = _factory.CrearServicio(CodEmpresa, filtro.usuario)
                           .fxValidacionSinpe(CodEmpresa, solicitud.nsolicitud.ToString(), filtro.usuario);
                        if (valida.Code != 0 && valida.Code != 1)
                        {
                            solicitud.bloqueo = true;
                            solicitud.detalle = valida.Description;
                        }
                    }

                }

                return response;
            });
        }

        private static void AjustarRangosPorUsuario(SqlConnection conn, TesAutorizacionFiltros filtro)
        {
            const string sql = FrmTesAutorizacionSql.SQL_TES_AUTORIZACIONES_RANGOS;

            var r = conn.Query<TesAutorizacionData>(sql, new { filtro.usuario }).FirstOrDefault();
            if (r != null)
            {
                filtro.monto_inicio = r.rango_gen_inicio ?? 0;
                filtro.monto_fin = r.rango_gen_corte ?? 0;
            }
        }

        private static int GetInterbancariaLength(SqlConnection conn, int idBanco)
        {
            return conn.Query<int?>(FrmTesAutorizacionSql.Query_Interbancaria, new { Banco = idBanco }).FirstOrDefault() ?? 0;
        }

        private static void EjecutarRevisionAutomatica(SqlConnection conn, int idBanco)
        {
            const string sql = "EXEC spTes_Cuentas_Revision_Automatica @Banco";
            conn.Execute(sql, new { Banco = idBanco });
        }

        private static int GetConteoPendientes(SqlConnection conn, int idBanco, string tipoDoc)
        {
            return conn.Query<int>(FrmTesAutorizacionSql.Query_ConteoPendientes, new { Banco = idBanco, TipoDoc = tipoDoc }).FirstOrDefault();
        }

        private static (string sql, DynamicParameters param) BuildFinalQueryAndParams(
 SqlConnection conn,
 string baseQuery,
 TesAutorizacionFiltros filtro,
 DateTime fechaInicio,
 DateTime fechaCorte,
 int lenInterbancaria)
        {
            var sb = new StringBuilder(baseQuery);
            var p = BuildBaseParams(filtro, fechaInicio, fechaCorte);


            if (EsTransferencia(filtro.tipo_doc))
            {
                AppendCuentaTipoFilter(sb, filtro.tipo_cuenta, lenInterbancaria);
                AppendMismoBancoFilter(conn, sb, filtro.mismo_banco, filtro.id_banco, lenInterbancaria);
            }

            //AppendAutorizacionFilter(sb, filtro);
            //AppendDetalleFilter(sb, filtro.detalle);
            //AppendAppFilter(sb, filtro.appid);

            //sb.Append(" ORDER BY T.nsolicitud ASC, T.fecha_solicitud ASC");

            //return (sb.ToString(), p);
            return (null, null);
        }


        private static DynamicParameters BuildBaseParams(TesAutorizacionFiltros f, DateTime ini, DateTime fin)
        {
            var p = new DynamicParameters();
            p.Add("Banco", f.id_banco);
            p.Add("TipoDoc", f.tipo_doc);
            p.Add("Usuario", f.usuario);
            p.Add("FechaInicio", ini);
            p.Add("FechaFin", fin);
            p.Add("SolicitudInicio", f.solicitud_inicio);
            p.Add("SolicitudCorte", f.solicitud_corte);
            p.Add("MontoInicio", f.monto_inicio);
            p.Add("MontoFin", f.monto_fin);
            p.Add("Token", f.token);
            p.Add("Detalle", $"%{f.detalle}%");
            p.Add("CodigoApp", $"%{f.appid}%");
            p.Add("Duplicados", f.duplicados ? 1 : 0);
            p.Add("TodasFechas", f.todas_fechas ? 1 : 0);
            p.Add("TodasSolicitudes", f.todas_solicitudes ? 1 : 0);
            p.Add("IncluirBloqueados", f.casos_bloqueados ? 1 : 0);
            return p;
        }

        private static bool EsTransferencia(string? tipoDoc)
         => string.Equals(tipoDoc, "TE", StringComparison.OrdinalIgnoreCase);

        private static void AppendCuentaTipoFilter(StringBuilder sb, string? tipoCuenta, int lenInter)
        {
            if (string.IsNullOrWhiteSpace(tipoCuenta)) return;

            switch (tipoCuenta.ToUpperInvariant())
            {
                case "L": // Locales
                    sb.Append($" AND LEN(RTRIM(T.cta_Ahorros)) <> {lenInter} ");
                    break;
                case "I": // Interbancarias
                    sb.Append($" AND LEN(RTRIM(T.cta_Ahorros)) = {lenInter} ");
                    break;
                default:
                    // Todas: sin filtro
                    break;
            }
        }

        private static void AppendMismoBancoFilter(
      SqlConnection conn,
      StringBuilder sb,
      bool mismoBanco,
      int idBanco,
      int lenInter)
        {
            if (!mismoBanco) return;

            const string sqlGrupo = "SELECT dbo.fxTes_BancoSFN(@Banco) AS Codigo";
            var grupo = conn.Query<int?>(sqlGrupo, new { Banco = idBanco }).FirstOrDefault() ?? 0;

            // SUBSTRING(...,1,10) LIKE '%grupo%' y largo interbancario
            sb.Append($" AND (SUBSTRING(RTRIM(T.cta_Ahorros), 1, 10) LIKE '%{grupo}%' AND LEN(RTRIM(T.cta_Ahorros)) = {lenInter}) ");
        }



        /// <summary>
        /// Aplicar autorizaci�n de solicitudes pendientes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="clave"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo_autorizacion"></param>
        /// <param name="solicitudesLista"></param>
        /// <returns></returns>
        public ErrorDto TES_Autorizacion_Aplicar(TesAutorizaParametros nsolicitud)
        {
            return null;
        }



        /// <summary>
        /// Obtener rangos de montos de autorizaci�n de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<TesAutorizacionData> TES_AutorizacionDoc_Obtener(int CodEmpresa, string usuario)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = FrmTesAutorizacionSql.SQL_TES_AUTORIZACIONES_RANGOS;

                return conn.Query<TesAutorizacionData>(query, new { usuario }).FirstOrDefault() ?? new TesAutorizacionData();
            });
        }

        /// <summary>
        /// Obtener rango de montos de autorizaci�n de firmas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<TesFirmasAutData> TES_AutorizacionFirma_Obtener(int CodEmpresa, string usuario, int banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select firmas_autoriza_inicio,firmas_autoriza_corte from TES_BANCO_FIRMASAUT 
                    where USUARIO = @usuario and ID_BANCO = @banco and aplica_rango_autorizacion = 1";

                return conn.Query<TesFirmasAutData>(query, new { usuario, banco }).FirstOrDefault() ?? new TesFirmasAutData();
            });
        }

        /// <summary>
        /// Método para buscar y obtener los usuarios activos de la empresa especificada, con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesAccesosUsuariosLista> TES_AutorizacionBuscar_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var result = new TesAccesosUsuariosLista
                {
                    total = 0,
                    lista = new List<DropDownListaGenericaModel>()
                };
                return result;
            });

        }

    }
}