using PgxAPI.Models.TES;
using Dapper;
using PgxAPI.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Text;
using PgxAPI.Models;

namespace PgxAPI.DataBaseTier.TES
{
    public class frmTES_ConsultaDesembolsosDB
    {
        private readonly IConfiguration? _config;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmTES_ConsultaDesembolsosDB(IConfiguration? config)
        {
            _config = config;
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Verifica si el usuario tiene autorización para realizar una operación en la empresa especificada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorDto VerificarAutorizacion(int codEmpresa, string usuario)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = string.Empty
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                string connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                const string query = @"SELECT COUNT(*) 
                               FROM TES_AUTORIZACIONES 
                               WHERE ESTADO = 'A' AND NOMBRE = @Usuario";

                using var connection = new SqlConnection(connectionString);

                int count = connection.ExecuteScalar<int>(query, new { Usuario = usuario });

                if (count == 0)
                {
                    response.Code = 2;
                    response.Description = "No tiene autorización para realizar esta operación.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al verificar la autorización: {ex.Message}";
            }

            return response;
        }


        /// <summary>
        /// Obtiene los grupos de bancos activos de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de grupos de bancos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Grupos_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                string connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                const string query = @"
                                    SELECT 
                                        COD_GRUPO as item, 
                                        DESCRIPCION
                                    FROM TES_BANCOS_GRUPOS
                                    WHERE ACTIVO = 1
                                    ORDER BY DESCRIPCION";

                using var connection = new SqlConnection(connectionString);
                connection.Open();
                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener los grupos de bancos: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


        /// <summary>
        /// Obtiene las cuentas de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codGrupo">Código del grupo de bancos (opcional).</param>
        /// <returns>Lista de cuentas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Cuentas_Obtener(int codEmpresa, string usuario, string? codGrupo = null)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            if (codGrupo == "null") codGrupo = null;

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                string connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                var sql = @"
                            SELECT 
                                id_Banco AS item,
                                RTRIM(Descripcion) AS descripcion
                            FROM Tes_Bancos
                            WHERE estado = 'A'";

                if (!string.IsNullOrWhiteSpace(codGrupo) && codGrupo.ToUpper() != "TODOS")
                {
                    sql += " AND Cod_Grupo = @CodGrupo";
                }

                sql += $@" AND id_Banco 
                                    in(select id_banco from tes_documentos_ASG Where nombre = @usuario and Solicita = 1 
                                    group by id_banco)";

                using var connection = new SqlConnection(connectionString);
                connection.Open();

