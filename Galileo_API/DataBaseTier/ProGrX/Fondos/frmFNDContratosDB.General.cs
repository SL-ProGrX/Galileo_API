using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {
        private const string SpContratoConsulta = "spFnd_Contrato_Consulta";
        private const string SpInversionPlazos = "spFnd_Inversion_Plazos";
        private const string SpContratoNotificaEmail = "spFnd_Contrato_Notifica_Email";
        private const string SpCdpCupones = "spFndCDPCupones";
        private const string SpCuponFrecuenciaMeses = "spFnd_Cupon_Frecuencia_Meses";
        private const string SpCuponFrecuencia = "spFnd_Cupon_Frecuencia";
        private const string SpInversionPlazosDias = "spFnd_Inversion_Plazos_Dias";

        private const string SqlListaOperadoras = @"
                    SELECT RTRIM(descripcion) AS descripcion,
                           cod_operadora AS item
                    FROM dbo.FND_Operadoras;";

        private const string SqlListaVendedores = @"
                    SELECT RTRIM(nombre) AS descripcion,
                           cod_vendedor AS item
                    FROM dbo.FND_vendedores;";

        private const string SqlListaBancos = @"
                    SELECT B.id_Banco AS item,
                           RTRIM(B.descripcion) AS descripcion
                    FROM dbo.Tes_Bancos B
                    INNER JOIN dbo.FND_BANCOS_X X
                        ON B.id_Banco = X.Id_Banco
                    WHERE B.Estado = 'A'
                      AND (X.Cheque = 1 OR X.Transferencia = 1);";

        private const string SqlListaCuponFrecuencia = @"
                    SELECT ID_FRECUENCIACUPON AS item,
                           dbo.fxSys_Cadena_Capitaliza(CUPON) AS descripcion
                    FROM dbo.FND_CDP_FRECUENCIACUPONES
                    WHERE Estado = 1
                    ORDER BY FRECUENCIA_DIAS ASC;";

        private const string SqlListaPlazoInversion = @"
                    SELECT ID_PLAZO AS item,
                           dbo.fxSys_Cadena_Capitaliza(PLAZO) AS descripcion
                    FROM dbo.FND_CDP_PLAZOS
                    WHERE Estado = 1
                    ORDER BY PLAZO_DIAS ASC;";

        private const string SqlPlanesUsuario = @"
                    SELECT cod_plan AS item,
                           descripcion
                    FROM dbo.fnd_planes
                    WHERE Cod_operadora = @operadora
                      AND (@EsPedro = 1 OR dbo.fxFnd_Seguridad_Acceso_Planes(@usuario, cod_operadora, cod_plan) = 1);";

        private const string SqlPlanDetalle = @"
                    SELECT Descripcion,
                           Monto_Minimo,
                           Plazo_Minimo,
                           cuenta_maestra,
                           Inversion_minimo,
                           Tipo_CDP,
                           WEB_VENCE,
                           ISNULL(PERMITE_GIRO_TERCEROS, 0) AS PlanPermiteGT,
                           cod_moneda,
                           PAGO_CUPONES,
                           TASA_MARGEN_NEGOCIACION,
                           dbo.MyGetDate() AS FechaServidor,
                           TIPO_DEDUC,
                           PORC_DEDUC,
                           DEDUCIR_PLANILLA,
                           SubCuentasMax
                    FROM dbo.fnd_Planes
                    WHERE cod_operadora = @operadora
                      AND cod_plan = @cod_plan;";

        private const string SqlContratosTotal = @"
                    SELECT COUNT(COD_CONTRATO)
                    FROM dbo.fnd_Contratos
                    WHERE cod_operadora = @operadora
                      AND cod_plan = @codigo
                      AND (@hasFilter = 0 OR
                          (COD_PLAN LIKE @filtro OR
                           CONVERT(varchar(50), COD_CONTRATO) LIKE @filtro OR
                           CEDULA LIKE @filtro));";

        private const string SqlContratosBuscar = @"
                    SELECT COD_PLAN,
                           COD_CONTRATO,
                           CEDULA,
                           FECHA_INICIO
                    FROM dbo.fnd_Contratos
                    WHERE cod_operadora = @operadora
                      AND cod_plan = @codigo
                      AND (@hasFilter = 0 OR
                          (COD_PLAN LIKE @filtro OR
                           CONVERT(varchar(50), COD_CONTRATO) LIKE @filtro OR
                           CEDULA LIKE @filtro))
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN COD_CONTRATO END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN COD_CONTRATO END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN COD_PLAN END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN COD_PLAN END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN CEDULA END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN CEDULA END DESC,
                        COD_CONTRATO ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlDeleteContrato = @"
                    DELETE dbo.FND_contratos
                    WHERE cod_contrato = @contrato
                      AND cod_operadora = @operadora
                      AND cod_plan = @cod_plan;";

        private const string SqlTasaPlus = @"
                    SELECT tasa_base,
                           UTILIZA_TBP,
                           TIPO_CDP,
                           dbo.fxFNDTasaPlus(cod_operadora, cod_plan, @plazo) AS PlusTasa
                    FROM dbo.fnd_planes
                    WHERE cod_operadora = @operadora
                      AND cod_plan = @plan;";

        private const string SqlSociosTotal = @"
                    SELECT COUNT(cedula)
                    FROM dbo.socios
                    WHERE @hasFilter = 0 OR
                          (cedula LIKE @filtro OR nombre LIKE @filtro);";

        private const string SqlSociosBuscar = @"
                    SELECT cedula AS item,
                           nombre AS descripcion
                    FROM dbo.socios
                    WHERE @hasFilter = 0 OR
                          (cedula LIKE @filtro OR nombre LIKE @filtro)
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN cedula END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN cedula END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN nombre END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN nombre END DESC,
                        cedula ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private static readonly IReadOnlyDictionary<string, string> ContratosListasSql = new Dictionary<string, string>
        {
            ["cboOperadora"] = SqlListaOperadoras,
            ["cboVendedor"] = SqlListaVendedores,
            ["cboBanco"] = SqlListaBancos,
            ["cboCuponFrecuencia"] = SqlListaCuponFrecuencia,
            ["cboPlazoInversion"] = SqlListaPlazoInversion
        };

        private static readonly IReadOnlyDictionary<string, int> ContratosSortMap = new Dictionary<string, int>
        {
            ["COD_CONTRATO"] = 1,
            ["COD_PLAN"] = 2,
            ["CEDULA"] = 3
        };

        private static readonly IReadOnlyDictionary<string, int> SociosSortMap = new Dictionary<string, int>
        {
            ["cedula"] = 1,
            ["nombre"] = 2
        };
       
        #region General
        /// <summary>
        /// Obtiene las listas genéricas utilizadas por el formulario de contratos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="lista">Nombre lógico de la lista solicitada.</param>
        /// <returns>Listado de elementos para combos del formulario.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_Listas_Obtener(int CodEmpresa, string lista)
        {
            if (!ContratosListasSql.TryGetValue(NormalizarTexto(lista), out var sql))
            {
                return DbHelper.CreateErrorResponse(
                    "La lista solicitada no es válida.",
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene los datos principales de un contrato de fondos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="cod_plan">Código del plan.</param>
        /// <param name="contrato">Número de contrato.</param>
        /// <param name="usuario">Usuario que consulta el contrato.</param>
        /// <returns>Datos del contrato solicitado.</returns>
        public ErrorDto<ContratosModels> Fnd_Contratos_Obtener(int CodEmpresa, int operadora, string cod_plan, int contrato, string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<ContratosModels>(
                    SpContratoConsulta,
                    new
                    {
                        Operadora = operadora,
                        Plan = NormalizarTexto(cod_plan),
                        Contrato = contrato,
                        Usuario = NormalizarTexto(usuario)
                    },
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al consultar contrato.",
                    result.Code.GetValueOrDefault(-1),
                    new ContratosModels());
            }

            var contratoResult = result.Result ?? new ContratosModels();
            contratoResult.aplicaBeneficiarios = fxAplicaBeneficiarios(CodEmpresa, cod_plan, operadora).Result;
            return DbHelper.CreateOkResponse(contratoResult);
        }
        /// <summary>
        /// Obtiene los planes disponibles para una operadora y usuario.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="usuario">Usuario que consulta los planes.</param>
        /// <returns>Listado de planes autorizados para el usuario.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_PlanLista_Obtener(int CodEmpresa, int operadora, string usuario)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPlanesUsuario,
                new
                {
                    operadora,
                    usuario = NormalizarTexto(usuario),
                    EsPedro = string.Equals(NormalizarTexto(usuario), "PEDRO", StringComparison.OrdinalIgnoreCase) ? 1 : 0
                });
        }
        /// <summary>
        /// Obtiene la configuración de un plan de fondos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="plan">Código del plan.</param>
        /// <returns>Datos configurados para el plan solicitado.</returns>
        public ErrorDto<ContratosPlanModels> Fnd_Contratos_Plan_Obtener(int CodEmpresa, int operadora, string plan)
        {
            var result = DbHelper.ExecuteSingleQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlPlanDetalle,
                new ContratosPlanModels(),
                new
                {
                    operadora,
                    cod_plan = NormalizarTexto(plan)
                });

            return new ErrorDto<ContratosPlanModels>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new ContratosPlanModels()
            };
        }

        /// <summary>
        /// Obtiene los plazos de inversión disponibles para un plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="codigo">Código del plan o catálogo de plazo.</param>
        /// <returns>Listado de plazos de inversión.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_InversionPlazos_Obtener(int CodEmpresa, string codigo)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query(
                    SpInversionPlazos,
                    new { Plan = NormalizarTexto(codigo) },
                    commandType: System.Data.CommandType.StoredProcedure)
                .Select(r => new DropDownListaGenericaModel
                {
                    item = Convert.ToString(r.IdX) ?? string.Empty,
                    descripcion = Convert.ToString(r.ItmX) ?? string.Empty
                }).ToList());

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<DropDownListaGenericaModel>()
            };
        }

        /// <summary>
        /// Busca contratos por operadora, plan y filtros de paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="plan">Código del plan.</param>
        /// <param name="filtros">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Listado paginado de contratos.</returns>
        public ErrorDto<FndContratosListaData> Fnd_Contratos_Buscar(int CodEmpresa, int operadora, string plan, Models.FiltrosLazyLoadData filtros)
        {
            var response = DbHelper.CreateOkResponse(new FndContratosListaData
            {
                total = 0,
                lineas = new List<FndContratosModels>()
            });

            try
            {
                var spec = LazyLoadHelper.Build(filtros, ContratosSortMap, "COD_CONTRATO");
                var parametros = CrearParametrosContratosBusqueda(operadora, plan, spec);

                var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new FndContratosListaData
                {
                    total = connection.QueryFirstOrDefault<int>(SqlContratosTotal, parametros),
                    lineas = connection.Query<FndContratosModels>(SqlContratosBuscar, parametros).ToList()
                });

                if (result.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        result.Description ?? "Error al buscar contratos.",
                        result.Code.GetValueOrDefault(-1),
                        new FndContratosListaData { total = 0, lineas = new List<FndContratosModels>() });
                }

                response.Result = result.Result ?? new FndContratosListaData { total = 0, lineas = new List<FndContratosModels>() };
            }
            catch (Exception ex)
            {
                response = DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndContratosListaData { total = 0, lineas = new List<FndContratosModels>() });
            }

            return response;
        }

        /// <summary>
        /// Envía el correo de solicitud asociado a un contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="codigo">Código del plan.</param>
        /// <param name="contrato">Número de contrato.</param>
        /// <param name="usuario">Usuario que ejecuta el envío.</param>
        /// <returns>Mensaje resultante del envío.</returns>
        public ErrorDto<string> Fnd_Contratos_Email_Enviar(int CodEmpresa, int operadora, string codigo, int contrato, string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<dynamic>(
                    SpContratoNotificaEmail,
                    new
                    {
                        operadora,
                        codigo = NormalizarTexto(codigo),
                        contrato,
                        usuario = NormalizarTexto(usuario)
                    },
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al enviar correo.", result.Code.GetValueOrDefault(-1), string.Empty);
            }

            if (result.Result is null)
            {
                return DbHelper.CreateErrorResponse("El procedimiento no devolvió resultados.", -1, string.Empty);
            }

            string mensaje = result.Result.Pass == 1
                ? "Correo de Solicitud de Contrato enviado a la persona."
                : Convert.ToString(result.Result.Mensaje) ?? "No se pudo enviar el correo.";

            return DbHelper.CreateOkResponse(mensaje);
        }


        /// <summary>
        /// Guarda un contrato nuevo o actualiza un contrato existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="vCambios">Datos de control para cambios del contrato.</param>
        /// <param name="contrato">Datos del contrato a guardar.</param>
        /// <returns>Resultado de la operación de guardado.</returns>
        public ErrorDto Fnd_Contratos_Guardar(int CodEmpresa, string usuario, FndCambios vCambios, ContratosModels contrato)
        {
            var response = fxVerificaDatos(CodEmpresa, contrato);

            if (response.Code == -1)
            {
                return response;
            }
            else
            {
                if (contrato.inversion > 0)
                {
                    // pPlazoInversionId = contrato.plazo_id ?? "Null"; // Removed useless assignment

                    if (contrato.pago_cuponescdp == true)
                    {
                        pCuponPaga = "1";
                        pCuponFrecuencia = contrato.cupon_frecuencia ?? string.Empty;
                        pCuponFrecuenciaId = contrato.idcupon_frecuencia ?? "Null";
                    }
                    else
                    {
                        pCuponPaga = "0";
                        pCuponFrecuencia = "N";
                        pCuponFrecuenciaId = "Null";
                    }
                }
                else
                {
                    pCuponFrecuencia = "N";
                    pCuponFrecuenciaId = "Null";
                    // pPlazoInversionId = "Null"; // Removed useless assignment
                    pCuponPaga = "0";
                }

                response = !contrato.isNew
                    ? actualizarContrato(CodEmpresa, usuario, vCambios, contrato)
                    : insertarContrato(CodEmpresa, usuario, contrato);

                if (response!.Code == 0 && contrato.tipo_cdp == true)
                {
                    DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                    {
                        connection.Execute(
                            SpCdpCupones,
                            new
                            {
                                contrato.cod_operadora,
                                cod_plan = NormalizarTexto(contrato.cod_plan),
                                contrato.cod_contrato,
                                usuario = NormalizarTexto(usuario)
                            },
                            commandType: System.Data.CommandType.StoredProcedure);

                        return true;
                    });
                }
            }

            return response;

        }


        /// <summary>
        /// Elimina un contrato de fondos y registra la operación en bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="codigo">Código del plan.</param>
        /// <param name="contrato">Número de contrato.</param>
        /// <param name="usuario">Usuario que realiza la eliminación.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto Fnd_Contratos_Borrar(int CodEmpresa, int operadora, string codigo, int contrato, string usuario)
        {
            var response = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlDeleteContrato,
                new
                {
                    contrato,
                    operadora,
                    cod_plan = NormalizarTexto(codigo)
                });

            if (response.Code == 0)
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = NormalizarTexto(usuario),
                    DetalleMovimiento = $"Contrato: {contrato}  Plan: {NormalizarTexto(codigo)}  Oper: {operadora}",
                    Movimiento = "Borra - WEB",
                    Modulo = vModulo
                });
            }

            return response;
        }

        /// <summary>
        /// Obtiene la cantidad de meses asociada a una frecuencia de cupón.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CuponFrecuencia">Código de frecuencia de cupón.</param>
        /// <returns>Cantidad de meses configurada para la frecuencia.</returns>
        public ErrorDto<int> Fnd_Contratos_FrecuenciaMeses_Obtener(int CodEmpresa, string CuponFrecuencia)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<int>(
                    SpCuponFrecuenciaMeses,
                    new { CuponFrecuencia = NormalizarTexto(CuponFrecuencia) },
                    commandType: System.Data.CommandType.StoredProcedure));

            return new ErrorDto<int>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result
            };
        }


        /// <summary>
        /// Obtiene las frecuencias de cupón disponibles para un plazo y plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="plazo_id">Identificador del plazo.</param>
        /// <param name="plan">Código del plan.</param>
        /// <returns>Listado de frecuencias de cupón.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_spFnd_Cupon_Frecuencia(int CodEmpresa, string plazo_id, string plan)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query(
                    SpCuponFrecuencia,
                    new
                    {
                        plazo_id = NormalizarTexto(plazo_id),
                        plan = NormalizarTexto(plan)
                    },
                    commandType: System.Data.CommandType.StoredProcedure)
                .Select(r => new DropDownListaGenericaModel
                {
                    item = Convert.ToString(r.IdX) ?? string.Empty,
                    descripcion = Convert.ToString(r.ItmX) ?? string.Empty
                }).ToList());

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<DropDownListaGenericaModel>()
            };
        }

        /// <summary>
        /// Obtiene la duración en días o meses de un plazo de inversión.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="plazo_inversion">Identificador del plazo de inversión.</param>
        /// <param name="cboPlazo">Tipo de plazo solicitado.</param>
        /// <returns>Valor del plazo según el tipo indicado.</returns>
        public ErrorDto<int> Fnd_Contratos_spFnd_Inversion_Plazos_Dias(int CodEmpresa, int plazo_inversion, string cboPlazo)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<dynamic>(
                    SpInversionPlazosDias,
                    new { PlazoId = plazo_inversion },
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener plazo de inversión.", result.Code.GetValueOrDefault(-1), 0);
            }

            if (result.Result is null)
            {
                return DbHelper.CreateErrorResponse("No se encontró información para el plazo indicado.", -1, -1);
            }

            var tipoPlazo = NormalizarTexto(cboPlazo).ToUpperInvariant();
            int plazo = tipoPlazo == "D" ? (int)result.Result.PLAZO_DIAS : (int)result.Result.PLAZO_MESES;
            return DbHelper.CreateOkResponse(plazo);
        }

        /// <summary>
        /// Obtiene la tasa adicional aplicable según plazo, tipo, plan y operadora.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="xPlazo">Plazo solicitado.</param>
        /// <param name="xTipo">Tipo de plazo.</param>
        /// <param name="xPlan">Código del plan.</param>
        /// <param name="xOperadora">Código de operadora.</param>
        /// <returns>Tasa adicional calculada.</returns>
        public ErrorDto<decimal> Fnd_Contratos_fxTasaPtsAdd(int CodEmpresa, long xPlazo, string xTipo, string xPlan, string xOperadora)
        {
            var plazo = string.Equals(NormalizarTexto(xTipo), "M", StringComparison.OrdinalIgnoreCase)
                ? xPlazo * 30
                : xPlazo;

            var result = DbHelper.ExecuteSingleQuery<dynamic>(
                CreatePortalDb(),
                CodEmpresa,
                SqlTasaPlus,
                default,
                new
                {
                    operadora = NormalizarTexto(xOperadora),
                    plan = NormalizarTexto(xPlan),
                    plazo
                });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener tasa plus.", result.Code.GetValueOrDefault(-1), 0m);
            }

            decimal tasa = result.Result is null ? 0m : Convert.ToDecimal(result.Result.PlusTasa);
            return DbHelper.CreateOkResponse(tasa);
        }

        /// <summary>
        /// Obtiene socios usando filtros de búsqueda, ordenamiento y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Listado paginado de socios.</returns>
        public ErrorDto<FndSociosListaData> Fnd_ContratosSocios_Obtener(int CodEmpresa, Models.FiltrosLazyLoadData filtros)
        {
            var response = DbHelper.CreateOkResponse(new FndSociosListaData
            {
                total = 0,
                socios = new List<DropDownListaGenericaModel>()
            });

            try
            {
                var spec = LazyLoadHelper.Build(filtros, SociosSortMap, "cedula");
                var parametros = CrearParametrosSociosBusqueda(spec);

                var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new FndSociosListaData
                {
                    total = connection.QueryFirstOrDefault<int>(SqlSociosTotal, parametros),
                    socios = connection.Query<DropDownListaGenericaModel>(SqlSociosBuscar, parametros).ToList()
                });

                if (result.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        result.Description ?? "Error al obtener socios.",
                        result.Code.GetValueOrDefault(-1),
                        new FndSociosListaData { total = 0, socios = new List<DropDownListaGenericaModel>() });
                }

                response.Result = result.Result ?? new FndSociosListaData { total = 0, socios = new List<DropDownListaGenericaModel>() };
            }
            catch (Exception ex)
            {
                response = DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndSociosListaData { total = 0, socios = new List<DropDownListaGenericaModel>() });
            }

            return response;
        }

        #endregion

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea los parámetros seguros para la búsqueda paginada de contratos.
        /// </summary>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="plan">Código de plan.</param>
        /// <param name="spec">Especificación de lazy load ya validada.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosContratosBusqueda(int operadora, string plan, LazyLoadSpec spec)
        {
            return new
            {
                operadora,
                codigo = NormalizarTexto(plan),
                hasFilter = spec.HasFilter ? 1 : 0,
                filtro = spec.HasFilter ? spec.Params.Get<string>("@filtro") : null,
                sortCode = spec.SortCode,
                isAsc = spec.IsAsc ? 1 : 0,
                offset = spec.Offset,
                fetch = spec.PageSize
            };
        }

        /// <summary>
        /// Crea los parámetros seguros para la búsqueda paginada de socios.
        /// </summary>
        /// <param name="spec">Especificación de lazy load ya validada.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosSociosBusqueda(LazyLoadSpec spec)
        {
            return new
            {
                hasFilter = spec.HasFilter ? 1 : 0,
                filtro = spec.HasFilter ? spec.Params.Get<string>("@filtro") : null,
                sortCode = spec.SortCode,
                isAsc = spec.IsAsc ? 1 : 0,
                offset = spec.Offset,
                fetch = spec.PageSize
            };
        }

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

    }
}