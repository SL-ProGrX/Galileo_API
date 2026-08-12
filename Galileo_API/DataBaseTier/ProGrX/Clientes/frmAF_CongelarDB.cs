using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmAFCongelarDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        private const string SqlSocios = @"
                    SELECT CEDULA AS item,
                           NOMBRE AS descripcion
                    FROM dbo.SOCIOS
                    ORDER BY CEDULA;";

        private const string SqlCongelarCausasActivas = @"
                    SELECT RTRIM(COD_CAUSA) AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM dbo.AFI_CONGELAR_CAUSAS
                    WHERE Activa = 1
                    ORDER BY COD_CAUSA;";

        private const string SqlCongelarCausasCatalogo = @"
                    SELECT COD_CAUSA AS item,
                           DESCRIPCION AS descripcion
                    FROM dbo.AFI_CONGELAR_CAUSAS
                    ORDER BY COD_CAUSA;";

        private const string SqlCongelarCausasMant = @"
                    SELECT COD_CAUSA,
                           descripcion,
                           Activa,
                           registro_fecha,
                           registro_usuario
                    FROM dbo.AFI_CONGELAR_CAUSAS
                    ORDER BY COD_CAUSA;";

        private const string SqlBloqueosBaseFromWhere = @"
                    FROM dbo.afi_congelar C
                    LEFT JOIN dbo.Socios S
                        ON LTRIM(RTRIM(C.cedula)) = LTRIM(RTRIM(S.cedula))
                    LEFT JOIN dbo.afi_congelar_causas X
                        ON C.cod_causa = X.cod_causa
                    WHERE (@Cedula = '' OR C.cedula LIKE '%' + @Cedula + '%')
                      AND (@Nombre = '' OR S.nombre LIKE '%' + @Nombre + '%')
                      AND (@AplicarFecha = 0 OR C.fecha_Inicia BETWEEN @FechaDesde AND @FechaHasta)
                      AND (@Estado = 'X' OR C.estado = @Estado)";

        private static readonly string SqlBloqueosTotal = $@"
                    SELECT COUNT(*)
                    {SqlBloqueosBaseFromWhere};";

        private static readonly string SqlBloqueosLista = $@"
                    SELECT C.*,
                           S.nombre,
                           RTRIM(C.cod_causa) AS CausaId,
                           RTRIM(X.descripcion) AS CausaDesc
                    {SqlBloqueosBaseFromWhere}
                      AND (@hasFilter = 0 OR
                           C.cod_congelar LIKE @filtro OR
                           C.cedula LIKE @filtro OR
                           S.nombre LIKE @filtro)
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN C.cod_congelar END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN C.cod_congelar END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN C.cedula END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN C.cedula END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN S.nombre END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN S.nombre END DESC,
                        C.cod_congelar DESC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private static readonly string SqlBloqueosExportar = $@"
                    SELECT C.*,
                           S.nombre,
                           RTRIM(C.cod_causa) AS CausaId,
                           RTRIM(X.descripcion) AS CausaDesc
                    {SqlBloqueosBaseFromWhere}
                    ORDER BY C.cod_congelar DESC;";

        private const string SqlCongelarUpdate = @"
                    UPDATE dbo.afi_congelar
                    SET notas = @Notas,
                        cod_causa = @CodCausa,
                        estado = @Estado,
                        per_liquidacion = @PerLiquidacion,
                        per_mostrar_ec = @PerMostrarEc,
                        per_abono_cajas = @PerAbonoCajas,
                        per_cierra_AcCreditos = @PerCierraAcCreditos,
                        per_cobro_judicial = @PerCobroJudicial,
                        per_traspaso_deudas = @PerTraspasoDeudas,
                        per_reversiones = @PerReversiones,
                        per_readecuaciones = @PerReadecuaciones,
                        per_deducciones_creditos = @PerDeduccionesCreditos,
                        per_deducciones_aportes = @PerDeduccionesAportes,
                        per_generacion_mora = @PerGeneracionMora,
                        per_cobro_FndSol = @PerCobroFndSol,
                        per_cobro_cuotaCr = @PerCobroCuotaCr,
                        fecha_inicia = @FechaInicia,
                        fecha_finaliza = @FechaFinaliza,
                        cedula = @Cedula
                    WHERE cod_congelar = @CodCongelar;";

        private const string SqlCongelarInsert = @"
                    INSERT INTO dbo.afi_congelar
                    (
                        cedula,
                        cod_causa,
                        notas,
                        fecha_crea,
                        usuario_crea,
                        estado,
                        fecha_Inicia,
                        fecha_Finaliza,
                        per_liquidacion,
                        per_mostrar_ec,
                        per_abono_cajas,
                        per_cierra_AcCreditos,
                        per_cobro_judicial,
                        per_traspaso_deudas,
                        per_reversiones,
                        per_readecuaciones,
                        per_deducciones_creditos,
                        per_deducciones_aportes,
                        per_generacion_mora,
                        per_cobro_FndSol,
                        per_cobro_cuotaCr
                    )
                    VALUES
                    (
                        @Cedula,
                        @CodCausa,
                        @Notas,
                        dbo.MyGetDate(),
                        @UsuarioCrea,
                        @Estado,
                        @FechaInicia,
                        @FechaFinaliza,
                        @PerLiquidacion,
                        @PerMostrarEc,
                        @PerAbonoCajas,
                        @PerCierraAcCreditos,
                        @PerCobroJudicial,
                        @PerTraspasoDeudas,
                        @PerReversiones,
                        @PerReadecuaciones,
                        @PerDeduccionesCreditos,
                        @PerDeduccionesAportes,
                        @PerGeneracionMora,
                        @PerCobroFndSol,
                        @PerCobroCuotaCr
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

        private const string SqlCausaUso = @"
                    SELECT COUNT(*)
                    FROM dbo.AFI_CONGELAR
                    WHERE cod_causa = @CodCausa;";

        private const string SqlCausaDelete = @"
                    DELETE FROM dbo.AFI_CONGELAR_CAUSAS
                    WHERE COD_CAUSA = @CodCausa;";

        private const string SqlCausaInsert = @"
                    INSERT INTO dbo.AFI_CONGELAR_CAUSAS
                    (
                        cod_causa,
                        descripcion,
                        activa,
                        registro_fecha,
                        registro_usuario
                    )
                    VALUES
                    (
                        @CodCausa,
                        @Descripcion,
                        @Activa,
                        dbo.MyGetDate(),
                        @Usuario
                    );";

        private const string SqlCausaUpdate = @"
                    UPDATE dbo.AFI_CONGELAR_CAUSAS
                    SET descripcion = @Descripcion,
                        Activa = @Activa
                    WHERE cod_causa = @CodCausa;";

        private static readonly IReadOnlyDictionary<string, int> BloqueosSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["cod_congelar"] = 1,
            ["cedula"] = 2,
            ["nombre"] = 3
        };

        public FrmAFCongelarDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        #region Consulta

        /// <summary>
        /// Método para obtener los socios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Congela_Socios_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlSocios);
        }


        /// <summary>
        /// Obtiene los bloqueos y congelamientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosCongelar"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> AF_BloqueosCongelamientos_Obtener(int CodEmpresa, string filtrosCongelar, FiltrosLazyLoadData filtros)
        {
            var filtro = DbHelper.DeserializeOrNew<AFCongelarFiltros>(filtrosCongelar);
            var response = DbHelper.CreateOkResponse(new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<AFCongelarDto>()
            });

            var spec = LazyLoadHelper.Build(filtros, BloqueosSortMap, "cod_congelar");
            AgregarParametrosBloqueos(spec, filtro);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new TablasListaGenericaModel
            {
                total = connection.QueryFirstOrDefault<int>(SqlBloqueosTotal, spec.Params),
                lista = connection.Query<AFCongelarDto>(SqlBloqueosLista, spec.Params).ToList()
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener bloqueos y congelamientos.",
                    result.Code.GetValueOrDefault(-1),
                    new TablasListaGenericaModel());
            }

            response.Result = result.Result ?? new TablasListaGenericaModel();
            return response;
        }


        /// <summary>
        /// Método para exportar los bloqueos y congelamientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosCongelar"></param>
        /// <returns></returns>
        public ErrorDto<List<AFCongelarDto>> AF_BloqueosCongelamientos_Exportar(int CodEmpresa, string filtrosCongelar)
        {
            var filtro = DbHelper.DeserializeOrNew<AFCongelarFiltros>(filtrosCongelar);
            return DbHelper.ExecuteListQuery<AFCongelarDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlBloqueosExportar,
                CrearParametrosBloqueos(filtro));
        }

        #endregion

        #region Registro

        /// <summary>
        /// Método para obtener tipos de causa para congelar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CongelarCausaLista_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlCongelarCausasActivas);
        }


        /// <summary>
        /// Obtener tipos de causa para congelar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CongelarCausa_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlCongelarCausasCatalogo);
        }


        /// <summary>
        /// Inserta o Actualiza Bloqueos o Congelamientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="congelar"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_BloqueosCongelamientos_Guardar(int CodEmpresa, string usuario, AFCongelarDto congelar)
        {
            try
            {
                if (congelar is null)
                    return DbHelper.CreateErrorResponse<int>("Los datos del bloqueo o congelamiento son requeridos.", -2);

                var parametros = CrearParametrosCongelar(usuario, congelar);

                if (congelar.cod_congelar != 0)
                {
                    var update = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, SqlCongelarUpdate, parametros);
                    return update.Code == 0
                        ? DbHelper.CreateOkResponse<int>(congelar.cod_congelar, "Guardado correctamente")
                        : DbHelper.CreateErrorResponse<int>(update.Description ?? "Error al guardar bloqueo o congelamiento.", update.Code.GetValueOrDefault(-1));
                }

                var insert = DbHelper.ExecuteSingleQuery<int>(CreatePortalDb(), CodEmpresa, SqlCongelarInsert, 0, parametros);



                return insert.Code == 0
                    ? DbHelper.CreateOkResponse<int>(insert.Result, "Guardado correctamente")
                    : DbHelper.CreateErrorResponse<int>(insert.Description ?? "Error al guardar bloqueo o congelamiento.", insert.Code.GetValueOrDefault(-1));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>($"Error al guardar bloqueo o congelamiento: {ex.Message}", -1);
            }
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Método para obtener tipos de causa para congelar en mantenimiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AFCongelaCausaDto>> AF_CongelarCausaMant_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AFCongelaCausaDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlCongelarCausasMant);
        }


        /// <summary>
        /// Método para eliminar una causa de congelamiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_causa"></param>
        /// <returns></returns>
        public ErrorDto AF_CongelarCausaMant_Eliminar(int CodEmpresa, string cod_causa)
        {
            var codCausa = NormalizarTexto(cod_causa);
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var uso = connection.QueryFirstOrDefault<int>(SqlCausaUso, new { CodCausa = codCausa });
                if (uso > 0)
                {
                    return DbHelper.ErrorResponse("No se puede eliminar la causa porque está siendo utilizada en congelamientos.");
                }

                connection.Execute(SqlCausaDelete, new { CodCausa = codCausa });
                return DbHelper.OkResponse("Ok");
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar causa de congelamiento.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Método para guardar una causa de congelamiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="causa"></param>
        /// <returns></returns>
        public ErrorDto AF_CongelarCausaMant_Guardar(int CodEmpresa, string usuario ,AFCongelaCausaDto causa)
        {
            if (causa is null)
            {
                return DbHelper.ErrorResponse("Los datos de la causa son requeridos.", -2);
            }

            return causa.isNew
                ? AF_CongelarCausaMant_Insertar(CodEmpresa, usuario, causa)
                : AF_CongelarCausaMant_Actualiza(CodEmpresa, usuario, causa);
        }


        /// <summary>
        /// Método para insertar una causa de congelamiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="causa"></param>
        /// <returns></returns>
        private ErrorDto AF_CongelarCausaMant_Insertar(int CodEmpresa, string usuario, AFCongelaCausaDto causa)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlCausaInsert,
                CrearParametrosCausa(usuario, causa));

            if (result.Code == 0)
            {
                RegistrarBitacoraCausa(CodEmpresa, usuario, causa, "Registra - WEB");
            }

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar causa de congelamiento.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Método para actualizar una causa de congelamiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="causa"></param>
        /// <returns></returns>
        private ErrorDto AF_CongelarCausaMant_Actualiza(int CodEmpresa, string usuario, AFCongelaCausaDto causa)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlCausaUpdate,
                CrearParametrosCausa(usuario, causa));

            if (result.Code == 0)
            {
                RegistrarBitacoraCausa(CodEmpresa, usuario, causa, "Modifica - WEB");
            }

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar causa de congelamiento.", result.Code.GetValueOrDefault(-1));
        }

        #endregion

        private PortalDB CreatePortalDb() => new(_config);

        private static object CrearParametrosBloqueos(AFCongelarFiltros filtro)
        {
            return new
            {
                Cedula = NormalizarTexto(filtro.cedula),
                Nombre = NormalizarTexto(filtro.nombre),
                AplicarFecha = filtro.chkTodasFechas ? 0 : 1,
                FechaDesde = filtro.fecha_desde.Date,
                FechaHasta = filtro.fecha_hasta.Date.AddDays(1).AddTicks(-1),
                Estado = NormalizarEstado(filtro.estado)
            };
        }

        private static void AgregarParametrosBloqueos(LazyLoadSpec spec, AFCongelarFiltros filtro)
        {
            var parametrosBase = CrearParametrosBloqueos(filtro);
            foreach (var property in parametrosBase.GetType().GetProperties())
            {
                spec.Params.Add(property.Name, property.GetValue(parametrosBase));
            }
        }

        private static object CrearParametrosCongelar(string usuario, AFCongelarDto congelar)
        {
            return new
            {
                Cedula = NormalizarTexto(congelar.cedula),
                CodCausa = NormalizarTexto(congelar.cod_causa),
                Notas = NormalizarTexto(congelar.notas),
                UsuarioCrea = NormalizarTexto(string.IsNullOrWhiteSpace(congelar.usuario_crea) ? usuario : congelar.usuario_crea),
                Estado = NormalizarTexto(congelar.estado),
                FechaInicia = congelar.fecha_inicia,
                FechaFinaliza = congelar.fecha_finaliza,
                PerLiquidacion = congelar.per_liquidacion,
                PerMostrarEc = congelar.per_mostrar_ec,
                PerAbonoCajas = congelar.per_abono_cajas,
                PerCierraAcCreditos = congelar.per_cierra_accreditos,
                PerCobroJudicial = congelar.per_cobro_judicial,
                PerTraspasoDeudas = congelar.per_traspaso_deudas,
                PerReversiones = congelar.per_reversiones,
                PerReadecuaciones = congelar.per_readecuaciones,
                PerDeduccionesCreditos = congelar.per_deducciones_creditos,
                PerDeduccionesAportes = congelar.per_deducciones_aportes,
                PerGeneracionMora = congelar.per_generacion_mora,
                PerCobroFndSol = congelar.per_cobro_fndsol,
                PerCobroCuotaCr = congelar.per_cobro_cuotacr,
                CodCongelar = congelar.cod_congelar
            };
        }

        private static object CrearParametrosCausa(string usuario, AFCongelaCausaDto causa)
        {
            return new
            {
                CodCausa = NormalizarTexto(causa.cod_causa),
                Descripcion = NormalizarTexto(causa.descripcion),
                Activa = causa.activa ? 1 : 0,
                Usuario = NormalizarTexto(usuario)
            };
        }

        private void RegistrarBitacoraCausa(int codEmpresa, string usuario, AFCongelaCausaDto causa, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = $"Causa de Congelamiento Cod: : {NormalizarTexto(causa.cod_causa)}",
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string NormalizarEstado(string? estado)
        {
            var valor = NormalizarTexto(estado);
            return string.IsNullOrWhiteSpace(valor) ? "X" : valor;
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
