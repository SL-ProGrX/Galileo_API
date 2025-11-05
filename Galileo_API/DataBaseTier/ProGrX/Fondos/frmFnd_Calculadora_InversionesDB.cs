using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Fondos;

namespace PgxAPI.DataBaseTier.ProGrX.Fondos
{
    public class frmFnd_Calculadora_InversionesDB
    {
        private readonly IConfiguration? _config;

        public frmFnd_Calculadora_InversionesDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtener dropdown planes 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="TipoInv"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Fnd_Calculadora_Planes_Obtener(int CodEmpresa, int TipoInv)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (TipoInv == 0)
                        query = "exec spFnd_Calculadora_Planes 'APL'";
                    else if (TipoInv == 1)
                        query = "exec spFnd_Calculadora_Planes 'CDP'";

                    response.Result = connection.Query(query)
                     .Select(row => new DropDownListaGenericaModel
                     {
                         item = row.IdX,
                         descripcion = row.itmX
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
        /// Obtener datos del plan seleccionado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodPlan"></param>
        /// <returns></returns>
        public ErrorDTO<Fnd_Calculadora_Planes> Fnd_Calculadora_ConsultaPlan_Obtener(int CodEmpresa, string CodPlan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<Fnd_Calculadora_Planes>
            {
                Code = 0,
                Description = "Ok",
                Result = new Fnd_Calculadora_Planes()
            };
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    query = @"select TIPO_DEDUC, PORC_DEDUC, TIPO_CDP, PAGO_CUPONES, WEB_VENCE, CAPITALIZA_RENDIMIENTOS, TASA_MARGEN_NEGOCIACION 
                        from fnd_Planes
                        where cod_operadora = 1 
                        and cod_plan = @CodPlan";
                    response.Result = connection.QueryFirstOrDefault<Fnd_Calculadora_Planes>(query, new { CodPlan });
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
        /// Obtener dropdown de los plazos de inversión disponibles
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodPlan"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Fnd_Calculadora_PlazosInv_Obtener(int CodEmpresa, string CodPlan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    query = "exec spFnd_Inversion_Plazos @CodPlan";

                    response.Result = connection.Query(query, new { CodPlan })
                     .Select(row => new DropDownListaGenericaModel
                     {
                         item = row.IdX,
                         descripcion = row.ItmX
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
        /// Obtener dropdown de los cupones disponibles
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Plazo"></param>
        /// <param name="CodPlan"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Fnd_Calculadora_Cupones_Obtener(int CodEmpresa, int Plazo, string CodPlan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    query = "exec spFnd_Cupon_Frecuencia @Plazo, @CodPlan";

                    response.Result = connection.Query(query, new { Plazo, CodPlan })
                     .Select(row => new DropDownListaGenericaModel
                     {
                         item = row.IdX,
                         descripcion = row.ItmX
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
        /// Obtener dropdown de los plazos en días disponibles
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Plazo"></param>
        /// <param name="CodPlan"></param>
        /// <returns></returns>
        public ErrorDTO<int> Fnd_Calculadora_PlazosDias_Obtener(int CodEmpresa, int Plazo, string CodPlan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<int>
            {
                Code = 0,
                Description = "Ok"
            };
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    query = "exec spFnd_Inversion_Plazos_Dias @Plazo";

                    var result = connection.QueryFirstOrDefault(query, new { Plazo });
                    if (CodPlan.Substring(0, 1) == "D")
                    {
                        response.Result = result.PLAZO_DIAS;
                    }
                    else
                    {
                        response.Result = result.PLAZO_MESES;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }

        /// <summary>
        /// Obtener tasa de referencia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="PlazoDias"></param>
        /// <param name="Tipo"></param>
        /// <param name="Plan"></param>
        /// <param name="Operadora"></param>
        /// <param name="chkCupon"></param>
        /// <param name="rpTipo"></param>
        /// <param name="PlazoInv"></param>
        /// <param name="CuponId"></param>
        /// <returns></returns>
        public ErrorDTO<decimal> Fnd_Calculadora_TasaRef_Obtener(int CodEmpresa, int PlazoDias, string Tipo, string Plan, int Operadora, bool chkCupon, int rpTipo, int PlazoInv, int? CuponId )
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<decimal>
            {
                Code = 0,
                Description = "Ok"
            };
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (!chkCupon || rpTipo == 0)
                    {
                        query = "select dbo.fxFNDCalcularTasaRefContrato(@Operadora, @Plan, @PlazoDias, @Tipo, Null, Null, 0) as 'TASA'";
                        response.Result = connection.QueryFirstOrDefault<decimal>(query, new { Operadora, Plan, PlazoDias, @Tipo });
                    }
                    else
                    {
                        query = "exec dbo.spFnd_Inversion_Tasas_Condiciones @Operadora, @Plan, @PlazoInv, @CuponId";
                        response.Result = connection.QueryFirstOrDefault<decimal>(query, new { Operadora, Plan, PlazoInv, CuponId });
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }

        /// <summary>
        /// Calcular flujo de inversión
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="FiltrosCalculadora"></param>
        /// <returns></returns>
        public ErrorDTO<List<Fnd_Calculadora_Inversiones_FlujoData>> Fnd_Calculadora_Inversiones_Calcular(int CodEmpresa, string FiltrosCalculadora)
        {
            var filtros = System.Text.Json.JsonSerializer.Deserialize<Filtros_Calculadora>(FiltrosCalculadora);
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<Fnd_Calculadora_Inversiones_FlujoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<Fnd_Calculadora_Inversiones_FlujoData>()
            };
            var query = "";
            int pTP_Indica = 0;
            int? pCalculoId = null;
            DateTime vFecha = DateTime.Now;
            try
            {
                if (filtros.pTP_Sol > filtros.pTasa)
                {
                    pTP_Indica = 1;
                }
                if (filtros.pCalculoId != 0)
                {
                    pCalculoId = filtros.pCalculoId;
                }
                using var connection = new SqlConnection(stringConn);
                {
                    query = @$"exec spFnd_Calculadora_Inversiones_Registro @pCalculoId, @txtInversion,
                        @vFecha, @plazo, @pTP_Sol, @pFrecuenciaPago, @txtMonto, 360, @capitaliza, 
                        @cedula, @plan, 'ProGrX', @usuario,  @pTP_Indica";
                    pCalculoId = connection.QueryFirstOrDefault<int>(query, 
                        new {
                            pCalculoId,
                            filtros.txtInversion,
                            vFecha,
                            plazo = filtros.Plazo,
                            filtros.pTP_Sol,
                            filtros.pFrecuenciaPago,
                            filtros.txtMonto,
                            capitaliza = filtros.chkCapitaliza ? 1 : 0,
                            cedula = filtros.Cedula.Trim(),
                            plan = filtros.Plan,
                            usuario = filtros.Usuario.ToUpper(),
                            pTP_Indica 
                        });

                    query = "exec spFnd_Calculadora_Inversiones_Flujo @pCalculoId";
                    response.Result = connection.Query<Fnd_Calculadora_Inversiones_FlujoData>(query, new { pCalculoId }).ToList();
                    if (response.Result.Count > 0)
                    {
                        response.Code = pCalculoId;
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

        /// <summary>
        /// Enviar email del cálculo procesado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CalculoId"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDTO Fnd_Calculadora_Inversiones_EmailEnviar(int CodEmpresa, int CalculoId, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"exec spFnd_Calculadora_Inversiones_Email @CalculoId, @Usuario";
                    connection.Execute(query, new { CalculoId, Usuario });

                    response.Description = "Correo enviado a la persona!";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }
    }
}
