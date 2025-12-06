using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Activos_Fijos
{
    public class FrmActivosPersonasDB
    {
        private const int ModuloActivos = 36; // Módulo de Activos
        private const string Todos = "TODOS";

        private readonly MSecurityMainDb _securityMainDb;
        //private readonly mReportingServicesDB _mReporting;
        //private readonly MProGrXAuxiliarDB _mAuxiliar;
        private readonly PortalDB _portalDb;

        public FrmActivosPersonasDB(IConfiguration config)
        {
            _securityMainDb = new MSecurityMainDb(config);
            //_mReporting = new mReportingServicesDB(_config);
            //_mAuxiliar = new MProGrXAuxiliarDB(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene lista paginada de personas con filtros y joins a Departamentos/Secciones (Tab Nómina).
        /// </summary>
        public ErrorDto<ActivosPersonasLista> Activos_Personas_Lista_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros,
            string? codDepartamento = null,
            string? codSeccion = null)
        {
            var result = new ErrorDto<ActivosPersonasLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosPersonasLista
                {
                    total = 0,
                    lista = new List<ActivosPersonasData>()
                }
            };

            try
            {
                using var connection = _portalDb.CreateConnection(CodEmpresa);

                // Normalizo filtros a NULL cuando no aplican
                string? filtroValor = string.IsNullOrWhiteSpace(filtros.filtro)
                    ? null
                    : $"%{filtros.filtro}%";

                string? codDepartamentoParam =
                    !string.IsNullOrWhiteSpace(codDepartamento) && codDepartamento != Todos
                        ? codDepartamento
                        : null;

                string? codSeccionParam =
                    !string.IsNullOrWhiteSpace(codSeccion) && codSeccion != Todos
                        ? codSeccion
                        : null;

                // Mapear sortField a índice de columna para ORDER BY seguro
                var sortFieldRaw = (filtros.sortField ?? "Per.IDENTIFICACION").Trim();
                var sortFieldNorm = sortFieldRaw.ToUpperInvariant();

                int orderIndex = sortFieldNorm switch
                {
                    "PER.IDENTIFICACION" or "IDENTIFICACION" => 1,
                    "PER.NOMBRE" or "NOMBRE"                 => 2,
                    "DEPT.DESCRIPCION" or "DEPARTAMENTO"     => 3,
                    "SEC.DESCRIPCION" or "SECCION"           => 4,
                    "PER.REGISTRO_USUARIO" or "USUARIO"      => 5,
                    _                                        => 1
                };

                // Dirección: 0 = DESC, 1 = ASC
                int orderDir = filtros.sortOrder == 0 ? 0 : 1;

                var parametros = new
                {
                    filtro = filtroValor,
                    codDepartamento = codDepartamentoParam,
                    codSeccion = codSeccionParam,
                    orderIndex,
                    orderDir,
                    offset = filtros.pagina,
                    rows = filtros.paginacion
                };

                // TOTAL (SQL 100% constante)
                const string sqlCount = @"
                    SELECT COUNT(1)
                    FROM ACTIVOS_PERSONAS Per
                    INNER JOIN ACTIVOS_DEPARTAMENTOS Dept 
                        ON Per.COD_DEPARTAMENTO = Dept.COD_DEPARTAMENTO
                    INNER JOIN ACTIVOS_SECCIONES Sec  
                        ON Per.COD_DEPARTAMENTO = Sec.COD_DEPARTAMENTO 
                       AND Per.COD_SECCION      = Sec.COD_SECCION
                    LEFT JOIN (
                        SELECT  IDENTIFICACION,
                                MAX(COD_TRASLADO) AS COD_TRASLADO
                        FROM    ACTIVOS_TRASLADOS
                        GROUP BY IDENTIFICACION
                    ) Tr
                        ON Tr.IDENTIFICACION = Per.IDENTIFICACION
                    WHERE 1 = 1
                      AND (
                            @filtro IS NULL
                            OR Per.IDENTIFICACION    LIKE @filtro
                            OR Per.NOMBRE           LIKE @filtro
                            OR Dept.DESCRIPCION     LIKE @filtro
                            OR Sec.DESCRIPCION      LIKE @filtro
                            OR Per.REGISTRO_USUARIO LIKE @filtro
                          )
                      AND (@codDepartamento IS NULL OR Per.COD_DEPARTAMENTO = @codDepartamento)
                      AND (@codSeccion      IS NULL OR Per.COD_SECCION      = @codSeccion);";

                result.Result.total = connection.QueryFirstOrDefault<int>(sqlCount, parametros);

                // PAGE con ORDER BY seguro (también SQL constante)
                const string sqlPage = @"
                    SELECT 
                        Per.IDENTIFICACION    AS identificacion,
                        Per.NOMBRE            AS nombre,
                        Per.COD_DEPARTAMENTO  AS cod_departamento,
                        Per.COD_SECCION       AS cod_seccion,
                        Per.COD_ALTERNO       AS cod_alterno,
                        CASE WHEN Per.ACTIVO = 1 THEN 1 ELSE 0 END AS activo,
                        Dept.DESCRIPCION      AS departamento,
                        Sec.DESCRIPCION       AS seccion,
                        Per.REGISTRO_USUARIO  AS usuario,
                        Tr.COD_TRASLADO       AS cod_traslado
                    FROM ACTIVOS_PERSONAS Per
                    INNER JOIN ACTIVOS_DEPARTAMENTOS Dept 
                        ON Per.COD_DEPARTAMENTO = Dept.COD_DEPARTAMENTO
                    INNER JOIN ACTIVOS_SECCIONES Sec  
                        ON Per.COD_DEPARTAMENTO = Sec.COD_DEPARTAMENTO 
                       AND Per.COD_SECCION      = Sec.COD_SECCION
                    LEFT JOIN (
                        SELECT  IDENTIFICACION,
                                MAX(COD_TRASLADO) AS COD_TRASLADO
                        FROM    ACTIVOS_TRASLADOS
                        GROUP BY IDENTIFICACION
                    ) Tr
                        ON Tr.IDENTIFICACION = Per.IDENTIFICACION
                    WHERE 1 = 1
                      AND (
                            @filtro IS NULL
                            OR Per.IDENTIFICACION    LIKE @filtro
                            OR Per.NOMBRE           LIKE @filtro
                            OR Dept.DESCRIPCION     LIKE @filtro
                            OR Sec.DESCRIPCION      LIKE @filtro
                            OR Per.REGISTRO_USUARIO LIKE @filtro
                          )
                      AND (@codDepartamento IS NULL OR Per.COD_DEPARTAMENTO = @codDepartamento)
                      AND (@codSeccion      IS NULL OR Per.COD_SECCION      = @codSeccion)
                    ORDER BY
                        -- ASC
                        CASE @orderDir WHEN 1 THEN
                            CASE @orderIndex
                                WHEN 1 THEN Per.IDENTIFICACION
                                WHEN 2 THEN Per.NOMBRE
                                WHEN 3 THEN Dept.DESCRIPCION
                                WHEN 4 THEN Sec.DESCRIPCION
                                WHEN 5 THEN Per.REGISTRO_USUARIO
                            END
                        END ASC,
                        -- DESC
                        CASE @orderDir WHEN 0 THEN
                            CASE @orderIndex
                                WHEN 1 THEN Per.IDENTIFICACION
                                WHEN 2 THEN Per.NOMBRE
                                WHEN 3 THEN Dept.DESCRIPCION
                                WHEN 4 THEN Sec.DESCRIPCION
                                WHEN 5 THEN Per.REGISTRO_USUARIO
                            END
                        END DESC
                    OFFSET @offset ROWS FETCH NEXT @rows ROWS ONLY;";

                result.Result.lista = connection.Query<ActivosPersonasData>(sqlPage, parametros).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<ActivosPersonasData>();
            }

            return result;
        }

        /// <summary>
        /// Obtiene lista completa de personas sin paginación (Tab Mantenimiento).
        /// </summary>
        public ErrorDto<List<ActivosPersonasData>> Activos_Personas_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros,
            string? codDepartamento = null,
            string? codSeccion = null)
        {
            var result = new ErrorDto<List<ActivosPersonasData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosPersonasData>()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(CodEmpresa);

                string? filtroValor = string.IsNullOrWhiteSpace(filtros.filtro)
                    ? null
                    : $"%{filtros.filtro}%";

                string? codDepartamentoParam =
                    !string.IsNullOrWhiteSpace(codDepartamento) && codDepartamento != Todos
                        ? codDepartamento
                        : null;

                string? codSeccionParam =
                    !string.IsNullOrWhiteSpace(codSeccion) && codSeccion != Todos
                        ? codSeccion
                        : null;

                var parametros = new
                {
                    filtro = filtroValor,
                    codDepartamento = codDepartamentoParam,
                    codSeccion = codSeccionParam
                };

                // SQL constante, con filtros opcionales
                const string sql = @"
                    SELECT
                        Per.IDENTIFICACION   AS identificacion,
                        Per.NOMBRE           AS nombre,
                        Per.COD_DEPARTAMENTO AS cod_departamento,
                        Per.COD_SECCION      AS cod_seccion,
                        Per.COD_ALTERNO      AS cod_alterno,
                        CASE WHEN Per.ACTIVO = 1 THEN 1 ELSE 0 END AS activo,
                        Dept.DESCRIPCION     AS departamento,
                        Sec.DESCRIPCION      AS seccion,
                        Per.REGISTRO_USUARIO AS usuario
                    FROM ACTIVOS_PERSONAS Per
                    LEFT JOIN ACTIVOS_DEPARTAMENTOS Dept 
                        ON Per.COD_DEPARTAMENTO = Dept.COD_DEPARTAMENTO
                    LEFT JOIN ACTIVOS_SECCIONES Sec 
                        ON Per.COD_DEPARTAMENTO = Sec.COD_DEPARTAMENTO 
                       AND Per.COD_SECCION      = Sec.COD_SECCION
                    WHERE 1 = 1
                      AND (
                            @filtro IS NULL
                            OR Per.IDENTIFICACION    LIKE @filtro
                            OR Per.NOMBRE           LIKE @filtro
                            OR Dept.DESCRIPCION     LIKE @filtro
                            OR Sec.DESCRIPCION      LIKE @filtro
                            OR Per.REGISTRO_USUARIO LIKE @filtro
                          )
                      AND (@codDepartamento IS NULL OR Per.COD_DEPARTAMENTO = @codDepartamento)
                      AND (@codSeccion      IS NULL OR Per.COD_SECCION      = @codSeccion)
                    ORDER BY Per.IDENTIFICACION;";

                result.Result = connection.Query<ActivosPersonasData>(sql, parametros).ToList();
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
        /// Inserta o actualiza persona según isNew (Tab Mantenimiento).
        /// </summary>
        public ErrorDto Activos_Personas_Guardar(int CodEmpresa, string usuario, ActivosPersonasData persona)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDb.CreateConnection(CodEmpresa);

                const string qExiste = @"
                    SELECT ISNULL(COUNT(*), 0) 
                    FROM ACTIVOS_PERSONAS 
                    WHERE UPPER(IDENTIFICACION) = @ident;";

                int existe = connection.QueryFirstOrDefault<int>(
                    qExiste,
                    new { ident = persona.identificacion.ToUpper() });

                if (persona.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code = -2;
                        result.Description = $"La persona con identificación {persona.identificacion} ya existe.";
                    }
                    else
                    {
                        result = Activos_Personas_Insertar(CodEmpresa, usuario, persona);
                    }
                }
                else if (existe == 0)
                {
                    result.Code = -2;
                    result.Description = $"La persona con identificación {persona.identificacion} no existe.";
                }
                else
                {
                    result = Activos_Personas_Actualizar(CodEmpresa, usuario, persona);
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Actualiza una persona existente (Tab Mantenimiento).
        /// </summary>
        private ErrorDto Activos_Personas_Actualizar(int CodEmpresa, string usuario, ActivosPersonasData persona)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDb.CreateConnection(CodEmpresa);

                const string query = @"
                    UPDATE ACTIVOS_PERSONAS
                       SET NOMBRE           = @nombre,
                           COD_DEPARTAMENTO = @cod_departamento,
                           COD_SECCION      = @cod_seccion,
                           COD_ALTERNO      = @cod_alterno,
                           ACTIVO           = @activo,
                           MODIFICA_USUARIO = @usuario,
                           MODIFICA_FECHA   = GETDATE()
                     WHERE IDENTIFICACION   = @identificacion;";

                connection.Execute(query, new
                {
                    nombre           = persona.nombre?.ToUpper(),
                    cod_departamento = persona.cod_departamento,
                    cod_seccion      = persona.cod_seccion,
                    cod_alterno      = persona.cod_alterno?.ToUpper(),
                    activo           = persona.activo ? 1 : 0,
                    usuario,
                    identificacion   = persona.identificacion.ToUpper()
                });

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Persona : {persona.identificacion}",
                    Movimiento = "Modifica - WEB",
                    Modulo = ModuloActivos
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Inserta una nueva persona (Tab Mantenimiento).
        /// </summary>
        private ErrorDto Activos_Personas_Insertar(int CodEmpresa, string usuario, ActivosPersonasData persona)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDb.CreateConnection(CodEmpresa);

                const string query = @"
                    INSERT INTO ACTIVOS_PERSONAS
                      (COD_DEPARTAMENTO, COD_SECCION, IDENTIFICACION, NOMBRE, COD_ALTERNO, ACTIVO, REGISTRO_USUARIO, REGISTRO_FECHA)
                    VALUES
                      (@cod_departamento, @cod_seccion, @identificacion, @nombre, @cod_alterno, @activo, @usuario, GETDATE());";

                connection.Execute(query, new
                {
                    cod_departamento = persona.cod_departamento,
                    cod_seccion      = persona.cod_seccion,
                    identificacion   = persona.identificacion.ToUpper(),
                    nombre           = persona.nombre?.ToUpper(),
                    cod_alterno      = persona.cod_alterno?.ToUpper(),
                    activo           = persona.activo ? 1 : 0,
                    usuario
                });

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Persona : {persona.identificacion}",
                    Movimiento = "Registra - WEB",
                    Modulo = ModuloActivos
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Elimina una persona por identificación (Tab Mantenimiento).
        /// </summary>
        public ErrorDto Activos_Personas_Eliminar(int CodEmpresa, string identificacion, string usuario)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = _portalDb.CreateConnection(CodEmpresa);

                const string sql = @"DELETE FROM ACTIVOS_PERSONAS WHERE IDENTIFICACION = @identificacion;";

                var rows = connection.Execute(sql, new
                {
                    identificacion = identificacion.ToUpper()
                });

                if (rows == 0)
                {
                    result.Code = -2;
                    result.Description = $"No se encontró la persona {identificacion} para eliminar.";
                    return result;
                }

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Persona : {identificacion}",
                    Movimiento = "Elimina - WEB",
                    Modulo = ModuloActivos
                });
            }
            catch (SqlException ex) when (ex.Number == 547) // FK constraint
            {
                result.Code = -2;
                result.Description = "Registro en uso. No se puede eliminar.";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Valida si una identificación ya existe en ACTIVOS_PERSONAS.
        /// </summary>
        public ErrorDto Activos_Personas_Valida(int CodEmpresa, string identificacion)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDb.CreateConnection(CodEmpresa);

                const string query = @"
                    SELECT COUNT(IDENTIFICACION) 
                    FROM ACTIVOS_PERSONAS 
                    WHERE UPPER(IDENTIFICACION) = @ident;";

                var existe = connection.QueryFirstOrDefault<int>(
                    query,
                    new { ident = identificacion.ToUpper() });

                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "La identificación ya existe.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "La identificación es válida.";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Aplica el cambio de Departamento/Sección llamando a spActivos_DepartamentoCambio. Devuelve Boleta.
        /// </summary>
        public ErrorDto<CambioDeptoResponse> Activos_Personas_CambioDepto_Aplicar(
            int CodEmpresa,
            string usuario,
            CambioDeptoRequest request)
        {
            var result = new ErrorDto<CambioDeptoResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new CambioDeptoResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(CodEmpresa);

                // Llamada segura al stored procedure (sin EXEC concatenado)
                var boleta = connection.QueryFirstOrDefault<string>(
                    "spActivos_DepartamentoCambio",
                    new
                    {
                        Identificacion  = request.identificacion,
                        CodDepartamento = request.cod_departamento,
                        CodSeccion      = request.cod_seccion,
                        Usuario         = usuario,
                        Fecha           = request.fecha
                    },
                    commandType: CommandType.StoredProcedure
                ) ?? string.Empty;

                result.Result.boleta = boleta;

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Aplica Cambio Dpto/Sec a: {request.identificacion} Boleta:{result.Result.boleta}",
                    Movimiento = "Aplica - WEB",
                    Modulo = ModuloActivos
                });
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
        /// Ejecuta la sincronización con RRHH llamando a spActivos_Sincroniza_RH.
        /// </summary>
        public ErrorDto Activos_Personas_SincronizarRH(int CodEmpresa, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDb.CreateConnection(CodEmpresa);

                connection.Execute(
                    "spActivos_Sincroniza_RH",
                    commandType: CommandType.StoredProcedure);

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = "Sincronización con RRHH finalizada",
                    Movimiento = "Procesa - WEB",
                    Modulo = ModuloActivos
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Obtiene catálogo de Departamentos (item, descripcion).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(CodEmpresa);

                const string q = @"
                    SELECT COD_DEPARTAMENTO AS item, DESCRIPCION
                    FROM ACTIVOS_DEPARTAMENTOS
                    ORDER BY COD_DEPARTAMENTO;";

                result.Result = connection.Query<DropDownListaGenericaModel>(q).ToList();
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
        /// Obtiene catálogo de Secciones por Departamento (item, descripcion).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Secciones_ObtenerPorDepto(int CodEmpresa, string cod_departamento)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(CodEmpresa);

                const string q = @"
                    SELECT COD_SECCION AS item, DESCRIPCION
                    FROM ACTIVOS_SECCIONES
                    WHERE COD_DEPARTAMENTO = @cod_departamento
                    ORDER BY COD_SECCION;";

                result.Result = connection.Query<DropDownListaGenericaModel>(
                    q,
                    new { cod_departamento }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }

        // ------------------------------------------------------------------------------------
        // CÓDIGO DE REPORTES MANTENIDO (COMENTADO)
        // ------------------------------------------------------------------------------------

        /// <summary>
        /// Generar emision de documentos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        // public ErrorDto<object> Activos_BoletaActivosAsignados_Lote(int codEmpresa,ActivosPersonasReporteLoteRequest request)
        // {
        //     var response = new ErrorDto<object> { Code = 0 };
        //
        //     if (request.Identificaciones == null || request.Identificaciones.Count == 0)
        //     {
        //         response.Code = -1;
        //         response.Description = "No se recibieron identificaciones para generar la boleta.";
        //         return response;
        //     }
        //
        //     try
        //     {
        //         var pdfs = new List<byte[]>();
        //
        //         foreach (var id in request.Identificaciones.Distinct())
        //         {
        //             var parametros = new
        //             {
        //                 filtros =
        //                     $" WHERE ACTIVOS_PERSONAS.IDENTIFICACION = '{id}'" +
        //                     $" AND ACTIVOS_PRINCIPAL.ESTADO <> 'R'",
        //                 Empresa = (string?)null,
        //                 fxUsuario = request.Usuario,
        //                 fxSubTitulo = "ACTIVOS VIGENTES"
        //             };
        //
        //             var reporteData = new FrmReporteGlobal
        //             {
        //                 codEmpresa = codEmpresa,
        //                 parametros = JsonConvert.SerializeObject(parametros),
        //                 nombreReporte = "Activos_BoletaActivosAsignados",
        //                 usuario = request.Usuario,
        //                 cod_reporte = "P",
        //                 folder = "Activos"
        //             };
        //
        //             var actionResult = _mReporting.ReporteRDLC_v2(reporteData);
        //
        //
        //             if (actionResult is ObjectResult objectResult)
        //             {
        //                 var res = objectResult.Value;
        //                 var jres = System.Text.Json.JsonSerializer.Serialize(res);
        //                 var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(jres);
        //
        //                 response.Code = -1;
        //                 response.Description =
        //                     err?.Description ?? $"Error al generar boleta para identificación {id}.";
        //                 return response;
        //             }
        //
        //             var fileResult = actionResult as FileContentResult;
        //
        //             if (fileResult?.FileContents == null || fileResult.FileContents.Length == 0)
        //             {
        //                 response.Code = -1;
        //                 response.Description =
        //                     $"Ocurrió un error al generar la boleta, contenido nulo/vacío para identificación {id}.";
        //                 return response;
        //             }
        //
        //             pdfs.Add(fileResult.FileContents);
        //         }
        //
        //         if (!pdfs.Any())
        //         {
        //             response.Code = -1;
        //             response.Description = "No se generaron boletas para las identificaciones indicadas.";
        //             return response;
        //         }
        //
        //         // Combinar los bytes de todos los PDFs en uno solo
        //         var combinadoBytes = MProGrXAuxiliarDB.CombinarBytesPdfSharp(pdfs.ToArray());
        //
        //         var fileCombinado = new FileContentResult(combinadoBytes, "application/pdf")
        //         {
        //             FileDownloadName = "Activos_BoletaActivosAsignados.pdf"
        //         };
        //
        //         // Igual que en Tesorería: devolver el FileContentResult serializado
        //         response.Result = JsonConvert.SerializeObject(fileCombinado, Formatting.Indented);
        //         return response;
        //     }
        //     catch (Exception ex)
        //     {
        //         response.Code = -1;
        //         response.Description = ex.Message;
        //         response.Result = null;
        //         return response;
        //     }
        // }

        //// <summary>
        /// Generar emision de documentos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        // public ErrorDto<object> Activos_ContratoResponsabilidad_Lote(int codEmpresa,ActivosPersonasReporteLoteRequest request)
        // {
        //     var response = new ErrorDto<object> { Code = 0 };
        //
        //     if (request.Identificaciones == null || request.Identificaciones.Count == 0)
        //     {
        //         response.Code = -1;
        //         response.Description = "No se recibieron identificaciones para generar el contrato.";
        //         return response;
        //     }
        //
        //     try
        //     {
        //         var pdfs = new List<byte[]>();
        //
        //         foreach (var id in request.Identificaciones.Distinct())
        //         {
        //             var parametros = new
        //             {
        //                 filtros =
        //                     $" WHERE ACTIVOS_PERSONAS.IDENTIFICACION = '{id}'" +
        //                     $" AND ACTIVOS_PRINCIPAL.ESTADO <> 'R'",
        //                 Empresa = (string?)null,
        //                 fxUsuario = request.Usuario,
        //                 fxSubTitulo = "CONTRATO DE RESPONSABILIDAD"
        //             };
        //
        //             var reporteData = new FrmReporteGlobal
        //             {
        //                 codEmpresa = codEmpresa,
        //                 parametros = JsonConvert.SerializeObject(parametros),
        //                 nombreReporte = "Activos_ContratoResponsabilidad",
        //                 usuario = request.Usuario,
        //                 cod_reporte = "P",
        //                 folder = "Activos"
        //             };
        //
        //             var actionResult = _mReporting.ReporteRDLC_v2(reporteData);
        //
        //             if (actionResult is ObjectResult objectResult)
        //             {
        //                 var res = objectResult.Value;
        //                 var jres = System.Text.Json.JsonSerializer.Serialize(res);
        //                 var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(jres);
        //
        //                 response.Code = -1;
        //                 response.Description =
        //                     err?.Description ?? $"Error al generar contrato para identificación {id}.";
        //                 return response;
        //             }
        //
        //             var fileResult = actionResult as FileContentResult;
        //
        //             if (fileResult?.FileContents == null || fileResult.FileContents.Length == 0)
        //             {
        //                 response.Code = -1;
        //                 response.Description =
        //                     $"Ocurrió un error al generar el contrato, contenido nulo/vacío para identificación {id}.";
        //                 return response;
        //             }
        //
        //             pdfs.Add(fileResult.FileContents);
        //         }
        //
        //         if (!pdfs.Any())
        //         {
        //             response.Code = -1;
        //             response.Description = "No se generaron contratos para las identificaciones indicadas.";
        //             return response;
        //         }
        //
        //         var combinadoBytes = MProGrXAuxiliarDB.CombinarBytesPdfSharp(pdfs.ToArray());
        //
        //         var fileCombinado = new FileContentResult(combinadoBytes, "application/pdf")
        //         {
        //             FileDownloadName = "Activos_ContratoResponsabilidad.pdf"
        //         };
        //
        //         response.Result = JsonConvert.SerializeObject(fileCombinado, Formatting.Indented);
        //         return response;
        //     }
        //     catch (Exception ex)
        //     {
        //         response.Code = -1;
        //         response.Description = ex.Message;
        //         response.Result = null;
        //         return response;
        //     }
        // }
    }
}