
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_RRHH;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_RRHH
{
    public class FrmRHCatConceptosDB
    {
        private const int ModuloRRHH = 23;

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MCntLinkDB _cntLinkDB;

        public FrmRHCatConceptosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _cntLinkDB = new MCntLinkDB(config);
        }

        #region Conceptos

        public ErrorDto<RhCatConceptosInicialData>
            RH_Cat_Conceptos_Inicial_Obtener(int CodEmpresa)
        {
            var resultado = new RhCatConceptosInicialData();

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                resultado.fecha_servidor =
                    connection.QuerySingle<DateTime>(
                        "select dbo.MyGetdate();");

                const string sqlGrupos = """
                    select
                        rtrim(COD_GRUPO) as item,
                        rtrim(DESCRIPCION) as descripcion
                    from RH_CONCEPTOS_GRUPOS
                    where ACTIVO = 1
                    order by COD_GRUPO;
                    """;

                resultado.grupos = connection
                    .Query<DropDownListaGenericaModel>(sqlGrupos)
                    .ToList();

                resultado.tipos_concepto =
                    CrearOpciones(
                        ("I", "Ingresos"),
                        ("E", "Egresos"),
                        ("P", "Provisión"),
                        ("S", "Salario"));

                resultado.tipos_aplicacion =
                    CrearOpciones(
                        ("P", "Porcentual"),
                        ("M", "Monto"),
                        ("T", "Tabla"),
                        ("F", "Formula"));

                resultado.bases_aplicacion =
                    CrearOpciones(
                        ("So", "Salario Ordinario"),
                        ("Scf", "Salario - Crédito Fiscal"),
                        ("Sr", "Salario Resultante"),
                        ("Sm", "Salario Mensual"),
                        ("Cv", "Concepto Vinculado"));

                resultado.resumenes = CrearOpciones(
                    ("NA", "NA"),
                    ("SALARIO_ORDINARIO", "SALARIO_ORDINARIO"),
                    ("SALARIO_DEVENGADO", "SALARIO_DEVENGADO"),
                    ("SALARIO_NETO", "SALARIO_NETO"),
                    ("INGRESOS", "INGRESOS"),
                    ("EGRESOS", "EGRESOS"),
                    ("HRS_ORDINARIAS", "HRS_ORDINARIAS"),
                    ("HRS_EXTRAS", "HRS_EXTRAS"),
                    ("HRS_DOBLES", "HRS_DOBLES"),
                    ("HRS_PERMISOS_CGS", "HRS_PERMISOS_CGS"),
                    ("HRS_PERMISOS_SGS", "HRS_PERMISOS_SGS"),
                    ("HRS_VACACIONES", "HRS_VACACIONES"),
                    ("HRS_INCAPACIDAD", "HRS_INCAPACIDAD"),
                    ("VACACIONES_PERIODO", "VACACIONES_PERIODO"),
                    ("VACACIONES_DISFRUTADAS", "VACACIONES_DISFRUTADAS"),
                    ("VACACIONES_LIQUIDADAS", "VACACIONES_LIQUIDADAS"),
                    ("FERIADOS", "FERIADOS"));

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message, -1, resultado);
            }
        }

        public ErrorDto<RhCatConceptoData>
            RH_Cat_Conceptos_Obtener(
                int CodEmpresa,
                int CodContabilidad,
                RhCatConceptoObtenerRequest? request)
        {
            var codigo = NormalizarTexto(request?.cod_concepto);

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return ErrorConcepto(
                    "Debe indicar el código del concepto.");
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                return ObtenerConceptoDesdeConexion(
                    connection,
                    CodEmpresa,
                    CodContabilidad,
                    codigo);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message, -1, new RhCatConceptoData());
            }
        }

        public ErrorDto<List<RhCatConceptoBusquedaData>>
            RH_Cat_Conceptos_F4_Obtener(
                int CodEmpresa,
                RhCatConceptoBusquedaRequest? request)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var texto = NormalizarTexto(request?.texto);
                var excluir = NormalizarTexto(
                    request?.cod_concepto_excluir);

                const string sql = """
                    select
                        rtrim(COD_CONCEPTO) as cod_concepto,
                        isnull(rtrim(DESCRIPCION), '') as descripcion
                    from RH_CONCEPTOS
                    where
                        (@texto = ''
                         or COD_CONCEPTO like @like
                         or DESCRIPCION like @like)
                        and (@excluir = ''
                             or COD_CONCEPTO <> @excluir)
                    order by COD_CONCEPTO;
                    """;

                var lista = connection
                    .Query<RhCatConceptoBusquedaData>(
                        sql,
                        new
                        {
                            texto,
                            like = $"%{texto}%",
                            excluir
                        })
                    .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<RhCatConceptoBusquedaData>());
            }
        }

        public ErrorDto<RhCatConceptoData>
            RH_Cat_Conceptos_Scroll_Obtener(
                int CodEmpresa,
                int CodContabilidad,
                RhCatConceptoScrollRequest? request)
        {
            if (request?.direccion is not (0 or 1))
            {
                return ErrorConcepto(
                    "Debe indicar una dirección válida.");
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var codigo = ObtenerCodigoScroll(
                    connection,
                    NormalizarTexto(request.cod_concepto),
                    request.direccion.Value);

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    return ErrorConcepto(
                        "No hay más conceptos para consultar.");
                }

                return ObtenerConceptoDesdeConexion(
                    connection,
                    CodEmpresa,
                    CodContabilidad,
                    codigo);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message, -1, new RhCatConceptoData());
            }
        }

        public ErrorDto<RhCatConceptoCodigoNuevoData>
            RH_Cat_Conceptos_Codigo_Nuevo(int CodEmpresa)
        {
            var resultado = new RhCatConceptoCodigoNuevoData();

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = """
                    select isnull(count(*), 0) + 1
                    from RH_CONCEPTOS;
                    """;

                var secuencia = connection.QuerySingle<int>(sql);

                resultado.cod_concepto =
                    secuencia.ToString("D4");

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message, -1, resultado);
            }
        }
        public ErrorDto<List<RhCatConceptoEntidadBusquedaData>>
            RH_Cat_Conceptos_Entidades_F4_Obtener(
                int CodEmpresa,
                RhCatConceptoEntidadBusquedaRequest? request)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var texto = NormalizarTexto(request?.texto);

                const string sql = """
            select
                rtrim(COD_ER) as cod_er,
                isnull(rtrim(NOMBRE), '') as nombre,
                isnull(rtrim(NOMBRE_CORTO), '') as nombre_corto
            from RH_ENTIDADES_RELACIONADAS
            where
                @texto = ''
                or COD_ER like @like
                or NOMBRE like @like
                or NOMBRE_CORTO like @like
            order by COD_ER;
            """;

                var lista = connection
                    .Query<RhCatConceptoEntidadBusquedaData>(
                        sql,
                        new
                        {
                            texto,
                            like = $"%{texto}%"
                        })
                    .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<RhCatConceptoEntidadBusquedaData>());
            }
        }



        public ErrorDto<RhCatConceptoPrioridadNuevaData>
            RH_Cat_Conceptos_Prioridad_Nueva(
                int CodEmpresa,
                RhCatConceptoPrioridadNuevaRequest? request)
        {
            var resultado = new RhCatConceptoPrioridadNuevaData();

            var tipo = NormalizarTexto(
                request?.tipo_concepto).ToUpperInvariant();

            if (!EsTipoConceptoValido(tipo))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar un tipo de concepto válido.",
                    -2,
                    resultado);
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = """
                    select isnull(count(*), 0) + 1
                    from RH_CONCEPTOS
                    where PRIORIDAD like @prefijo;
                    """;

                var secuencia = connection.QuerySingle<int>(
                    sql,
                    new { prefijo = $"{tipo}-%" });

                resultado.prioridad =
                    $"{tipo}-{secuencia:D4}";

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message, -1, resultado);
            }
        }

        public ErrorDto RH_Cat_Conceptos_Guardar(
            int CodEmpresa,
            int CodContabilidad,
            string? usuario,
            RhCatConceptoGuardarRequest? request)
        {
            var usuarioNormalizado =
                NormalizarUsuario(usuario);

            var validacion = ValidarGuardar(
                usuarioNormalizado,
                request?.concepto);

            if (validacion != null)
            {
                return validacion;
            }

            var concepto = request!.concepto;
            NormalizarConcepto(concepto);

            try
            {
                var errorCuentas = PrepararCuentas(
                    CodEmpresa,
                    CodContabilidad,
                    concepto);

                if (errorCuentas != null)
                {
                    return errorCuentas;
                }

                bool existe;

                using (var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa))
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    existe = ExisteConcepto(
                        connection,
                        concepto.cod_concepto,
                        transaction);

                    var parametros = CrearParametrosConcepto(
                        concepto,
                        usuarioNormalizado);

                    var filas = existe
                        ? ActualizarConcepto(
                            connection, transaction, parametros)
                        : InsertarConcepto(
                            connection, transaction, parametros);

                    if (filas <= 0)
                    {
                        transaction.Rollback();

                        return DbHelper.ErrorResponse(
                            "No fue posible guardar el concepto.",
                            -2);
                    }

                    transaction.Commit();
                }

                var bitacora = RegistrarBitacoraConcepto(
                    CodEmpresa,
                    usuarioNormalizado,
                    concepto.cod_concepto,
                    existe);

                if (bitacora.Code < 0)
                {
                    return DbHelper.ErrorResponse(
                        "El concepto se guardó, pero no fue posible "
                        + "registrar la bitácora: "
                        + bitacora.Description);
                }

                return DbHelper.OkResponse(
                    "Información guardada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Tablas

        public ErrorDto<RhCatConceptoTablasLista>
            RH_Cat_Conceptos_Tabla_Lista_Obtener(
                int CodEmpresa,
                RhCatConceptoTablaListaRequest? request)
        {
            var resultado = new RhCatConceptoTablasLista();
            var codigo = NormalizarTexto(request?.cod_concepto);

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el código del concepto.",
                    -2,
                    resultado);
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = """
            select
                TABLA_ID as tabla_id,
                rtrim(COD_CONCEPTO) as cod_concepto,
                MNT_INICIO as mnt_inicio,
                MNT_CORTE as mnt_corte,
                PORCENTAJE as porcentaje,
                ACTIVO as activo,
                REGISTRO_USUARIO as registro_usuario,
                REGISTRO_FECHA as registro_fecha
            from RH_CONCEPTOS_TABLA
            where COD_CONCEPTO = @codigo
            order by TABLA_ID;
            """;

                resultado.lista = connection
                    .Query<RhCatConceptoTablaData>(
                        sql,
                        new { codigo })
                    .ToList();

                resultado.total = resultado.lista.Count;

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message, -1, resultado);
            }
        }

        public ErrorDto RH_Cat_Conceptos_Tabla_Guardar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoTablaGuardarRequest? request)
        {
            var usuarioNormalizado = NormalizarUsuario(usuario);
            var tabla = request?.tabla;

            var validacion = ValidarTablaGuardar(
                usuarioNormalizado,
                tabla);

            if (validacion != null)
            {
                return validacion;
            }

            var codigo = NormalizarTexto(tabla!.cod_concepto);

            try
            {
                long tablaId;
                bool modifica;

                using (var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa))
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction(
                            IsolationLevel.Serializable);

                    if (!ExisteConcepto(
                        connection,
                        codigo,
                        transaction))
                    {
                        return DbHelper.ErrorResponse(
                            "No existe el concepto indicado.", -2);
                    }

                    modifica = tabla.tabla_id.GetValueOrDefault() > 0;

                    if (modifica)
                    {
                        tablaId = tabla.tabla_id!.Value;

                        var filas = ActualizarTablaConcepto(
                            connection,
                            transaction,
                            tabla,
                            codigo);

                        if (filas == 0)
                        {
                            return DbHelper.ErrorResponse(
                                "No se encontró el registro de tabla "
                                + "que desea modificar.", -2);
                        }
                    }
                    else
                    {
                        tablaId = ObtenerNuevoTablaId(
                            connection,
                            transaction);

                        InsertarTablaConcepto(
                            connection,
                            transaction,
                            tabla,
                            codigo,
                            tablaId,
                            usuarioNormalizado);
                    }

                    transaction.Commit();
                }

                var bitacora = RegistrarBitacoraTabla(
                    CodEmpresa,
                    usuarioNormalizado,
                    tablaId,
                    modifica ? "Modifica" : "Registra");

                if (bitacora.Code < 0)
                {
                    return DbHelper.ErrorResponse(
                        "La tabla se guardó, pero no fue posible "
                        + "registrar la bitácora: "
                        + bitacora.Description);
                }

                return DbHelper.OkResponse(
                    "Información guardada satisfactoriamente. "
                    + $"Tabla Id: {tablaId}.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        public ErrorDto RH_Cat_Conceptos_Tabla_Eliminar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoTablaEliminarRequest? request)
        {
            var usuarioNormalizado = NormalizarUsuario(usuario);
            var codigo = NormalizarTexto(request?.cod_concepto);
            var tablaId = request?.tabla_id;

            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el usuario de sesión.", -2);
            }

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el código del concepto.", -2);
            }

            if (tablaId is null or <= 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar un identificador de tabla válido.",
                    -2);
            }

            try
            {
                using (var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa))
                {
                    const string sql = """
                delete from RH_CONCEPTOS_TABLA
                where COD_CONCEPTO = @codigo
                  and TABLA_ID = @tablaId;
                """;

                    var filas = connection.Execute(
                        sql,
                        new { codigo, tablaId });

                    if (filas == 0)
                    {
                        return DbHelper.ErrorResponse(
                            "No se encontró el registro de tabla "
                            + "que desea eliminar.", -2);
                    }
                }

                var bitacora = RegistrarBitacoraTabla(
                    CodEmpresa,
                    usuarioNormalizado,
                    tablaId.Value,
                    "Elimina");

                if (bitacora.Code < 0)
                {
                    return DbHelper.ErrorResponse(
                        "El registro se eliminó, pero no fue posible "
                        + "registrar la bitácora: "
                        + bitacora.Description);
                }

                return DbHelper.OkResponse(
                    "Registro eliminado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Detalle

        public ErrorDto<RhCatConceptoDetallesLista>
            RH_Cat_Conceptos_Detalle_Lista_Obtener(
                int CodEmpresa,
                RhCatConceptoDetalleListaRequest? request)
        {
            var resultado = new RhCatConceptoDetallesLista();
            var codigo = NormalizarTexto(request?.cod_concepto);

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el código del concepto.",
                    -2,
                    resultado);
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = """
            select
                rtrim(COD_CONCEPTO) as cod_concepto,
                rtrim(COD_DETALLE) as cod_detalle,
                rtrim(DESCRIPCION) as descripcion,
                BASE_MINIMA as base_minima,
                VALOR as valor,
                REGISTRO_USUARIO as registro_usuario,
                REGISTRO_FECHA as registro_fecha
            from RH_CONCEPTOS_DETALLE
            where COD_CONCEPTO = @codigo
            order by COD_DETALLE;
            """;

                resultado.lista = connection
                    .Query<RhCatConceptoDetalleData>(
                        sql,
                        new { codigo })
                    .ToList();

                resultado.total = resultado.lista.Count;

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message, -1, resultado);
            }
        }

        public ErrorDto RH_Cat_Conceptos_Detalle_Guardar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoDetalleGuardarRequest? request)
        {
            var usuarioNormalizado = NormalizarUsuario(usuario);
            var detalle = request?.detalle;

            var validacion = ValidarDetalleGuardar(
                usuarioNormalizado,
                detalle);

            if (validacion != null)
            {
                return validacion;
            }

            NormalizarDetalle(detalle!);

            try
            {
                bool modifica;

                using (var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa))
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    if (!ExisteConcepto(
                        connection,
                        detalle!.cod_concepto,
                        transaction))
                    {
                        return DbHelper.ErrorResponse(
                            "No existe el concepto indicado.", -2);
                    }

                    modifica = ExisteDetalleConcepto(
                        connection,
                        transaction,
                        detalle.cod_concepto,
                        detalle.cod_detalle);

                    var filas = modifica
                        ? ActualizarDetalleConcepto(
                            connection,
                            transaction,
                            detalle)
                        : InsertarDetalleConcepto(
                            connection,
                            transaction,
                            detalle,
                            usuarioNormalizado);

                    if (filas <= 0)
                    {
                        return DbHelper.ErrorResponse(
                            "No fue posible guardar el detalle.", -2);
                    }

                    transaction.Commit();
                }

                var bitacora = RegistrarBitacoraDetalle(
                    CodEmpresa,
                    usuarioNormalizado,
                    detalle!.cod_concepto,
                    detalle.cod_detalle,
                    modifica ? "Modifica" : "Registra");

                if (bitacora.Code < 0)
                {
                    return DbHelper.ErrorResponse(
                        "El detalle se guardó, pero no fue posible "
                        + "registrar la bitácora: "
                        + bitacora.Description);
                }

                return DbHelper.OkResponse(
                    "Detalle guardado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        public ErrorDto RH_Cat_Conceptos_Detalle_Eliminar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoDetalleEliminarRequest? request)
        {
            var usuarioNormalizado = NormalizarUsuario(usuario);
            var codigo = NormalizarTexto(request?.cod_concepto);
            var codigoDetalle = NormalizarTexto(request?.cod_detalle);

            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el usuario de sesión.", -2);
            }

            if (string.IsNullOrWhiteSpace(codigo) ||
                string.IsNullOrWhiteSpace(codigoDetalle))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el concepto y el código del detalle.",
                    -2);
            }

            try
            {
                using (var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa))
                {
                    const string sql = """
                delete from RH_CONCEPTOS_DETALLE
                where COD_CONCEPTO = @codigo
                  and COD_DETALLE = @codigoDetalle;
                """;

                    var filas = connection.Execute(
                        sql,
                        new { codigo, codigoDetalle });

                    if (filas == 0)
                    {
                        return DbHelper.ErrorResponse(
                            "No se encontró el detalle que desea eliminar.",
                            -2);
                    }
                }

                var bitacora = RegistrarBitacoraDetalle(
                    CodEmpresa,
                    usuarioNormalizado,
                    codigo,
                    codigoDetalle,
                    "Elimina");

                if (bitacora.Code < 0)
                {
                    return DbHelper.ErrorResponse(
                        "El detalle se eliminó, pero no fue posible "
                        + "registrar la bitácora: "
                        + bitacora.Description);
                }

                return DbHelper.OkResponse(
                    "Detalle eliminado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Exclusiones

        public ErrorDto<RhCatConceptoExclusionesLista>
            RH_Cat_Conceptos_Exclusion_Lista_Obtener(
                int CodEmpresa,
                RhCatConceptoExclusionListaRequest? request)
        {
            var resultado = new RhCatConceptoExclusionesLista();
            var codigo = NormalizarTexto(request?.cod_concepto);

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el código del concepto.",
                    -2,
                    resultado);
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = """
            exec spRH_Concepto_Excluye_Lista @codigo;
            """;

                resultado.lista = connection
                    .Query<RhCatConceptoExclusionData>(
                        sql,
                        new { codigo })
                    .ToList();

                resultado.total = resultado.lista.Count;

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message, -1, resultado);
            }
        }

        public ErrorDto RH_Cat_Conceptos_Exclusion_Guardar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoExclusionGuardarRequest? request)
        {
            return EjecutarMovimientoExclusion(
                CodEmpresa,
                usuario,
                request?.cod_concepto,
                request?.cod_concepto_exc,
                "A");
        }

        public ErrorDto RH_Cat_Conceptos_Exclusion_Eliminar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoExclusionEliminarRequest? request)
        {
            return EjecutarMovimientoExclusion(
                CodEmpresa,
                usuario,
                request?.cod_concepto,
                request?.cod_concepto_exc,
                "E");
        }

        #endregion

        #region Actualizar Personas

        public ErrorDto RH_Cat_Conceptos_Codigo_Validar(
            int CodEmpresa,
            RhCatConceptoCodigoValidarRequest? request)
        {
            var codigo = NormalizarTexto(request?.cod_concepto);

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el código del concepto.", -2);
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = """
            select count(1)
            from RH_CONCEPTOS
            where COD_CONCEPTO = @codigo;
            """;

                var existe = connection.QuerySingle<int>(
                    sql,
                    new { codigo }) > 0;

                if (existe)
                {
                    return DbHelper.ErrorResponse(
                        $"El código de concepto {codigo} ya existe.",
                        -2);
                }

                return DbHelper.OkResponse(
                    $"El código de concepto {codigo} está disponible.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        public ErrorDto RH_Cat_Conceptos_Personas_Actualizar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoPersonasActualizarRequest? request)
        {
            var usuarioNormalizado = NormalizarUsuario(usuario);
            var codigo = NormalizarTexto(request?.cod_concepto);

            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el usuario de sesión.", -2);
            }

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el código del concepto.", -2);
            }

            try
            {
                using (var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa))
                {
                    if (!ExisteConcepto(connection, codigo))
                    {
                        return DbHelper.ErrorResponse(
                            "El concepto indicado no existe.", -2);
                    }

                    const string sql = """
                exec spRH_Concepto_Persona_Actualiza
                    @codigo,
                    @usuario;
                """;

                    connection.Execute(
                        sql,
                        new
                        {
                            codigo,
                            usuario = usuarioNormalizado
                        },
                        commandTimeout: 300);
                }

                var bitacora = _securityMainDb.Bitacora(
                    new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuarioNormalizado,
                        Modulo = ModuloRRHH,
                        Movimiento = "Actualiza - WEB",
                        DetalleMovimiento =
                            "Concepto a Personas vinculadas: "
                            + codigo
                    });

                if (bitacora.Code < 0)
                {
                    return DbHelper.ErrorResponse(
                        "El concepto se actualizó en las personas "
                        + "vinculadas, pero no fue posible registrar "
                        + "la bitácora: "
                        + bitacora.Description);
                }

                return DbHelper.OkResponse(
                    "Concepto actualizado a todos los empleados "
                    + "vinculados satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Helpers privados

        private static string NormalizarTexto(string? valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        private static string NormalizarUsuario(string? usuario)
        {
            return NormalizarTexto(usuario).ToUpperInvariant();
        }

        private static bool EsTipoConceptoValido(string tipo)
        {
            return tipo is "I" or "E" or "P" or "S";
        }

        private static List<DropDownListaGenericaModel> CrearOpciones(
            params (string Codigo, string Descripcion)[] opciones)
        {
            return opciones.Select(opcion =>
                new DropDownListaGenericaModel
                {
                    item = opcion.Codigo,
                    descripcion = opcion.Descripcion
                }).ToList();
        }

        private static ErrorDto<RhCatConceptoData>
            ErrorConcepto(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje, -2, new RhCatConceptoData());
        }

        private static ErrorDto<RhCatConceptoData>
            ObtenerConceptoDesdeConexion(
                SqlConnection connection,
                int codEmpresa,
                int codContabilidad,
                string codigo)
        {
            const string sql = """
                select *
                from vRH_CONCEPTOS
                where COD_CONCEPTO = @codigo;
                """;

            var concepto = connection
                .QueryFirstOrDefault<RhCatConceptoData>(
                    sql, new { codigo });

            if (concepto == null)
            {
                return ErrorConcepto(
                    "No se encontró el registro.");
            }

            return DbHelper.CreateOkResponse(concepto);
        }

        private static string ObtenerCodigoScroll(
            SqlConnection connection,
            string codigo,
            int direccion)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return direccion == 1
                    ? ObtenerPrimerCodigo(connection)
                    : string.Empty;
            }

            const string sqlAnterior = """
                select top 1 rtrim(COD_CONCEPTO)
                from RH_CONCEPTOS
                where COD_CONCEPTO < @codigo
                order by COD_CONCEPTO desc;
                """;

            const string sqlSiguiente = """
                select top 1 rtrim(COD_CONCEPTO)
                from RH_CONCEPTOS
                where COD_CONCEPTO > @codigo
                order by COD_CONCEPTO asc;
                """;

            return connection.QueryFirstOrDefault<string>(
                direccion == 0 ? sqlAnterior : sqlSiguiente,
                new { codigo }) ?? string.Empty;
        }

        private static string ObtenerPrimerCodigo(
            SqlConnection connection)
        {
            const string sql = """
                select top 1 rtrim(COD_CONCEPTO)
                from RH_CONCEPTOS
                order by COD_CONCEPTO asc;
                """;

            return connection.QueryFirstOrDefault<string>(sql)
                ?? string.Empty;
        }

        private static bool ExisteConcepto(
            SqlConnection connection,
            string codigo,
            SqlTransaction transaction)
        {
            const string sql = """
                select count(1)
                from RH_CONCEPTOS with (UPDLOCK, HOLDLOCK)
                where COD_CONCEPTO = @codigo;
                """;

            return connection.QuerySingle<int>(
                sql,
                new { codigo },
                transaction) > 0;
        }

        private static ErrorDto? ValidarGuardar(
            string usuario,
            RhCatConceptoData? concepto)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el usuario de sesión.", -2);
            }

            if (concepto == null)
            {
                return DbHelper.ErrorResponse(
                    "No se recibió información del concepto.", -2);
            }

            if (string.IsNullOrWhiteSpace(concepto.cod_concepto))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el código del concepto.", -2);
            }

            if (string.IsNullOrWhiteSpace(concepto.descripcion))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar la descripción del concepto.", -2);
            }

            return null;
        }

        private static void NormalizarConcepto(
            RhCatConceptoData concepto)
        {
            concepto.cod_concepto =
                NormalizarTexto(concepto.cod_concepto);

            concepto.descripcion =
                NormalizarTexto(concepto.descripcion);

            concepto.prioridad =
                NormalizarTexto(concepto.prioridad);

            concepto.tipo_concepto =
                NormalizarTexto(concepto.tipo_concepto)
                    .ToUpperInvariant();

            concepto.aplicacion_tipo =
                NormalizarTexto(concepto.aplicacion_tipo)
                    .ToUpperInvariant();

            concepto.aplicacion_base =
                NormalizarTexto(concepto.aplicacion_base);

            concepto.aplicacion_formula =
                NormalizarTexto(concepto.aplicacion_formula);

            concepto.sumar_en =
                NormalizarTexto(concepto.sumar_en);

            concepto.cod_er =
                TextoONull(concepto.cod_er);

            concepto.cod_concepto_base =
                TextoONull(concepto.cod_concepto_base);

            concepto.cod_grupo =
                TextoONull(concepto.cod_grupo);
        }

        private static string? TextoONull(string? valor)
        {
            var texto = NormalizarTexto(valor);
            return texto.Length == 0 ? null : texto;
        }

        private ErrorDto? PrepararCuentas(
            int codEmpresa,
            int codContabilidad,
            RhCatConceptoData concepto)
        {
            var cuenta = NormalizarCuenta(
                codEmpresa, concepto.cod_cuenta);

            var cuentaCierre = NormalizarCuenta(
                codEmpresa, concepto.cod_cuenta_cierre);

            if (cuenta == null || cuentaCierre == null)
            {
                return DbHelper.ErrorResponse(
                    "Una de las cuentas contables tiene "
                    + "un formato inválido.", -2);
            }

            concepto.cod_cuenta = cuenta;
            concepto.cod_cuenta_cierre = cuentaCierre;

            return null;
        }

        private string? NormalizarCuenta(
            int codEmpresa,
            string? cuenta)
        {
            var texto = NormalizarTexto(cuenta);

            if (texto.Length == 0)
            {
                return string.Empty;
            }

            var resultado = _cntLinkDB.fxgCntCuentaFormato(
                codEmpresa, false, texto, 0);

            return string.IsNullOrWhiteSpace(resultado)
                ? null
                : resultado;
        }

        private static object CrearParametrosConcepto(
            RhCatConceptoData concepto,
            string usuario)
        {
            return new
            {
                cod_concepto = concepto.cod_concepto,
                descripcion = concepto.descripcion,
                prioridad = concepto.prioridad,
                activo = concepto.activo,
                tipo_concepto = concepto.tipo_concepto,
                patrono_control = concepto.patrono_control,
                colilla_muestra = concepto.colilla_muestra,
                revisa_salario_imbargable =
                    concepto.revisa_salario_imbargable,
                registro_contable = concepto.registro_contable,
                aplicacion_general = concepto.aplicacion_general,
                aplicacion_permite_cambios =
                    concepto.aplicacion_permite_cambios,
                aplicacion_credito_fiscal =
                    concepto.aplicacion_credito_fiscal,
                aplicacion_tipo = concepto.aplicacion_tipo,
                aplicacion_base = concepto.aplicacion_base,
                aplicacion_valor = concepto.aplicacion_valor,
                aplicacion_formula = concepto.aplicacion_formula,
                aplicacion_inicio = concepto.aplicacion_inicio,
                aplicacion_corte = concepto.aplicacion_corte,
                sumar_en = concepto.sumar_en,
                cod_cuenta = concepto.cod_cuenta,
                cod_cuenta_cierre = concepto.cod_cuenta_cierre,
                cod_er = concepto.cod_er,
                cod_concepto_base = concepto.cod_concepto_base,
                cod_grupo = concepto.cod_grupo,
                usuario
            };
        }

        private static int InsertarConcepto(
            SqlConnection connection,
            SqlTransaction transaction,
            object parametros)
        {
            const string sql = """
                insert into RH_CONCEPTOS
                (
                    COD_CONCEPTO, DESCRIPCION, PRIORIDAD,
                    ACTIVO, TIPO_CONCEPTO, PATRONO_CONTROL,
                    COLILLA_MUESTRA, REVISA_SALARIO_IMBARGABLE,
                    REGISTRO_CONTABLE, APLICACION_GENERAL,
                    APLICACION_PERMITE_CAMBIOS,
                    APLICACION_CREDITO_FISCAL,
                    APLICACION_TIPO, APLICACION_BASE,
                    APLICACION_VALOR, APLICACION_FORMULA,
                    APLICACION_INICIO, APLICACION_CORTE,
                    SUMAR_EN, COD_CUENTA, COD_CUENTA_CIERRE,
                    COD_ER, COD_CONCEPTO_BASE, COD_GRUPO,
                    REGISTRO_USUARIO, REGISTRO_FECHA
                )
                values
                (
                    @cod_concepto, @descripcion, @prioridad,
                    @activo, @tipo_concepto, @patrono_control,
                    @colilla_muestra, @revisa_salario_imbargable,
                    @registro_contable, @aplicacion_general,
                    @aplicacion_permite_cambios,
                    @aplicacion_credito_fiscal,
                    @aplicacion_tipo, @aplicacion_base,
                    @aplicacion_valor, @aplicacion_formula,
                    @aplicacion_inicio, @aplicacion_corte,
                    @sumar_en, @cod_cuenta, @cod_cuenta_cierre,
                    @cod_er, @cod_concepto_base, @cod_grupo,
                    @usuario, dbo.MyGetdate()
                );
                """;

            return connection.Execute(
                sql, parametros, transaction);
        }

        private static int ActualizarConcepto(
            SqlConnection connection,
            SqlTransaction transaction,
            object parametros)
        {
            const string sql = """
                update RH_CONCEPTOS
                set
                    DESCRIPCION = @descripcion,
                    PRIORIDAD = @prioridad,
                    ACTIVO = @activo,
                    TIPO_CONCEPTO = @tipo_concepto,
                    PATRONO_CONTROL = @patrono_control,
                    COLILLA_MUESTRA = @colilla_muestra,
                    REVISA_SALARIO_IMBARGABLE =
                        @revisa_salario_imbargable,
                    REGISTRO_CONTABLE = @registro_contable,
                    APLICACION_GENERAL = @aplicacion_general,
                    APLICACION_PERMITE_CAMBIOS =
                        @aplicacion_permite_cambios,
                    APLICACION_CREDITO_FISCAL =
                        @aplicacion_credito_fiscal,
                    APLICACION_TIPO = @aplicacion_tipo,
                    APLICACION_BASE = @aplicacion_base,
                    APLICACION_VALOR = @aplicacion_valor,
                    APLICACION_FORMULA = @aplicacion_formula,
                    APLICACION_INICIO = @aplicacion_inicio,
                    APLICACION_CORTE = @aplicacion_corte,
                    SUMAR_EN = @sumar_en,
                    COD_CUENTA = @cod_cuenta,
                    COD_CUENTA_CIERRE = @cod_cuenta_cierre,
                    COD_ER = @cod_er,
                    COD_CONCEPTO_BASE = @cod_concepto_base,
                    COD_GRUPO = @cod_grupo
                where COD_CONCEPTO = @cod_concepto;
                """;

            return connection.Execute(
                sql, parametros, transaction);
        }

        private ErrorDto RegistrarBitacoraConcepto(
            int codEmpresa,
            string usuario,
            string codigo,
            bool modifica)
        {
            return _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    Modulo = ModuloRRHH,
                    Movimiento = modifica
                        ? "Modifica - WEB"
                        : "Registra - WEB",
                    DetalleMovimiento =
                        $"Concepto de Planilla: {codigo}"
                });
        }

        private static ErrorDto? ValidarTablaGuardar(
            string usuario,
            RhCatConceptoTablaData? tabla)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el usuario de sesión.", -2);
            }

            if (tabla == null)
            {
                return DbHelper.ErrorResponse(
                    "No se recibió la información de la tabla.", -2);
            }

            if (string.IsNullOrWhiteSpace(tabla.cod_concepto))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el código del concepto.", -2);
            }

            if (tabla.tabla_id < 0)
            {
                return DbHelper.ErrorResponse(
                    "El identificador de tabla no es válido.", -2);
            }

            if (!tabla.mnt_inicio.HasValue ||
                !tabla.mnt_corte.HasValue ||
                !tabla.porcentaje.HasValue)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar los montos de inicio, corte "
                    + "y el porcentaje.", -2);
            }

            if (tabla.mnt_inicio.Value > tabla.mnt_corte.Value)
            {
                return DbHelper.ErrorResponse(
                    "El monto inicial no puede ser mayor "
                    + "que el monto de corte.", -2);
            }

            return null;
        }

        private static long ObtenerNuevoTablaId(
            SqlConnection connection,
            SqlTransaction transaction)
        {
            const string sql = """
        select isnull(max(TABLA_ID), 0) + 1
        from RH_CONCEPTOS_TABLA
            with (TABLOCKX, HOLDLOCK);
        """;

            return connection.QuerySingle<long>(
                sql,
                transaction: transaction);
        }

        private static int InsertarTablaConcepto(
            SqlConnection connection,
            SqlTransaction transaction,
            RhCatConceptoTablaData tabla,
            string codigo,
            long tablaId,
            string usuario)
        {
            const string sql = """
        insert into RH_CONCEPTOS_TABLA
        (
            COD_CONCEPTO,
            TABLA_ID,
            MNT_INICIO,
            MNT_CORTE,
            PORCENTAJE,
            ACTIVO,
            REGISTRO_USUARIO,
            REGISTRO_FECHA
        )
        values
        (
            @codigo,
            @tablaId,
            @mntInicio,
            @mntCorte,
            @porcentaje,
            @activo,
            @usuario,
            dbo.MyGetdate()
        );
        """;

            return connection.Execute(
                sql,
                new
                {
                    codigo,
                    tablaId,
                    mntInicio = tabla.mnt_inicio,
                    mntCorte = tabla.mnt_corte,
                    porcentaje = tabla.porcentaje,
                    activo = tabla.activo ?? true,
                    usuario
                },
                transaction);
        }

        private static int ActualizarTablaConcepto(
            SqlConnection connection,
            SqlTransaction transaction,
            RhCatConceptoTablaData tabla,
            string codigo)
        {
            const string sql = """
        update RH_CONCEPTOS_TABLA
        set
            MNT_INICIO = @mntInicio,
            MNT_CORTE = @mntCorte,
            PORCENTAJE = @porcentaje,
            ACTIVO = @activo
        where COD_CONCEPTO = @codigo
          and TABLA_ID = @tablaId;
        """;

            return connection.Execute(
                sql,
                new
                {
                    codigo,
                    tablaId = tabla.tabla_id,
                    mntInicio = tabla.mnt_inicio,
                    mntCorte = tabla.mnt_corte,
                    porcentaje = tabla.porcentaje,
                    activo = tabla.activo ?? true
                },
                transaction);
        }

        private ErrorDto RegistrarBitacoraTabla(
            int codEmpresa,
            string usuario,
            long tablaId,
            string movimiento)
        {
            return _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    Modulo = ModuloRRHH,
                    Movimiento = $"{movimiento} - WEB",
                    DetalleMovimiento =
                        $"Concepto de Planilla, Tabla Id: {tablaId}"
                });
        }

        private static ErrorDto? ValidarDetalleGuardar(
            string usuario,
            RhCatConceptoDetalleData? detalle)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el usuario de sesión.", -2);
            }

            if (detalle == null)
            {
                return DbHelper.ErrorResponse(
                    "No se recibió la información del detalle.", -2);
            }

            if (string.IsNullOrWhiteSpace(detalle.cod_concepto))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el código del concepto.", -2);
            }

            if (string.IsNullOrWhiteSpace(detalle.cod_detalle))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el código del detalle.", -2);
            }

            if (string.IsNullOrWhiteSpace(detalle.descripcion))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar la descripción del detalle.", -2);
            }

            if (!detalle.base_minima.HasValue ||
                !detalle.valor.HasValue)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar la base mínima y el valor.", -2);
            }

            return null;
        }

        private static void NormalizarDetalle(
            RhCatConceptoDetalleData detalle)
        {
            detalle.cod_concepto =
                NormalizarTexto(detalle.cod_concepto);

            detalle.cod_detalle =
                NormalizarTexto(detalle.cod_detalle);

            detalle.descripcion =
                NormalizarTexto(detalle.descripcion);
        }

        private static bool ExisteDetalleConcepto(
            SqlConnection connection,
            SqlTransaction transaction,
            string codigo,
            string codigoDetalle)
        {
            const string sql = """
        select count(1)
        from RH_CONCEPTOS_DETALLE with (UPDLOCK, HOLDLOCK)
        where COD_CONCEPTO = @codigo
          and COD_DETALLE = @codigoDetalle;
        """;

            return connection.QuerySingle<int>(
                sql,
                new { codigo, codigoDetalle },
                transaction) > 0;
        }

        private static int InsertarDetalleConcepto(
            SqlConnection connection,
            SqlTransaction transaction,
            RhCatConceptoDetalleData detalle,
            string usuario)
        {
            const string sql = """
        insert into RH_CONCEPTOS_DETALLE
        (
            COD_CONCEPTO,
            COD_DETALLE,
            DESCRIPCION,
            BASE_MINIMA,
            VALOR,
            REGISTRO_USUARIO,
            REGISTRO_FECHA
        )
        values
        (
            @cod_concepto,
            @cod_detalle,
            @descripcion,
            @base_minima,
            @valor,
            @usuario,
            dbo.MyGetdate()
        );
        """;

            return connection.Execute(
                sql,
                new
                {
                    detalle.cod_concepto,
                    detalle.cod_detalle,
                    detalle.descripcion,
                    detalle.base_minima,
                    detalle.valor,
                    usuario
                },
                transaction);
        }

        private static int ActualizarDetalleConcepto(
            SqlConnection connection,
            SqlTransaction transaction,
            RhCatConceptoDetalleData detalle)
        {
            const string sql = """
        update RH_CONCEPTOS_DETALLE
        set
            DESCRIPCION = @descripcion,
            BASE_MINIMA = @base_minima,
            VALOR = @valor
        where COD_CONCEPTO = @cod_concepto
          and COD_DETALLE = @cod_detalle;
        """;

            return connection.Execute(
                sql,
                new
                {
                    detalle.cod_concepto,
                    detalle.cod_detalle,
                    detalle.descripcion,
                    detalle.base_minima,
                    detalle.valor
                },
                transaction);
        }

        private ErrorDto RegistrarBitacoraDetalle(
            int codEmpresa,
            string usuario,
            string codigo,
            string codigoDetalle,
            string movimiento)
        {
            return _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    Modulo = ModuloRRHH,
                    Movimiento = $"{movimiento} - WEB",
                    DetalleMovimiento =
                        $"Detalle Concepto: {codigo}, "
                        + $"ID Detalle: {codigoDetalle}"
                });
        }


        private ErrorDto EjecutarMovimientoExclusion(
            int codEmpresa,
            string? usuario,
            string? codConcepto,
            string? codConceptoExc,
            string movimiento)
        {
            var usuarioNormalizado = NormalizarUsuario(usuario);
            var codigo = NormalizarTexto(codConcepto);
            var codigoExcluido = NormalizarTexto(codConceptoExc);

            var validacion = ValidarMovimientoExclusion(
                usuarioNormalizado,
                codigo,
                codigoExcluido,
                movimiento);

            if (validacion != null)
            {
                return validacion;
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDB, codEmpresa);

                connection.Execute(
                    "spRH_Concepto_Excluye_Registro",
                    new
                    {
                        Concepto = codigo,
                        CExcluye = codigoExcluido,
                        Usuario = usuarioNormalizado,
                        Mov = movimiento
                    },
                    commandType: CommandType.StoredProcedure);

                return DbHelper.OkResponse(
                    movimiento == "A"
                        ? "Exclusión guardada satisfactoriamente."
                        : "Exclusión eliminada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }


        private static ErrorDto? ValidarMovimientoExclusion(
            string usuario,
            string codigo,
            string codigoExcluido,
            string movimiento)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el usuario de sesión.", -2);
            }

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el código del concepto.", -2);
            }

            if (string.IsNullOrWhiteSpace(codigoExcluido))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el concepto que desea excluir.", -2);
            }

            if (movimiento is not ("A" or "E"))
            {
                return DbHelper.ErrorResponse(
                    "El movimiento de exclusión no es válido.", -2);
            }

            if (codigo.Equals(
                codigoExcluido,
                StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.ErrorResponse(
                    "Un concepto no puede excluirse a sí mismo.", -2);
            }

            return null;
        }

        private static bool ExisteConcepto(
            SqlConnection connection,
            string codigo)
        {
            const string sql = """
        SELECT COUNT(1)
        FROM RH_CONCEPTOS
        WHERE COD_CONCEPTO = @codigo;
        """;

            return connection.QuerySingle<int>(
                sql,
                new { codigo }) > 0;
        }

        #endregion
    }
}