                response.Result = connection.Query<DropDownListaGenericaModel>(sql, new
                {
                    CodGrupo = codGrupo,
                    usuario = usuario
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener las cuentas: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


        /// <summary>
        /// Obtiene los conceptos de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de conceptos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Conceptos_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                string connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                var sql = @"
                            SELECT 
                                COD_CONCEPTO as item,
                                RTRIM(Descripcion) AS descripcion
                            FROM TES_CONCEPTOS
                            WHERE estado = 'A'";

                using var connection = new SqlConnection(connectionString);
                connection.Open();

                response.Result = connection.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener los conceptos: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


        /// <summary>
        /// Obtiene los tipos de documentos de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de tipos de documentos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Tipos_Documentos_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                string connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                var sql = @"
                            SELECT 
                                TIPO as item,
                                RTRIM(DESCRIPCION) AS descripcion
                            FROM TES_TIPOS_DOC";

                using var connection = new SqlConnection(connectionString);

                response.Result = connection.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener los tipos de documentos: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


        /// <summary>
        /// Busca desembolsos en la base de datos según los filtros proporcionados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="CodConta"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorDto<DesembolsosLista> Desembolsos_Buscar(int codEmpresa, int CodConta, FiltrosBusqueda filtros)
        {
            var response = new ErrorDto<DesembolsosLista>
            {
                Code = 0,
                Description = "OK",
                Result = new DesembolsosLista
                {
                    totales = new DesembolsoTotales()
                    {
                        total = 0,
                        montototal = 0
                    },
                    lista = new List<Desembolsos>()
                }
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                string connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                if (filtros.sortField == "" || filtros.sortField == null)
                {
                    filtros.sortField = "fecha_emision";
                }


                if (filtros.filtro != null && filtros.filtro != "")
                {
                    filtros.filtro = "where  ( nsolicitud LIKE '%" + filtros.filtro + "%' " +
                        " OR doc_interno LIKE '%" + filtros.filtro + "%' " +
                        " OR beneficiario LIKE '%" + filtros.filtro + "%' ) ";

                    //" +
                    //    " OR estado LIKE '%" + filtros.filtro + "%' " +
                    //    " OR cta_ahorros LIKE '%" + filtros.filtro + "%' " +
                    //    " OR banco LIKE '%" + filtros.filtro + "%' " +
                    //    " OR codigo LIKE '%" + filtros.filtro + "%' " +
                    //    " OR detalle LIKE '%" + filtros.filtro + "%' " +
                    //    " OR unidad LIKE '%" + filtros.filtro + "%' " +
                    //    " OR doc_banco LIKE '%" + filtros.filtro + "%'
                }


                var sql = new StringBuilder();
                sql.Append($@"
                    SELECT Id,nsolicitud,doc_interno,doc_banco,tipo,monto,estado,fecha_emision,fecha_anula,beneficiario,cta_ahorros,banco,codigo,detalle,
ref_banco,unidad,concepto,tipo_cliente,User_Solicita,User_Genera,User_Anula,cod_divisa,Tipo_Cambio,grupo_bancario,Periodo,REF_01,
REF_02,REF_03,id_desembolso,REFERENCIA_SINPE, NOMBRE_ORIGEN, USER_AUTORIZA, fecha_autoriza FROM (
                    SELECT 
                        0 AS Id,
                        C.nsolicitud,
                        ISNULL(C.ndocumento, 0) AS doc_interno,
                        ISNULL(C.DOCUMENTO_BANCO,'') AS doc_banco,
                        C.tipo,
                        C.monto,
                        CASE 
                           WHEN C.estado = 'A' THEN 'Anulado'
                            WHEN C.estado = 'P' AND c.fecha_autorizacion IS NOT NULL AND c.fecha_emision IS NULL THEN 'Autorizado'
                            WHEN ( C.estado = 'T' OR C.estado = 'I' ) AND c.fecha_emision IS NOT NULL THEN 'Emitido'
                            WHEN C.estado = 'P' THEN 'Pendiente'
                        END AS estado,
                        ISNULL(C.fecha_emision,'') AS fecha_emision,
                        ISNULL(C.fecha_Anula,'') AS fecha_anula,
                        C.beneficiario,
                        C.cta_ahorros,
                        B.descripcion AS banco,
                        C.codigo,
                        (ISNULL(C.Detalle1,'') + ' ' + ISNULL(C.Detalle2,'') + ' ' + ISNULL(C.Detalle3,'') + ' ' + ISNULL(C.Detalle4,'') + ' ' + ISNULL(C.Detalle5,'')) AS detalle,
                        ISNULL(C.REFERENCIA_BANCARIA,'') AS ref_banco,
                        U.descripcion AS unidad,
                        Con.descripcion AS concepto,
                        CASE C.Tipo_Beneficiario
                            WHEN 1 THEN 'Personas'
                            WHEN 2 THEN 'Bancos'
                            WHEN 3 THEN 'Proveedores'
                            WHEN 4 THEN 'Acreedores'
                        END AS tipo_cliente,
                        C.User_Solicita,
                        C.User_Genera,
                        C.User_Anula,
                        C.cod_divisa,
                        C.Tipo_Cambio,
                        Grp.Descripcion AS grupo_bancario,
                        CASE MONTH(C.fecha_emision)
                            WHEN 1 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 01 Enero'
                            WHEN 2 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 02 Febrero'
                            WHEN 3 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 03 Marzo'
                            WHEN 4 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 04 Abril'
                            WHEN 5 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 05 Mayo'
                            WHEN 6 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 06 Junio'
                            WHEN 7 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 07 Julio'
                            WHEN 8 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 08 Agosto'
                            WHEN 9 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 09 Septiembre'
                            WHEN 10 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 10 Octubre'
                            WHEN 11 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 11 Noviembre'
                            WHEN 12 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 12 Diciembre'
                            ELSE ''
                        END AS Periodo,
                        C.REF_01,
                        C.REF_02,
                        C.REF_03,
                        C.ID_DESEMBOLSO AS id_desembolso,
                        C.REFERENCIA_SINPE,
                        C.NOMBRE_ORIGEN,
                        C.USER_AUTORIZA,
                        ISNULL(C.FECHA_AUTORIZACION,'') AS fecha_autoriza
                    FROM Tes_Transacciones C
                    INNER JOIN Tes_Bancos B ON C.id_banco = B.id_Banco
                    LEFT JOIN tes_bancos_grupos Grp ON B.cod_grupo = Grp.Cod_Grupo
                    LEFT JOIN CntX_Unidades U ON C.cod_unidad = U.cod_unidad AND U.cod_contabilidad = @CodContabilidad
                    LEFT JOIN Tes_Conceptos Con ON C.cod_concepto = Con.cod_concepto
                     WHERE 1 = 1 
                ");

                //Busco Totales
                var sqlTotal = new StringBuilder();
                sqlTotal.Append(@"
                    SELECT 
                       COUNT(C.nsolicitud) as 'total', SUM(C.monto) as 'montototal'
                    FROM Tes_Transacciones C
                    INNER JOIN Tes_Bancos B ON C.id_banco = B.id_Banco
                    LEFT JOIN tes_bancos_grupos Grp ON B.cod_grupo = Grp.Cod_Grupo
                    LEFT JOIN CntX_Unidades U ON C.cod_unidad = U.cod_unidad AND U.cod_contabilidad = @CodContabilidad
                    LEFT JOIN Tes_Conceptos Con ON C.cod_concepto = Con.cod_concepto
                   WHERE 1 = 1
                ");


                

                var parameters = new DynamicParameters();
                parameters.Add("@CodContabilidad", CodConta);

                var sqlFiltros = new StringBuilder();

                if (!string.IsNullOrEmpty(filtros.Estado))
                {
                    sqlFiltros.Append(filtros.Estado switch
                    {
                        "E" => " AND C.estado IN ('E','T') ",
                        "T" => " AND C.estado IN ('E','T') ",
                        "A" => " AND C.estado = 'A' ",
                        "S" => " AND C.estado = 'P' ",
                        "I" => " AND C.estado = 'I' ",
                        _ => ""
                    });
                }

                if (!filtros.ChkProtegido)
                    sqlFiltros.Append(" AND ISNULL(C.MODO_PROTEGIDO,0) = 0 ");

                if (!string.IsNullOrEmpty(filtros.Usuario) && !string.IsNullOrEmpty(filtros.TipoUsuario))
                {
                    string col = filtros.TipoUsuario switch
                    {
                        "S" => "C.user_solicita",
                        "A" => "C.user_autoriza",
                        "E" => "C.user_genera",
                        "N" => "C.user_anula",
                        _ => ""
                    };

                    if (!string.IsNullOrEmpty(col))
                    {
                        sqlFiltros.Append($" AND {col} LIKE @Usuario ");
                        parameters.Add("@Usuario", $"%{filtros.Usuario}%");
                    }
                }

                if (!string.IsNullOrEmpty(filtros.Codigo))
                {
                    sqlFiltros.Append(" AND C.codigo LIKE @Codigo ");
                    parameters.Add("@Codigo", $"%{filtros.Codigo}%");
                }

                if (!string.IsNullOrEmpty(filtros.Beneficiario))
                {
                    sqlFiltros.Append(" AND C.beneficiario LIKE @Beneficiario ");
                    parameters.Add("@Beneficiario", $"%{filtros.Beneficiario}%");
                }

                if (!string.IsNullOrEmpty(filtros.Detalle))
                {
                    sqlFiltros.Append(" AND (C.Detalle1 + C.Detalle2 + ISNULL(C.Detalle3,'') + ISNULL(C.Detalle4,'') + ISNULL(C.Detalle5,'')) LIKE @Detalle ");
                    parameters.Add("@Detalle", $"%{filtros.Detalle.Trim()}%");
                }

                if (!string.IsNullOrEmpty(filtros.NoDocumento))
                {
                    sqlFiltros.Append(" AND C.ndocumento LIKE @NoDocumento ");
                    parameters.Add("@NoDocumento", $"%{filtros.NoDocumento}%");
                }

                if (!string.IsNullOrEmpty(filtros.IdAplicacion))
                {
                    sqlFiltros.Append(" AND ISNULL(C.Cod_App, '') LIKE @IdAplicacion ");
                    parameters.Add("@IdAplicacion", $"%{filtros.IdAplicacion}%");
                }

                if (!string.IsNullOrEmpty(filtros.Ref01))
                {
                    sqlFiltros.Append(" AND ISNULL(C.Ref_01, '') LIKE @Ref01 ");
                    parameters.Add("@Ref01", $"%{filtros.Ref01}%");
                }

                if (!string.IsNullOrEmpty(filtros.Ref02))
                {
                    sqlFiltros.Append(" AND ISNULL(C.Ref_02, '') LIKE @Ref02 ");
                    parameters.Add("@Ref02", $"%{filtros.Ref02}%");
                }

                if (!string.IsNullOrEmpty(filtros.Ref03))
                {
                    sqlFiltros.Append(" AND ISNULL(C.Ref_03, '') LIKE @Ref03 ");
                    parameters.Add("@Ref03", $"%{filtros.Ref03}%");
                }

                if (!string.IsNullOrEmpty(filtros.Transferencia))
                {
                    sqlFiltros.Append(" AND C.Documento_Base LIKE @Transferencia ");
                    parameters.Add("@Transferencia", $"%{filtros.Transferencia}%");
                }

                if (filtros.Cuentas?.Count > 0)
                {
                    sqlFiltros.Append(" AND C.id_banco IN @Cuentas ");
                    parameters.Add("@Cuentas", filtros.Cuentas.Select(x => x.item).ToList());
                }

                if (filtros.TiposDocumento?.Count > 0)
                {
                    sqlFiltros.Append(" AND C.tipo IN @TiposDocumento ");
                    parameters.Add("@TiposDocumento", filtros.TiposDocumento.Select(x => x.item).ToList());

                }

                if (filtros.Conceptos?.Count > 0)
                {
                    sqlFiltros.Append(" AND C.cod_concepto IN @Conceptos ");
                    parameters.Add("@Conceptos", filtros.Conceptos.Select(x => x.item).ToList());
                }

                if (filtros.TipoFecha == "E")
                {
                    sqlFiltros.Append(" AND C.fecha_emision BETWEEN @FechaInicio AND @FechaCorte ");
                }
                else if (filtros.TipoFecha == "A")
                {
                    sqlFiltros.Append(" AND C.fecha_anula BETWEEN @FechaInicio AND @FechaCorte ");
                }
                else if (filtros.TipoFecha == "S")
                {
                    sqlFiltros.Append(" AND C.fecha_solicitud BETWEEN @FechaInicio AND @FechaCorte ");
                }
                else if (filtros.TipoFecha == "I")
                {
                    sqlFiltros.Append(" AND C.fecha_autorizacion BETWEEN @FechaInicio AND @FechaCorte ");
                }

                string vFechaIni = _AuxiliarDB.validaFechaGlobal(filtros.FechaInicio.Date);
                string vFechaCorte = _AuxiliarDB.validaFechaGlobal(filtros.FechaCorte.Date.AddDays(1).AddTicks(-1));

                parameters.Add("@FechaInicio", vFechaIni);
                parameters.Add("@FechaCorte", vFechaCorte);

                sql.Append(sqlFiltros.ToString());

                sql.Append($@" ) T {filtros.filtro}");

                sql.Append( $@" order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT { filtros.paginacion } ROWS ONLY");

                using var connection = new SqlConnection(connectionString);
                var resultado = connection.Query<Desembolsos>(sql.ToString(), parameters);
                response.Result.lista = resultado.ToList();

                response.Result.totales = connection.Query<DesembolsoTotales>(sqlTotal.ToString() + sqlFiltros.ToString(), parameters).FirstOrDefault();




            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al buscar desembolsos: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


        public ErrorDto<List<Desembolsos>> Desembolsos_Exportar(int codEmpresa, int CodConta, FiltrosBusqueda filtros)
        {
            var response = new ErrorDto<List<Desembolsos>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<Desembolsos>()
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                string connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                if (filtros.sortField == "" || filtros.sortField == null)
                {
                    filtros.sortField = "fecha_emision";
                }


                if (filtros.filtro != null && filtros.filtro != "")
                {
                    filtros.filtro = "where  ( nsolicitud LIKE '%" + filtros.filtro + "%' " +
                        " OR doc_interno LIKE '%" + filtros.filtro + "%' " +
                        " OR beneficiario LIKE '%" + filtros.filtro + "%' ) ";
                }


                var sql = new StringBuilder();
                sql.Append($@"
                    SELECT Id,nsolicitud,doc_interno,doc_banco,tipo,monto,estado,fecha_emision,fecha_anula,beneficiario,cta_ahorros,banco,codigo,detalle,
ref_banco,unidad,concepto,tipo_cliente,User_Solicita,User_Genera,User_Anula,cod_divisa,Tipo_Cambio,grupo_bancario,Periodo,REF_01,
REF_02,REF_03,id_desembolso,REFERENCIA_SINPE, NOMBRE_ORIGEN, USER_AUTORIZA, fecha_autoriza FROM (
                    SELECT 
                        0 AS Id,
                        C.nsolicitud,
                        ISNULL(C.ndocumento, 0) AS doc_interno,
                        ISNULL(C.DOCUMENTO_BANCO,'') AS doc_banco,
                        C.tipo,
                        C.monto,
                        CASE 
                          WHEN C.estado = 'A' THEN 'Anulado'
                          WHEN c.fecha_autorizacion is not null AND c.FECHA_EMISION is null THEN 'Autorizado' 
                          WHEN c.FECHA_EMISION is not null THEN 'Emitido'
                          WHEN C.estado = 'P' AND c.fecha_autorizacion is null THEN 'Pendiente'
                        END AS estado,
                        ISNULL(C.fecha_emision,'') AS fecha_emision,
                        ISNULL(C.fecha_Anula,'') AS fecha_anula,
                        C.beneficiario,
                        C.cta_ahorros,
                        B.descripcion AS banco,
                        C.codigo,
                        (ISNULL(C.Detalle1,'') + ' ' + ISNULL(C.Detalle2,'') + ' ' + ISNULL(C.Detalle3,'') + ' ' + ISNULL(C.Detalle4,'') + ' ' + ISNULL(C.Detalle5,'')) AS detalle,
                        ISNULL(C.REFERENCIA_BANCARIA,'') AS ref_banco,
                        U.descripcion AS unidad,
                        Con.descripcion AS concepto,
                        CASE C.Tipo_Beneficiario
                            WHEN 1 THEN 'Personas'
                            WHEN 2 THEN 'Bancos'
                            WHEN 3 THEN 'Proveedores'
                            WHEN 4 THEN 'Acreedores'
                        END AS tipo_cliente,
                        C.User_Solicita,
                        C.User_Genera,
                        C.User_Anula,
                        C.cod_divisa,
                        C.Tipo_Cambio,
                        Grp.Descripcion AS grupo_bancario,
                        CASE MONTH(C.fecha_emision)
                            WHEN 1 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 01 Enero'
                            WHEN 2 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 02 Febrero'
                            WHEN 3 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 03 Marzo'
                            WHEN 4 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 04 Abril'
                            WHEN 5 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 05 Mayo'
                            WHEN 6 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 06 Junio'
                            WHEN 7 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 07 Julio'
                            WHEN 8 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 08 Agosto'
                            WHEN 9 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 09 Septiembre'
                            WHEN 10 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 10 Octubre'
                            WHEN 11 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 11 Noviembre'
                            WHEN 12 THEN CONVERT(varchar(4),YEAR(C.fecha_emision)) + ' - 12 Diciembre'
                            ELSE ''
                        END AS Periodo,
                        C.REF_01,
                        C.REF_02,
                        C.REF_03,
                        C.ID_DESEMBOLSO AS id_desembolso,
                        C.REFERENCIA_SINPE,
                        C.NOMBRE_ORIGEN,
                        C.USER_AUTORIZA,
                        ISNULL(C.FECHA_AUTORIZACION,'') AS fecha_autoriza
                    FROM Tes_Transacciones C
                    INNER JOIN Tes_Bancos B ON C.id_banco = B.id_Banco
                    LEFT JOIN tes_bancos_grupos Grp ON B.cod_grupo = Grp.Cod_Grupo
                    LEFT JOIN CntX_Unidades U ON C.cod_unidad = U.cod_unidad AND U.cod_contabilidad = @CodContabilidad
                    LEFT JOIN Tes_Conceptos Con ON C.cod_concepto = Con.cod_concepto
                     WHERE 1 = 1 
                ");

                var parameters = new DynamicParameters();
                parameters.Add("@CodContabilidad", CodConta);

                var sqlFiltros = new StringBuilder();

                if (!string.IsNullOrEmpty(filtros.Estado))
                {
                    sqlFiltros.Append(filtros.Estado switch
                    {
                        "E" => " AND C.estado IN ('E','T') ",
                        "T" => " AND C.estado IN ('E','T') ",
                        "A" => " AND C.estado = 'A' ",
                        "S" => " AND C.estado = 'P' ",
                        "I" => " AND C.estado = 'I' ",
                        _ => ""
                    });
                }

                if (!filtros.ChkProtegido)
                    sqlFiltros.Append(" AND ISNULL(C.MODO_PROTEGIDO,0) = 0 ");

                if (!string.IsNullOrEmpty(filtros.Usuario) && !string.IsNullOrEmpty(filtros.TipoUsuario))
                {
                    string col = filtros.TipoUsuario switch
                    {
                        "S" => "C.user_solicita",
                        "A" => "C.user_autoriza",
                        "E" => "C.user_genera",
                        "N" => "C.user_anula",
                        _ => ""
                    };

                    if (!string.IsNullOrEmpty(col))
                    {
                        sqlFiltros.Append($" AND {col} LIKE @Usuario ");
                        parameters.Add("@Usuario", $"%{filtros.Usuario}%");
                    }
                }

                if (!string.IsNullOrEmpty(filtros.Codigo))
                {
                    sqlFiltros.Append(" AND C.codigo LIKE @Codigo ");
                    parameters.Add("@Codigo", $"%{filtros.Codigo}%");
                }

                if (!string.IsNullOrEmpty(filtros.Beneficiario))
                {
                    sqlFiltros.Append(" AND C.beneficiario LIKE @Beneficiario ");
                    parameters.Add("@Beneficiario", $"%{filtros.Beneficiario}%");
                }

                if (!string.IsNullOrEmpty(filtros.Detalle))
                {
                    sqlFiltros.Append(" AND (C.Detalle1 + C.Detalle2 + ISNULL(C.Detalle3,'') + ISNULL(C.Detalle4,'') + ISNULL(C.Detalle5,'')) LIKE @Detalle ");
                    parameters.Add("@Detalle", $"%{filtros.Detalle.Trim()}%");
                }

                if (!string.IsNullOrEmpty(filtros.NoDocumento))
                {
                    sqlFiltros.Append(" AND C.ndocumento LIKE @NoDocumento ");
                    parameters.Add("@NoDocumento", $"%{filtros.NoDocumento}%");
                }

                if (!string.IsNullOrEmpty(filtros.IdAplicacion))
                {
                    sqlFiltros.Append(" AND ISNULL(C.Cod_App, '') LIKE @IdAplicacion ");
                    parameters.Add("@IdAplicacion", $"%{filtros.IdAplicacion}%");
                }

                if (!string.IsNullOrEmpty(filtros.Ref01))
                {
                    sqlFiltros.Append(" AND ISNULL(C.Ref_01, '') LIKE @Ref01 ");
                    parameters.Add("@Ref01", $"%{filtros.Ref01}%");
                }

                if (!string.IsNullOrEmpty(filtros.Ref02))
                {
                    sqlFiltros.Append(" AND ISNULL(C.Ref_02, '') LIKE @Ref02 ");
                    parameters.Add("@Ref02", $"%{filtros.Ref02}%");
                }

                if (!string.IsNullOrEmpty(filtros.Ref03))
                {
                    sqlFiltros.Append(" AND ISNULL(C.Ref_03, '') LIKE @Ref03 ");
                    parameters.Add("@Ref03", $"%{filtros.Ref03}%");
                }

                if (!string.IsNullOrEmpty(filtros.Transferencia))
                {
                    sqlFiltros.Append(" AND C.Documento_Base LIKE @Transferencia ");
                    parameters.Add("@Transferencia", $"%{filtros.Transferencia}%");
                }

                if (filtros.Cuentas?.Count > 0)
                {
                    sqlFiltros.Append(" AND C.id_banco IN @Cuentas ");
                    parameters.Add("@Cuentas", filtros.Cuentas.Select(x => x.item).ToList());
                }

                if (filtros.TiposDocumento?.Count > 0)
                {
                    sqlFiltros.Append(" AND C.tipo IN @TiposDocumento ");
                    parameters.Add("@TiposDocumento", filtros.TiposDocumento.Select(x => x.item).ToList());

                }

                if (filtros.Conceptos?.Count > 0)
                {
                    sqlFiltros.Append(" AND C.cod_concepto IN @Conceptos ");
                    parameters.Add("@Conceptos", filtros.Conceptos.Select(x => x.item).ToList());
                }

                if (filtros.TipoFecha == "E")
                {
                    sqlFiltros.Append(" AND C.fecha_emision BETWEEN @FechaInicio AND @FechaCorte ");
                }
                else if (filtros.TipoFecha == "A")
                {
                    sqlFiltros.Append(" AND C.fecha_anula BETWEEN @FechaInicio AND @FechaCorte ");
                }
                else if (filtros.TipoFecha == "S")
                {
                    sqlFiltros.Append(" AND C.fecha_solicitud BETWEEN @FechaInicio AND @FechaCorte ");
                }
                else if (filtros.TipoFecha == "I")
                {
                    sqlFiltros.Append(" AND C.fecha_autorizacion BETWEEN @FechaInicio AND @FechaCorte ");
                }

                string vFechaIni = _AuxiliarDB.validaFechaGlobal(filtros.FechaInicio.Date);
                string vFechaCorte = _AuxiliarDB.validaFechaGlobal(filtros.FechaCorte.Date.AddDays(1).AddTicks(-1));

                parameters.Add("@FechaInicio", vFechaIni);
                parameters.Add("@FechaCorte", vFechaCorte);

                sql.Append(sqlFiltros.ToString());

                sql.Append($@" ) T {filtros.filtro}");

                using var connection = new SqlConnection(connectionString);
                var resultado = connection.Query<Desembolsos>(sql.ToString(), parameters, commandTimeout: 900);
                response.Result = resultado.ToList();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al buscar desembolsos: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


    }
}