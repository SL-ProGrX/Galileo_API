using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using static PgxAPI.Models.ProGrX.Clientes.FrmAfLiquidacionWModels;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_LiquidacionWDB
    {
        private readonly IConfiguration? _config;
        public frmAF_LiquidacionWDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la lista de bancos disponibles para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtro de usuario y divisa.</param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionBancos>> AF_Liquidacion_Bancos_Obtener(int CodEmpresa, AfLiquidacionBancosFiltro filtro)
        {
            var result = new ErrorDto<List<AfLiquidacionBancos>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfLiquidacionBancos>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    Usuario = filtro.Usuario,
                    Divisa = filtro.Divisa
                };

                result.Result = connection.Query<AfLiquidacionBancos>(
                    "spCrd_SGT_Bancos",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene los tipos de documento emitidos para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtro de banco y mortalidad.</param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionEmiteTDoc>> AF_Liquidacion_Emite_TDoc(int CodEmpresa, AfLiquidacionEmiteTDocFiltro filtro)
        {
            var result = new ErrorDto<List<AfLiquidacionEmiteTDoc>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfLiquidacionEmiteTDoc>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    BancoId = filtro.BancoId,
                    Mortalidad = filtro.Mortalidad,
                };

                result.Result = connection.Query<AfLiquidacionEmiteTDoc>(
                    "spAFI_Renuncia_Emite_TDoc",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de tipos de acción para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Liquidacion_TipoAccion_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"select Id_Documento as item, Descripcion as descripcion from AFI_CR_RENUNCIAS_TIPO_DOCUMENTO";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene el detalle de una causa de renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Causa">ID de la causa.</param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionCausasDetalle> AF_Liquidacion_Causas_ObtenerDetalle(int CodEmpresa, int Causa)
        {
            var result = new ErrorDto<AfLiquidacionCausasDetalle>()
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    select mortalidad, liq_alterna, Tipo_Apl, AJUSTE_TASAS
                    from causas_renuncias
                    where id_causa = @Causa";

                result.Result = connection.QueryFirstOrDefault<AfLiquidacionCausasDetalle>(query, new { Causa });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene las cuentas bancarias asociadas a un socio para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtro de identificación, banco y divisa.</param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionCuentaBancaria>> AF_Liquidacion_CuentasBancarias_Obtener(int CodEmpresa, AfLiquidacionCuentaBancariaFiltro filtro)
        {
            var result = new ErrorDto<List<AfLiquidacionCuentaBancaria>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfLiquidacionCuentaBancaria>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    filtro.Identificacion,
                    filtro.BancoId,
                    filtro.DivisaCheck
                };

                result.Result = connection.Query<AfLiquidacionCuentaBancaria>(
                    "spSys_Cuentas_Bancarias",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Consulta si existe plan de fondos para liquidación (fxAFI_Liquidacion_FP_Fondos).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<short> AF_Liquidacion_Fondos(int CodEmpresa)
        {
            var result = new ErrorDto<short>
            {
                Code = 0,
                Description = "Ok",
                Result = new short()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = "SELECT dbo.fxAFI_Liquidacion_FP_Fondos() AS Flag";
                result.Result = connection.QueryFirstOrDefault<short>(query);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = 0;
            }
            return result;
        }

        /// <summary>
        /// Consulta si está activado el control en afi_cr_parametros.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<bool> AF_Liquidacion_ActivarControl(int CodEmpresa)
        {
            var result = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = "SELECT TOP 1 Activar_Control FROM afi_cr_parametros";
                short valor = connection.QueryFirstOrDefault<short>(query);
                result.Result = valor == 1;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = false;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de renuncias sin liquidar o socios según el control de renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="activar_control">Si es true consulta vAFI_Renuncias_SinLiquidar, si es false consulta socios.</param>
        /// <returns></returns>
        public ErrorDto<List<object>> AF_Liquidacion_Renuncias_Obtener(int CodEmpresa, bool activar_control)
        {
            var result = new ErrorDto<List<object>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<object>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                if (activar_control)
                {
                    string query = @"SELECT Cedula, Id_Alterno, Nombre FROM vAFI_Renuncias_SinLiquidar";
                    var data = connection.Query<AfLiquidacionRenunciaSinLiquidar>(query).ToList<object>();
                    result.Result = data;
                }
                else
                {
                    string query = @"SELECT Cedula, Nombre FROM socios WHERE EstadoActual IN ('S','A') order by Cedula";
                    var data = connection.Query<AfLiquidacionSocio>(query).ToList<object>();
                    result.Result = data;
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de socios con renuncia activa según el control de renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="activar_control">Si es true consulta a los socios con renuncia activa, si es false muestra todos los socios.</param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionSocio>> AF_Liquidacion_SociosRenuncia_Obtener(int CodEmpresa, bool activar_control)
        {
            var result = new ErrorDto<List<AfLiquidacionSocio>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfLiquidacionSocio>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query;
                if (activar_control)
                {
                    query = @"
                        SELECT S.Cedula, S.Nombre
                        FROM socios S
                        INNER JOIN afi_cr_renuncias R
                          ON S.cedula = R.cedula
                         AND R.liq IS NULL
                         AND R.estado IN ('P','V')";
                }
                else
                {
                    query = @"SELECT S.Cedula, S.Nombre FROM socios S";
                }

                result.Result = connection.Query<AfLiquidacionSocio>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Actualiza el estado de las renuncias a 'V' donde la fecha de vencimiento es menor a la fecha actual.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<int> AF_Liquidacion_ActualizarEstadoRenuncias(int CodEmpresa)
        {
            var result = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"UPDATE afi_cr_renuncias SET estado = 'V' WHERE vencimiento < dbo.MyGetdate() AND estado = 'T'";
                int rowsAffected = connection.Execute(query);
                result.Result = rowsAffected;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = 0;
            }
            return result;
        }

        /// <summary>
        /// Obtiene el detalle de un socio para liquidación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cedula">Cédula del socio.</param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionSocioDetalle> AF_Liquidacion_SocioDetalle_Obtener(int CodEmpresa, string Cedula)
        {
            var result = new ErrorDto<AfLiquidacionSocioDetalle>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    select S.cedula, S.nombre, S.fechaingreso, S.estadoactual,
                           0 as Boleta, isnull(E.descripcion,'') as EstadoPersona
                    from socios S
                    inner join AFI_ESTADOS_PERSONA E on S.estadoActual = E.cod_estado
                    where S.cedula = @Cedula";

                result.Result = connection.QueryFirstOrDefault<AfLiquidacionSocioDetalle>(query, new { Cedula });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
    }
}
