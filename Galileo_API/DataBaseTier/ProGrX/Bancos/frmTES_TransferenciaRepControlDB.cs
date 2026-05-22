using Dapper;
using Galileo.BusinessLogic;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesTransferenciaRepControlDB
    {
        private readonly PortalDB _portalDB;
        private readonly MTesoreria mTesoreria;
        private readonly MTesFuncionesDb mTesFunciones;

        public FrmTesTransferenciaRepControlDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            mTesoreria = new MTesoreria(config);
            mTesFunciones = new MTesFuncionesDb(config);
        }

        #region ===== Helpers (mínimos; la mayoría se movió a MTesFuncionesDb) =====

        private static ErrorDto<object> Err(string msg, int code = -1)
            => DbHelper.CreateErrorResponse<object>(msg, code, default!);

        #endregion

        #region ===== Catálogos =====

        public ErrorDto<TransferenciaRepControlCatalogoDto> TES_TransferenciaRepControl_Catalogos_Obtener(int CodEmpresa, int Banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var response = new TransferenciaRepControlCatalogoDto();

                const string query1 = "exec spTes_Formatos_Bancos @Banco";

                const string query2 = @"
select rtrim(T.tipo) as IdX, rtrim(T.descripcion) as ItmX 
from tes_banco_docs D
inner join tes_tipos_doc T on D.tipo = T.tipo 
where D.comprobante = '04' and D.id_Banco = @Banco";

                const string query3 = @"
select Bp.COD_PLAN as IdX, Bp.COD_PLAN as ItmX
from TES_BANCOS B
inner join TES_BANCO_PLANES_TE Bp on B.ID_BANCO = Bp.ID_BANCO
Where B.ID_BANCO = @Banco And B.UTILIZA_PLAN = 1
order by Bp.COD_PLAN asc";

                response.Formatos = conn.Query<DropDownCatalogoBancos>(query1, new { Banco }).ToList();
                response.Tipos = conn.Query<DropDownCatalogoBancos>(query2, new { Banco }).ToList();
                response.Planes = conn.Query<DropDownCatalogoBancos>(query3, new { Banco }).ToList();

                if (response.Planes == null || response.Planes.Count == 0)
                {
                    response.Planes = new List<DropDownCatalogoBancos>
                    {
                        new DropDownCatalogoBancos { idx = "-sp-", itmx = "Sin Plan" }
                    };
                }

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TransferenciaRepControlCatalogoDto>(ex.Message);
            }
        }

        #endregion

        #region ===== Generación Archivo =====

        public ErrorDto<object> TES_TransferenciaRepControl_Archivo_Generar(
            int CodEmpresa,
            int Banco,
            int NTransac,
            string TipoDoc,
            string Formato,
            string Plan)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string queryTransac = @"
Select *
From Tes_Transacciones
Where Estado = 'T'
  And Tipo = @TipoDoc 
  And ID_Banco= @Banco
  And Autoriza='S'
  And documento_base = @NTransac
