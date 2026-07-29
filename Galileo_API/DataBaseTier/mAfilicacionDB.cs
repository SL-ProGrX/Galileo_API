using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier;
using Galileo_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace Galileo.DataBaseTier
{
    public class MAfilicacionDB
    {
        private const decimal AjusteTasa = 3m;

        private readonly PortalDB _portalDB;

        public MAfilicacionDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }


        /// <summary>
        /// Obtiene el valor de un parametro de afiliacion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pCodigo"></param>
        /// <returns></returns>
        public string fxgAFIParametro_Obtener(
            int CodEmpresa,
            string pCodigo)
        {
            const string query = """
                SELECT RTRIM(ISNULL(valor, ''))
                FROM afi_parametros
                WHERE cod_parametro = @codigo;
                """;

            return ObtenerTexto(
                CodEmpresa,
                query,
                new
                {
                    codigo =
                        (pCodigo ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Obtiene el valor de un parametro de comisiones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pCodigo"></param>
        /// <returns></returns>
        public string fxgAFIParametroComision_Obtener(
            int CodEmpresa,
            string pCodigo)
        {
            const string query = """
                SELECT RTRIM(ISNULL(valor, ''))
                FROM AFI_COMISIONES_PARAMETROS
                WHERE cod_parametro = @codigo;
                """;

            return ObtenerTexto(
                CodEmpresa,
                query,
                new
                {
                    codigo =
                        (pCodigo ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Obtiene el nombre de una persona por su cedula.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="strCedula"></param>
        /// <returns></returns>
        public string fxNombre_Obtener(
            int CodEmpresa,
            string strCedula)
        {
            const string query = """
                SELECT RTRIM(ISNULL(nombre, ''))
                FROM socios
                WHERE cedula = @cedula;
                """;

            return ObtenerTexto(
                CodEmpresa,
                query,
                new
                {
                    cedula =
                        (strCedula ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Valida si la persona tiene congelado el parametro indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="vParametro"></param>
        /// <returns></returns>
        public bool fxgCongelamiento_Obtener(
            int CodEmpresa,
            string vCedula,
            string vParametro)
        {
            string cedula =
                (vCedula ?? string.Empty).Trim();

            const string consultaAbonoCajas = """
                SELECT COUNT_BIG(1)
                FROM dbo.afi_congelar
                WHERE estado = 'A'
                  AND per_abono_cajas = 0
                  AND cedula = @cedula
                  AND Getdate()
                      BETWEEN fecha_inicia
                          AND fecha_finaliza;
                """;

            const string consultaValorCuota = """
                SELECT COUNT_BIG(1)
                FROM dbo.afi_congelar
                WHERE estado = 'A'
                  AND valor_cuota = 0
                  AND cedula = @cedula
                  AND Getdate()
                      BETWEEN fecha_inicia
                          AND fecha_finaliza;
                """;

            string query =
                vParametro switch
                {
                    "per_abono_cajas" =>
                        consultaAbonoCajas,
                    "VALOR_CUOTA" =>
                        consultaValorCuota,
                    _ => string.Empty
                };

            if (string.IsNullOrEmpty(query))
            {
                return false;
            }

            using var connection =
                DbHelper.OpenConnection(
                    _portalDB,
                    CodEmpresa);

            long cantidad =
                connection.QueryFirstOrDefault<long>(
                    query,
                    new
                    {
                        cedula
                    });

            return cantidad > 0;
        }

        /// <summary>
        /// Registra un movimiento en la bitacora especial de afiliacion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pMovimiento"></param>
        /// <param name="pDetalle"></param>
        /// <param name="pCedula"></param>
        /// <param name="usuario"></param>
        public void sbgAFIBitacora_Registrar(
            int CodEmpresa,
            string pMovimiento,
            string pDetalle,
            string pCedula,
            string usuario)
        {
            const string query = """
                EXEC spAFI_Persona_Bitacora_Especial_Add
                     @Cedula,
                     @Movimiento,
                     @Detalle,
                     @Usuario;
                """;

            using var connection =
                DbHelper.OpenConnection(
                    _portalDB,
                    CodEmpresa);

            connection.Execute(
                query,
                new
                {
                    Cedula =
                        (pCedula ?? string.Empty).Trim(),
                    Movimiento =
                        (pMovimiento ?? string.Empty).Trim(),
                    Detalle =
                        (pDetalle ?? string.Empty).Trim(),
                    Usuario =
                        (usuario ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Convierte un codigo de parentesco en descripcion o viceversa.
        /// </summary>
        /// <param name="parentesco"></param>
        /// <returns></returns>
        public static string fxParentesco_Obtener(
            string parentesco)
        {
            return (parentesco ?? string.Empty)
                .Trim() switch
            {
                "E" => "Esposo(a)",
                "H" => "Hijo(a)",
                "R" => "Hermano(a)",
                "S" => "Sobrino(a)",
                "M" => "Madre",
                "P" => "Padre",
                "A" => "Abuelo(a)",
                "I" => "Primo(a)",
                "G" => "Amigo(a)",
                "T" => "Tio(a)",
                "J" => "Madrastra",
                "K" => "Padrastro",
                "N" => "Nieto(a)",
                "L" => "Hermanastro(a)",
                "O" => "Otro...",
                "Esposo(a)" => "E",
                "Hijo(a)" => "H",
                "Hermano(a)" => "R",
                "Sobrino(a)" => "S",
                "Madre" => "M",
                "Padre" => "P",
                "Abuelo(a)" => "A",
                "Primo(a)" => "I",
                "Amigo(a)" => "G",
                "Tio(a)" => "T",
                "Madrastra" => "J",
                "Padrastro" => "K",
                "Nieto(a)" => "N",
                "Hermanastro(a)" => "L",
                "Otro..." => "O",
                _ => "Otro..."
            };
        }

        /// <summary>
        /// Valida que un correo electronico tenga un formato valido.
        /// </summary>
        /// <param name="correo"></param>
        /// <returns></returns>
        public static bool fxEmail_Validar(
            string correo)
        {
            correo =
                (correo ?? string.Empty).Trim();

            if (correo.Length < 8)
            {
                return false;
            }

            return Regex.IsMatch(
                correo,
                @"^[\w\.-]+@[\w\.-]+\.\w+$",
                RegexOptions.IgnoreCase,
                TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Convierte el tipo de cuenta bancaria en su descripcion.
        /// </summary>
        /// <param name="intTipo"></param>
        /// <returns></returns>
        public static string fxTipoCuentaBanco_Obtener(
            int intTipo)
        {
            return intTipo switch
            {
                0 => "Cuentas Corrientes",
                1 => "Tarjeta Debito",
                2 => "Tarjeta Credito",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Valida la cantidad de palabras y longitud de una direccion.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool fxDireccion_Validar(
            MAfilicacionDireccionValidarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string direccionSinRelleno =
                request.direccion;

            if (
                !string.IsNullOrEmpty(
                    request.caracteres_relleno))
            {
                foreach (
                    string caracter
                    in request.caracteres_relleno.Split(','))
                {
                    if (string.IsNullOrEmpty(caracter))
                    {
                        continue;
                    }

                    direccionSinRelleno =
                        direccionSinRelleno.Replace(
                            caracter,
                            string.Empty,
                            StringComparison.Ordinal);
                }
            }

            int cantidadPalabras =
                direccionSinRelleno.Split(
                    ' ',
                    StringSplitOptions
                        .RemoveEmptyEntries).Length;

            return cantidadPalabras >=
                       request.cantidad_palabras &&
                   direccionSinRelleno.Length >=
                       request.largo_direccion;
        }

        /// <summary>
        /// Obtiene el consecutivo de ingreso de una persona para una fecha.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pCedula"></param>
        /// <param name="pFecha"></param>
        /// <returns></returns>
        public int fxgAFIIngresoConsecutivo_Obtener(
                int CodEmpresa,
                string pCedula,
                DateTime pFecha)
        {
            const string query = """
                SELECT ISNULL(
                    (
                        SELECT TOP (1)
                            consec
                        FROM afi_ingresos
                        WHERE cedula = @cedula
                          AND fecha_ingreso >= @fecha
                          AND fecha_ingreso <
                              DATEADD(DAY, 1, @fecha)
                    ),
                    0
                );
                """;

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                CodEmpresa,
                query,
                0,
                new
                {
                    cedula =
                        (pCedula ?? string.Empty).Trim(),
                    fecha = pFecha.Date
                }).Result;
        }

        /// <summary>
        /// Obtiene los parametros generales del modulo de afiliacion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<MAfilicacionParametrosData?>
            sbAFIParametrosCargaArreglo_Obtener(
                int CodEmpresa)
        {
            const string query = """
                SELECT
                    ISNULL(
                        TRY_CONVERT(
                            int,
                            MAX(
                                CASE
                                    WHEN cod_parametro = '01'
                                        THEN valor
                                END
                            )
                        ),
                        0
                    ) AS largo_cedula,
                    CONVERT(
                        bit,
                        CASE
                            WHEN MAX(
                                CASE
                                    WHEN cod_parametro = '02'
                                        THEN RTRIM(valor)
                                END
                            ) = 'S'
                                THEN 1
                            ELSE 0
                        END
                    ) AS solicitar_telefonos,
                    CONVERT(
                        bit,
                        CASE
                            WHEN MAX(
                                CASE
                                    WHEN cod_parametro = '03'
                                        THEN RTRIM(valor)
                                END
                            ) = 'S'
                                THEN 1
                            ELSE 0
                        END
                    ) AS solicitar_cuentas,
                    CONVERT(
                        bit,
                        CASE
                            WHEN MAX(
                                CASE
                                    WHEN cod_parametro = '04'
                                        THEN RTRIM(valor)
                                END
                            ) = 'S'
                                THEN 1
                            ELSE 0
                        END
                    ) AS solicitar_beneficiario,
                    CONVERT(
                        bit,
                        CASE
                            WHEN MAX(
                                CASE
                                    WHEN cod_parametro = '05'
                                        THEN RTRIM(valor)
                                END
                            ) = 'S'
                                THEN 1
                            ELSE 0
                        END
                    ) AS verifica_nombre,
                    CONVERT(
                        bit,
                        CASE
                            WHEN MAX(
                                CASE
                                    WHEN cod_parametro = '06'
                                        THEN RTRIM(valor)
                                END
                            ) = 'S'
                                THEN 1
                            ELSE 0
                        END
                    ) AS verifica_padron,
                    CONVERT(
                        bit,
                        CASE
                            WHEN MAX(
                                CASE
                                    WHEN cod_parametro = '07'
                                        THEN RTRIM(valor)
                                END
                            ) = 'S'
                                THEN 1
                            ELSE 0
                        END
                    ) AS bitacora_especial
                FROM afi_parametros
                WHERE cod_parametro IN
                (
                    '01',
                    '02',
                    '03',
                    '04',
                    '05',
                    '06',
                    '07'
                );
                """;

            return DbHelper.ExecuteSingleQuery(
                _portalDB,
                CodEmpresa,
                query,
                new MAfilicacionParametrosData());
        }

        /// <summary>
        /// Obtiene la descripcion de un departamento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pInstitucion"></param>
        /// <param name="pCodigo"></param>
        /// <param name="sysAseVersion"></param>
        /// <returns></returns>
        public string? fxgAFIDepartamento_Obtener(
            int CodEmpresa,
            int pInstitucion,
            string pCodigo,
            bool sysAseVersion)
        {
            string codigo =
                (pCodigo ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(codigo))
            {
                return string.Empty;
            }

            if (sysAseVersion)
            {
                const string query = """
                    SELECT RTRIM(
                        ISNULL(descripcion, '')
                    )
                    FROM uprogramatica
                    WHERE codigo = @codigo;
                    """;

                return ObtenerTexto(
                    CodEmpresa,
                    query,
                    new
                    {
                        codigo
                    });
            }

            const string consultaDepartamento = """
                SELECT RTRIM(
                    ISNULL(descripcion, '')
                )
                FROM AFDepartamentos
                WHERE cod_institucion = @institucion
                  AND cod_departamento = @codigo;
                """;

            return ObtenerTexto(
                CodEmpresa,
                consultaDepartamento,
                new
                {
                    institucion = pInstitucion,
                    codigo
                });
        }

        /// <summary>
        /// Obtiene la descripcion de una seccion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public string? fxgAFISeccion_Obtener(
            int CodEmpresa,
            MAfilicacionSeccionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string codigoDepartamento =
                request.cod_departamento.Trim();

            string codigoSeccion =
                request.cod_seccion.Trim();

            if (string.IsNullOrEmpty(codigoSeccion))
            {
                return string.Empty;
            }

            if (request.sys_ase_version)
            {
                const string query = """
                    SELECT RTRIM(
                        ISNULL(ut_descripcion, '')
                    )
                    FROM utrabajo
                    WHERE ut_codigo = @seccion;
                    """;

                return ObtenerTexto(
                    CodEmpresa,
                    query,
                    new
                    {
                        seccion = codigoSeccion
                    });
            }

            const string consultaSeccion = """
                SELECT RTRIM(
                    ISNULL(descripcion, '')
                )
                FROM AFSecciones
                WHERE cod_institucion = @institucion
                  AND cod_departamento = @departamento
                  AND cod_seccion = @seccion;
                """;

            return ObtenerTexto(
                CodEmpresa,
                consultaSeccion,
                new
                {
                    institucion =
                        request.cod_institucion,
                    departamento =
                        codigoDepartamento,
                    seccion =
                        codigoSeccion
                });
        }

        /// <summary>
        /// Convierte el tipo de telefono en su descripcion.
        /// </summary>
        /// <param name="intTipo"></param>
        /// <returns></returns>
        public static string fxTipoTelefono_Obtener(
            int intTipo)
        {
            return intTipo switch
            {
                1 => "Habitacion",
                2 => "Trabajo",
                3 => "Celular",
                4 => "Beeper",
                _ => "Otro..."
            };
        }

        /// <summary>
        /// Obtiene el primer periodo de deduccion de una institucion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vInstitucion"></param>
        /// <returns></returns>
        public int? fxgPrimerDeduccionIng_Obtener(
            int CodEmpresa,
            int vInstitucion)
        {
            const string query = """
                SELECT ISNULL(
                    (
                        SELECT TOP (1)
                            CONVERT(
                                int,
                                dbo.fxSIFPrmProcesoSig(
                                    CONVERT(
                                        decimal(6, 0),
                                        YEAR(
                                            ISNULL(
                                                pr_fecha_corte,
                                                Getdate()
                                            )
                                        ) * 100 +
                                        MONTH(
                                            ISNULL(
                                                pr_fecha_corte,
                                                Getdate()
                                            )
                                        )
                                    )
                                )
                            )
                        FROM instituciones
                        WHERE cod_institucion =
                                  @institucion
                    ),
                    0
                );
                """;

            using var connection =
                DbHelper.OpenConnection(
                    _portalDB,
                    CodEmpresa);

            return connection.QueryFirstOrDefault<int>(
                query,
                new
                {
                    institucion = vInstitucion
                });
        }

        /// <summary>
        /// Ejecuta el proceso de reingreso de una persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto sbReIngreso_Registrar(
            int CodEmpresa,
            MAfilicacionReIngresoRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string cedula =
                request.cedula.Trim();

            if (string.IsNullOrEmpty(cedula))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar la cedula.",
                    -2);
            }

            const string query = """
                EXEC spAFIREingreso
                     @Cedula,
                     @Ingreso,
                     @Institucion,
                     @Promotor,
                     @Boleta,
                     @Usuario,
                     @Oficina;
                """;

            return DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                query,
                new
                {
                    Cedula = cedula,
                    Ingreso =
                        request.fecha_ingreso.Date,
                    Institucion =
                        request.cod_institucion,
                    Promotor =
                        request.cod_promotor,
                    Boleta =
                        request.boleta.Trim(),
                    Usuario =
                        request.usuario.Trim(),
                    Oficina =
                        request.oficina.Trim()
                });
        }

        /// <summary>
        /// Obtiene la descripcion o el codigo de una provincia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pProvincia"></param>
        /// <param name="pTipo"></param>
        /// <returns></returns>
        public string? fxProvincia_Obtener(
            int CodEmpresa,
            string pProvincia,
            string pTipo = "D")
        {
            string provincia =
                (pProvincia ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(provincia))
            {
                return string.Empty;
            }

            bool buscarPorCodigo =
                int.TryParse(
                    provincia,
                    out int codigoProvincia);

            string tipo =
                (pTipo ?? "D").Trim();

            using var connection =
                DbHelper.OpenConnection(
                    _portalDB,
                    CodEmpresa);

            if (buscarPorCodigo)
            {
                const string query = """
                    SELECT TOP (1)
                        CASE
                            WHEN @tipo = 'D'
                                THEN RTRIM(
                                    ISNULL(descripcion, '')
                                )
                            ELSE CONVERT(
                                varchar(20),
                                provincia
                            )
                        END
                    FROM provincias
                    WHERE provincia = @codigo;
                    """;

                return connection
                           .QueryFirstOrDefault<string>(
                               query,
                               new
                               {
                                   tipo,
                                   codigo = codigoProvincia
                               }) ??
                       provincia;
            }

            const string consultaDescripcion = """
                SELECT TOP (1)
                    CASE
                        WHEN @tipo = 'D'
                            THEN RTRIM(
                                ISNULL(descripcion, '')
                            )
                        ELSE CONVERT(
                            varchar(20),
                            provincia
                        )
                    END
                FROM provincias
                WHERE descripcion = @descripcion;
                """;

            return connection
                       .QueryFirstOrDefault<string>(
                           consultaDescripcion,
                           new
                           {
                               tipo,
                               descripcion = provincia
                           }) ??
                   provincia;
        }

        /// <summary>
        /// Obtiene la descripcion de un canton.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pProvincia"></param>
        /// <param name="pCanton"></param>
        /// <returns></returns>
        public string? fxCanton_Obtener(
            int CodEmpresa,
            int pProvincia,
            int pCanton)
        {
            const string query = """
                SELECT RTRIM(
                    ISNULL(descripcion, '')
                )
                FROM cantones
                WHERE provincia = @provincia
                  AND canton = @canton;
                """;

            return ObtenerTexto(
                CodEmpresa,
                query,
                new
                {
                    provincia = pProvincia,
                    canton = pCanton
                });
        }

        /// <summary>
        /// Convierte un estado civil utilizando la funcion del sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vEstadoCivil"></param>
        /// <returns></returns>
        public string? fxEstadoCivil_Obtener(
            int CodEmpresa,
            string vEstadoCivil)
        {
            const string query = """
                SELECT ISNULL(
                    CONVERT(
                        varchar(100),
                        dbo.fxSys_Estado_Civil(
                            @estadoCivil
                        )
                    ),
                    ''
                );
                """;

            return ObtenerTexto(
                CodEmpresa,
                query,
                new
                {
                    estadoCivil =
                        (vEstadoCivil ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Elimina de un texto las apariciones del caracter indicado.
        /// </summary>
        /// <param name="texto"></param>
        /// <param name="caracter"></param>
        /// <returns></returns>
        public static string fxDepuraString(
            string? texto,
            string caracter = "'")
        {
            string resultado =
                (texto ?? string.Empty).Trim();

            return string.IsNullOrEmpty(caracter)
                ? resultado
                : resultado.Replace(
                    caracter,
                    string.Empty,
                    StringComparison.Ordinal);
        }

        /// <summary>
        /// Sube o baja tres puntos a la tasa de los creditos activos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="vSube"></param>
        /// <returns></returns>
        public ErrorDto sbSubirBajarTasa_Registrar(
            int CodEmpresa,
            string vCedula,
            bool vSube = true)
        {
            string cedula =
                (vCedula ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(cedula))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar la cedula.",
                    -2);
            }

            return EjecutarEnTransaccion(
                CodEmpresa,
                (connection, transaction) =>
                {
                    IReadOnlyCollection<
                        MAfilicacionCreditoTasaData
                    > creditos =
                        ConsultarCreditosTasa(
                            connection,
                            transaction,
                            cedula,
                            vSube);

                    foreach (
                        MAfilicacionCreditoTasaData credito
                        in creditos)
                    {
                        ActualizarCreditoTasa(
                            connection,
                            transaction,
                            credito,
                            vSube);
                    }

                    return DbHelper.CreateOkResponse();
                },
                "No fue posible actualizar las tasas de la persona.");
        }

        /// <summary>
        /// Revierte las tasas de los creditos incluidos en una liquidacion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vLiq"></param>
        /// <param name="sysPlanPagos"></param>
        /// <returns></returns>
        public ErrorDto sbSubirBajarTasav2_Registrar(
            int CodEmpresa,
            long vLiq,
            bool sysPlanPagos)
        {
            if (vLiq <= 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar una liquidacion valida.",
                    -2);
            }

            return EjecutarEnTransaccion(
                CodEmpresa,
                (connection, transaction) =>
                {
                    MAfilicacionLiquidacionTasaData?
                        liquidacion =
                            ConsultarLiquidacionTasa(
                                connection,
                                transaction,
                                vLiq);

                    if (liquidacion is null)
                    {
                        return DbHelper.ErrorResponse(
                            "No se encontro la liquidacion indicada.",
                            -2);
                    }

                    IReadOnlyCollection<
                        MAfilicacionCreditoLiquidacionTasaData
                    > creditos =
                        ConsultarCreditosLiquidacionTasa(
                            connection,
                            transaction,
                            vLiq);

                    foreach (
                        MAfilicacionCreditoLiquidacionTasaData credito
                        in creditos)
                    {
                        ProcesarCreditoLiquidacionTasa(
                            connection,
                            transaction,
                            credito,
                            liquidacion,
                            sysPlanPagos);
                    }

                    return DbHelper.CreateOkResponse();
                },
                "No fue posible revertir las tasas de la liquidacion.");
        }

        private ErrorDto EjecutarEnTransaccion(
            int CodEmpresa,
            Func<
                SqlConnection,
                SqlTransaction,
                ErrorDto
            > proceso,
            string mensajeError)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDB,
                        CodEmpresa);

                using var transaction =
                    connection.BeginTransaction();

                try
                {
                    ErrorDto resultado =
                        proceso(
                            connection,
                            transaction);

                    if (resultado.Code != 0)
                    {
                        transaction.Rollback();
                        return resultado;
                    }

                    transaction.Commit();
                    return resultado;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
                when (
                    ex is SqlException or
                    InvalidOperationException or
                    ArgumentException or
                    DataException or
                    ArithmeticException)
            {
                return DbHelper.ErrorResponse(
                    mensajeError,
                    -1);
            }
        }

        private static MAfilicacionLiquidacionTasaData?
            ConsultarLiquidacionTasa(
                SqlConnection connection,
                SqlTransaction transaction,
                long consecutivo)
        {
            const string query = """
                SELECT TOP (1)
                    ISNULL(C.liq_alterna, 0)
                        AS liq_alterna,
                    ISNULL(C.tasa_planilla, 0)
                        AS tasa_planilla,
                    ISNULL(C.tasa_ventanilla, 0)
                        AS tasa_ventanilla
                FROM Liquidacion AS L
                INNER JOIN causas_renuncias AS C
                    ON L.id_causa = C.id_causa
                WHERE L.consec = @consecutivo;
                """;

            return connection.QueryFirstOrDefault<
                MAfilicacionLiquidacionTasaData>(
                    query,
                    new
                    {
                        consecutivo
                    },
                    transaction);
        }

        private static IReadOnlyCollection<
            MAfilicacionCreditoLiquidacionTasaData
        > ConsultarCreditosLiquidacionTasa(
            SqlConnection connection,
            SqlTransaction transaction,
            long consecutivo)
        {
            const string query = """
                SELECT
                    R.id_solicitud AS id_solicitud,
                    ISNULL(R.saldo, 0) AS saldo,
                    ISNULL(R.opex, 0) AS opex,
                    ISNULL(R.LiqTasa, 0) AS liq_tasa,
                    TRY_CONVERT(int, R.prideduc)
                        AS prideduc,
                    TRY_CONVERT(int, R.fecult)
                        AS fecult,
                    ISNULL(R.[int], 0) AS interes,
                    ISNULL(R.interesv, 0) AS interesv,
                    ISNULL(R.plazo, 0) AS plazo,
                    ISNULL(C.liq_valor, 0)
                        AS liq_valor,
                    RTRIM(
                        ISNULL(C.liq_tipoAumento, '')
                    ) AS liq_tipo_aumento,
                    RTRIM(
                        ISNULL(C.IND_DEDUCE_PLANILLA, '')
                    ) AS ind_deduce_planilla
                FROM reg_creditos AS R
                INNER JOIN Catalogo AS C
                    ON R.codigo = C.codigo
                INNER JOIN Liquida_Detalle AS L
                    ON L.id_solicitud =
                       R.id_solicitud
                WHERE L.consec = @consecutivo;
                """;

            return connection.Query<
                MAfilicacionCreditoLiquidacionTasaData>(
                    query,
                    new
                    {
                        consecutivo
                    },
                    transaction)
                .AsList();
        }

        private static void
            ProcesarCreditoLiquidacionTasa(
                SqlConnection connection,
                SqlTransaction transaction,
                MAfilicacionCreditoLiquidacionTasaData credito,
                MAfilicacionLiquidacionTasaData liquidacion,
                bool sysPlanPagos)
        {
            if (credito.opex == 1)
            {
                ActualizarOpexCredito(
                    connection,
                    transaction,
                    credito.id_solicitud);
            }

            if (
                credito.liq_tasa != 1 ||
                credito.interes <= 0)
            {
                return;
            }

            int meses =
                CalcularMesesPendientes(
                    credito.prideduc,
                    credito.fecult,
                    credito.plazo);

            decimal nuevaTasa =
                CalcularTasaLiquidacion(
                    credito,
                    liquidacion);

            decimal nuevaCuota =
                CalcularCuota(
                    credito.saldo,
                    meses,
                    nuevaTasa);

            ActualizarTasaCreditoLiquidacion(
                connection,
                transaction,
                credito.id_solicitud,
                nuevaCuota,
                nuevaTasa);

            if (sysPlanPagos)
            {
                ActualizarPlanPagos(
                    connection,
                    transaction,
                    credito.id_solicitud);
            }
        }

        private static decimal CalcularTasaLiquidacion(
            MAfilicacionCreditoLiquidacionTasaData credito,
            MAfilicacionLiquidacionTasaData liquidacion)
        {
            if (liquidacion.liq_alterna == 0)
            {
                return string.Equals(
                    credito.liq_tipo_aumento.Trim(),
                    "P",
                    StringComparison.OrdinalIgnoreCase)
                    ? credito.interesv -
                      credito.liq_valor
                    : credito.liq_valor;
            }

            bool deducePlanilla =
                string.Equals(
                    credito.ind_deduce_planilla.Trim(),
                    "S",
                    StringComparison.OrdinalIgnoreCase);

            decimal disminucion =
                deducePlanilla
                    ? liquidacion.tasa_planilla
                    : liquidacion.tasa_ventanilla;

            return credito.interesv -
                   disminucion;
        }

        private static void ActualizarOpexCredito(
            SqlConnection connection,
            SqlTransaction transaction,
            long idSolicitud)
        {
            const string query = """
                UPDATE reg_creditos
                SET opex = 0
                WHERE id_solicitud = @idSolicitud;
                """;

            connection.Execute(
                query,
                new
                {
                    idSolicitud
                },
                transaction);
        }

        private static void
            ActualizarTasaCreditoLiquidacion(
                SqlConnection connection,
                SqlTransaction transaction,
                long idSolicitud,
                decimal cuota,
                decimal tasa)
        {
            const string query = """
                UPDATE reg_creditos
                SET
                    cuota = @cuota,
                    interesv = @tasa,
                    LiqTasa = 0
                WHERE id_solicitud = @idSolicitud;
                """;

            connection.Execute(
                query,
                new
                {
                    cuota,
                    tasa,
                    idSolicitud
                },
                transaction);
        }

        private static void ActualizarPlanPagos(
            SqlConnection connection,
            SqlTransaction transaction,
            long idSolicitud)
        {
            const string query = """
                EXEC spCrdPlanPagos
                     @idSolicitud;
                """;

            connection.Execute(
                query,
                new
                {
                    idSolicitud
                },
                transaction);
        }

        private static IReadOnlyCollection<
            MAfilicacionCreditoTasaData
        > ConsultarCreditosTasa(
            SqlConnection connection,
            SqlTransaction transaction,
            string cedula,
            bool subir)
        {
            const string consultaSubir = """
                SELECT
                    R.id_solicitud AS id_solicitud,
                    ISNULL(R.saldo, 0) AS saldo,
                    TRY_CONVERT(int, R.prideduc)
                        AS prideduc,
                    TRY_CONVERT(int, R.fecult)
                        AS fecult,
                    ISNULL(R.[int], 0) AS interes,
                    ISNULL(R.interesv, 0) AS interesv,
                    ISNULL(R.plazo, 0) AS plazo
                FROM reg_creditos AS R
                INNER JOIN Catalogo AS C
                    ON R.codigo = C.codigo
                WHERE R.estado = 'A'
                  AND R.saldo > 0
                  AND R.proceso <> 'J'
                  AND C.poliza = 'N'
                  AND C.retencion = 'N'
                  AND R.cedula = @cedula
                  AND ISNULL(R.LiqTasa, 0) = 0;
                """;

            const string consultaBajar = """
                SELECT
                    R.id_solicitud AS id_solicitud,
                    ISNULL(R.saldo, 0) AS saldo,
                    TRY_CONVERT(int, R.prideduc)
                        AS prideduc,
                    TRY_CONVERT(int, R.fecult)
                        AS fecult,
                    ISNULL(R.[int], 0) AS interes,
                    ISNULL(R.interesv, 0) AS interesv,
                    ISNULL(R.plazo, 0) AS plazo
                FROM reg_creditos AS R
                WHERE R.LiqTasa = 1
                  AND R.estado = 'A'
                  AND R.cedula = @cedula;
                """;

            string query =
                subir
                    ? consultaSubir
                    : consultaBajar;

            return connection.Query<
                MAfilicacionCreditoTasaData>(
                    query,
                    new
                    {
                        cedula
                    },
                    transaction)
                .AsList();
        }

        private static void ActualizarCreditoTasa(
            SqlConnection connection,
            SqlTransaction transaction,
            MAfilicacionCreditoTasaData credito,
            bool subir)
        {
            if (credito.interes <= 0)
            {
                return;
            }

            int meses =
                CalcularMesesPendientes(
                    credito.prideduc,
                    credito.fecult,
                    credito.plazo);

            decimal nuevaTasa =
                credito.interesv +
                (
                    subir
                        ? AjusteTasa
                        : -AjusteTasa
                );

            decimal nuevaCuota =
                CalcularCuota(
                    credito.saldo,
                    meses,
                    nuevaTasa);

            const string query = """
                UPDATE reg_creditos
                SET
                    cuota = @cuota,
                    interesv = @tasa,
                    LiqTasa = @liqTasa
                WHERE id_solicitud = @idSolicitud;
                """;

            connection.Execute(
                query,
                new
                {
                    cuota = nuevaCuota,
                    tasa = nuevaTasa,
                    liqTasa = subir ? 1 : 0,
                    idSolicitud =
                        credito.id_solicitud
                },
                transaction);
        }

        private static decimal CalcularCuota(
            decimal saldo,
            int plazo,
            decimal tasa)
        {
            return MValidacionDb
                .MValidacion_FxCalcula_Cuota_Obtener(
                    new MValidacionCuotaRequest
                    {
                        monto = saldo,
                        plazo = plazo,
                        interes = tasa,
                        frecuencia = "M"
                    });
        }

        private static int CalcularMesesPendientes(
            int? primerDeduccion,
            int? fechaUltima,
            int plazo)
        {
            DateTime fechaPrimerDeduccion =
                ConvertirPeriodoFecha(
                    primerDeduccion,
                    nameof(primerDeduccion));

            DateTime fechaUltimoMovimiento =
                ConvertirPeriodoFecha(
                    fechaUltima,
                    nameof(fechaUltima));

            int mesesTranscurridos =
                (
                    fechaUltimoMovimiento.Year -
                    fechaPrimerDeduccion.Year
                ) * 12 +
                fechaUltimoMovimiento.Month -
                fechaPrimerDeduccion.Month;

            return Math.Max(
                plazo - mesesTranscurridos,
                1);
        }

        private static DateTime ConvertirPeriodoFecha(
            int? periodo,
            string nombreCampo)
        {
            if (!periodo.HasValue)
            {
                throw new InvalidOperationException(
                    $"El campo {nombreCampo} no contiene un periodo valido.");
            }

            int anio =
                periodo.Value / 100;

            int mes =
                periodo.Value % 100;

            if (
                anio < 1 ||
                mes is < 1 or > 12)
            {
                throw new InvalidOperationException(
                    $"El campo {nombreCampo} no contiene un periodo valido.");
            }

            return new DateTime(
                anio,
                mes,
                1);
        }

        private string ObtenerTexto(
            int CodEmpresa,
            string query,
            object? parametros)
        {
            using var connection =
                DbHelper.OpenConnection(
                    _portalDB,
                    CodEmpresa);

            return connection
                       .QueryFirstOrDefault<string>(
                           query,
                           parametros) ??
                   string.Empty;
        }
    }
}