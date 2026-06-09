using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using static Galileo.DataBaseTier.MCredito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrRemesasCreditoDB
    {
        private const int ModuloCreditos = 3;
        private const string Todos = "TODOS";
        private const string TagRemesa = "S05";
        private const string PARAMETROSINVALIDOS = "Parámetros inválidos.";
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCrRemesasCreditoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la lista de fuentes disponibles.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Fuente_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>
            {
                new() { item = "1", descripcion = "Formalizaciones" },
                new() { item = "2", descripcion = "Readecuaciones de Plazos" },
                new() { item = "3", descripcion = "Traspaso de Deudas" },
                new() { item = "4", descripcion = "Retenciones" }
            });
        }

        /// <summary>
        /// Obtiene la lista de estados disponibles.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Estado_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>
            {
                new() { item = "Activas", descripcion = "Activas" },
                new() { item = "Canceladas", descripcion = "Canceladas" },
                new() { item = "Nulas", descripcion = "Nulas" },
                new() { item = "Activas y Canceladas", descripcion = "Activas y Canceladas" },
                new() { item = "Todas", descripcion = "Todas" }
            });
        }

        /// <summary>
        /// Obtiene la lista de grupos de crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Grupos_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select rtrim(cod_grupo) as item, rtrim(descripcion) as descripcion
                    from crd_grupos
                    order by descripcion;";

                return AgregarTodos(conn.Query<DropDownListaGenericaModel>(sql).ToList());
            });
        }

        /// <summary>
        /// Obtiene la lista de usuarios según grupo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Usuarios_Dropdown_Obtener(int CodEmpresa, string? codGrupo)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                codGrupo = (codGrupo ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(codGrupo) || EsTodos(codGrupo))
                {
                    return new List<DropDownListaGenericaModel> { new() { item = Todos, descripcion = Todos } };
                }

                const string sql = @"
                    select upper(rtrim(usuario)) as item, upper(rtrim(usuario)) as descripcion
                    from CRD_GRPUSERS
                    where COD_GRUPO = @codGrupo
                    order by usuario;";

                return AgregarTodos(conn.Query<DropDownListaGenericaModel>(sql, new { codGrupo }).ToList());
            });
        }

        /// <summary>
        /// Obtiene la lista de destinos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Destinos_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select rtrim(cod_destino) as item, rtrim(descripcion) as descripcion
                    from catalogo_destinos
                    order by descripcion;";

                return AgregarTodos(conn.Query<DropDownListaGenericaModel>(sql).ToList());
            });
        }

        /// <summary>
        /// Obtiene la lista de destinos asignados a una línea.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_DestinosLinea_Dropdown_Obtener(int CodEmpresa, string? codigo)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                codigo = (codigo ?? string.Empty).Trim();

                const string sql = @"
                    select rtrim(R.cod_destino) as item, rtrim(R.descripcion) as descripcion
                    from catalogo_destinos R
                    inner join catalogo_destinosAsg A on R.cod_destino = A.cod_destino
                    where A.codigo = @codigo
                    order by R.descripcion;";

                return AgregarTodos(conn.Query<DropDownListaGenericaModel>(sql, new { codigo }).ToList());
            });
        }

        /// <summary>
        /// Obtiene la lista de oficinas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Oficinas_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select rtrim(cod_oficina) as item, rtrim(descripcion) as descripcion
                    from SIF_Oficinas
                    order by cod_oficina;";

                return AgregarTodos(conn.Query<DropDownListaGenericaModel>(sql).ToList());
            });
        }

        /// <summary>
        /// Obtiene la lista de etiquetas activas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Tags_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select rtrim(tag_codigo) as item, rtrim(descripcion) as descripcion
                    from crd_remesas_tags
                    where isnull(activo,0) = 1
                    order by tag_codigo;";

                return AgregarTodos(conn.Query<DropDownListaGenericaModel>(sql).ToList());
            });
        }

        /// <summary>
        /// Obtiene la lista principal de operaciones para crear remesa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoLista> CR_RemesasCredito_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var requestResult = ParseFiltros<CrRemesasCreditoListaRequest>(parametros);
            if (requestResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoLista>(
                    requestResult.Description ?? PARAMETROSINVALIDOS);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var request = requestResult.Result ?? new CrRemesasCreditoListaRequest();
                var filtro = NormalizarFiltroPrincipal(request.filtro);
                var tagRevision = ObtenerParametrosRevision(conn);

                var lista = filtro.fuente switch
                {
                    1 => BuscarFormalizaciones(conn, filtro, tagRevision, false),
                    2 => BuscarReadecuaciones(conn, filtro),
                    3 => BuscarFormalizaciones(conn, filtro, tagRevision, true),
                    4 => BuscarRetenciones(conn, filtro),
                    _ => new List<CrRemesasCreditoData>()
                };

                return DbHelper.CreateOkResponse(AplicarLazy(lista, request.filtros));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoLista>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista principal de operaciones para crear remesa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoLista> CR_RemesasCredito_Lista_Export(int CodEmpresa, string parametros)
        {
            var requestResult = ParseFiltros<CrRemesasCreditoListaRequest>(parametros);
            if (requestResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoLista>(
                    requestResult.Description ?? PARAMETROSINVALIDOS);
            }

            var request = requestResult.Result ?? new CrRemesasCreditoListaRequest();
            request.filtros.pagina = 0;
            request.filtros.paginacion = 0;

            return CR_RemesasCredito_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(request));
        }

        /// <summary>
        /// Crea una remesa de crédito con las operaciones seleccionadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoCrearResult> CR_RemesasCredito_Crear(int CodEmpresa, CrRemesasCreditoCrearRequest request)
        {
            var result = new CrRemesasCreditoCrearResult();

            try
            {
                ValidarCrearRequest(request);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                if (conn.State != ConnectionState.Open) conn.Open();

                using var tx = conn.BeginTransaction();

                var remesa = ObtenerNuevaRemesa(conn, tx);
                var tagInfo = PrepararTagRemesa(conn, tx, request.tag_codigo);

                InsertarRemesa(conn, tx, remesa, request, tagInfo);
                InsertarRemesaDetalle(conn, tx, remesa, request);

                tx.Commit();

                result.remesa = remesa;
                result.cantidad = request.operaciones.Count;

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = request.usuario,
                    DetalleMovimiento = $"Remesa de Crédito creada: {remesa}",
                    Movimiento = "REGISTRA-WEB",
                    Modulo = ModuloCreditos
                });

                return DbHelper.CreateOkResponse(
                    result,
                    $"Remesa Creada Satisfactoriamente : Remesa({remesa})");
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, result);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, result);
            }
        }

        private static List<CrRemesasCreditoData> BuscarFormalizaciones(
            SqlConnection conn,
            CrRemesasCreditoFiltroRequest filtro,
            (bool requiere, string tag) revision,
            bool traspaso)
        {
            var sql = new StringBuilder(@"
                select
                    V.id_solicitud,
                    rtrim(V.codigo) as codigo,
                    rtrim(V.garantiax) as garantia,
                    isnull(R.Fecha_Registro, V.fechaforp) as fecha,
                    V.montoapr as monto,
                    rtrim(V.cedula) as cedula,
                    rtrim(V.nombre) as nombre,
                    case when V.estado in ('A','C') then 'Activo' else 'Nulo' end as estado,
                    rtrim(V.userfor) as usuario,
                    rtrim(isnull(V.destinoX,'')) as destino,
                    rtrim(isnull(R.observacion,'')) as observacion,
                    cast(0 as bigint) as referencia,
                    cast(0 as bit) as seleccionado
                from vCRDCreditosReportes01 V
                inner join Reg_Creditos R on V.id_solicitud = R.id_solicitud
                left join CRD_REMESA_ASG Asg
                       on V.id_solicitud = Asg.ID_SOLICITUD
                      and Asg.REFERENCIA = 0 ");

            if (revision.requiere && !filtro.creditos_no_revisados)
            {
                sql.Append(@"
                left join CRD_OPERACION_TAGS T
                       on V.ID_SOLICITUD = T.ID_SOLICITUD
                      and T.TAG_CODIGO = @tagRevision
                inner join dbo.vCRDOperacionTagsMax OT
                        on T.ID_SOLICITUD = OT.ID_SOLICITUD
                       and T.LINEA = OT.LINEA ");
            }

            sql.Append(@"
                where V.fechaforp between @fechaInicio and @fechaCorte
                  and Asg.ID_SOLICITUD is null
                  and (@codigo = '' or V.codigo = @codigo)
                  and (@codGrupo = '' or V.cod_grupo = @codGrupo)
                  and (@codOficina = '' or V.cod_oficina_f = @codOficina)
                  and (@codDestino = '' or V.cod_destino = @codDestino)
                  and ((@traspaso = 1 and V.referencia is not null)
                    or (@traspaso = 0 and V.referencia is null)) ");

            sql.Append(EstadoSql("V.estado"));

            if (revision.requiere && !filtro.creditos_no_revisados)
            {
                sql.Append(@"
                  and R.ANALISTAS_REVISION = 1
                  and (@usuario = '' or T.REGISTRO_USUARIO = @usuario)
                order by T.REGISTRO_USUARIO,T.REGISTRO_FECHA,V.codigo,V.nombre asc;");
            }
            else
            {
                sql.Append(" order by V.codigo,V.nombre asc;");
            }

            return conn.Query<CrRemesasCreditoData>(
                sql.ToString(),
                ParamsBase(filtro, revision.tag, traspaso)).ToList();
        }

        private static List<CrRemesasCreditoData> BuscarReadecuaciones(SqlConnection conn, CrRemesasCreditoFiltroRequest filtro)
        {
            var sql = new StringBuilder(@"
                select
                    C.id_solicitud,
                    rtrim(C.codigo) as codigo,
                    rtrim(T.descripcion) as garantia,
                    C.fecha,
                    R.montoapr as monto,
                    rtrim(R.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    rtrim(R.estado) as estado,
                    rtrim(C.usuario) as usuario,
                    rtrim(isnull(D.descripcion,'')) as destino,
                    rtrim(isnull(C.Detalle,'')) as observacion,
                    C.id_credito_suBit as referencia,
                    cast(0 as bit) as seleccionado
                from credito_suBit C
                inner join reg_Creditos R on C.id_solicitud = R.id_solicitud
                inner join Socios S on R.cedula = S.cedula
                inner join Crd_Garantia_Tipos T on R.garantia = T.garantia
                left join Catalogo_Destinos D on R.cod_destino = D.cod_destino
                where C.tipo = 'C'
                  and C.Movimiento = '01'
                  and C.id_credito_suBit not in(select referencia from CRD_REMESA_ASG)
                  and C.fecha between @fechaInicio and @fechaCorte
                  and (@codigo = '' or R.codigo = @codigo) ");

            sql.Append(EstadoSql("R.estado"));
            sql.Append(" order by R.codigo,S.nombre asc;");

            return conn.Query<CrRemesasCreditoData>(
                sql.ToString(),
                ParamsBase(filtro)).ToList();
        }

        private static List<CrRemesasCreditoData> BuscarRetenciones(SqlConnection conn, CrRemesasCreditoFiltroRequest filtro)
        {
            var sql = new StringBuilder(@"
                select
                    R.id_solicitud,
                    rtrim(R.codigo) as codigo,
                    rtrim(T.descripcion) as garantia,
                    R.fechaforp as fecha,
                    R.cuota * R.plazo as monto,
                    rtrim(R.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    rtrim(R.estado) as estado,
                    rtrim(R.userfor) as usuario,
                    '' as destino,
                    '' as observacion,
                    cast(0 as bigint) as referencia,
                    cast(0 as bit) as seleccionado
                from Reg_creditos R
                inner join Catalogo C
                        on R.codigo = C.codigo
                       and C.retencion = 'S'
                inner join Socios S on R.cedula = S.cedula
                inner join Crd_Garantia_Tipos T on R.garantia = T.garantia
                where R.fechaforp between @fechaInicio and @fechaCorte
                  and R.id_solicitud not in(
                        select id_solicitud
                        from CRD_REMESA_ASG
                        where referencia = 0)
                  and (@codigo = '' or R.codigo = @codigo) ");

            sql.Append(EstadoSql("R.estado"));
            sql.Append(" order by R.codigo,S.nombre asc;");

            return conn.Query<CrRemesasCreditoData>(
                sql.ToString(),
                ParamsBase(filtro)).ToList();
        }

        private static int ObtenerNuevaRemesa(SqlConnection conn, SqlTransaction tx)
        {
            const string sql = "select isnull(max(remesa),0) + 1 from crd_remesas;";
            return conn.QuerySingle<int>(sql, transaction: tx);
        }

        private static (string tag, int consecutivo) PrepararTagRemesa(SqlConnection conn, SqlTransaction tx, string tagCodigo)
        {
            tagCodigo = (tagCodigo ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(tagCodigo) || EsTodos(tagCodigo))
            {
                return (string.Empty, 0);
            }

            const string sqlConsec = @"
                select isnull(consecutivo,0) + 1
                from crd_remesas_tags
                where tag_codigo = @tag;";

            var consecutivo = conn.QuerySingle<int>(sqlConsec, new { tag = tagCodigo }, tx);

            const string sqlUpdate = @"
                update crd_remesas_tags
                set consecutivo = @consecutivo
                where tag_codigo = @tag;";

            conn.Execute(sqlUpdate, new { tag = tagCodigo, consecutivo }, tx);

            return (tagCodigo, consecutivo);
        }

        private static void InsertarRemesa(
            SqlConnection conn,
            SqlTransaction tx,
            int remesa,
            CrRemesasCreditoCrearRequest request,
            (string tag, int consecutivo) tagInfo)
        {
            const string sql = @"
                insert CRD_REMESAS(remesa,fecha,usuario,notas,tag_codigo,tag_consecutivo)
                values(@remesa,dbo.MyGetdate(),@usuario,@notas,nullif(@tag,''),nullif(@consecutivo,0));";

            conn.Execute(sql, new
            {
                remesa,
                usuario = (request.usuario ?? string.Empty).Trim(),
                notas = (request.notas ?? string.Empty).Trim(),
                tag = tagInfo.tag,
                consecutivo = tagInfo.consecutivo
            }, tx);
        }

        private static void InsertarRemesaDetalle(
            SqlConnection conn,
            SqlTransaction tx,
            int remesa,
            CrRemesasCreditoCrearRequest request)
        {
            var linea = 1;

            foreach (var item in request.operaciones)
            {
                const string sql = @"
                    insert CRD_REMESA_ASG(remesa,id_solicitud,referencia,linea)
                    values(@remesa,@idSolicitud,@referencia,@linea);";

                conn.Execute(sql, new
                {
                    remesa,
                    idSolicitud = item.id_solicitud,
                    referencia = item.referencia,
                    linea
                }, tx);

                MCredito.sbCrdOperacionTags(conn, tx, new CrOperacionTagRegistrarRequest
                {
                    operacion = item.id_solicitud,
                    linea = item.codigo,
                    tag = TagRemesa,
                    usuario = request.usuario,
                    notas = $"Remesa de Crédito No..:{remesa}"
                });

                linea++;
            }
        }

        private static void ValidarCrearRequest(CrRemesasCreditoCrearRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("La solicitud es requerida.");
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                throw new InvalidOperationException("El usuario es requerido.");
            }

            if (request.operaciones == null || request.operaciones.Count == 0)
            {
                throw new InvalidOperationException("Debe seleccionar al menos una operación.");
            }
        }
        /// <summary>
        /// Obtiene la lista de secuencias/tags de remesas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoTagLista> CR_RemesasCredito_Tags_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var filtrosResult = ParseFiltros<FiltrosLazyLoadData>(parametros);
            if (filtrosResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoTagLista>(
                    filtrosResult.Description ?? PARAMETROSINVALIDOS);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var filtros = filtrosResult.Result ?? new FiltrosLazyLoadData();
                var texto = (filtros.filtro ?? string.Empty).Trim();
                var like = string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%";

                const string sql = @"
                    select
                        rtrim(tag_codigo) as tag_codigo,
                        rtrim(descripcion) as descripcion,
                        isnull(activo,0) as activo,
                        isnull(consecutivo,0) as consecutivo,
                        cast(0 as bit) as isNew
                    from crd_remesas_tags
                    where (@texto = '' or tag_codigo like @like or descripcion like @like)
                    order by tag_codigo;";

                var lista = conn.Query<CrRemesasCreditoTagData>(sql, new
                {
                    texto,
                    like
                }).ToList();

                return DbHelper.CreateOkResponse(AplicarLazyTags(lista, filtros));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoTagLista>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista de secuencias/tags de remesas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoTagLista> CR_RemesasCredito_Tags_Lista_Export(int CodEmpresa, string parametros)
        {
            var filtrosResult = ParseFiltros<FiltrosLazyLoadData>(parametros);
            if (filtrosResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoTagLista>(
                    filtrosResult.Description ?? PARAMETROSINVALIDOS);
            }

            var filtros = filtrosResult.Result ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_RemesasCredito_Tags_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda una secuencia/tag de remesas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_RemesasCredito_Tags_Guardar(int CodEmpresa, CrRemesasCreditoTagGuardarRequest request)
        {
            try
            {
                ValidarTagRequest(request);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                if (conn.State != ConnectionState.Open) conn.Open();

                var existe = ExisteTagRemesa(conn, request.tag_codigo);

                if (existe)
                {
                    ActualizarTagRemesa(conn, request);
                }
                else
                {
                    InsertarTagRemesa(conn, request);
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = request.usuario,
                    DetalleMovimiento = $"Remesas de Crédito [TAG] : {request.tag_codigo}",
                    Movimiento = existe ? "MODIFICA-WEB" : "REGISTRA-WEB",
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Secuencia/Tag guardado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de remesas para informes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoInformeLista> CR_RemesasCredito_Informes_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var requestResult = ParseFiltros<CrRemesasCreditoInformeListaRequest>(parametros);
            if (requestResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoInformeLista>(
                    requestResult.Description ?? PARAMETROSINVALIDOS);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var request = requestResult.Result ?? new CrRemesasCreditoInformeListaRequest();
                var filtro = NormalizarInformeFiltro(request.filtro);
                var lista = ObtenerInformes(conn, filtro);

                return DbHelper.CreateOkResponse(AplicarLazyInformes(lista, request.filtros));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoInformeLista>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista de remesas para informes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoInformeLista> CR_RemesasCredito_Informes_Lista_Export(int CodEmpresa, string parametros)
        {
            var requestResult = ParseFiltros<CrRemesasCreditoInformeListaRequest>(parametros);
            if (requestResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoInformeLista>(
                    requestResult.Description ?? PARAMETROSINVALIDOS);
            }

            var request = requestResult.Result ?? new CrRemesasCreditoInformeListaRequest();
            request.filtros.pagina = 0;
            request.filtros.paginacion = 0;

            return CR_RemesasCredito_Informes_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(request));
        }

        /// <summary>
        /// Consulta los datos de archivo digital de una remesa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoArchivoDigitalDto> CR_RemesasCredito_ArchivoDigital_Consultar(int CodEmpresa, int remesa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        remesa,
                        isnull(rtrim(microfilm_usuario),'') as microfilm_usuario,
                        microfilm_fecha
                    from crd_remesas
                    where remesa = @remesa;";

                var data = conn.QueryFirstOrDefault<CrRemesasCreditoArchivoDigitalDto>(sql, new { remesa });

                return data == null
                    ? DbHelper.CreateErrorResponse<CrRemesasCreditoArchivoDigitalDto>("La remesa indicada no existe.", -1, new CrRemesasCreditoArchivoDigitalDto { remesa = remesa })
                    : DbHelper.CreateOkResponse(data);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoArchivoDigitalDto>(ex.Message);
            }
        }

        /// <summary>
        /// Marca una remesa como recibida en archivo digital.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_RemesasCredito_ArchivoDigital_Recibir(int CodEmpresa, CrRemesasCreditoArchivoDigitalRequest request)
        {
            try
            {
                if (request == null || request.remesa <= 0)
                {
                    return DbHelper.ErrorResponse("La remesa es requerida.");
                }

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                if (conn.State != ConnectionState.Open) conn.Open();

                ValidarArchivoDigital(conn, request.remesa);

                const string sql = @"
                    update crd_remesas
                    set Microfilm_Fecha = dbo.MyGetdate(),
                        Microfilm_usuario = @usuario
                    where remesa = @remesa;";

                conn.Execute(sql, new
                {
                    request.remesa,
                    usuario = (request.usuario ?? string.Empty).Trim()
                });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = request.usuario ?? string.Empty,
                    DetalleMovimiento = $"Remesa de Crédito recibida en Archivo Digital: {request.remesa}",
                    Movimiento = "MODIFICA-WEB",
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Recibo (Microfilm ) Satisfactoriamente...!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la remesa asociada a una operación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoConsultaDto> CR_RemesasCredito_Consulta_Operacion_Obtener(int CodEmpresa, long operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select top 1
                        A.remesa,
                        A.fecha,
                        rtrim(A.usuario) as usuario
                    from crd_remesas A
                    inner join crd_remesa_asg X on A.remesa = X.remesa
                    where X.id_solicitud = @operacion
                    order by A.fecha desc;";

                var data = conn.QueryFirstOrDefault<CrRemesasCreditoConsultaDto>(sql, new { operacion });

                if (data == null)
                {
                    return DbHelper.CreateOkResponse(new CrRemesasCreditoConsultaDto
                    {
                        texto = "** No se encontró operación en las remesas registradas **"
                    });
                }

                data.texto = $"Remesa\t ...:{data.remesa}{Environment.NewLine}Fecha\t ...:{data.fecha}{Environment.NewLine}Usuario\t ...:{data.usuario}";
                return DbHelper.CreateOkResponse(data);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoConsultaDto>(ex.Message);
            }
        }

        /// <summary>
        /// Carga operaciones desde listado Excel y devuelve datos de remesas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoListadoCargaResult> CR_RemesasCredito_Listados_Cargar(int CodEmpresa, CrRemesasCreditoListadoCargaRequest request)
        {
            var response = new CrRemesasCreditoListadoCargaResult();

            try
            {
                if (request == null || request.operaciones == null || request.operaciones.Count == 0)
                {
                    return DbHelper.CreateErrorResponse("No existen operaciones para cargar.", -1, response);
                }

                var operaciones = request.operaciones
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                if (operaciones.Count == 0)
                {
                    return DbHelper.CreateErrorResponse("No existen operaciones válidas para cargar.", -1, response);
                }

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var lista = ObtenerListadoCarga(conn, operaciones);

                response.lista = lista;
                response.total = lista.Count;

                return DbHelper.CreateOkResponse(response, "Información Cargada Satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Exporta operaciones cargadas desde listado Excel.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoListadoCargaResult> CR_RemesasCredito_Listados_Export(int CodEmpresa, CrRemesasCreditoListadoCargaRequest request)
        {
            return CR_RemesasCredito_Listados_Cargar(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene los datos base para imprimir reporte de remesa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrRemesasCreditoReporteDto> CR_RemesasCredito_Reporte_Datos_Obtener(int CodEmpresa, CrRemesasCreditoReporteRequest request)
        {
            try
            {
                if (request == null || request.remesa <= 0)
                {
                    return DbHelper.CreateErrorResponse<CrRemesasCreditoReporteDto>("La remesa es requerida.");
                }

                var tipo = (request.tipo_reporte ?? string.Empty).Trim();

                return DbHelper.CreateOkResponse(new CrRemesasCreditoReporteDto
                {
                    remesa = request.remesa,
                    titulo = "REMESA DE CREDITOS",
                    filtro = string.Empty,
                    nombre_reporte = NombreReporte(tipo),
                    subtitulo = SubtituloReporte(tipo, request.remesa)
                });
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrRemesasCreditoReporteDto>(ex.Message);
            }
        }

        private static List<CrRemesasCreditoInformeData> ObtenerInformes(SqlConnection conn, CrRemesasCreditoInformeFiltroRequest filtro)
        {
            var sql = new StringBuilder(@"
                select top (@top)
                    remesa,
                    fecha,
                    rtrim(usuario) as usuario,
                    isnull(notas,'') as notas,
                    microfilm_fecha,
                    isnull(rtrim(microfilm_usuario),'') as microfilm_usuario,
                    isnull(rtrim(tag_codigo),'') as tag_codigo,
                    isnull(tag_consecutivo,0) as tag_consecutivo
                from crd_remesas
                where (@tag = '' or tag_codigo = @tag)
                  and (@consecutivo <= 0 or tag_consecutivo = @consecutivo) ");

            if (!filtro.todas_fechas)
            {
                sql.Append(" and fecha between @fechaInicio and @fechaCorte ");
            }

            sql.Append(" order by fecha desc;");

            return conn.Query<CrRemesasCreditoInformeData>(sql.ToString(), new
            {
                top = filtro.top,
                tag = (filtro.tag_codigo ?? string.Empty).Trim(),
                consecutivo = filtro.tag_consecutivo,
                fechaInicio = filtro.fecha_inicio?.Date,
                fechaCorte = filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1)
            }).ToList();
        }

        private static List<CrRemesasCreditoListadoCargaData> ObtenerListadoCarga(SqlConnection conn, List<long> operaciones)
        {
            const string sql = @"
                select
                    R.id_solicitud,
                    rtrim(R.codigo) as codigo,
                    isnull(rtrim(D.descripcion),'') as destino,
                    rtrim(G.descripcion) as garantia,
                    R.montoapr as monto,
                    R.fechaforp as fecha,
                    rtrim(R.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    X.remesa,
                    isnull(rtrim(X.usuario),'') as usuario,
                    X.microfilm_fecha,
                    isnull(rtrim(X.microfilm_usuario),'') as microfilm_usuario,
                    isnull(rtrim(T.descripcion),'') as tag_descripcion,
                    X.tag_consecutivo,
                    case
                        when R.estado = 'A' then 'Activo'
                        when R.estado = 'C' then 'Cancelado'
                        when R.estado = 'N' then 'Anulada'
                        else 'No Ident.'
                    end as estado
                from reg_creditos R
                inner join Socios S on R.cedula = S.cedula
                inner join crd_garantia_tipos G on R.garantia = G.garantia
                left join catalogo_destinos D on R.cod_destino = D.cod_destino
                left join crd_remesa_asg A on R.id_solicitud = A.id_solicitud
                left join crd_remesas X on A.remesa = X.remesa
                left join crd_remesas_tags T on X.tag_codigo = T.tag_codigo
                where R.id_solicitud in @operaciones
                order by R.id_solicitud;";

            return conn.Query<CrRemesasCreditoListadoCargaData>(sql, new { operaciones }).ToList();
        }

        private static bool ExisteTagRemesa(SqlConnection conn, string tagCodigo)
        {
            const string sql = @"
                select count(1)
                from crd_remesas_tags
                where tag_codigo = @tagCodigo;";

            return conn.QuerySingle<int>(sql, new { tagCodigo = (tagCodigo ?? string.Empty).Trim() }) > 0;
        }

        private static void InsertarTagRemesa(SqlConnection conn, CrRemesasCreditoTagGuardarRequest request)
        {
            const string sql = @"
                insert crd_remesas_tags(tag_codigo,descripcion,activo,consecutivo)
                values(@tagCodigo,@descripcion,@activo,@consecutivo);";

            conn.Execute(sql, TagParams(request));
        }

        private static void ActualizarTagRemesa(SqlConnection conn, CrRemesasCreditoTagGuardarRequest request)
        {
            const string sql = @"
                update crd_remesas_tags
                set descripcion = @descripcion,
                    activo = @activo,
                    consecutivo = @consecutivo
                where tag_codigo = @tagCodigo;";

            conn.Execute(sql, TagParams(request));
        }

        private static object TagParams(CrRemesasCreditoTagGuardarRequest request)
        {
            return new
            {
                tagCodigo = (request.tag_codigo ?? string.Empty).Trim(),
                descripcion = (request.descripcion ?? string.Empty).Trim(),
                activo = request.activo.GetValueOrDefault(),
                consecutivo = request.consecutivo.GetValueOrDefault()
            };
        }

        private static void ValidarTagRequest(CrRemesasCreditoTagGuardarRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("La solicitud es requerida.");
            }

            if (string.IsNullOrWhiteSpace(request.tag_codigo))
            {
                throw new InvalidOperationException("El código de tag es requerido.");
            }

            if (string.IsNullOrWhiteSpace(request.descripcion))
            {
                throw new InvalidOperationException("La descripción es requerida.");
            }
        }

        private static void ValidarArchivoDigital(SqlConnection conn, int remesa)
        {
            const string sql = @"
                select microfilm_fecha
                from crd_remesas
                where remesa = @remesa;";

            var fecha = conn.QueryFirstOrDefault<DateTime?>(sql, new { remesa });

            if (fecha.HasValue)
            {
                throw new InvalidOperationException("La remesa ya fue recibida en Microfilm o no existe...verifique!");
            }

            const string sqlExiste = "select count(1) from crd_remesas where remesa = @remesa;";
            if (conn.QuerySingle<int>(sqlExiste, new { remesa }) <= 0)
            {
                throw new InvalidOperationException("La remesa ya fue recibida en Microfilm o no existe...verifique!");
            }
        }

        private static string NombreReporte(string tipo)
        {
            return tipo switch
            {
                "AGRUPADO" => "Credito_RemesasDetalleAgrupado",
                "READECUACIONES" => "Credito_RemesasReadecuaciones",
                "ORDEN_REVISION" => "Credito_RemesasDetalleOrdenRevision",
                _ => "Credito_RemesasDetalle"
            };
        }

        private static string SubtituloReporte(string tipo, int remesa)
        {
            return tipo switch
            {
                "AGRUPADO" => $"REMESA : {remesa} LISTADO : DETALLADO AGRUPADO",
                "READECUACIONES" => $"REMESA : {remesa} LISTADO : READECUACIONES",
                "ORDEN_REVISION" => $"REMESA : {remesa} LISTADO : DETALLADO ORDEN REVISIÓN",
                _ => $"REMESA : {remesa} LISTADO : DETALLADO"
            };
        }
        private static ErrorDto<T> ParseFiltros<T>(string parametros) where T : new()
        {
            try
            {
                return DbHelper.CreateOkResponse(
                    JsonConvert.DeserializeObject<T>(parametros) ?? new T());
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<T>(ex.Message);
            }
        }

        private static CrRemesasCreditoFiltroRequest NormalizarFiltroPrincipal(CrRemesasCreditoFiltroRequest filtro)
        {
            filtro ??= new CrRemesasCreditoFiltroRequest();

            filtro.fuente = filtro.fuente <= 0 ? 1 : filtro.fuente;
            filtro.fecha_inicio ??= DateTime.Today;
            filtro.fecha_corte ??= filtro.fecha_inicio;
            filtro.estado = string.IsNullOrWhiteSpace(filtro.estado) ? "Activas" : filtro.estado.Trim();
            filtro.cod_grupo = NormalizarTodos(filtro.cod_grupo);
            filtro.usuario = NormalizarTodos(filtro.usuario);
            filtro.cod_destino = NormalizarTodos(filtro.cod_destino);
            filtro.cod_oficina = NormalizarTodos(filtro.cod_oficina);
            filtro.codigo = (filtro.codigo ?? string.Empty).Trim();

            return filtro;
        }

        private static CrRemesasCreditoInformeFiltroRequest NormalizarInformeFiltro(CrRemesasCreditoInformeFiltroRequest filtro)
        {
            filtro ??= new CrRemesasCreditoInformeFiltroRequest();

            filtro.fecha_inicio ??= DateTime.Today;
            filtro.fecha_corte ??= filtro.fecha_inicio;
            filtro.tag_codigo = NormalizarTodos(filtro.tag_codigo);
            filtro.top = filtro.top <= 0 ? 15 : filtro.top;

            return filtro;
        }

        private static (bool requiere, string tag) ObtenerParametrosRevision(SqlConnection conn)
        {
            const string sql = @"
                select
                    isnull((select valor from CRD_PARAMETROS where cod_parametro = '25'),'') as requiere,
                    isnull((select valor from CRD_PARAMETROS where cod_parametro = '26'),'') as tag;";

            var data = conn.QueryFirstOrDefault<dynamic>(sql);

            return (
                string.Equals(Convert.ToString(data?.requiere), "S", StringComparison.OrdinalIgnoreCase),
                Convert.ToString(data?.tag) ?? string.Empty
            );
        }

        private static object ParamsBase(CrRemesasCreditoFiltroRequest filtro)
        {
            return ParamsBase(filtro, string.Empty, false);
        }

        private static object ParamsBase(CrRemesasCreditoFiltroRequest filtro, string tagRevision, bool traspaso)
        {
            return new
            {
                fechaInicio = filtro.fecha_inicio?.Date,
                fechaCorte = filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1),
                estado = filtro.estado,
                codGrupo = filtro.cod_grupo,
                usuario = filtro.usuario,
                codDestino = filtro.cod_destino,
                codOficina = filtro.cod_oficina,
                codigo = filtro.codigo,
                tagRevision,
                traspaso = traspaso ? 1 : 0
            };
        }

        private static string EstadoSql(string campo)
        {
            return $@"
                and (
                       @estado = 'Todas'
                    or (@estado = 'Activas' and {campo} = 'A')
                    or (@estado = 'Canceladas' and {campo} = 'C')
                    or (@estado = 'Nulas' and {campo} = 'N')
                    or (@estado = 'Activas y Canceladas' and {campo} in ('A','C'))
                ) ";
        }

        private static CrRemesasCreditoLista AplicarLazy(List<CrRemesasCreditoData> lista, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            var data = AplicarFiltroOrdenLista(lista, filtros);
            var total = data.Count;

            if (filtros.paginacion > 0)
            {
                data = data
                    .Skip(Math.Max(filtros.pagina, 0) * filtros.paginacion)
                    .Take(filtros.paginacion)
                    .ToList();
            }

            return new CrRemesasCreditoLista
            {
                total = total,
                lista = data
            };
        }

        private static CrRemesasCreditoTagLista AplicarLazyTags(List<CrRemesasCreditoTagData> lista, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            lista = OrdenarTags(lista, filtros);

            var total = lista.Count;

            if (filtros.paginacion > 0)
            {
                lista = lista
                    .Skip(Math.Max(filtros.pagina, 0) * filtros.paginacion)
                    .Take(filtros.paginacion)
                    .ToList();
            }

            return new CrRemesasCreditoTagLista
            {
                total = total,
                lista = lista
            };
        }

        private static List<CrRemesasCreditoTagData> OrdenarTags(List<CrRemesasCreditoTagData> lista, FiltrosLazyLoadData filtros)
        {
            var asc = filtros.sortOrder == 0;
            var sort = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();

            return sort switch
            {
                "tag_codigo" => Ordenar(lista, x => x.tag_codigo, asc),
                "descripcion" => Ordenar(lista, x => x.descripcion, asc),
                "consecutivo" => Ordenar(lista, x => x.consecutivo, asc),
                "activo" => Ordenar(lista, x => x.activo, asc),
                _ => lista
            };
        }

        private static CrRemesasCreditoInformeLista AplicarLazyInformes(List<CrRemesasCreditoInformeData> lista, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            lista = AplicarFiltroOrdenInformes(lista, filtros);

            var total = lista.Count;

            if (filtros.paginacion > 0)
            {
                lista = lista
                    .Skip(Math.Max(filtros.pagina, 0) * filtros.paginacion)
                    .Take(filtros.paginacion)
                    .ToList();
            }

            return new CrRemesasCreditoInformeLista
            {
                total = total,
                lista = lista
            };
        }
        private static List<CrRemesasCreditoInformeData> AplicarFiltroOrdenInformes(List<CrRemesasCreditoInformeData> lista, FiltrosLazyLoadData filtros)
        {
            var texto = (filtros.filtro ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                lista = lista
                    .Where(x =>
                        Contiene(x.remesa.ToString(), texto) ||
                        Contiene(x.usuario, texto) ||
                        Contiene(x.notas, texto) ||
                        Contiene(x.microfilm_usuario, texto) ||
                        Contiene(x.tag_codigo, texto) ||
                        Contiene(x.tag_consecutivo.ToString(), texto))
                    .ToList();
            }

            return OrdenarInformes(lista, filtros);
        }
        private static List<CrRemesasCreditoData> AplicarFiltroOrdenLista(List<CrRemesasCreditoData> lista, FiltrosLazyLoadData filtros)
        {
            var texto = (filtros.filtro ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                lista = lista
                    .Where(x =>
                        Contiene(x.id_solicitud.ToString(), texto) ||
                        Contiene(x.codigo, texto) ||
                        Contiene(x.garantia, texto) ||
                        Contiene(x.cedula, texto) ||
                        Contiene(x.nombre, texto) ||
                        Contiene(x.estado, texto) ||
                        Contiene(x.usuario, texto) ||
                        Contiene(x.destino, texto) ||
                        Contiene(x.observacion, texto))
                    .ToList();
            }

            return OrdenarLista(lista, filtros);
        }
        private static List<CrRemesasCreditoData> OrdenarLista(List<CrRemesasCreditoData> lista, FiltrosLazyLoadData filtros)
        {
            var asc = filtros.sortOrder == 0;
            var sort = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();

            return sort switch
            {
                "id_solicitud" => Ordenar(lista, x => x.id_solicitud, asc),
                "codigo" => Ordenar(lista, x => x.codigo, asc),
                "garantia" => Ordenar(lista, x => x.garantia, asc),
                "fecha" => Ordenar(lista, x => x.fecha, asc),
                "monto" => Ordenar(lista, x => x.monto, asc),
                "cedula" => Ordenar(lista, x => x.cedula, asc),
                "nombre" => Ordenar(lista, x => x.nombre, asc),
                "estado" => Ordenar(lista, x => x.estado, asc),
                "usuario" => Ordenar(lista, x => x.usuario, asc),
                "destino" => Ordenar(lista, x => x.destino, asc),
                "observacion" => Ordenar(lista, x => x.observacion, asc),
                "referencia" => Ordenar(lista, x => x.referencia, asc),
                _ => lista
            };
        }
        private static List<CrRemesasCreditoInformeData> OrdenarInformes(List<CrRemesasCreditoInformeData> lista, FiltrosLazyLoadData filtros)
        {
            var asc = filtros.sortOrder == 0;
            var sort = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();

            return sort switch
            {
                "remesa" => Ordenar(lista, x => x.remesa, asc),
                "fecha" => Ordenar(lista, x => x.fecha, asc),
                "usuario" => Ordenar(lista, x => x.usuario, asc),
                "notas" => Ordenar(lista, x => x.notas, asc),
                "microfilm_fecha" => Ordenar(lista, x => x.microfilm_fecha, asc),
                "microfilm_usuario" => Ordenar(lista, x => x.microfilm_usuario, asc),
                "tag_codigo" => Ordenar(lista, x => x.tag_codigo, asc),
                "tag_consecutivo" => Ordenar(lista, x => x.tag_consecutivo, asc),
                _ => lista
            };
        }
        private static List<T> Ordenar<T, TKey>(List<T> lista, Func<T, TKey> selector, bool asc)
        {
            return asc
                ? lista.OrderBy(selector).ToList()
                : lista.OrderByDescending(selector).ToList();
        }

        private static List<DropDownListaGenericaModel> AgregarTodos(List<DropDownListaGenericaModel> lista)
        {
            return new List<DropDownListaGenericaModel>
            {
                new() { item = Todos, descripcion = Todos }
            }.Concat(lista ?? new List<DropDownListaGenericaModel>()).ToList();
        }

        private static string NormalizarTodos(string? value)
        {
            value = (value ?? string.Empty).Trim();
            return EsTodos(value) ? string.Empty : value;
        }

        private static bool EsTodos(string? value)
        {
            return string.Equals((value ?? string.Empty).Trim(), Todos, StringComparison.OrdinalIgnoreCase);
        }

        private static bool Contiene(string? value, string texto)
        {
            return (value ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase);
        }
    }
}