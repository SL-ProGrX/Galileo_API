using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFRemesasLiquidacionesDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;
        private const string FormNameRemesasLiquidaciones = "frmAF_RemesasLiquidaciones";
        private const string MsgSinPermisos = "No tiene los permisos para realizar esta opción, verifique...!!!";
        private readonly MSecurityMainDb _Security_MainDB;

        private const string EstadoCerrada = "C";
        private const string OficinaTodas = "T";

        private const string SqlRemesasTotal = @"
                    SELECT COUNT(*)
                    FROM dbo.AFI_REMESAS_LIQ
                    WHERE @hasFilter = 0 OR
                          COD_REMESA LIKE @filtro OR
                          USUARIO LIKE @filtro OR
                          NOTAS LIKE @filtro OR
                          CONVERT(VARCHAR(30), FECHA, 120) LIKE @filtro;";

        private const string SqlRemesasLista = @"
                    SELECT *
                    FROM dbo.AFI_REMESAS_LIQ
                    WHERE @hasFilter = 0 OR
                          COD_REMESA LIKE @filtro OR
                          USUARIO LIKE @filtro OR
                          NOTAS LIKE @filtro OR
                          CONVERT(VARCHAR(30), FECHA, 120) LIKE @filtro
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN COD_REMESA END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN COD_REMESA END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN FECHA END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN FECHA END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN USUARIO END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN USUARIO END DESC,
                        COD_REMESA DESC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlRemesaPorCodigo = @"
                    SELECT *
                    FROM dbo.AFI_REMESAS_LIQ
                    WHERE Cod_Remesa = @CodRemesa;";

        private const string SqlRemesaExiste = @"
                    SELECT COUNT(*)
                    FROM dbo.AFI_REMESAS_LIQ
                    WHERE COD_REMESA = @CodRemesa;";

        private const string SqlRemesaNuevoCodigo = @"
                    SELECT ISNULL(MAX(cod_remesa), 0) + 1 AS Ultimo
                    FROM dbo.AFI_REMESAS_LIQ;";

        private const string SqlRemesaInsert = @"
                    INSERT INTO dbo.AFI_REMESAS_LIQ
                    (
                        cod_remesa,
                        usuario,
                        fecha,
                        estado,
                        fecha_inicio,
                        fecha_corte,
                        notas
                    )
                    VALUES
                    (
                        @CodRemesa,
                        @Usuario,
                        @Fecha,
                        'A',
                        @FechaInicio,
                        @FechaCorte,
                        @Notas
                    );";

        private const string SqlRemesaUpdate = @"
                    UPDATE dbo.AFI_REMESAS_LIQ
                    SET USUARIO = @Usuario,
                        FECHA_INICIO = @FechaInicio,
                        FECHA_CORTE = @FechaCorte,
                        NOTAS = @Notas
                    WHERE COD_REMESA = @CodRemesa;";

        private const string SqlRemesaDelete = @"
                    DELETE FROM dbo.AFI_REMESAS_LIQ
                    WHERE COD_REMESA = @CodRemesa;";

        private const string SqlRemesasActivas = @"
                    SELECT *
                    FROM dbo.AFI_REMESAS_LIQ
                    WHERE estado = 'A'
                    ORDER BY fecha DESC;";

        private const string SqlRemesaFechas = @"
                    SELECT fecha_inicio,
                           fecha_corte
                    FROM dbo.AFI_REMESAS_LIQ
                    WHERE cod_remesa = @CodRemesa;";

        private const string SqlOficinasRemesa = @"
                    SELECT RTRIM(cod_oficina) AS item,
                           RTRIM(cod_oficina) + ' - ' + RTRIM(descripcion) AS descripcion
                    FROM dbo.SIF_Oficinas
                    WHERE cod_oficina IN
                    (
                        SELECT cod_oficina
                        FROM dbo.Liquidacion
                        WHERE Fecliq BETWEEN @FechaInicio AND @FechaCorte
                          AND cod_remesa IS NULL
                    )
                    ORDER BY cod_oficina;";

        private const string SqlCargaListaTodas = @"
                    SELECT L.Consec,
                           L.Cedula,
                           S.nombre,
                           L.FecLiq
                    FROM dbo.Liquidacion L
                    INNER JOIN dbo.Socios S
                        ON L.cedula = S.cedula
                    WHERE L.FecLiq BETWEEN @FechaInicio AND @FechaCorte
                      AND L.cod_remesa IS NULL
                      AND dbo.fxSIFTagCierre(L.CEDULA, L.CONSEC, 'LIQ') = 1
                    ORDER BY L.consec;";

        private const string SqlCargaListaOficina = @"
                    SELECT L.Consec,
                           L.Cedula,
                           S.nombre,
                           L.FecLiq
                    FROM dbo.Liquidacion L
                    INNER JOIN dbo.Socios S
                        ON L.cedula = S.cedula
                    WHERE L.FecLiq BETWEEN @FechaInicio AND @FechaCorte
                      AND L.cod_remesa IS NULL
                      AND dbo.fxSIFTagCierre(L.CEDULA, L.CONSEC, 'LIQ') = 1
                      AND L.cod_Oficina = @Oficina
                    ORDER BY L.consec;";

        private const string SqlRemesaAbiertaExiste = @"
                    SELECT COUNT(*) AS Existe
                    FROM dbo.AFI_REMESAS_LIQ
                    WHERE cod_remesa = @CodRemesa
                      AND estado = 'A';";

        private const string SqlLiquidacionAsignarRemesa = @"
                    UPDATE dbo.Liquidacion
                    SET cod_remesa = @CodRemesa
                    WHERE consec = @Consec;";

        private const string SqlRemesaCerrar = @"
                    UPDATE dbo.AFI_REMESAS_LIQ
                    SET estado = 'C'
                    WHERE cod_remesa = @CodRemesa;";

        private const string SqlReporteRemesas = @"
                    SELECT TOP (@Top) *
                    FROM dbo.AFI_REMESAS_LIQ
                    WHERE fecha BETWEEN @FechaInicio AND @FechaCorte
                    ORDER BY fecha DESC;";

        private const string SqlRemesaMicrofilm = @"
                    UPDATE dbo.AFI_REMESAS_LIQ
                    SET Microfilm_Fecha = dbo.MyGetdate(),
                        Microfilm_usuario = @Usuario
                    WHERE cod_remesa = @CodRemesa;";

        private const string SqlConsultaLiquidacionRemesas = @"
                    SELECT 'Remesa' + CHAR(9) + '...: ' + CAST(A.cod_remesa AS VARCHAR(20))
                           + CHAR(13) + CHAR(10)
                           + 'Fecha' + CHAR(9) + '...: ' + CONVERT(VARCHAR(10), A.fecha, 103)
                           + CHAR(13) + CHAR(10)
                           + 'Usuario' + CHAR(9) + '...: ' + RTRIM(A.usuario) AS descripcion
                    FROM dbo.AFI_REMESAS_LIQ A
                    INNER JOIN dbo.Liquidacion X
                        ON A.cod_remesa = X.cod_remesa
                    WHERE X.consec = @Consec;";


        private static readonly IReadOnlyDictionary<string, int> RemesasSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["COD_REMESA"] = 1,
            ["FECHA"] = 2,
            ["USUARIO"] = 3
        };

        public FrmAFRemesasLiquidacionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        #region Remesas
        /// <summary>
        /// Método para obtener las remesas de liquidaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<AfRemesasLiquidacionesLista> AF_RemesasLiquidaciones_Remesa_Obtener(int CodEmpresa, FiltrosLazyLoadData filtro)
        {
            var parametros = CrearParametrosRemesas(filtro);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new AfRemesasLiquidacionesLista
            {
                total = connection.ExecuteScalar<int>(SqlRemesasTotal, parametros),
                lista = connection.Query<AfRemesaLiquidacionDto>(SqlRemesasLista, parametros).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaRemesasVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener remesas de liquidaciones.",
                    result.Code.GetValueOrDefault(-1),
                    CrearListaRemesasVacia());
        }

        /// <summary>
        /// Método para obtener una remesa de liquidacion por su codigo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto<AfRemesaLiquidacionDto?> AF_RemesasLiquidaciones_Remesa_Obtener(int CodEmpresa, int remesa)
        {
            return DbHelper.ExecuteSingleQuery<AfRemesaLiquidacionDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRemesaPorCodigo,
                null,
                new { CodRemesa = remesa });
        }

        /// <summary>
        /// Método para guardar una remesa de liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto AF_RemesasLiquidaciones_Remesa_Guardar(int CodEmpresa, AfRemesaLiquidacionDto remesa)
        {
            if (remesa is null)
            {
                return DbHelper.ErrorResponse("Los datos de la remesa son requeridos.", -2);
            }

            if (EsRemesaCerrada(remesa.estado))
            {
                return DbHelper.ErrorResponse("No se puede modificar la remesa, porque esta ya fue cerrada...", -1);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                GuardarRemesa(connection, CodEmpresa, remesa));

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar remesa.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Método para eliminar una remesa de liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto AF_RemesasLiquidaciones_Remesa_Eliminar(int CodEmpresa, string usuario, int cod_remesa, string estado)
        {
            var validacion = ValidarEliminacion(CodEmpresa, usuario, estado);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlRemesaDelete,
                new { CodRemesa = cod_remesa });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar remesa.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Remesa deAfiliaciones(Ingresos) : {cod_remesa}", "Elimina - WEB");
            return DbHelper.OkResponse("Ok");
        }
        #endregion

        #region Cargas
        /// <summary>
        /// Método para obtener las remesa de liquidacion activas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfRemesaLiquidacionDto>> AF_RemesasLiquidaciones_Carga_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AfRemesaLiquidacionDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRemesasActivas);
        }

        /// <summary>
        /// Método para obtener las oficinas asociadas a una remesa de liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto<AfRemesasLiquiCargaDatos> AF_RemesasLiqui_CargaOficinas_Obtener(int CodEmpresa, int remesa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var datos = CrearCargaDatosVacia();
                var remesaData = connection.QueryFirstOrDefault<AfRemesasLiquiCargaDatos>(
                    SqlRemesaFechas,
                    new { CodRemesa = remesa });

                if (remesaData is null)
                {
                    return datos;
                }

                datos.fecha_inicio = remesaData.fecha_inicio;
                datos.fecha_corte = remesaData.fecha_corte;
                datos.cboOficinas = connection.Query<DropDownListaGenericaModel>(
                    SqlOficinasRemesa,
                    CrearParametrosFechas(remesaData.fecha_inicio, remesaData.fecha_corte)).ToList();
                datos.cboOficinas.Insert(0, new DropDownListaGenericaModel
                {
                    item = OficinaTodas,
                    descripcion = "T - TODAS LAS OFICINAS"
                });

                return datos;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearCargaDatosVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener oficinas de remesa.",
                    result.Code.GetValueOrDefault(-1),
                    CrearCargaDatosVacia());
        }

        /// <summary>
        /// Método para obtener las liquidaciones asociadas a una remesa de liquidacion y oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <param name="oficina"></param>
        /// <returns></returns>
        public ErrorDto<List<AfRemesasLiquiCargaLista>> AF_RemesasLiqui_CargaLista_Obtener(int CodEmpresa, int remesa, string oficina)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var remesaData = connection.QueryFirstOrDefault<AfRemesasLiquiCargaDatos>(
                    SqlRemesaFechas,
                    new { CodRemesa = remesa });

                if (remesaData is null)
                {
                    throw new InvalidOperationException("No se encontró información de la remesa.");
                }

                var filtraOficina = !string.IsNullOrWhiteSpace(oficina)
                    && !string.Equals(oficina, OficinaTodas, StringComparison.OrdinalIgnoreCase);

                return connection.Query<AfRemesasLiquiCargaLista>(
                    filtraOficina ? SqlCargaListaOficina : SqlCargaListaTodas,
                    CrearParametrosCargaLista(remesaData.fecha_inicio, remesaData.fecha_corte, oficina)).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfRemesasLiquiCargaLista>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener liquidaciones de remesa.",
                    result.Code.GetValueOrDefault(-1),
                    new List<AfRemesasLiquiCargaLista>());
        }

        /// <summary>
        /// Método para cargar las liquidaciones a una remesa de liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto AF_RemesasLiquidaciones_Carga_Cargar(int CodEmpresa, int remesa, string usuario, List<AfRemesasLiquiCargaLista> datos)
        {
            if (datos is null || datos.Count == 0)
            {
                return DbHelper.OkResponse("No hay liquidaciones para cargar.");
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                ValidarRemesaAbierta(connection, remesa);
                foreach (var item in datos)
                {
                    connection.Execute(SqlLiquidacionAsignarRemesa, new { CodRemesa = remesa, Consec = item.consec });
                }

                return true;
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al cargar remesa de liquidaciones.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Carga Remesa Liquidaciones a Microfilmado : {remesa}", "Aplica - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Método para cerrar la carga de liquidaciones a una remesa de liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="remesa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto AF_RemesasLiquidaciones_Carga_Cerrar(int CodEmpresa, int remesa, string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                ValidarRemesaAbierta(connection, remesa);
                connection.Execute(SqlRemesaCerrar, new { CodRemesa = remesa });
                return true;
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al cerrar remesa de liquidaciones.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Cierra Remesa Liquidaciones a Microfilmado :  {remesa}", "Aplica - WEB");
            return DbHelper.OkResponse("Ok");
        }

        #endregion

        #region Reportes

        /// <summary>
        /// Método para obtener las remesas de liquidaciones para reporte
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public ErrorDto<List<AfRemesaLiquidacionDto>> AF_RemesasLiquidaciones_Reporte_Obtener(int CodEmpresa, DateTime fechaInicio, DateTime fechaCorte, int top)
        {
            return DbHelper.ExecuteListQuery<AfRemesaLiquidacionDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlReporteRemesas,
                new
                {
                    Top = Math.Max(1, top),
                    FechaInicio = fechaInicio.Date,
                    FechaCorte = fechaCorte.Date.AddDays(1).AddSeconds(-1)
                });
        }

        /// <summary>
        /// Método para aplicar el reporte de remesas de liquidaciones a microfilmado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDto AF_RemesasLiquidaciones_Reporte_Aplicar(int CodEmpresa, string usuario, int remesa)
        {
            var permiso = ValidarPermiso(CodEmpresa, usuario, "cmdMicrofilm");
            if (permiso.Code != 0)
            {
                return permiso;
            }

            var remesaData = AF_RemesasLiquidaciones_Remesa_Obtener(CodEmpresa, remesa);
            if (remesaData.Code != 0 || remesaData.Result is null)
            {
                return DbHelper.ErrorResponse("La Remesa seleccionada no existe, verifique...!!!", -1);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlRemesaMicrofilm,
                new
                {
                    Usuario = NormalizarTexto(usuario),
                    CodRemesa = remesa
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al aplicar microfilmado.", result.Code.GetValueOrDefault(-1));
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Método para obtener las remesas de liquidaciones asociadas a una liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consec"></param>
        /// <returns></returns>
        public ErrorDto<string> AF_RemesasLiquidaciones_Consultas_Obtener(int CodEmpresa, string consec)
        {
            var result = DbHelper.ExecuteListQuery<string>(
                CreatePortalDb(),
                CodEmpresa,
                SqlConsultaLiquidacionRemesas,
                new { Consec = NormalizarTexto(consec) });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<string>(
                    result.Description ?? "Error al consultar remesas de la liquidación.",
                    result.Code.GetValueOrDefault(-1),
                    null);
            }

            return DbHelper.CreateOkResponse(FormatearConsultaRemesas(result.Result));
        }

        #endregion

        /// <summary>
        /// Valida permisos de usuario
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private ErrorDto<int> Derecho(int CodEmpresa, string usuario, string formName, string boton)
        {
            var result = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            result.Result = _Security_MainDB.Derecho(new ParametrosAccesoDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario.ToUpper(),
                Modulo = vModulo,
                FormName = formName,
                Boton = boton
            });

            return result;
        }



        /// <summary>
        /// Crea parámetros seguros para consultar remesas paginadas.
        /// </summary>
        private static object CrearParametrosRemesas(FiltrosLazyLoadData? filtro)
        {
            filtro ??= new FiltrosLazyLoadData();
            var textoFiltro = NormalizarTexto(filtro.filtro);
            var sortField = string.IsNullOrWhiteSpace(filtro.sortField) ? "FECHA" : filtro.sortField;
            if (!RemesasSortMap.TryGetValue(sortField, out var sortCode))
            {
                sortCode = 2;
            }

            return new
            {
                hasFilter = string.IsNullOrWhiteSpace(textoFiltro) ? 0 : 1,
                filtro = string.IsNullOrWhiteSpace(textoFiltro) ? null : $"%{textoFiltro}%",
                sortCode,
                isAsc = filtro.sortOrder == -1 ? 1 : 0,
                offset = Math.Max(0, filtro.pagina),
                fetch = Math.Max(1, filtro.paginacion)
            };
        }

        /// <summary>
        /// Guarda una remesa nueva o actualiza una existente.
        /// </summary>
        private ErrorDto GuardarRemesa(SqlConnection connection, int codEmpresa, AfRemesaLiquidacionDto remesa)
        {
            var existe = connection.ExecuteScalar<int>(SqlRemesaExiste, new { CodRemesa = remesa.cod_remesa });
            return existe == 0
                ? InsertarRemesa(connection, codEmpresa, remesa)
                : ActualizarRemesa(connection, codEmpresa, remesa);
        }

        /// <summary>
        /// Inserta una remesa nueva.
        /// </summary>
        private ErrorDto InsertarRemesa(SqlConnection connection, int codEmpresa, AfRemesaLiquidacionDto remesa)
        {
            var permiso = ValidarPermiso(codEmpresa, remesa.usuario, "nuevo");
            if (permiso.Code != 0)
            {
                return permiso;
            }

            remesa.cod_remesa = connection.ExecuteScalar<int>(SqlRemesaNuevoCodigo);
            connection.Execute(SqlRemesaInsert, CrearParametrosRemesa(remesa));
            RegistrarBitacora(codEmpresa, remesa.usuario, $"Remesa de Afliaciones  a Microfilmado : {remesa.cod_remesa}", "Registra - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Actualiza una remesa existente.
        /// </summary>
        private ErrorDto ActualizarRemesa(SqlConnection connection, int codEmpresa, AfRemesaLiquidacionDto remesa)
        {
            var permiso = ValidarPermiso(codEmpresa, remesa.usuario, "nuevo");
            if (permiso.Code != 0)
            {
                return permiso;
            }

            connection.Execute(SqlRemesaUpdate, CrearParametrosRemesa(remesa));
            RegistrarBitacora(codEmpresa, remesa.usuario, $"Remesa de Afliaciones  a Microfilmado : {remesa.cod_remesa}", "Modifica - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Crea parámetros seguros para insertar o actualizar remesas.
        /// </summary>
        private static object CrearParametrosRemesa(AfRemesaLiquidacionDto remesa)
        {
            return new
            {
                CodRemesa = remesa.cod_remesa,
                Usuario = NormalizarTexto(remesa.usuario).ToUpperInvariant(),
                Fecha = remesa.fecha,
                FechaInicio = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_inicio, "yyyy-MM-dd") ?? string.Empty,
                FechaCorte = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_corte, "yyyy-MM-dd") ?? string.Empty,
                Notas = NormalizarTexto(remesa.notas)
            };
        }

        /// <summary>
        /// Valida si una remesa se encuentra cerrada.
        /// </summary>
        private static bool EsRemesaCerrada(string? estado)
        {
            return string.Equals(NormalizarTexto(estado), EstadoCerrada, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Valida los permisos y estado antes de eliminar.
        /// </summary>
        private ErrorDto ValidarEliminacion(int codEmpresa, string usuario, string estado)
        {
            var permiso = ValidarPermiso(codEmpresa, usuario, "borrar");
            if (permiso.Code != 0)
            {
                return permiso;
            }

            return EsRemesaCerrada(estado)
                ? DbHelper.ErrorResponse("No se puede modificar la remesa, porque esta ya fue cerrada...", -1)
                : DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida permiso para el botón indicado.
        /// </summary>
        private ErrorDto ValidarPermiso(int codEmpresa, string? usuario, string boton)
        {
            var usuarioSeguro = NormalizarTexto(usuario);
            if (string.IsNullOrWhiteSpace(usuarioSeguro))
            {
                return DbHelper.ErrorResponse("El usuario es obligatorio para validar permisos...", -1);
            }

            var permiso = Derecho(codEmpresa, usuarioSeguro, FormNameRemesasLiquidaciones, boton).Result;
            return permiso == 0
                ? DbHelper.ErrorResponse(MsgSinPermisos, -1)
                : DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Crea parámetros de fechas para consultas de carga.
        /// </summary>
        private static object CrearParametrosFechas(DateTime fechaInicio, DateTime fechaCorte)
        {
            return new
            {
                FechaInicio = fechaInicio.Date,
                FechaCorte = fechaCorte.Date.AddDays(1).AddSeconds(-1)
            };
        }

        /// <summary>
        /// Crea parámetros para consultar liquidaciones pendientes de carga.
        /// </summary>
        private static object CrearParametrosCargaLista(DateTime fechaInicio, DateTime fechaCorte, string oficina)
        {
            return new
            {
                FechaInicio = fechaInicio.Date,
                FechaCorte = fechaCorte.Date.AddDays(1).AddSeconds(-1),
                Oficina = NormalizarTexto(oficina)
            };
        }

        /// <summary>
        /// Valida que una remesa esté abierta.
        /// </summary>
        private static void ValidarRemesaAbierta(SqlConnection connection, int remesa)
        {
            var existe = connection.ExecuteScalar<int>(SqlRemesaAbiertaExiste, new { CodRemesa = remesa });
            if (existe == 0)
            {
                throw new InvalidOperationException("La Remesa actual; ya se encuentra cerrada...");
            }
        }

        /// <summary>
        /// Crea una lista vacía de remesas.
        /// </summary>
        private static AfRemesasLiquidacionesLista CrearListaRemesasVacia()
        {
            return new AfRemesasLiquidacionesLista
            {
                total = 0,
                lista = new List<AfRemesaLiquidacionDto>()
            };
        }

        /// <summary>
        /// Crea datos vacíos de carga.
        /// </summary>
        private static AfRemesasLiquiCargaDatos CrearCargaDatosVacia()
        {
            return new AfRemesasLiquiCargaDatos
            {
                cboOficinas = new List<DropDownListaGenericaModel>()
            };
        }

        /// <summary>
        /// Registra movimiento en bitácora.
        /// </summary>
        private void RegistrarBitacora(int codEmpresa, string? usuario, string detalle, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Formatea la consulta textual de remesas asociadas a liquidaciones.
        /// </summary>
        private static string FormatearConsultaRemesas(List<string>? remesas)
        {
            if (remesas is null || remesas.Count == 0)
            {
                return "** No se encontró Liquidación en las remesas registradas **";
            }

            return string.Join(Environment.NewLine + Environment.NewLine, remesas);
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

    }
}