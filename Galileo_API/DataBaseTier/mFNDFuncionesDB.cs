using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;
using PgxAPI.Models.ProGrX.Fondos;

namespace PgxAPI.DataBaseTier
{
    public class MFndFuncionesDb
    {
        private readonly IConfiguration _config;
        public MFndFuncionesDb(IConfiguration config)
        {
            _config = config;
        }

        public string fxgFNDTipoPago(int CodEmpresa, string vModo, string vTipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string result = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (vModo == "D")
                    {
                        switch (vTipo.Trim().ToUpper())
                        {
                            case "TRANSFERENCIA":
                                result = "TE";
                                break;
                            case "CHEQUE":
                                result = "CK";
                                break;
                        }
                    }
                    else if (vModo == "C")
                    {
                        switch (vTipo.Trim().ToUpper())
                        {
                            case "TE":
                                result = "Transferencia";
                                break;
                            case "CK":
                                result = "Cheque";
                                break;
                        }
                    }

                }
            }
            catch (Exception)
            {
                return "";
            }
            return result;
        }

        public decimal fxgFNDCodigoMulta(int CodEmpresa, int vOperadora, string vPlan, int vContrato, decimal vMonto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            decimal result = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select dbo.fxFNDMulta(@vOperadora, @vPlan, @vContrato, @vMonto) as 'Multa'";
                    result = connection.QueryFirstOrDefault<decimal>(query, new { vOperadora, vPlan, vContrato, vMonto });
                }
            }
            catch (Exception)
            {
                return 0;
            }
            return result;
        }

        public static string fxTipoDocumento(string vTipo)
        {
            switch (vTipo)
            {
                case "CK":
                    return "Cheque";
                case "TE":
                    return "Transferencia";
                case "EF":
                case "RE":
                    return "Efectivo";
                case "ND":
                    return "Nota Debito";
                case "NC":
                    return "Nota Credito";
                case "OT":
                    return "Otro...";
                case "CD":
                    return "Ctrl Desembolsos";
                case "CP":
                    return "Proveedor";
                case "RC":
                    return "Retiro en Caja";
                case "FD":
                    return "Fondo Transitorio";
                case "TS":
                    return "Transferencia SINPE";

                // --- Inverso ---
                case "Cheque":
                    return "CK";
                case "Transferencia":
                    return "TE";
                case "Efectivo":
                    return "EF";
                case "Nota Debito":
                    return "ND";
                case "Nota Credito":
                    return "NC";
                case "Otro...":
                    return "OT";
                case "Ctrl Desembolsos":
                    return "CD";
                case "Proveedor":
                    return "CP";
                case "Retiro en Caja":
                    return "RC";
                case "Fondo Transitorio":
                    return "FD";
                case "Transferencia SINPE":
                    return "TS";

                default:
                    return "";
            }
        }

        public string fxFndParametro(int CodEmpresa, string pParametro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string? result = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select valor from Fnd_parametros where cod_parametro = @pParametro";
                    var queryResult = connection.QueryFirstOrDefault<string>(query, new { pParametro });
                    result = queryResult ?? "";
                }
            }
            catch (Exception)
            {
                return "";
            }
            return result;
        }

        /// <summary>
        /// Metodo para obtener los cupones de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pOperadora"></param>
        /// <param name="pPlan"></param>
        /// <param name="pContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndContratosCuponesData>> sbFnd_Contratos_Cupones(int CodEmpresa, int pOperadora,string pPlan, long pContrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FndContratosCuponesData>> 
            { 
                Code = 0,
                Description = "OK",
                Result = new List<FndContratosCuponesData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from vFnd_Contratos_Cupones
                                    where cod_operadora = @operadora
                                    and cod_plan = @plan and cod_contrato = @contrato order by Fecha_Vence";
                    response.Result = connection.Query<FndContratosCuponesData>(query, new
                    {
                        operadora = pOperadora,
                        plan = pPlan,
                        contrato = pContrato    
                    }).ToList();
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

        /// <summary>
        /// Metodo para obtener la bitacora de cambios de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pOperadora"></param>
        /// <param name="pPlan"></param>
        /// <param name="pContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndContratoBitacoraData>> sbFnd_Contratos_Bitacora(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FndContratoBitacoraData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<FndContratoBitacoraData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.*,S.cedula,S.nombre,M.Descripcion as MovimientoDesc,case when C.revisado_fecha is null then 0 else 1 end as 'Revisado'
                                        from fnd_contratos_cambios C inner join fnd_contratos X on C.cod_operadora = X.cod_operadora
                                        and C.cod_plan = X.cod_plan and C.cod_contrato = X.cod_contrato
                                        inner join Socios S on X.cedula = S.cedula
                                        inner join US_MOVIMIENTOS_BE M on C.Movimiento = M.Movimiento and M.modulo = 18
                                        where C.cod_operadora = @operadora
                                        and C.cod_plan = @plan and C.cod_contrato = @contrato
                                        order by C.fecha desc";
                    response.Result = connection.Query<FndContratoBitacoraData>(query, new
                    {
                        operadora = pOperadora,
                        plan = pPlan,
                        contrato = pContrato
                    }).ToList();
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
