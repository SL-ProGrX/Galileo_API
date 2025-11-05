using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_MonitoreoDB
    {
        private readonly IConfiguration? _config;
        private mCntLinkDB mCntLink;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmTES_MonitoreoDB(IConfiguration? config)
        {
            _config = config;
            mCntLink = new mCntLinkDB(_config);
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Obtener el monitoreo de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TES_MonitoreoDTO>> TES_Monitoreo_Obtener(int CodEmpresa, DateTime fechaCorte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TES_MonitoreoDTO>>
            {
                Code = 0,
                Description = "",
                Result = new List<TES_MonitoreoDTO>(),
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    string fechaCorteStr = _AuxiliarDB.validaFechaGlobal(fechaCorte);

                    var query = $@"exec spTes_Monitoreo_Saldos_Movimientos @pFechaCorte ";
                    response.Result = connection.Query<TES_MonitoreoDTO>(query,
                    new
                        {
                        pFechaCorte = fechaCorteStr
                    },
                    commandTimeout: 120 // en segundos
                    ).ToList();
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener el monitoreo de los documentos de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Corte"></param>
        /// <returns></returns>
        public ErrorDto<List<TES_MonitoreoDTO>> TES_Monitoreo_Documentos_Obtener(int CodEmpresa, string Corte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TES_MonitoreoDTO>>
            {
                Code = 0,
                Description = "",
                Result = new List<TES_MonitoreoDTO>(),
            };
            try
            {
                DateTime fecha = DateTime.Parse(Corte);
                var fechaCorte = fecha.Date.AddDays(1).AddTicks(-1);
                var Lista = TES_Monitoreo_Obtener(CodEmpresa, fechaCorte).Result;
                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var item in Lista)
                    {
                        var fechaInicio = item.inicio.AddDays(-1).Date;
                        string ctaConta = mCntLink.fxgCntCuentaFormato(CodEmpresa, false, item.cuentaBanco, 0);

                        //Emisiones de Documentos
                        var queryE = @"select D.debehaber as Movimiento,sum(D.monto / D.Tipo_Cambio) as Total
                        from Tes_Transacciones C inner join Tes_Trans_Asiento D on C.nsolicitud = D.nsolicitud
                        where C.fecha_emision between @inicio 
                        and @corte and C.estado in('I','T','A') 
                        and D.cuenta_contable = @ctaconta 
                        and D.Tipo_Cambio <> 0
                        group by D.debehaber";
                        var emisionesDoc = connection.Query<TES_Monitoreo_DocumentosDTO>(queryE,
                            new
                            {
                                inicio = fechaInicio,
                                corte = fechaCorte,
                                ctaconta = ctaConta
                            }).ToList();
                        foreach (var rs in emisionesDoc)
                        {
                            if (rs.movimiento == "D")
                            {
                                item.totalCreditos = rs.total;
                            }
                            else
                            {
                                item.totalDebitos = rs.total;
                            }
                        }

                        //Anulaciones de Documentos
                        var queryA = @"select D.debehaber as Movimiento,sum(D.monto/ D.Tipo_Cambio) as Total
                        from Tes_Transacciones C inner join Tes_Trans_Asiento D on C.nsolicitud = D.nsolicitud
                        where C.fecha_anula between @inicio 
                        and @corte and C.estado in('A') 
                        and D.cuenta_contable = @ctaconta 
                        and D.Tipo_Cambio <> 0
                        group by D.debehaber";
                        var anulacionesDoc = connection.Query<TES_Monitoreo_DocumentosDTO>(queryA,
                            new
                            {
                                inicio = fechaInicio,
                                corte = fechaCorte,
                                ctaconta = ctaConta
                            }).ToList();
                        foreach (var rs in emisionesDoc)
                        {
                            if (rs.movimiento == "D")
                            {
                                item.totalDebitos += rs.total;
                            }
                            else
                            {
                                item.totalCreditos += rs.total;
                            }
                        }
                        item.saldoFinal = item.saldoInicial - item.totalDebitos + item.totalCreditos;
                        
                        response.Result.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }
    }
}
