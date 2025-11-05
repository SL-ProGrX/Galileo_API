using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_CRRenunciaDB
    {
        private readonly IConfiguration? _config;
        public frmAF_CRRenunciaDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene los socios para las renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciasSocios>> AF_CR_RenunciasSocios_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<AfRenunciasSocios>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRenunciasSocios>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var sb = new StringBuilder(@"
                    select Cedula, Nombre, CedulaR
                    from socios
                    WHERE estadoactual in('S','A')
                    order by Cedula
                ");

                result.Result = connection.Query<AfRenunciasSocios>(sb.ToString()).ToList();
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
        /// Obtiene el estado de un socio para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <returns></returns>
        public ErrorDto<AfRenunciasSocioDetalle> AF_CR_Renuncias_Estado_Obtener(int CodEmpresa, string cedula)
        {
            var result = new ErrorDto<AfRenunciasSocioDetalle>()
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
                    select
                      S.cedula,
                      S.nombre,
                      S.fechaingreso,
                      S.estadoactual,
                      0 as Boleta,
                      isnull(E.descripcion,'') as EstadoPersona,
                      dbo.fxAFI_Renuncia_Activa(S.Cedula) as Valida,
                      dbo.fxCBR_Cobro_Judicial_Indica(S.Cedula) as CbrJud
                    from socios S
                    inner join AFI_ESTADOS_PERSONA E on S.estadoActual = E.cod_estado
                    where S.cedula = @Cedula";

                result.Result = connection.QueryFirstOrDefault<AfRenunciasSocioDetalle>(query, new { Cedula = cedula });
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
        /// Obtiene la lista de bancos disponibles para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtro de usuario y divisa.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaBancos>> AF_CR_Renuncias_Bancos_Obtener(int CodEmpresa, AfRenunciaBancoFiltro filtro)
        {
            var result = new ErrorDto<List<AfRenunciaBancos>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRenunciaBancos>()
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

                result.Result = connection.Query<AfRenunciaBancos>(
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
        public ErrorDto<List<AfRenunciaEmiteTDoc>> AF_CR_Renuncias_Emite_TDoc(int CodEmpresa, AfRenunciaEmiteTDocFiltro filtro)
        {
            var result = new ErrorDto<List<AfRenunciaEmiteTDoc>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRenunciaEmiteTDoc>()
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

                result.Result = connection.Query<AfRenunciaEmiteTDoc>(
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
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Renuncias_TipoAccion_Obtener(int CodEmpresa)
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
        public ErrorDto<AfRenunciaCausasDetalle> AF_CR_Renuncias_Causas_ObtenerDetalle(int CodEmpresa, int Causa)
        {
            var result = new ErrorDto<AfRenunciaCausasDetalle>()
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

                result.Result = connection.QueryFirstOrDefault<AfRenunciaCausasDetalle>(query, new { Causa });
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
        /// Consulta el patrimonio de un socio para liquidación de renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cedula">Cédula del socio.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaLiqConsultaPatrimonio>> AF_CR_Renuncias_Liq_Consulta_Patrimonio(int CodEmpresa, string Cedula)
        {
            var result = new ErrorDto<List<AfRenunciaLiqConsultaPatrimonio>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRenunciaLiqConsultaPatrimonio>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    Cedula
                };

                result.Result = connection.Query<AfRenunciaLiqConsultaPatrimonio>(
                    "spAFI_Liq_Consulta_Patrimonio",
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
        /// Obtiene la renta detallada para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Monto">Monto a consultar.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaExcRentaDetallada>> AF_CR_Renuncias_Exc_Renta_Detallada(int CodEmpresa, decimal Monto)
        {
            var result = new ErrorDto<List<AfRenunciaExcRentaDetallada>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRenunciaExcRentaDetallada>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    Monto
                };

                result.Result = connection.Query<AfRenunciaExcRentaDetallada>(
                    "spExc_Renta_Detallada",
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
        /// Obtiene la lista de planes para liquidación de renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtro de cédula y tipo de liquidación.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaLiquidaListaPlanes>> AF_CR_Renuncias_Liquida_ListaPlanes(int CodEmpresa, AfRenunciaLiquidaListaPlanesFiltro filtro)
        {
            var result = new ErrorDto<List<AfRenunciaLiquidaListaPlanes>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRenunciaLiquidaListaPlanes>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    Cedula = filtro.Cedula,
                    TipoLiq = filtro.TipoLiq
                };

                result.Result = connection.Query<AfRenunciaLiquidaListaPlanes>(
                    "spAfiLiquidaListaPlanes",
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
        /// Obtiene las cuentas bancarias asociadas a un socio para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtro de identificación, banco y divisa.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaCuentaBancaria>> AF_CR_Renuncias_CuentasBancarias_Obtener(int CodEmpresa, AfRenunciaCuentaBancariaFiltro filtro)
        {
            var result = new ErrorDto<List<AfRenunciaCuentaBancaria>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRenunciaCuentaBancaria>()
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

                result.Result = connection.Query<AfRenunciaCuentaBancaria>(
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
        /// Obtiene la lista de promotores activos para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaPromotor>> AF_CR_Renuncias_Promotores_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<AfRenunciaPromotor>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRenunciaPromotor>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    select id_Promotor, Nombre
                    from Promotores
                    where Estado = 1 and tipo <> 'C'
                    order by Nombre";

                result.Result = connection.Query<AfRenunciaPromotor>(query).ToList();
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
        /// Valida si un socio tiene renuncia activa.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <returns></returns>
        public ErrorDto<int> AF_CR_Renuncias_Activa(int CodEmpresa, string cedula)
        {
            var result = new ErrorDto<int>()
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = "select dbo.fxAFI_Renuncia_Activa(@Cedula) as Resultado";
                result.Result = connection.QueryFirstOrDefault<int>(query, new { Cedula = cedula });
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
        /// Valida si un socio tiene otra renuncia activa.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <param name="codigo">Código de renuncia.</param>
        /// <returns></returns>
        public ErrorDto<int> AF_CR_Renuncias_Activa_Otra(int CodEmpresa, string cedula, int codigo)
        {
            var result = new ErrorDto<int>()
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = "select dbo.fxAFI_Renuncia_Activa_Otra(@Cedula, @Codigo) as Resultado";
                result.Result = connection.QueryFirstOrDefault<int>(query, new { Cedula = cedula, Codigo = codigo });
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
        /// Valida si un socio existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <returns></returns>
        public ErrorDto<int> AF_CR_Renuncias_Socio_Existe(int CodEmpresa, string cedula)
        {
            var result = new ErrorDto<int>()
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = "select isnull(count(*),0) as Existe from socios where cedula = @Cedula";
                result.Result = connection.QueryFirstOrDefault<int>(query, new { Cedula = cedula });
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
        /// Obtiene una renuncia por su identificador.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRenuncia">Código de la renuncia.</param>
        /// <returns></returns>
        public ErrorDto<AfRenuncia> AF_CR_Renuncias_ObtenerPorId(int CodEmpresa, long CodRenuncia)
        {
            var result = new ErrorDto<AfRenuncia>()
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
                    select *, dbo.fxSys_Cuentas_Mask(cuenta) as Cuenta_Desc
                    from vAFI_Renuncias
                    where cod_renuncia = @CodRenuncia";

                result.Result = connection.QueryFirstOrDefault<AfRenuncia>(query, new { CodRenuncia });
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
        /// Obtiene la lista de causas activas para seguimiento de renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Tipo">Tipo de aplicación.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Renuncia_Obtener_Causas(int CodEmpresa, String Tipo)
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

                string query = @"SELECT id_causa AS item, RTRIM(descripcion) AS descripcion FROM causas_renuncias WHERE ACTIVO = 1 and Tipo_Apl in('A', @Tipo)";
                result.Result = connection.Query<DropDownListaGenericaModel>(query, new {Tipo}).ToList();
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
        /// Obtiene los valores de la renta global
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtros cedula, corte, mnt retiro, plan </param>
        /// <returns></returns>
        public ErrorDto<AfRenunciaRentaGlobal> AF_CR_Renuncias_Renta_Global(int CodEmpresa, AfRenunciaRentaGlobalFiltro filtro)
        {
            var result = new ErrorDto<AfRenunciaRentaGlobal>()
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    Cedula = filtro.Cedula,
                    Corte = filtro.Corte,
                    MntRetiro = filtro.MntRetiro,
                    Plan = filtro.Plan
                };

                result.Result = connection.QueryFirstOrDefault<AfRenunciaRentaGlobal>(
                    "spFnd_Renta_Global",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
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
        /// Obtiene la liquidación de créditos
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtros para la liquidación de créditos</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaLiquidacionCreditosPersona>> AF_CR_Renuncias_Liquidacion_CreditosPersona(int CodEmpresa, AfRenunciaLiquidacionCreditosPersonaFiltro filtro)
        {
            var result = new ErrorDto<List<AfRenunciaLiquidacionCreditosPersona>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRenunciaLiquidacionCreditosPersona>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    Cedula = filtro.Cedula,
                    Abono = filtro.Abono
                };

                result.Result = connection.Query<AfRenunciaLiquidacionCreditosPersona>(
                    "spAfi_Liquidacion_CreditosPersona",
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
        /// Obtiene el monto de sinpe negativo
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cedula">Cédula de una persona</param>
        /// <returns></returns>
        public ErrorDto<AfRenunciaSinpeNegativo> AF_CR_Renuncias_Sinpe_Negativo(int CodEmpresa, string Cedula )
        {
            var result = new ErrorDto<AfRenunciaSinpeNegativo>()
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    Cedula
                };

                result.Result = connection.QueryFirstOrDefault<AfRenunciaSinpeNegativo>(
                    "spFnd_Sinpe_Negativo",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
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
        /// Obtiene el historico de renuncias de una persona
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cedula">Cédyla de una persona</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaDetalleHistorico>> AF_CR_Renuncias_ObtenerHistorico(int CodEmpresa, string Cedula)
        {
            var result = new ErrorDto<List<AfRenunciaDetalleHistorico>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfRenunciaDetalleHistorico>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    Select  R.*,
                            rTrim(C.Descripcion)        as CausaX,
                            S.nombre,
                            isnull(P.id_promotor,0)     as Id_Promotor,
                            isnull(P.nombre,'AFILIACION UNIVERSAL') as PromotorX
                    from afi_cr_renuncias R
                    inner join causas_renuncias C on R.id_causa = C.id_causa
                    inner join Socios S           on R.cedula   = S.cedula
                    left  join Promotores P       on R.id_Promotor = P.id_Promotor
                    where R.cedula = @Cedula";

                result.Result = connection.Query<AfRenunciaDetalleHistorico>(query, new { Cedula }).ToList();
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
        /// Obtiene una renuncia por un ID
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRenuncia">Código de renuncia</param>
        /// <returns></returns>
        public ErrorDto<AfRenuncia> AF_CR_Renuncias_ObtenerPorCodigo(int CodEmpresa, long CodRenuncia)
        {
            var result = new ErrorDto<AfRenuncia>()
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
                    select * from afi_cr_renuncias where cod_renuncia = @CodRenuncia";

                result.Result = connection.QueryFirstOrDefault<AfRenuncia>(query, new { CodRenuncia });
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
        /// Guarda la liquidación de una renuncia
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Filtros para guardar la liquidación</param>
        /// <returns></returns>
        public ErrorDto<int> AF_CR_Renuncias_Liquidacion_Guarda(int CodEmpresa, AfRenunciaLiquidacion request)
        {
            var result = new ErrorDto<int>()
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var parameters = new DynamicParameters();

                parameters.Add("@Codigo", request.CodRenuncia);
                parameters.Add("@Cedula", request.Cedula);
                parameters.Add("@Causa", request.IdCausa);
                parameters.Add("@Promotor", request.IdPromotor);
                parameters.Add("@Mortalidad", request.Mortalidad);
                parameters.Add("@Reingreso", request.Reingreso);
                parameters.Add("@AltPlanilla", request.AltPlanilla);
                parameters.Add("@Volver", request.Volver);
                parameters.Add("@AumentoPuntos", request.AumentoPuntos);
                parameters.Add("@AporteObrero", request.AporteObrero);
                parameters.Add("@AportePatronal", request.AportePatronal);
                parameters.Add("@Capitalizacion", request.Capitalizacion);
                parameters.Add("@AhorroExtraordinario", request.AhorroExtraordinario);
                parameters.Add("@AceptaPatronal", request.AceptaPatronal);
                parameters.Add("@Tipo", request.Tipo);
                parameters.Add("@Usuario", request.Usuario);
                parameters.Add("@Notas", request.Notas);
                parameters.Add("@Oficina", request.Oficina);
                parameters.Add("@Documento", request.Documento);
                parameters.Add("@Banco", request.Banco);
                parameters.Add("@Cuenta", request.Cuenta);
                parameters.Add("@CodPlan", request.CodPlan);
                parameters.Add("@TotalNeto", request.TotalNeto);
                parameters.Add("@Disponible", request.Disponible);
                parameters.Add("@RetenerMonto", request.RetenerMonto);
                parameters.Add("@AcFecha", request.AcFecha);
                parameters.Add("@Boleta", request.Boleta);
                parameters.Add("@Equipo", request.Equipo);
                parameters.Add("@Version", request.Version);
                parameters.Add("@IdDocumento", request.IdDocumento);

                // El SP retorna el consecutivo como RenunciaId
                var renunciaId = connection.QuerySingle<int>(
                    "spAFI_Renuncia_Liquidacion_Guarda",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );

                result.Result = renunciaId;
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
        /// Inserta un plan asociado a una renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del plan a insertar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> AF_CR_Renuncias_Plan_Insertar(int CodEmpresa, AfRenunciaPlan request)
        {
            var result = new ErrorDto<bool>()
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO AFI_CR_RENUNCIAS_PLANES
                    (COD_RENUNCIA, COD_CONTRATO, COD_OPERADORA, COD_PLAN, DISPONIBLE, MULTA, REND_PENDIENTE, LIQ_FND, APORTES, RENDIMIENTOS, COD_DIVISA, TIPO_CAMBIO, MARCADA)
                    VALUES (@CodRenuncia, @CodContrato, @CodOperadora, @CodPlan, @Disponible, @Multa, @RendPendiente, 0, @Aportes, @Rendimientos, @CodDivisa, @TipoCambio, @Marcada)";
                connection.Execute(query, new
                {
                    request.CodRenuncia,
                    request.CodContrato,
                    request.CodOperadora,
                    request.CodPlan,
                    request.Disponible,
                    request.Multa,
                    request.RendPendiente,
                    request.Aportes,
                    request.Rendimientos,
                    request.CodDivisa,
                    request.TipoCambio,
                    Marcada = request.Marcada ? 1 : 0
                });
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
        /// Inserta un abono asociado a una renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del abono a insertar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> AF_CR_Renuncias_Abono_Insertar(int CodEmpresa, AfRenunciaAbono request)
        {
            var result = new ErrorDto<bool>()
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO AFI_CR_RENUNCIAS_ABONOS
                    (COD_RENUNCIA, ID_SOLICITUD, CODIGO, ABONO, SALDO, CARGOS, MORA_INTC, MORA_INTM, MORA_PRIN, COD_DIVISA, TIPO_CAMBIO, TIPO, GARANTIA, MARCADO)
                    VALUES (@CodRenuncia, @IdSolicitud, @Codigo, @Abono, @Saldo, @Cargos, @MoraIntC, @MoraIntM, @MoraPrin, @CodDivisa, @TipoCambio, @Tipo, @Garantia, 1)";
                connection.Execute(query, request);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = false;
            }
            return result;
        }
    }
}
