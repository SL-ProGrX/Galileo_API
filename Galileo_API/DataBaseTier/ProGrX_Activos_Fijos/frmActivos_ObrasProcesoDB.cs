using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosObrasProcesoDB
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        public FrmActivosObrasProcesoDB(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Metodo para actualizar datos de finiquito de una obra en proceso
        /// </summary>
        public ErrorDto Activos_Obras_Actualizar(int CodEmpresa, string estado, DateTime fecha_finiquito, string contrato)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    UPDATE Activos_obras 
                       SET estado         = @estado,
                           fecha_finiquito = @fecha_finiquito 
                     WHERE contrato       = @contrato";

                connection.Execute(query, new { estado, fecha_finiquito, contrato });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Metodo de consulta de tipos de obras en Proceso
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ObrasTipos_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT RTRIM(cod_tipo)     AS item,
                           RTRIM(descripcion) AS descripcion 
                    FROM   Activos_obras_tipos";

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
        /// Consulta de tipos de desembolso
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ObrasTiposDesem_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT cod_desembolso AS item,
                           descripcion    AS descripcion 
                    FROM   Activos_obras_tDesem";

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
        /// Consulta el listado de obras en proceso
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Obras_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT RTRIM(contrato)   AS item,
                           RTRIM(descripcion) AS descripcion 
                    FROM   Activos_obras";

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
        /// Metodo para consultar el listado de proveedores
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Obra_Proveedores_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT cod_proveedor AS item,
                           descripcion   AS descripcion 
                    FROM   Activos_proveedores";

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
        /// Consulta lo datos de una obra en proceso
        /// </summary>
        public ErrorDto<ActivosObrasData> Activos_Obras_Consultar(int CodEmpresa, string contrato)
        {
            var result = new ErrorDto<ActivosObrasData>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosObrasData()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT  o.contrato,
                            o.Descripcion,
                            o.Estado,
                            o.Notas,
                            o.COD_PROVEEDOR,
                            o.fecha_finiquito,
                            o.encargado,
                            o.fecha_Inicio,
                            o.fecha_estimada,
                            o.ubicacion,
                            o.presu_original,
                            o.addendums,
                            o.presu_actual,
                            o.desembolsado,
                            o.distribuido,
                            o.Registro_Usuario,
                            o.Registro_fecha,
                            o.cod_tipo,
                            T.descripcion AS TipoObra,
                            P.descripcion AS Proveedor
                    FROM    Activos_obras O 
                    INNER JOIN Activos_obras_Tipos T ON O.cod_tipo      = T.cod_tipo
                    INNER JOIN cxp_proveedores      P ON O.cod_proveedor = P.cod_proveedor
                    WHERE   O.contrato = @contrato";

                result.Result = connection
                    .Query<ActivosObrasData>(query, new { contrato })
                    .FirstOrDefault();
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
        /// Consulta de adendums (paginado + filtro, seguro para S2077)
        /// </summary>
        public ErrorDto<List<ActivosObrasProcesoAdendumsData>> Activos_ObrasAdendums_Obtener(int CodEmpresa, string contrato, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<ActivosObrasProcesoAdendumsData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosObrasProcesoAdendumsData>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add("@contrato", contrato);

                string? filtroLike = string.IsNullOrWhiteSpace(filtros?.filtro)
                    ? null
                    : $"%{filtros.filtro.Trim()}%";
                p.Add("@filtro", filtroLike, DbType.String);

                // sortField con whitelist
                var sortFieldRaw = (filtros?.sortField ?? "cod_Adendum").Trim();
                var sortFieldNorm = sortFieldRaw.ToLowerInvariant();

                string orderByCol = sortFieldNorm switch
                {
                    "cod_adendum" => "cod_Adendum",
                    "descripcion" => "descripcion",
                    "fecha"       => "fecha",
                    "monto"       => "monto",
                    _             => "cod_Adendum"
                };

                string orderDir = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";

                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 50;
                p.Add("@offset", pagina, DbType.Int32);
                p.Add("@rows", paginacion, DbType.Int32);

                const string whereSql = @"
                    WHERE contrato = @contrato
                      AND (
                            @filtro IS NULL
                            OR cod_Adendum                      LIKE @filtro
                            OR descripcion                      LIKE @filtro
                            OR CONVERT(varchar(10), fecha,120) LIKE @filtro
                            OR CONVERT(varchar(30), monto)      LIKE @filtro
                          )";

                string query = $@"
                    SELECT cod_Adendum,
                           descripcion,
                           fecha,
                           monto
                    FROM   Activos_obras_ade
                    {whereSql}
                    ORDER BY {orderByCol} {orderDir}
                    OFFSET @offset ROWS 
                    FETCH NEXT @rows ROWS ONLY;";

                result.Result = connection
                    .Query<ActivosObrasProcesoAdendumsData>(query, p)
                    .ToList();
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
        /// Consulta de lista de desembolsos (paginado + filtro, seguro para S2077)
        /// </summary>
        public ErrorDto<List<ActivosObrasProcesoDesembolsosData>> Activos_ObrasDesembolsos_Obtener(int CodEmpresa, string contrato, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<ActivosObrasProcesoDesembolsosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosObrasProcesoDesembolsosData>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add("@contrato", contrato);

                string? filtroLike = string.IsNullOrWhiteSpace(filtros?.filtro)
                    ? null
                    : $"%{filtros.filtro.Trim()}%";
                p.Add("@filtro", filtroLike, DbType.String);

                // Normalización de sortField (puede venir como "D.secuencia")
                var sortFieldRaw = (filtros?.sortField ?? "D.secuencia").Trim();
                var sfNorm = sortFieldRaw.ToLowerInvariant().Replace("d.", "");

                string orderByCol = sfNorm switch
                {
                    "secuencia"      => "D.secuencia",
                    "cod_desembolso" => "D.cod_desembolso",
                    "cod_proveedor"  => "D.COD_PROVEEDOR",
                    "documento"      => "D.Documento",
                    "fecha"          => "D.fecha",
                    "monto"          => "D.monto",
                    _                => "D.secuencia"
                };

                string orderDir = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";

                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 50;
                p.Add("@offset", pagina, DbType.Int32);
                p.Add("@rows", paginacion, DbType.Int32);

                const string whereSql = @"
                    WHERE D.contrato = @contrato
                      AND (
                            @filtro IS NULL
                            OR D.cod_desembolso                   LIKE @filtro
                            OR D.COD_PROVEEDOR                    LIKE @filtro
                            OR D.Documento                        LIKE @filtro
                            OR CONVERT(varchar(10), D.fecha,120)  LIKE @filtro
                            OR CONVERT(varchar(30), D.monto)      LIKE @filtro
                          )";

                string query = $@"
                    SELECT D.secuencia,
                           D.cod_desembolso,
                           D.COD_PROVEEDOR,
                           D.Documento,
                           D.fecha,
                           D.monto,
                           T.descripcion AS Desembolso,
                           P.descripcion AS Proveedor
                    FROM   Activos_Obras_Desem D
                    INNER JOIN Activos_obras_tDesem T ON D.cod_desembolso = T.cod_desembolso
                    INNER JOIN Activos_Proveedores P ON D.cod_proveedor  = P.cod_Proveedor 
                    {whereSql}
                    ORDER BY {orderByCol} {orderDir}
                    OFFSET @offset ROWS 
                    FETCH NEXT @rows ROWS ONLY;";

                result.Result = connection
                    .Query<ActivosObrasProcesoDesembolsosData>(query, p)
                    .ToList();
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
        /// Metodo para consulta de resultado de obras en proceso
        /// </summary>
        public ErrorDto<List<ActivosObrasProcesoResultadosData>> Activos_ObrasResultados_Obtener(int CodEmpresa, string contrato)
        {
            var result = new ErrorDto<List<ActivosObrasProcesoResultadosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosObrasProcesoResultadosData>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT O.ID_RESULTADOS,
                           'ACTIVO' AS Tipo,
                           O.num_placa,
                           A.valor_historico AS Monto,
                           O.id_adicion,
                           A.nombre,
                           T.descripcion AS TA
                    FROM   Activos_obras_resultados O 
                    INNER JOIN Activos_Principal   A ON O.num_placa   = A.num_placa
                    INNER JOIN Activos_tipo_activo T ON A.tipo_activo = T.tipo_activo
                    WHERE  O.tipo     = 'A' 
                       AND O.contrato = @contrato
                    UNION
                    SELECT O.ID_RESULTADOS,
                           'MEJORAS' AS Tipo,
                           O.num_placa,
                           A.Monto,
                           O.id_adicion,
                           A.descripcion AS nombre,
                           T.descripcion AS TA
                    FROM   Activos_obras_resultados O 
                    INNER JOIN Activos_retiro_adicion A ON O.num_placa  = A.num_placa
                                                       AND O.id_adicion = A.ID_ADDRET
                    INNER JOIN Activos_justificaciones T ON A.cod_justificacion = T.cod_justificacion
                    WHERE  O.tipo     = 'M' 
                       AND O.contrato = @contrato";

                result.Result = connection
                    .Query<ActivosObrasProcesoResultadosData>(query, new { contrato })
                    .ToList();
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
        /// Metodo para modificar el registro de la obra en proceso
        /// </summary>
        public ErrorDto Activos_Obras_Modificar(int CodEmpresa, ActivosObrasData data, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    UPDATE Activos_obras 
                       SET descripcion    = @descripcion,
                           encargado      = @encargado,
                           notas          = @notas,
                           cod_proveedor  = @cod_proveedor,
                           presu_original = @presu_original,
                           presu_actual   = @presu_actual,
                           ubicacion      = @ubicacion,
                           fecha_inicio   = @fecha_inicio,
                           fecha_estimada = @fecha_estimada,
                           cod_tipo       = @cod_tipo
                     WHERE contrato      = @contrato";

                connection.Execute(query, new
                {
                    data.descripcion,
                    data.encargado,
                    data.notas,
                    data.cod_proveedor,
                    data.presu_original,
                    data.presu_actual,
                    data.ubicacion,
                    data.fecha_inicio,
                    data.fecha_estimada,
                    data.cod_tipo,
                    data.contrato
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo Activo :  {data.contrato}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
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
        /// Metodo para insertar un nuevo registro de una obra en proceso
        /// </summary>
        public ErrorDto Activos_Obras_Insertar(int CodEmpresa, ActivosObrasData data, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    INSERT INTO Activos_obras
                        (contrato, cod_tipo, descripcion, estado, encargado, cod_proveedor,
                         fecha_inicio, fecha_estimada, notas, ubicacion, presu_original,
                         addendums, presu_actual, desembolsado, distribuido, 
                         registro_usuario, registro_fecha)
                    VALUES
                        (@contrato, @cod_tipo, @descripcion, 'P', @encargado, @cod_proveedor,
                         @fecha_inicio, @fecha_estimada, @notas, @ubicacion, @presu_original,
                         @addendums, @presu_actual, @desembolsado, @distribuido,
                         @usuario, GETDATE())";

                connection.Execute(query, new
                {
                    data.contrato,
                    data.cod_tipo,
                    data.descripcion,
                    data.encargado,
                    data.cod_proveedor,
                    data.fecha_inicio,
                    data.fecha_estimada,
                    data.notas,
                    data.ubicacion,
                    data.presu_original,
                    data.addendums,
                    data.presu_actual,
                    data.desembolsado,
                    data.distribuido,
                    usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo Activo :  {data.contrato}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
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
        /// Metodo para eliminar un registro de obras en proceso
        /// </summary>
        public ErrorDto Activos_Obra_Eliminar(int CodEmpresa, string contrato, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"DELETE FROM Activos_Obras WHERE contrato = @contrato";
                connection.Execute(query, new { contrato });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $" Tipo Activo : {contrato}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
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
        /// Metodo para guardar adendum de una obra en proceso
        /// </summary>
        public ErrorDto Activos_ObrasAdendum_Guardar(int CodEmpresa, ActivosObrasProcesoAdendumsData dato, string usuario, string contrato, decimal addendums, decimal presu_actual)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT COALESCE(COUNT(*),0) AS Existe 
                    FROM   Activos_obras_ade 
                    WHERE  cod_adendum = @cod_adendum";

                var existe = connection.QueryFirstOrDefault<int>(query, new { cod_adendum = dato.cod_Adendum });
                if (existe == 0)
                {
                    Activos_ObrasAdendum_Insertar(CodEmpresa, dato, contrato);
                    Activos_ObrasAdendum_Actualizar(CodEmpresa, contrato, dato.monto);
                }
                else
                {
                    result.Code = -2;
                    result.Description = "No se puede modificar la informacion procesada...";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        private void Activos_ObrasAdendum_Actualizar(int CodEmpresa, string contrato, decimal monto)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);
            const string query = @"
                UPDATE Activos_obras 
                   SET addendums   = addendums   + @monto,
                       presu_actual = presu_actual + @monto  
                 WHERE contrato   = @contrato";

            connection.Execute(query, new { monto, contrato });
        }

        private void Activos_ObrasAdendum_Insertar(int CodEmpresa, ActivosObrasProcesoAdendumsData data, string contrato)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);
            const string query = @"
                INSERT INTO Activos_obras_ade
                    (cod_adendum, contrato, descripcion, fecha, monto)
                VALUES
                    (@cod_adendum, @contrato, @descripcion, @fecha, @monto)";

            connection.Execute(query, new
            {
                cod_adendum = data.cod_Adendum,
                contrato,
                data.descripcion,
                data.fecha,
                data.monto
            });
        }

        /// <summary>
        /// Metodo para guardar nuevo desembolso de obra en proceso
        /// </summary>
        public ErrorDto Activos_ObrasDesembolso_Guardar(int CodEmpresa, ActivosObrasProcesoDesembolsosData dato, string usuario, string contrato)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT COALESCE(COUNT(*),0) + 1 AS Secuencia 
                    FROM   Activos_obras_desem 
                    WHERE  contrato = @contrato";

                var secuencia = connection.QueryFirstOrDefault<int>(query, new { contrato });
                dato.secuencia = secuencia;

                Activos_Desembolso_Insertar(CodEmpresa, dato, contrato);
                Activos_ObrasDesembolso_Actualizar(CodEmpresa, contrato, dato.monto);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        private void Activos_ObrasDesembolso_Actualizar(int CodEmpresa, string contrato, decimal monto)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);
            const string query = @"
                UPDATE Activos_obras 
                   SET desembolsado = desembolsado + @monto,
                       presu_actual = presu_actual - @monto  
                 WHERE contrato    = @contrato";

            connection.Execute(query, new { monto, contrato });
        }

        private void Activos_Desembolso_Insertar(int CodEmpresa, ActivosObrasProcesoDesembolsosData data, string contrato)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);
            const string query = @"
                INSERT INTO Activos_obras_desem
                    (secuencia, contrato, cod_desembolso, cod_proveedor, documento, fecha, monto)
                VALUES
                    (@secuencia, @contrato, @cod_desembolso, @cod_proveedor, @documento, @fecha, @monto)";

            connection.Execute(query, new
            {
                data.secuencia,
                contrato,
                data.cod_desembolso,
                data.cod_proveedor,
                data.documento,
                data.fecha,
                data.monto
            });
        }
    }
}