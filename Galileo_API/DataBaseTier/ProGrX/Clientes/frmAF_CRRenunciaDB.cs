using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFCrenunciaDB
    {
        private readonly IConfiguration _config;

        private const string SpBancos = "spCrd_SGT_Bancos";
        private const string SpRenunciaEmiteTDoc = "spAFI_Renuncia_Emite_TDoc";
        private const string SpLiqConsultaPatrimonio = "spAFI_Liq_Consulta_Patrimonio";
        private const string SpExcRentaDetallada = "spExc_Renta_Detallada";
        private const string SpLiquidaListaPlanes = "spAfiLiquidaListaPlanes";
        private const string SpCuentasBancarias = "spSys_Cuentas_Bancarias";
        private const string SpRentaGlobal = "spFnd_Renta_Global";
        private const string SpLiquidacionCreditosPersona = "spAfi_Liquidacion_CreditosPersona";
        private const string SpSinpeNegativo = "spFnd_Sinpe_Negativo";

        private const string SqlRenunciasSocios = @"
                    SELECT Cedula,
                           Nombre,
                           CedulaR
                    FROM dbo.socios
                    WHERE estadoactual IN ('S', 'A')
                    ORDER BY Cedula;";

        private const string SqlRenunciasEstado = @"
                    SELECT S.cedula,
                           S.nombre,
                           S.fechaingreso,
                           S.estadoactual,
                           0 AS Boleta,
                           ISNULL(E.descripcion, '') AS EstadoPersona,
                           dbo.fxAFI_Renuncia_Activa(S.Cedula) AS Valida,
                           dbo.fxCBR_Cobro_Judicial_Indica(S.Cedula) AS CbrJud
                    FROM dbo.socios S
                    INNER JOIN dbo.AFI_ESTADOS_PERSONA E
                        ON S.estadoActual = E.cod_estado
                    WHERE S.cedula = @Cedula;";

        private const string SqlTiposDocumento = @"
                    SELECT Id_Documento AS item,
                           Descripcion AS descripcion
                    FROM dbo.AFI_CR_RENUNCIAS_TIPO_DOCUMENTO;";

        private const string SqlCausaDetalle = @"
                    SELECT mortalidad,
                           liq_alterna,
                           Tipo_Apl,
                           AJUSTE_TASAS
                    FROM dbo.causas_renuncias
                    WHERE id_causa = @Causa;";

        private const string SqlPromotoresActivos = @"
                    SELECT id_Promotor,
                           Nombre
                    FROM dbo.Promotores
                    WHERE Estado = 1
                      AND tipo <> 'C'
                    ORDER BY Nombre;";

        private const string SqlRenunciaActiva = "SELECT dbo.fxAFI_Renuncia_Activa(@Cedula) AS Resultado;";

        private const string SqlRenunciaActivaOtra = "SELECT dbo.fxAFI_Renuncia_Activa_Otra(@Cedula, @Codigo) AS Resultado;";

        private const string SqlSocioExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.socios
                    WHERE cedula = @Cedula;";

        private const string SqlRenunciaPorId = @"
                    SELECT *,
                           dbo.fxSys_Cuentas_Mask(cuenta) AS Cuenta_Desc
                    FROM dbo.vAFI_Renuncias
                    WHERE cod_renuncia = @CodRenuncia;";

        private const string SqlCausasActivas = @"
                    SELECT id_causa AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM dbo.causas_renuncias
                    WHERE ACTIVO = 1
                      AND Tipo_Apl IN ('A', @Tipo);";

        private const string SqlRenunciasHistorico = @"
                    SELECT R.*,
                           RTRIM(C.Descripcion) AS CausaX,
                           S.nombre,
                           ISNULL(P.id_promotor, 0) AS Id_Promotor,
                           ISNULL(P.nombre, 'AFILIACION UNIVERSAL') AS PromotorX
                    FROM dbo.afi_cr_renuncias R
                    INNER JOIN dbo.causas_renuncias C
                        ON R.id_causa = C.id_causa
                    INNER JOIN dbo.Socios S
                        ON R.cedula = S.cedula
                    LEFT JOIN dbo.Promotores P
                        ON R.id_Promotor = P.id_Promotor
                    WHERE R.cedula = @Cedula;";

        private const string SqlRenunciaPorCodigo = @"
                    SELECT *
                    FROM dbo.afi_cr_renuncias
                    WHERE cod_renuncia = @CodRenuncia;";

        public FrmAFCrenunciaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los socios para las renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciasSocios>> AF_CR_RenunciasSocios_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AfRenunciasSocios>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRenunciasSocios);
        }


        /// <summary>
        /// Obtiene el estado de un socio para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <returns></returns>
        public ErrorDto<AfRenunciasSocioDetalle?> AF_CR_Renuncias_Estado_Obtener(int CodEmpresa, string cedula)
        {
            return DbHelper.ExecuteSingleQuery<AfRenunciasSocioDetalle>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRenunciasEstado,
                null,
                new { Cedula = NormalizarTexto(cedula) });
        }


        /// <summary>
        /// Obtiene la lista de bancos disponibles para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtro de usuario y divisa.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaBancos>> AF_CR_Renuncias_Bancos_Obtener(int CodEmpresa, AfRenunciaBancoFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de banco son requeridos.", -2, new List<AfRenunciaBancos>());
            }

            return EjecutarStoredProcedureList<AfRenunciaBancos>(
                CodEmpresa,
                SpBancos,
                new
                {
                    Usuario = NormalizarTexto(filtro.Usuario),
                    Divisa = NormalizarTexto(filtro.Divisa)
                });
        }


        /// <summary>
        /// Obtiene los tipos de documento emitidos para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtro de banco y mortalidad.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaEmiteTDoc>> AF_CR_Renuncias_Emite_TDoc(int CodEmpresa, AfRenunciaEmiteTDocFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de documento son requeridos.", -2, new List<AfRenunciaEmiteTDoc>());
            }

            return EjecutarStoredProcedureList<AfRenunciaEmiteTDoc>(
                CodEmpresa,
                SpRenunciaEmiteTDoc,
                new
                {
                    filtro.BancoId,
                    filtro.Mortalidad
                });
        }


        /// <summary>
        /// Obtiene la lista de tipos de acción para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Renuncias_TipoAccion_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlTiposDocumento);
        }


        /// <summary>
        /// Obtiene el detalle de una causa de renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Causa">ID de la causa.</param>
        /// <returns></returns>
        public ErrorDto<AfRenunciaCausasDetalle?> AF_CR_Renuncias_Causas_ObtenerDetalle(int CodEmpresa, int Causa)
        {
            return DbHelper.ExecuteSingleQuery<AfRenunciaCausasDetalle>(
                CreatePortalDb(),
                CodEmpresa,
                SqlCausaDetalle,
                null,
                new { Causa });
        }


        /// <summary>
        /// Consulta el patrimonio de un socio para liquidación de renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cedula">Cédula del socio.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaLiqConsultaPatrimonio>> AF_CR_Renuncias_Liq_Consulta_Patrimonio(int CodEmpresa, string Cedula)
        {
            return EjecutarStoredProcedureList<AfRenunciaLiqConsultaPatrimonio>(
                CodEmpresa,
                SpLiqConsultaPatrimonio,
                new { Cedula = NormalizarTexto(Cedula) });
        }


        /// <summary>
        /// Obtiene la renta detallada para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Monto">Monto a consultar.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaExcRentaDetallada>> AF_CR_Renuncias_Exc_Renta_Detallada(int CodEmpresa, decimal Monto)
        {
            return EjecutarStoredProcedureList<AfRenunciaExcRentaDetallada>(
                CodEmpresa,
                SpExcRentaDetallada,
                new { Monto });
        }


        /// <summary>
        /// Obtiene la lista de planes para liquidación de renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtro de cédula y tipo de liquidación.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaLiquidaListaPlanes>> AF_CR_Renuncias_Liquida_ListaPlanes(int CodEmpresa, AfRenunciaLiquidaListaPlanesFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de planes son requeridos.", -2, new List<AfRenunciaLiquidaListaPlanes>());
            }

            return EjecutarStoredProcedureList<AfRenunciaLiquidaListaPlanes>(
                CodEmpresa,
                SpLiquidaListaPlanes,
                new
                {
                    Cedula = NormalizarTexto(filtro.Cedula),
                    filtro.TipoLiq
                });
        }


        /// <summary>
        /// Obtiene las cuentas bancarias asociadas a un socio para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtro de identificación, banco y divisa.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaCuentaBancaria>> AF_CR_Renuncias_CuentasBancarias_Obtener(int CodEmpresa, AfRenunciaCuentaBancariaFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de cuenta bancaria son requeridos.", -2, new List<AfRenunciaCuentaBancaria>());
            }

            return EjecutarStoredProcedureList<AfRenunciaCuentaBancaria>(
                CodEmpresa,
                SpCuentasBancarias,
                new
                {
                    Identificacion = NormalizarTexto(filtro.Identificacion),
                    filtro.BancoId,
                    filtro.DivisaCheck
                });
        }


        /// <summary>
        /// Obtiene la lista de promotores activos para renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaPromotor>> AF_CR_Renuncias_Promotores_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AfRenunciaPromotor>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPromotoresActivos);
        }


        /// <summary>
        /// Valida si un socio tiene renuncia activa.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <returns></returns>
        public ErrorDto<int> AF_CR_Renuncias_Activa(int CodEmpresa, string cedula)
        {
            return DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRenunciaActiva,
                0,
                new { Cedula = NormalizarTexto(cedula) });
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
            return DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRenunciaActivaOtra,
                0,
                new
                {
                    Cedula = NormalizarTexto(cedula),
                    Codigo = codigo
                });
        }


        /// <summary>
        /// Valida si un socio existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <returns></returns>
        public ErrorDto<int> AF_CR_Renuncias_Socio_Existe(int CodEmpresa, string cedula)
        {
            return DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                SqlSocioExiste,
                0,
                new { Cedula = NormalizarTexto(cedula) });
        }


        /// <summary>
        /// Obtiene una renuncia por su identificador.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRenuncia">Código de la renuncia.</param>
        /// <returns></returns>
        public ErrorDto<AfRenuncia?> AF_CR_Renuncias_ObtenerPorId(int CodEmpresa, long CodRenuncia)
        {
            return DbHelper.ExecuteSingleQuery<AfRenuncia>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRenunciaPorId,
                null,
                new { CodRenuncia });
        }


        /// <summary>
        /// Obtiene la lista de causas activas para seguimiento de renuncias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Tipo">Tipo de aplicación.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Renuncia_Obtener_Causas(int CodEmpresa, String Tipo)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlCausasActivas,
                new { Tipo = NormalizarTexto(Tipo) });
        }


        /// <summary>
        /// Obtiene los valores de la renta global
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtros cedula, corte, mnt retiro, plan </param>
        /// <returns></returns>
        public ErrorDto<AfRenunciaRentaGlobal> AF_CR_Renuncias_Renta_Global(int CodEmpresa, AfRenunciaRentaGlobalFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse<AfRenunciaRentaGlobal>("Los filtros de renta global son requeridos.", -2, null!);
            }

            return EjecutarStoredProcedureSingle<AfRenunciaRentaGlobal>(
                CodEmpresa,
                SpRentaGlobal,
                new
                {
                    Cedula = NormalizarTexto(filtro.Cedula),
                    filtro.Corte,
                    filtro.MntRetiro,
                    Plan = NormalizarTexto(filtro.Plan)
                });
        }


        /// <summary>
        /// Obtiene la liquidación de créditos
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Filtros para la liquidación de créditos</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaLiquidacionCreditosPersona>> AF_CR_Renuncias_Liquidacion_CreditosPersona(int CodEmpresa, AfRenunciaLiquidacionCreditosPersonaFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de liquidación de créditos son requeridos.", -2, new List<AfRenunciaLiquidacionCreditosPersona>());
            }

            return EjecutarStoredProcedureList<AfRenunciaLiquidacionCreditosPersona>(
                CodEmpresa,
                SpLiquidacionCreditosPersona,
                new
                {
                    Cedula = NormalizarTexto(filtro.Cedula),
                    filtro.Abono
                });
        }


        /// <summary>
        /// Obtiene el monto de sinpe negativo
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cedula">Cédula de una persona</param>
        /// <returns></returns>
        public ErrorDto<AfRenunciaSinpeNegativo> AF_CR_Renuncias_Sinpe_Negativo(int CodEmpresa, string Cedula)
        {
            return EjecutarStoredProcedureSingle<AfRenunciaSinpeNegativo>(
                CodEmpresa,
                SpSinpeNegativo,
                new { Cedula = NormalizarTexto(Cedula) });
        }


        /// <summary>
        /// Obtiene el historico de renuncias de una persona
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cedula">Cédyla de una persona</param>
        /// <returns></returns>
        public ErrorDto<List<AfRenunciaDetalleHistorico>> AF_CR_Renuncias_ObtenerHistorico(int CodEmpresa, string Cedula)
        {
            return DbHelper.ExecuteListQuery<AfRenunciaDetalleHistorico>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRenunciasHistorico,
                new { Cedula = NormalizarTexto(Cedula) });
        }


        /// <summary>
        /// Obtiene una renuncia por un ID
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRenuncia">Código de renuncia</param>
        /// <returns></returns>
        public ErrorDto<AfRenuncia?> AF_CR_Renuncias_ObtenerPorCodigo(int CodEmpresa, long CodRenuncia)
        {
            return DbHelper.ExecuteSingleQuery<AfRenuncia>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRenunciaPorCodigo,
                null,
                new { CodRenuncia });
        }


        /// <summary>
        /// Ejecuta un procedimiento almacenado que retorna una lista.
        /// </summary>
        private ErrorDto<List<T>> EjecutarStoredProcedureList<T>(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<T>(
                    storedProcedure,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al ejecutar procedimiento almacenado.",
                    result.Code.GetValueOrDefault(-1),
                    new List<T>());
        }


        /// <summary>
        /// Ejecuta un procedimiento almacenado que retorna un registro.
        /// </summary>
        private ErrorDto<T> EjecutarStoredProcedureSingle<T>(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.QueryFirstOrDefault<T>(
                    storedProcedure,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure));

            return result.Code == 0

        ? DbHelper.CreateOkResponse<T>(result.Result!)
        : DbHelper.CreateErrorResponse<T>(
        result.Description ?? "Error al ejecutar procedimiento almacenado.",
        result.Code.GetValueOrDefault(-1),
        default!);
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();


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