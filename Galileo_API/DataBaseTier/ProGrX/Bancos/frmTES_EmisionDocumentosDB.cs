using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesEmisionDocumentosDb
    {
        private readonly MTesoreria mTesoreria;
         private readonly PortalDB _portalDB;

        public FrmTesEmisionDocumentosDb(IConfiguration config)
        {
            mTesoreria = new MTesoreria(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener formatos de banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Formato_Obtener(int CodEmpresa, int banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var query = @"exec spTes_Formatos_Bancos @banco";
                var result = conn.Query(query, new { banco })
                    .Select(row => new DropDownListaGenericaModel
                    {
                        item = row.IDX,
                        descripcion = row.ItmX
                    }).ToList();
                return result;
            });
        }

        /// <summary>
        /// Obtener planes 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Plan_Obtener(int CodEmpresa, int banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var query = @"select Bp.COD_PLAN as item, Bp.COD_PLAN as descripcion
                        from TES_BANCOS B inner join TES_BANCO_PLANES_TE Bp on B.ID_BANCO = Bp.ID_BANCO 
                        Where B.ID_BANCO = @banco And B.UTILIZA_PLAN = 1 order by Bp.COD_PLAN asc";
                return conn.Query<DropDownListaGenericaModel>(query, new { banco = banco }).ToList();
            });
        }

        /// <summary>
        /// Buscar información para emisión de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDoc"></param>
        /// <param name="banco"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto<TesTransaccionesData> TES_EmisionDocumento_Buscar(int CodEmpresa, string tipoDoc, int banco, string plan)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var query = @"select isnull(count(*),0) as Total,isnull(Min(nsolicitud),0) as Minimo,
                        isnull(Max(nsolicitud),0) as Maximo from Tes_Transacciones
                        Where Estado='P' And Tipo = @tipoDoc and ID_Banco = @banco";
                var solicitudes = conn.QueryFirstOrDefault<TesTransaccionesData>(query, new { tipoDoc = tipoDoc, banco = banco }) ?? new TesTransaccionesData();

                // Si no hay solicitudes
                if (solicitudes.total == 0)
                {
                    solicitudes.minimo = 0;
                    solicitudes.maximo = 0;
                }

                // Obtener consecutivo inicial
                solicitudes.docInicial = mTesoreria.fxTesTipoDocConsec(CodEmpresa, banco, tipoDoc, "/", plan).Result;

                // Verificar si se puede modificar
                string vDato = mTesoreria.fxTesTipoDocExtraeDato(CodEmpresa, banco, tipoDoc, "mod_consec").Result ?? "0";
                solicitudes.docBloqueo = vDato != "1";

                return solicitudes;
            });
        }


        /// <summary>
        /// Despliega en pantalla las solicitudes pendientes que estan autorizadas 
        /// y que estan dentro del rango de parametros suministrado por el usuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<TesSolicitudesGenData>> TES_EmisionDocumento_Solicitudes_Obtener(int CodEmpresa, string filtros)
        {
            TesEmisionDocFiltros filtro = JsonConvert.DeserializeObject<TesEmisionDocFiltros>(filtros) ?? new TesEmisionDocFiltros();
            long consecInt = 0;
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                consecInt = mTesoreria.fxTesTipoDocConsecInterno(CodEmpresa, filtro.banco, filtro.tipoDoc, "/", filtro.plan).Result;

                var query = @$"Select TOP {filtro.cantidad} *, dbo.fxTes_Cuentas_Bancarias_Pass(id_Banco,Cta_Ahorros) as 'Pass'
                        From Tes_Transacciones Where Estado='P' And Tipo = @tipoDoc
                        And Id_Banco=@banco And Autoriza = 'S' and fecha_hold is null";

                if (filtro.generarPor == "solicitudes")
                {
                    query += " And NSolicitud Between @minimo And @maximo";
                }
                else if (filtro.generarPor == "fechas")
                {
                    query += @" And Fecha_Solicitud Between @fechaInicio And @fechaCorte";
                }
                query += " Order by NSolicitud";

                var fechaInicio = filtro.fecha_inicio?.Date;
                var fechaCorte = filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1);

                var result = conn.Query<TesSolicitudesGenData>(query,
                            new
                            {
                                tipoDoc = filtro.tipoDoc,
                                banco = filtro.banco,
                                minimo = filtro.minimo,
                                maximo = filtro.maximo,
                                fechaInicio = fechaInicio,
                                fechaCorte = fechaCorte,

                            }).ToList();

                foreach (var item in result)
                {
                    if (filtro.tipoDoc == "TE")
                    {
                        item.documento = $"{filtro.docInicial:000}-{consecInt}";
                    }
                    else
                    {
                        item.documento = filtro.docInicial.ToString();
                    }
                    item.fecha = DateTime.Now; //Devuelve la fecha del servidor
                    item.firmas = (item.firmas_autoriza_fecha == null) ? "No" : "Sí";
                }

                return result;
            });
        }

        /// <summary>
        /// Valida el numero de documento, si ya está asignado dentro del rango
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <param name="tipoDoc"></param>
        /// <param name="docInicial"></param>
        /// <param name="cantidadList"></param>
        /// <returns></returns>
        public ErrorDto TES_EmisionDocumento_ValidaNumDocumento(int CodEmpresa, int banco, string tipoDoc, int docInicial, int cantidadList)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                int docFinal = docInicial + (cantidadList - 1);

                var query = @" SELECT ndocumento FROM Tes_Transacciones
                        WHERE id_Banco = @banco AND ndocumento BETWEEN @docInicial AND @docFinal
                        AND Tipo = @tipoDoc'";
                var lista = conn.Query<int>(query,
                    new
                    {
                        banco = banco,
                        docInicial = docInicial,
                        docFinal = docFinal,
                        tipoDoc = tipoDoc
                    }).ToList();

                var docExistente = lista.FirstOrDefault(nDoc => nDoc >= docInicial && nDoc <= docFinal);
                if (docExistente != 0)
                {
                    return DbHelper.CreateErrorResponse($"\nYa existe un Documento asignado [{docExistente}] dentro del rango suministrado", -2);
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta un SP que Revisa Cuentas Bancarias de Solicitudes Pendientes de Emitir
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto TES_EmisionDocumento_RevisaCuentas_SP(int CodEmpresa, int banco)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB,CodEmpresa);

                var query = "exec spTes_Cuentas_Revisa @banco";
                conn.Execute(query, new { banco = banco });
                return  DbHelper.CreateOkResponse("Cuentas verificadas correctamente!", 0);

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtener cuentas puente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_CtasPuente_Obtener(
            int CodEmpresa,
            string Usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var query = @"
            select 
                B.id_Banco as item,
                rtrim(B.descripcion) as descripcion
            from Tes_Bancos B
            inner join tes_Banco_ASG A 
                on B.id_Banco = A.id_Banco
               and A.nombre = @usuario
            where B.estado = 'A'
              and B.puente = 1";

                return conn.Query<DropDownListaGenericaModel>(
                    query,
                    new { usuario = Usuario }
                ).ToList();
            });
        }

        /// <summary>
        /// Traslada las Solicitudes seleccionadas entre Cuentas (Puente)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <param name="Usuario"></param>
        /// <param name="Solicitudes"></param>
        /// <returns></returns>
        public ErrorDto TES_EmisionDocumento_CtaPuente_Aplicar(
            int CodEmpresa,
            int Banco,
            string Usuario,
            string Solicitudes)
        {
            try
            {
                var listaSolicitudes =
                    JsonConvert.DeserializeObject<List<int>>(Solicitudes) ?? new List<int>();

                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                foreach (var solicitud in listaSolicitudes)
                {
                    var query = @"exec spTes_Traslados_Cuenta_Puente 
                          @solicitud, @banco, @usuario";

                    connection.Execute(query, new
                    {
                        solicitud,
                        banco = Banco,
                        usuario = Usuario
                    });
                }

                return DbHelper.CreateOkResponse("Solicitudes movidas correctamente", 0);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message);
            }
        }


    }//End class
}
