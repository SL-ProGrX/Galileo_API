using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.DataBaseTier.ProGrX.Activos_Fijos
{
    public class FrmActivosPersonasDB
    {
        private const int ModuloActivos = 36; // Módulo de Activos
        private const string Todos = "TODOS";

        private readonly MSecurityMainDb _securityMainDb;
        private readonly MReportingServicesDB _mReporting;
        private readonly PortalDB _portalDb;

        public FrmActivosPersonasDB(IConfiguration config)
        {
            _securityMainDb = new MSecurityMainDb(config);
            _mReporting     = new MReportingServicesDB(config);
            _portalDb       = new PortalDB(config);
        }

        #region Helpers privados

        private static (string? filtro, string? codDepartamento, string? codSeccion) NormalizarFiltros(
            FiltrosLazyLoadData filtros,
            string? codDepartamento,
            string? codSeccion)
        {
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

            return (filtroValor, codDepartamentoParam, codSeccionParam);
        }

        private static (int orderIndex, int orderDir) ObtenerOrden(FiltrosLazyLoadData filtros)
        {
            var sortFieldRaw  = (filtros.sortField ?? "Per.IDENTIFICACION").Trim();
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

            return (orderIndex, orderDir);
        }

        private ErrorDto<object> GenerarDocumentosLote(
            int codEmpresa,
            ActivosPersonasReporteLoteRequest request,
            string nombreReporte,
            string subTitulo,
            string nombreArchivoSalida)
        {
            var response = new ErrorDto<object> { Code = 0 };

            if (request.Identificaciones == null || request.Identificaciones.Count == 0)
            {
                response.Code        = -1;
                response.Description = $"No se recibieron identificaciones para generar {subTitulo.ToLower()}.";
                return response;
            }

            try
            {
                var pdfs = new List<byte[]>();

                foreach (var id in request.Identificaciones.Distinct())
                {
                    var parametros = new
                    {
                        filtros =
                            $" WHERE ACTIVOS_PERSONAS.IDENTIFICACION = '{id}'" +
                            $" AND ACTIVOS_PRINCIPAL.ESTADO <> 'R'",
                        Empresa    = (string?)null,
                        fxUsuario  = request.Usuario,
                        fxSubTitulo = subTitulo
                    };

                    var reporteData = new FrmReporteGlobal
                    {
                        codEmpresa   = codEmpresa,
                        parametros   = JsonConvert.SerializeObject(parametros),
                        nombreReporte = nombreReporte,
                        usuario      = request.Usuario,
                        cod_reporte  = "P",
                        folder       = "Activos"
                    };

                    var actionResult = _mReporting.ReporteRDLC_v2(reporteData);

                    if (actionResult is ObjectResult objectResult)
                    {
                        var res  = objectResult.Value;
                        var jres = System.Text.Json.JsonSerializer.Serialize(res);
                        var err  = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(jres);

                        response.Code        = -1;
                        response.Description =
                            err?.Description ?? $"Error al generar documento para identificación {id}.";
                        return response;
                    }

                    var fileResult = actionResult as FileContentResult;

                    if (fileResult?.FileContents == null || fileResult.FileContents.Length == 0)
                    {
                        response.Code        = -1;
                        response.Description =
                            $"Ocurrió un error al generar el documento, contenido nulo/vacío para identificación {id}.";
                        return response;
                    }

                    pdfs.Add(fileResult.FileContents);
                }

                if (!pdfs.Any())
                {
                    response.Code        = -1;
                    response.Description = "No se generaron documentos para las identificaciones indicadas.";
                    return response;
                }

                var combinadoBytes = MProGrXAuxiliarDB.CombinarBytesPdfSharp(pdfs.ToArray());

                var fileCombinado = new FileContentResult(combinadoBytes, "application/pdf")
                {
                    FileDownloadName = nombreArchivoSalida
                };

                response.Result = JsonConvert.SerializeObject(fileCombinado, Formatting.Indented);
                return response;
            }
            catch (Exception ex)
            {
                response.Code        = -1;
                response.Description = ex.Message;
                response.Result      = null;
                return response;
            }
        }

        #endregion

        /// <summary>
        /// Obtiene lista paginada de personas con filtros y joins a Departamentos/Secciones (Tab Nómina).
        /// </summary>
        public ErrorDto<ActivosPersonasLista> Activos_Personas_Lista_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros,
            string? codDepartamento = null,
            string? codSeccion      = null)
        {
            var listaVacia = new ActivosPersonasLista
            {
                total = 0,
                lista = new List<ActivosPersonasData>()
            };

            try
            {
                var (filtroValor, codDepartamentoParam, codSeccionParam) =
                    NormalizarFiltros(filtros, codDepartamento, codSeccion);

                var (orderIndex, orderDir) = ObtenerOrden(filtros);

                var parametros = new
                {
                    filtro         = filtroValor,
                    codDepartamento = codDepartamentoParam,
                    codSeccion      = codSeccionParam,
                    orderIndex,
                    orderDir,
                    offset         = filtros.pagina,
                    rows           = filtros.paginacion
                };

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

                var totalResult = DbHelper.ExecuteSingleQuery<int>(
                    _portalDb,
                    CodEmpresa,
                    sqlCount,
                    defaultValue: 0,
                    parameters: parametros);

                if (totalResult.Code != 0)
                {
                    return new ErrorDto<ActivosPersonasLista>
                    {
                        Code        = totalResult.Code,
                        Description = totalResult.Description,
                        Result      = listaVacia
                    };
                }

                var listaResult = DbHelper.ExecuteListQuery<ActivosPersonasData>(
                    _portalDb,
                    CodEmpresa,
                    sqlPage,
                    parametros);

                if (listaResult.Code != 0)
                {
                    return new ErrorDto<ActivosPersonasLista>
                    {
                        Code        = listaResult.Code,
                        Description = listaResult.Description,
                        Result      = listaVacia
                    };
                }

                return new ErrorDto<ActivosPersonasLista>
                {
                    Code        = 0,
                    Description = "Ok",
                    Result      = new ActivosPersonasLista
                    {
                        total = totalResult.Result,
                        lista = listaResult.Result ?? new List<ActivosPersonasData>()
                    }
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<ActivosPersonasLista>
                {
                    Code        = -1,
                    Description = ex.Message,
                    Result      = listaVacia
                };
            }
        }

        /// <summary>
        /// Obtiene lista completa de personas sin paginación (Tab Mantenimiento).
        /// </summary>
        public ErrorDto<List<ActivosPersonasData>> Activos_Personas_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros,
            string? codDepartamento = null,
            string? codSeccion      = null)
        {
            var (filtroValor, codDepartamentoParam, codSeccionParam) =
                NormalizarFiltros(filtros, codDepartamento, codSeccion);

            var parametros = new
            {
                filtro         = filtroValor,
                codDepartamento = codDepartamentoParam,
                codSeccion      = codSeccionParam
            };

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

            return DbHelper.ExecuteListQuery<ActivosPersonasData>(
                _portalDb,
                CodEmpresa,
                sql,
                parametros);
        }

        /// <summary>
        /// Inserta o actualiza persona según isNew (Tab Mantenimiento).
        /// </summary>
        public ErrorDto Activos_Personas_Guardar(int CodEmpresa, string usuario, ActivosPersonasData persona)
        {
            try
            {
                const string qExiste = @"
                    SELECT ISNULL(COUNT(*), 0) 
                    FROM ACTIVOS_PERSONAS 
                    WHERE UPPER(IDENTIFICACION) = @ident;";

                var existeResult = DbHelper.ExecuteSingleQuery<int>(
                    _portalDb,
                    CodEmpresa,
                    qExiste,
                    defaultValue: 0,
                    parameters: new { ident = persona.identificacion.ToUpper() });

                if (existeResult.Code != 0)
                {
                    // Error de base de datos
                    return new ErrorDto
                    {
                        Code        = existeResult.Code,
                        Description = existeResult.Description
                    };
                }

                int existe = existeResult.Result;

                if (persona.isNew)
                {
                    if (existe > 0)
                    {
                        return new ErrorDto
                        {
                            Code        = -2,
                            Description = $"La persona con identificación {persona.identificacion} ya existe."
                        };
                    }

                    return Activos_Personas_Insertar(CodEmpresa, usuario, persona);
                }

                // Modificación
                if (existe == 0)
                {
                    return new ErrorDto
                    {
                        Code        = -2,
                        Description = $"La persona con identificación {persona.identificacion} no existe."
                    };
                }

                return Activos_Personas_Actualizar(CodEmpresa, usuario, persona);
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code        = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Actualiza una persona existente (Tab Mantenimiento).
        /// </summary>
        private ErrorDto Activos_Personas_Actualizar(int CodEmpresa, string usuario, ActivosPersonasData persona)
        {
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

            var dbResult = DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                query,
                new
                {
                    nombre           = persona.nombre?.ToUpper(),
                    cod_departamento = persona.cod_departamento,
                    cod_seccion      = persona.cod_seccion,
                    cod_alterno      = persona.cod_alterno?.ToUpper(),
                    activo           = persona.activo ? 1 : 0,
                    usuario,
                    identificacion   = persona.identificacion.ToUpper()
                });

            if (dbResult.Code != 0)
                return dbResult;

            try
            {
                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId        = CodEmpresa,
                    Usuario          = usuario,
                    DetalleMovimiento = $"Persona : {persona.identificacion}",
                    Movimiento       = "Modifica - WEB",
                    Modulo           = ModuloActivos
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code        = -1,
                    Description = ex.Message
                };
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Inserta una nueva persona (Tab Mantenimiento).
        /// </summary>
        private ErrorDto Activos_Personas_Insertar(int CodEmpresa, string usuario, ActivosPersonasData persona)
        {
            const string query = @"
                INSERT INTO ACTIVOS_PERSONAS
                  (COD_DEPARTAMENTO, COD_SECCION, IDENTIFICACION, NOMBRE, COD_ALTERNO, ACTIVO, REGISTRO_USUARIO, REGISTRO_FECHA)
                VALUES
                  (@cod_departamento, @cod_seccion, @identificacion, @nombre, @cod_alterno, @activo, @usuario, GETDATE());";

            var dbResult = DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                query,
                new
                {
                    cod_departamento = persona.cod_departamento,
                    cod_seccion      = persona.cod_seccion,
                    identificacion   = persona.identificacion.ToUpper(),
                    nombre           = persona.nombre?.ToUpper(),
                    cod_alterno      = persona.cod_alterno?.ToUpper(),
                    activo           = persona.activo ? 1 : 0,
                    usuario
                });

            if (dbResult.Code != 0)
                return dbResult;

            try
            {
                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId        = CodEmpresa,
                    Usuario          = usuario,
                    DetalleMovimiento = $"Persona : {persona.identificacion}",
                    Movimiento       = "Registra - WEB",
                    Modulo           = ModuloActivos
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code        = -1,
                    Description = ex.Message
                };
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Elimina una persona por identificación (Tab Mantenimiento).
        /// </summary>
        public ErrorDto Activos_Personas_Eliminar(int CodEmpresa, string identificacion, string usuario)
        {
            var result = DbHelper.CreateOkResponse();

            try
            {
                const string sql = @"DELETE FROM ACTIVOS_PERSONAS WHERE IDENTIFICACION = @identificacion;";

                int rows;
                using (var connection = _portalDb.CreateConnection(CodEmpresa))
                {
                    rows = connection.Execute(sql, new
                    {
                        identificacion = identificacion.ToUpper()
                    });
                }

                if (rows == 0)
                {
                    result.Code        = -2;
                    result.Description = $"No se encontró la persona {identificacion} para eliminar.";
                    return result;
                }

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId        = CodEmpresa,
                    Usuario          = usuario,
                    DetalleMovimiento = $"Persona : {identificacion}",
                    Movimiento       = "Elimina - WEB",
                    Modulo           = ModuloActivos
                });
            }
            catch (SqlException ex) when (ex.Number == 547) // FK constraint
            {
                result.Code        = -2;
                result.Description = "Registro en uso. No se puede eliminar.";
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Valida si una identificación ya existe en ACTIVOS_PERSONAS.
        /// </summary>
        public ErrorDto Activos_Personas_Valida(int CodEmpresa, string identificacion)
        {
            const string query = @"
                SELECT COUNT(IDENTIFICACION) 
                FROM ACTIVOS_PERSONAS 
                WHERE UPPER(IDENTIFICACION) = @ident;";

            try
            {
                var dbResult = DbHelper.ExecuteSingleQuery<int>(
                    _portalDb,
                    CodEmpresa,
                    query,
                    defaultValue: 0,
                    parameters: new { ident = identificacion.ToUpper() });

                if (dbResult.Code != 0)
                {
                    return new ErrorDto
                    {
                        Code        = dbResult.Code,
                        Description = dbResult.Description
                    };
                }

                if (dbResult.Result > 0)
                {
                    return new ErrorDto
                    {
                        Code        = -1,
                        Description = "La identificación ya existe."
                    };
                }

                return new ErrorDto
                {
                    Code        = 0,
                    Description = "La identificación es válida."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code        = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Aplica el cambio de Departamento/Sección llamando a spActivos_DepartamentoCambio. Devuelve Boleta.
        /// </summary>
        public ErrorDto<CambioDeptoResponse> Activos_Personas_CambioDepto_Aplicar(
            int CodEmpresa,
            string usuario,
            CambioDeptoRequest request)
        {
            var response = new ErrorDto<CambioDeptoResponse>
            {
                Code        = 0,
                Description = "Ok",
                Result      = new CambioDeptoResponse()
            };

            try
            {
                // Usamos el helper con EXEC parametrizado
                const string sql = @"
                    EXEC spActivos_DepartamentoCambio 
                        @Identificacion, 
                        @CodDepartamento, 
                        @CodSeccion, 
                        @Usuario, 
                        @Fecha;";

                var dbResult = DbHelper.ExecuteSingleQuery<string>(
                    _portalDb,
                    CodEmpresa,
                    sql,
                    defaultValue: string.Empty,
                    parameters: new
                    {
                        Identificacion  = request.identificacion,
                        CodDepartamento = request.cod_departamento,
                        CodSeccion      = request.cod_seccion,
                        Usuario         = usuario,
                        Fecha           = request.fecha
                    });

                if (dbResult.Code != 0)
                {
                    response.Code        = dbResult.Code;
                    response.Description = dbResult.Description;
                    response.Result      = null;
                    return response;
                }

                response.Result!.boleta = dbResult.Result ?? string.Empty;

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId        = CodEmpresa,
                    Usuario          = usuario,
                    DetalleMovimiento = $"Aplica Cambio Dpto/Sec a: {request.identificacion} Boleta:{response.Result.boleta}",
                    Movimiento       = "Aplica - WEB",
                    Modulo           = ModuloActivos
                });

                return response;
            }
            catch (Exception ex)
            {
                response.Code        = -1;
                response.Description = ex.Message;
                response.Result      = null;
                return response;
            }
        }

        /// <summary>
        /// Ejecuta la sincronización con RRHH llamando a spActivos_Sincroniza_RH.
        /// </summary>
        public ErrorDto Activos_Personas_SincronizarRH(int CodEmpresa, string usuario)
        {
            const string sql = "EXEC spActivos_Sincroniza_RH;";

            var dbResult = DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sql);

            if (dbResult.Code != 0)
                return dbResult;

            try
            {
                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId        = CodEmpresa,
                    Usuario          = usuario,
                    DetalleMovimiento = "Sincronización con RRHH finalizada",
                    Movimiento       = "Procesa - WEB",
                    Modulo           = ModuloActivos
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code        = -1,
                    Description = ex.Message
                };
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Obtiene catálogo de Departamentos (item, descripcion).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Obtener(int CodEmpresa)
        {
            const string q = @"
                SELECT COD_DEPARTAMENTO AS item, DESCRIPCION
                FROM ACTIVOS_DEPARTAMENTOS
                ORDER BY COD_DEPARTAMENTO;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                q);
        }

        /// <summary>
        /// Obtiene catálogo de Secciones por Departamento (item, descripcion).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Secciones_ObtenerPorDepto(
            int CodEmpresa,
            string cod_departamento)
        {
            const string q = @"
                SELECT COD_SECCION AS item, DESCRIPCION
                FROM ACTIVOS_SECCIONES
                WHERE COD_DEPARTAMENTO = @cod_departamento
                ORDER BY COD_SECCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                q,
                new { cod_departamento });
        }

        /// <summary>
        /// Generar emisión de boletas de activos asignados en lote.
        /// </summary>
        public ErrorDto<object> Activos_BoletaActivosAsignados_Lote(
            int codEmpresa,
            ActivosPersonasReporteLoteRequest request)
        {
            return GenerarDocumentosLote(
                codEmpresa,
                request,
                nombreReporte: "Activos_BoletaActivosAsignados",
                subTitulo: "ACTIVOS VIGENTES",
                nombreArchivoSalida: "Activos_BoletaActivosAsignados.pdf");
        }

        /// <summary>
        /// Generar emisión de contratos de responsabilidad en lote.
        /// </summary>
        public ErrorDto<object> Activos_ContratoResponsabilidad_Lote(
            int codEmpresa,
            ActivosPersonasReporteLoteRequest request)
        {
            return GenerarDocumentosLote(
                codEmpresa,
                request,
                nombreReporte: "Activos_ContratoResponsabilidad",
                subTitulo: "CONTRATO DE RESPONSABILIDAD",
                nombreArchivoSalida: "Activos_ContratoResponsabilidad.pdf");
        }
    }
}