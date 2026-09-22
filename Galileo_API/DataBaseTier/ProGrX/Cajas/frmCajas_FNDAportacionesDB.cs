using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using System.Data;

namespace Galileo.DataBaseTier
{
    public partial class FrmCajasFndaportacionesDB
    {
        private readonly PortalDB _portalDb;
        private readonly MCajas _mCajas;
        private readonly MFndFuncionesDb _mFndFunciones;

        public FrmCajasFndaportacionesDB(IConfiguration config)
        {
            var safeConfig = config ?? throw new ArgumentNullException(nameof(config));
            _portalDb = new PortalDB(safeConfig);
            _mCajas = new MCajas(safeConfig);
            _mFndFunciones = new MFndFuncionesDb(safeConfig);
        }

        /// <summary>
        /// Obtener los tipos de documentos.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codCaja">Codigo de caja.</param>
        /// <returns>Lista de documentos habilitados para la caja.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            const string sql = @"
                    SELECT 
                        RTRIM(C.tipo_documento) AS item,
                        RTRIM(D.Descripcion)    AS descripcion
                    FROM SIF_DOCUMENTOS D
                    INNER JOIN CAJAS_DOCUMENTOS C 
                        ON D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO
                    WHERE C.cod_caja = @cod_caja
                      AND D.Tipo_Movimiento IN ('A', 'D')
                    ORDER BY C.tipo_documento;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_caja = codCaja });
        }

        /// <summary>
        /// Aplicar el aporte a fondos.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Datos del aporte.</param>
        /// <returns>Resultado de la aplicacion del aporte.</returns>
        public ErrorDto Fondos_Aporte_Aplicar(int codEmpresa, FondosAporteAplicarDto request)
        {
            try
            {

                var vTipoDoc = string.IsNullOrWhiteSpace(request.tipodoc);
                if(vTipoDoc == false)
                {
                    return new ErrorDto { Code = -1, Description = "El tipo de documento es requerido." };
                }

                var validacion = ValidarAporte(codEmpresa, request);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                var codOficina = ObtenerCodigoOficina(codEmpresa, request);
                var aplica = Cajas_FNDAportaciones_Aporte_Registrar(codEmpresa, request, codOficina);

                if (aplica.Code != 0)
                {
                    return new ErrorDto { Code = aplica.Code, Description = aplica.Description };
                }

                if (aplica.Result?.Pass != 1)
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = aplica.Result?.Mensaje ?? "No se pudo aplicar el aporte."
                    };
                }

                return new ErrorDto
                {
                    Code = 0,
                    Description = aplica.Result.NumDoc
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = $"Error al aplicar aporte: {ex.Message}"
                };
            }
        }


        private ErrorDto ValidarAporte(int codEmpresa, FondosAporteAplicarDto request)
        {
            var contrato = ObtenerContratoValidacion(codEmpresa, request);
            if (contrato.Code != 0)
            {
                return new ErrorDto { Code = contrato.Code, Description = contrato.Description };
            }

            if (contrato.Result == null)
            {
                return new ErrorDto { Code = -1, Description = "No se encontraron datos del contrato de fondos." };
            }

            if (contrato.Result.Permite_Mov_Cajas == 0)
            {
                return new ErrorDto { Code = -1, Description = "Este plan no permite movimientos en Cajas, verifique..." };
            }

            if (string.Equals(contrato.Result.Estado?.Trim(), "L", StringComparison.OrdinalIgnoreCase))
            {
                return new ErrorDto { Code = -1, Description = "Este contrato se encuentra Liquidado, verifique..." };
            }

            if (request.aporte <= 0 || request.totalcajas <= 0)
            {
                return new ErrorDto { Code = -1, Description = "No se especificó ningún aporte, verifique..." };
            }

            if (request.aporte != request.totalcajas)
            {
                return new ErrorDto { Code = -1, Description = "El Monto en Cajas es diferente al Aporte a Registrar, verifique..." };
            }

            if (_mCajas.fxCajasAperturaEstado(codEmpresa, request.caja ?? string.Empty, request.apertura) == "C")
            {
                return new ErrorDto { Code = -1, Description = $"- La apertura ..:{request.apertura} de esta caja ha sido cerrada!" };
            }

            var seguridad = Cajas_FNDAportaciones_Seguridad_Validar(codEmpresa, request);
            if (seguridad.Code != 0)
            {
                return seguridad;
            }

            return ValidarTransaccionCajas(codEmpresa, request);
        }

        private ErrorDto<FondosContratoValidacionDto?> ObtenerContratoValidacion(int codEmpresa, FondosAporteAplicarDto request)
        {
            const string sql = @"
                SELECT
                    C.estado,
                    P.Permite_Mov_Cajas
                FROM dbo.fnd_contratos C
                INNER JOIN dbo.fnd_planes P
                    ON C.cod_plan = P.cod_plan
                   AND C.cod_operadora = P.cod_operadora
                WHERE C.cod_operadora = @Operadora
                  AND C.cod_plan = @Plan
                  AND C.cod_contrato = @Contrato;";

            return DbHelper.ExecuteSingleQuery<FondosContratoValidacionDto?>(
                _portalDb,
                codEmpresa,
                sql,
                default,
                new
                {
                    Operadora = request.operadora,
                    Plan = request.plan,
                    Contrato = request.contrato
                });
        }

        private ErrorDto ValidarTransaccionCajas(int codEmpresa, FondosAporteAplicarDto request)
        {
            var validacion = DbHelper.ExecuteSingleQuery<CajasTransacValidacionResult?>(
                _portalDb,
                codEmpresa,
                @"EXEC spCajas_Transac_Validacion
                    @Caja,
                    @Usuario,
                    @Apertura,
                    @SesionId,
                    @TipoProc,
                    @Producto,
                    @Monto,
                    @Ticket;",
                default,
                new
                {
                    Caja = request.caja,
                    Usuario = request.usuario,
                    Apertura = request.apertura,
                    SesionId = request.sesionid,
                    TipoProc = "Fnd",
                    Producto = request.plan?.Trim(),
                    Monto = request.totalcajas,
                    Ticket = request.tiquete
                });

            if (validacion.Code != 0)
            {
                return new ErrorDto { Code = validacion.Code, Description = validacion.Description };
            }

            if (!string.IsNullOrWhiteSpace(validacion.Result?.Validacion))
            {
                return new ErrorDto { Code = -1, Description = validacion.Result.Validacion };
            }

            return new ErrorDto { Code = 0, Description = validacion.Result?.Advertencias ?? "Ok" };
        }

        private string ObtenerCodigoOficina(int codEmpresa, FondosAporteAplicarDto request)
        {
            const string sql = @"
                SELECT TOP 1 C.cod_oficina
                FROM CAJAS_USUARIOS Cu
                INNER JOIN cajas_definicion C
                    ON Cu.cod_caja = C.cod_caja
                WHERE Cu.usuario = @Usuario
                  AND Cu.Cod_Caja = @Caja;";

            return DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                string.Empty,
                new { Usuario = request.usuario, Caja = request.caja }).Result ?? string.Empty;
        }



        /// <summary>
        /// Verifica si el aporte requiere autorizacion.
        /// </summary>
        /// <param name="codempresa">Codigo de empresa.</param>
        /// <param name="plan">Codigo de plan.</param>
        /// <param name="usuario">Usuario que solicita el aporte.</param>
        /// <param name="aporte">Monto del aporte.</param>
        /// <returns>Resultado de la validacion de autorizacion.</returns>
        public ErrorDto<FondosRequiereAutorizacionDto> Fondos_Aporte_RequiereAutorizacion(int codempresa, string plan, string usuario, decimal aporte)
        {
            return Cajas_FNDAportaciones_Autorizacion_Consultar(codempresa, new FondosGestionRegistroAddDto
            {
                plan = plan, usuario = usuario, aporte = aporte,
                contrato = 0, montoautorizado = 0
            });
        }

        /// <summary>
        /// Verifica el estado de la gestion.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="gestionId">Identificador de la gestion.</param>
        /// <returns>Estado actual de la gestion.</returns>
        public ErrorDto<GestionEstadoDto> Fondos_Gestion_Estado(int codEmpresa, int gestionId)
        {
            var response = DbHelper.CreateOkResponse<GestionEstadoDto>(default);

            try
            {
                var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
                var result = DbHelper.ExecuteStoredProcedureSingle<GestionEstadoDto>(
                    connectionString,
                    "spFnd_Gestion_Estado",
                    default,
                    new { GestionId = gestionId });

                response.Code = result.Code;
                response.Description = result.Code == 0 ? "Ok" : $"Error al consultar estado de gestión: {result.Description}";
                response.Result = result.Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al consultar estado de gestión: {ex.Message}";
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Registra la gestion.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa.</param>
        /// <param name="request">Datos de la gestion.</param>
        /// <returns>Gestion registrada.</returns>
        public ErrorDto<FondosGestionRegistroDto> fondos_gestion_registro(int CodEmpresa, FondosGestionRegistroAddDto request)
        {
            var response = DbHelper.CreateOkResponse<FondosGestionRegistroDto>(default);

            try
            {
                var autorizacion = Cajas_FNDAportaciones_Autorizacion_Consultar(CodEmpresa, request);
                if (autorizacion.Code != 0 || autorizacion.Result == null)
                    return DbHelper.CreateErrorResponse<FondosGestionRegistroDto>(autorizacion.Description ?? "No se pudo consultar la autorización.");
                if (!autorizacion.Result.autorizado || !autorizacion.Result.requiere || request.aporte <= 0)
                    return DbHelper.CreateErrorResponse<FondosGestionRegistroDto>("El aporte no requiere una gestión o el usuario no está autorizado.");
                if (string.IsNullOrWhiteSpace(request.nota) || request.nota.Trim().Length < 30)
                    return DbHelper.CreateErrorResponse<FondosGestionRegistroDto>("La anotación de la gestión debe tener al menos 30 caracteres.");

                const string sql = @"
                    EXEC spFnd_Gestion_Registro
                        @Cedula,
                        @Tipo,
                        @Operadora,
                        @Plan,
                        @Contrato,
                        @MntSol,
                        @MntCal,
                        @Usuario,
                        @GestionNota;";

                var result = DbHelper.ExecuteSingleQuery<FondosGestionRegistroDto>(
                    _portalDb,
                    CodEmpresa,
                    sql,
                    default,
                    new
                    {
                        Cedula = request.cedula,
                        Tipo = request.tipo,
                        Operadora = request.operadora,
                        Plan = request.plan,
                        Contrato = request.contrato,
                        MntSol = autorizacion.Result.montomaximo,
                        MntCal = request.aporte,
                        Usuario = request.usuario,
                        GestionNota = request.nota
                    });

                if (result.Code != 0)
                {
                    response.Code = -1;
                    response.Description = $"error en registro de gestión: {result.Description}";
                    response.Result = null;
                    return response;
                }

                if (result.Result == null)
                {
                    response.Code = -1;
                    response.Description = "no se pudo registrar la gestión";
                    response.Result = null;
                    return response;
                }

                response.Result = result.Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"error en registro de gestión: {ex.Message}";
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Obtiene los datos del contrato de fondos usados al abrir aportaciones.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta la informacion.</param>
        /// <param name="codCaja">Codigo de caja para validar el concepto auxiliar.</param>
        /// <param name="operadora">Codigo de operadora del contrato.</param>
        /// <param name="plan">Codigo de plan del contrato.</param>
        /// <param name="contrato">Codigo de contrato de fondos.</param>
        /// <returns>Datos generales del contrato de fondos.</returns>
        public ErrorDto<FondosContratoDatosDto> Fondos_Contrato_Datos_Obtener(
            int codEmpresa,
            string codCaja,
            int operadora,
            string plan,
            int contrato)
        {
            const string sql = @"
                SELECT
                    C.cedula,
                    S.nombre,
                    P.descripcion AS plan_desc,
                    O.descripcion AS operadora_desc,
                    C.monto,
                    C.cod_plan,
                    C.cod_contrato,
                    C.cod_operadora,
                    C.estado,
                    C.fecha_inicio,
                    P.cod_moneda,
                    C.aportes,
                    C.inversion,
                    P.tipo_cdp,
                    ISNULL(P.cuenta_maestra, 0) AS cuenta_maestra,
                    P.permite_mov_cajas,
                    dbo.fxCajas_Valida_Auxiliar(@CodCaja, 'FND', C.cod_plan) AS caja_valida_concepto
                FROM fnd_contratos C
                INNER JOIN socios S
                    ON C.cedula = S.cedula
                INNER JOIN fnd_planes P
                    ON C.cod_plan = P.cod_plan
                   AND C.cod_operadora = P.cod_operadora
                INNER JOIN fnd_operadoras O
                    ON C.cod_operadora = O.cod_operadora
                WHERE C.cod_operadora = @Operadora
                  AND C.cod_plan = @Plan
                  AND C.cod_contrato = @Contrato;";

            var result = DbHelper.ExecuteSingleQuery<FondosContratoDatosDto?>(
                _portalDb,
                codEmpresa,
                sql,
                default,
                new
                {
                    CodCaja = codCaja,
                    Operadora = operadora,
                    Plan = plan,
                    Contrato = contrato
                });

            return new ErrorDto<FondosContratoDatosDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FondosContratoDatosDto()
            };
        }


        /// <summary>
        /// Obtiene las subcuentas.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa.</param>
        /// <param name="operadora">Codigo de operadora.</param>
        /// <param name="plan">Codigo de plan.</param>
        /// <param name="contrato">Codigo de contrato.</param>
        /// <returns>Lista de subcuentas activas.</returns>
        public ErrorDto<List<FndSubCuentasDto>> SubCuentas_Obtener(int CodEmpresa, string operadora, string plan, int contrato)
        {
            const string sql = @"SELECT IDx,
                         Cedula,
                         Nombre,
                         0 AS ValorFijo
                  FROM fnd_subCuentas
                  WHERE cod_operadora = @Operadora
                    AND cod_plan = @Plan
                    AND cod_contrato = @Contrato
                    AND estado = 'A';";

            return DbHelper.ExecuteListQuery<FndSubCuentasDto>(
                _portalDb,
                CodEmpresa,
                sql,
                new
                {
                    Operadora = operadora,
                    Plan = plan,
                    Contrato = contrato
                });
        }


        /// <summary>
        /// Obtiene el consecutivo del documento.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="vTipo">Tipo de documento.</param>
        /// <param name="sysDocVersion">Version de consecutivo de documentos.</param>
        /// <returns>Consecutivo asignado al documento.</returns>
        public long FxDocumentoConsecutivo(int codEmpresa, string vTipo, int sysDocVersion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            try
            {
                if (sysDocVersion == 1)
                {
                    var consecutivoSql = ObtenerSqlConsecutivo(vTipo);

                    long consecutivo = connection.QueryFirstOrDefault<long>(consecutivoSql.SelectSql);
                    connection.Execute(consecutivoSql.UpdateSql);

                    return consecutivo;
                }

                return connection.QueryFirstOrDefault<long>(
                    "exec spSIFDocsConsecutivo @Tipo",
                    new { Tipo = vTipo },
                    commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al obtener consecutivo para tipo {vTipo}: {ex.Message}", ex);
            }
        }

        private static (string SelectSql, string UpdateSql) ObtenerSqlConsecutivo(string vTipo)
        {
            return vTipo.ToUpperInvariant() switch
            {
                "RE" => (
                    "SELECT CS_RECIBO AS Consecutivo FROM ase_consecutivos",
                    "UPDATE ase_consecutivos SET CS_RECIBO = CS_RECIBO + 1"),
                "DP" => (
                    "SELECT CS_DEPOSITO AS Consecutivo FROM ase_consecutivos",
                    "UPDATE ase_consecutivos SET CS_DEPOSITO = CS_DEPOSITO + 1"),
                "ND" => (
                    "SELECT CS_NOTA_DEBITO AS Consecutivo FROM ase_consecutivos",
                    "UPDATE ase_consecutivos SET CS_NOTA_DEBITO = CS_NOTA_DEBITO + 1"),
                "NC" => (
                    "SELECT CS_NOTA_CREDITO AS Consecutivo FROM ase_consecutivos",
                    "UPDATE ase_consecutivos SET CS_NOTA_CREDITO = CS_NOTA_CREDITO + 1"),
                _ => throw new InvalidOperationException($"Tipo de documento {vTipo} no válido")
            };
        }


    }


}