Order by Nsolicitud";

                var parametros = new { Banco, TipoDoc, NTransac };

                List<TesTransaccionDto> LoadTransacciones()
                    => conn.Query<TesTransaccionDto>(queryTransac, parametros).ToList();

                TesEmisionDocFiltros filtro = new()
                {
                    tipoDoc = TipoDoc,
                    usuario = "sinpe"
                };



                return Formato switch
                {
                    "A" => // Banco Nacional
                        ProcesarFormatoA(CodEmpresa, Banco, TipoDoc, NTransac, conn, parametros, LoadTransacciones()),
                    "B" => // Banco Popular
                        MTesFuncionesDb.SbTeBancoPopularCore(
                            codEmpresa: CodEmpresa,
                            bancoId: Banco,
                            tipoDoc: TipoDoc,
                            transaccionesList: LoadTransacciones(),
                            resolveConsecutivo: () => NTransac),
                    "C" => // BCR Planilla
                        ProcesarFormatoC(CodEmpresa, Banco, TipoDoc, NTransac, conn, parametros, LoadTransacciones()),

                    "D" => MTesFuncionesDb.SbTeBcrEmpresarialCore(
                            conn,
                            CodEmpresa,
                            Banco,
                            TipoDoc,
                            100000,
                            null,
                            null,
                            null,
                            null,
                            resolveConsecutivo: () => NTransac),

                    "E" => sbTeBCT_Enlace(CodEmpresa, Banco, TipoDoc, NTransac),

                    "F" => mTesFunciones.SbTeBcrComercial(
                        conn,
                        CodEmpresa,
                        Banco,
                        TipoDoc,
                        100000,
                        null,
                        null,
                        null,
                        null,
                        resolveConsecutivo: () => NTransac),

                    "G" => sbTeBNCR_Sinpe(CodEmpresa, Banco, TipoDoc, NTransac),
                    "DV1" or "DV2" => sbTeFormatoEstandar(CodEmpresa, Banco, TipoDoc, NTransac, Formato, Plan),
                    "S" => Err("SINPE está en espera / no implementado."),
                    "SG" => mTesFunciones.SbTesBancoSinpeGeneralCore(CodEmpresa, filtro, LoadTransacciones()),
                    _ => sbTeFormatoEstandar(CodEmpresa, Banco, TipoDoc, NTransac, Formato, Plan)
                };
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> ProcesarFormatoA(
            int CodEmpresa,
            int Banco,
            string TipoDoc,
            int NTransac,
            SqlConnection conn,
            object parametros,
            List<TesTransaccionDto> transacciones)
        {
            const string queryA = @"
Select sum(monto) as Monto
From Tes_Transacciones
Where Estado = 'T'
  And Tipo = @TipoDoc
  And ID_Banco= @Banco
  And Autoriza='S'
  And documento_base = @NTransac";

            int vMonto = conn.QueryFirstOrDefault<int?>(queryA, parametros) ?? 0;

            return mTesFunciones.SbTeBancoNacionalCore(
                conn: conn,
                codEmpresa: CodEmpresa,
                bancoId: Banco,
                tipoDoc: TipoDoc,
                transaccionesList: transacciones,
                curPlanilla: vMonto,
                resolveConsecutivo: () => NTransac
            );
        }

        private ErrorDto<object> ProcesarFormatoC(
            int CodEmpresa,
            int Banco,
            string TipoDoc,
            int NTransac,
            SqlConnection conn,
            object parametros,
            List<TesTransaccionDto> transacciones)
        {
            const string queryC = @"
select sum(dbo.fxTESBCRTestkey(cta_ahorros,monto)) as TestKeyX,
       sum(Monto) as Monto
From Tes_Transacciones 
Where Estado = 'T'
  And Tipo = @TipoDoc
  And ID_Banco= @Banco 
  And Autoriza='S'
  And documento_base = @NTransac";

            var resultC = conn.QueryFirstOrDefault(queryC, parametros);

            long xTestKey = 0;
            decimal totalMonto = 0;

            if (resultC != null)
            {
                long testKeyX = (long?)resultC.TestKeyX ?? 0;
                xTestKey = testKeyX > 2147483468 ? 2147483468 : testKeyX;
                totalMonto = (decimal?)resultC.Monto ?? 0m;
            }

            FormatoBcrRequest request = new()
            {
                conn = conn,
                codEmpresa = CodEmpresa,
                bancoId = Banco,
                tipoDoc = TipoDoc,
                transaccionesList = transacciones,
                vTestKey = (int)xTestKey,
                vMontoTotal = totalMonto,
                resolveConsecutivoArchivoDelDia = (c, banco, fecha) =>
                    MTesFuncionesDb.GetConsecutivoArchivoDelDia(conn, banco, fecha),
                resolveBancoConsec = () => NTransac
            };

            return mTesFunciones.SbTeBcrCore(request);
        }

        #endregion

        #region ===== Implementaciones =====
        

        private ErrorDto<object> sbTeBCT_Enlace(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                long bancoConsec = vNTransac;
                var sb = new StringBuilder();

                const string q = @"exec spTES_BCT_Enlace_ArchivoLog @banco, @bancoTDoc, @bancoConsec";
                var r = conn.QueryFirstOrDefault(q, new { banco = vBanco, bancoTDoc = vTipoDoc, bancoConsec });

                MTesFuncionesDb.AppendIfNotEmpty(sb, r?.Linea?.ToString());
                return MTesFuncionesDb.ArchivoResponse(bancoConsec, "txt", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBNCR_Sinpe(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                long bancoConsec = vNTransac;
                var sb = new StringBuilder();

                const string l1 = @"exec spTES_BNCR_SINPE_Archivo 1, @banco, @bancoTDoc, @bancoConsec, 0";
                const string l2 = @"exec spTES_BNCR_SINPE_Archivo 2, @banco, @bancoTDoc, @bancoConsec, 0";
                const string l3 = @"exec spTES_BNCR_SINPE_Archivo 3, @banco, @bancoTDoc, @bancoConsec, 0";

                var p = new { banco = vBanco, bancoTDoc = vTipoDoc, bancoConsec };

                MTesFuncionesDb.AppendIfNotEmpty(sb, conn.QueryFirstOrDefault<string>(l1, p));
                MTesFuncionesDb.AppendIfNotEmpty(sb, conn.QueryFirstOrDefault<string>(l2, p));
                MTesFuncionesDb.AppendIfNotEmpty(sb, conn.QueryFirstOrDefault<string>(l3, p));

                return MTesFuncionesDb.ArchivoResponse(bancoConsec, "tef", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        /// <summary>
        /// Formatos estándar: ejecuta por wrapper SP (spTES_EjecutarFormatoArchivo) y evita concatenar "EXEC {proc}_Archivo".
        /// </summary>
        private ErrorDto<object> sbTeFormatoEstandar(
            int CodEmpresa,
            int vBanco,
            string vTipoDoc,
            int vNTransac,
            string vFormato,
            string vPlan)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int bancoId = vBanco;
                var (numNegocio, _) = MTesFuncionesDb.GetEmpresaNumNegocioYReg(conn);

               
                var formatoData = mTesFunciones.vTesFormatos(conn, vFormato);
                if(formatoData.Code == -1)
                {
                    return DbHelper.CreateErrorResponse<object>($"Error al obtener configuración del formato");
                }

                string vExtension = formatoData.Result?.Extension?.ToString() ?? "txt";
                string vProcedimientoBase = formatoData.Result?.Procedimiento?.ToString() ?? string.Empty;


                const string wrapperSp = "spTES_EjecutarFormatoArchivo";

                long bancoConsec = vNTransac;
                string bancoPlan = vPlan;

                var sb = new StringBuilder();

                var parametrosBase = new
                {
                    procedimiento = vProcedimientoBase, // base, sin "_Archivo" (lo construye el wrapper SP)
                    bancoID = bancoId,
                    bancoTDoc = vTipoDoc,
                    numNegocio,
                    bancoConsec,
                    bancoPlan
                };

                foreach (var linea in MTesFuncionesDb.ExecSP3Lineas(conn, wrapperSp, parametrosBase))
                    MTesFuncionesDb.AppendIfNotEmpty(sb, linea);

                return MTesFuncionesDb.ArchivoResponse(bancoConsec, vExtension, sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        #endregion

        public ErrorDto<TesReporteTransferenciaDto> sbTesReporteTransferencia(
            int CodEmpresa,
            int vBanco,
            long vTransac,
            string? vTipo = "C",
            string? vDocumento = "TE",
            string? vPlan = "-sp-")
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            return mTesoreria.sbTesReporteTransferencia(conn, CodEmpresa, vBanco, vTransac, vTipo, vDocumento, vPlan);
        }
    }
}
