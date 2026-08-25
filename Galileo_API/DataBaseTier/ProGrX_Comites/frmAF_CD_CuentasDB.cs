using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.Data.SqlClient;
using System.Reflection;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdCuentasDb
    {
        private readonly PortalDB _portalDb;
        private readonly IConfiguration? _config;
        private const string GaConnectionStringName = "GAConnString";

        public FrmAfCdCuentasDb(IConfiguration config)
            : this(new PortalDB(config), config)
        {
        }

        public FrmAfCdCuentasDb(PortalDB portalDb)
            : this(portalDb, null)
        {
        }

        private FrmAfCdCuentasDb(PortalDB portalDb, IConfiguration? config)
        {
            _portalDb = portalDb;
            _config = config;
        }

        /// <summary>
        /// Obtiene informacion de la cuenta
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<AfCdCuentaData?> AfCdCuenta_Obtener(int codEmpresa, int operacion)
        {
            const string sql = @"EXEC spAFI_CD_Cuenta_Load @Operacion;";

            return DbHelper.ExecuteSingleQuery<AfCdCuentaData>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new
                {
                    Operacion = operacion
                }
            );
        }

        /// <summary>
        /// Navegacion para obtener siguiente o anterior cuenta
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="scrollCode"></param>
        /// <returns></returns>
        public ErrorDto<AfCdCuentaData?> AfCdCuentas_Scroll_Obtener(int codEmpresa, int operacion, int scrollCode)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string query = @"
                select top 1 R.NOperacion
                from Afi_CD_Cuentas R
                where (
                        (@scrollCode = 1 and R.NOperacion > @numOperacion)
                     or (@scrollCode <> 1 and R.NOperacion < @numOperacion)
                      )
                order by
                    case when @scrollCode = 1 then R.NOperacion end asc,
                    case when @scrollCode <> 1 then R.NOperacion end desc;";

                var operacionDestino = conn.QueryFirstOrDefault<int?>(query, new
                {
                    numOperacion = operacion,
                    scrollCode
                });

                var numeroObjetivo = operacionDestino ?? operacion;

                return AfCdCuenta_Obtener(codEmpresa, numeroObjetivo);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<AfCdCuentaData?>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de actividades para una cuenta
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="totalAsoc"></param>
        /// <param name="operacion"></param>
        /// <param name="comite"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdActividadData>> AfCdActividades_Lista_Obtener(
            int codEmpresa, string tipo, int totalAsoc, int operacion, int comite)
        {
            const string query = @"EXEC spAFI_CD_Actividades_List @Tipo, @TotalAsoc, @Operacion, @Comite;";

            return DbHelper.ExecuteListQuery<AfCdActividadData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Tipo = tipo,
                    TotalAsoc = totalAsoc,
                    Operacion = operacion,
                    Comite = comite
                }
            );
        }

        /// <summary>
        /// Obtiene la lista de adjuntos para una cuenta
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdCuentaAdjuntosData>> AfCdCuenta_Adjuntos_Obtener(int codEmpresa, int operacion)
        {
            const string query = @"exec spAFI_CD_Cuenta_Adjuntos @Operacion;";

            var response = DbHelper.ExecuteListQuery<AfCdCuentaAdjuntosData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Operacion = operacion
                }
            );

            if (response.Code < 0)
            {
                return response;
            }

            response.Result ??= [];
            response.Result.AddRange(AfCdCuenta_GaFilesAdjuntos_Obtener(codEmpresa, operacion));
            return response;
        }

        private List<AfCdCuentaAdjuntosData> AfCdCuenta_GaFilesAdjuntos_Obtener(int codEmpresa, int operacion)
        {
            if (_config == null)
            {
                return [];
            }

            const string query = @"
                SELECT
                    TRY_CONVERT(int, FileId) AS idArchivoAdjunto,
                    TRY_CONVERT(int, Llave_02) AS noperacion,
                    ISNULL(FileName, '') AS nombreArchivo,
                    TRY_CONVERT(int, TypeId) AS idtipoarchivo,
                    ISNULL(Notas, '') AS nota,
                    RegistroFecha AS registrofecha,
                    ISNULL(RegistroUsuario, '') AS registroUsuario,
                    ISNULL(TypeId, '') AS nombreTipoArchivo
                FROM GA_Files
                WHERE EmpresaId = @codEmpresa
                  AND ModuloId = 'CD_01'
                  AND Llave_02 = @operacion
                  AND ISNULL(Llave_03, '') = ''
                ORDER BY RegistroFecha DESC, FileId DESC;";

            try
            {
                using var connection = new SqlConnection(
                    _config.GetConnectionString(GaConnectionStringName));

                return connection.Query<AfCdCuentaAdjuntosData>(
                    query,
                    new
                    {
                        codEmpresa,
                        operacion = operacion.ToString()
                    }).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return [];
            }
        }

        /// <summary>
        /// Obtiene la bitacora de una cuenta
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdCuentaBitacoraData>> AfCdCuenta_Bitacora_Obtener(int codEmpresa, int operacion)
        {
            const string query = @"exec spAFI_CD_Cuenta_Bitacora @Operacion;";

            return DbHelper.ExecuteListQuery<AfCdCuentaBitacoraData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Operacion = operacion
                }
            );
        }

        /// <summary>
        /// Obtiene la lista de cuentas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdCuentaData>> AfCdCuentas_Lista_Obtener(int codEmpresa)
        {
            string query = @"select NOperacion, Cod_Comite, Cedula, Saldo from afi_cd_Cuentas order by NOperacion desc";
            return DbHelper.ExecuteListQuery<AfCdCuentaData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de comites
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
        {
            string query = @"select COD_COMITE as item, DESCRIPCION from AFI_CD_COMITES where ACTIVO = 1";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de diferentes catalogos (bancos, tipos de emisión, tipo de actividad y autorizacion)
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="origen"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AfCdCatalogo_Lista_Obtener(int codEmpresa, string origen)
        {
            string query = "";
            switch (origen)
            {
                case "cboBanco":
                    query = "select id_banco as item, descripcion from AFI_CD_vBancos";
                    break;
                case "cboEmite":
                    query = "select CodTipoCuenta as item, NombreTipoCuenta as descripcion from AFI_CD_TIPO_CUENTA where Activo = 1";
                    break;
                case "cboActividadTipo":
                    query = "select CodTipoActividad as item, NombreTipoActividad as descripcion from AFI_CD_TIPO_ACTIVIDAD where Activo = 1";
                    break;
                case "cboAutorizacion":
                    query = "select CodTipoAprobacion as item, NombreTipoAprobacion as descripcion from AFI_CD_TIPO_APROBACION where Activo = 1";
                    break;
            }
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de cuentas bancarias
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="idBanco"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdCuentaBancariaData>> AfCdCuentasBancarias_Obtener(int codEmpresa, string cedula, int idBanco)
        {
            string query = @"exec spSys_Cuentas_Bancarias @cedula, @idBanco, 1";
            return DbHelper.ExecuteListQuery<AfCdCuentaBancariaData>(
                _portalDb, codEmpresa, query, new { cedula, idBanco });
        }

        /// <summary>
        /// Obtiene la lista de miembros de un comite
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codComite"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AfCdMiembros_Obtener(int codEmpresa, int codComite)
        {
            string query = @"select N.cedula as item, S.nombre as descripcion 
                from afi_cd_comites C left join afi_cd_nombramientos N on C.cod_comite = N.cod_comite 
                inner join socios S on S.cedula = N.cedula 
                where N.cod_comite = @Comite and N.APL_DESEMBOLSOS = 1";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb, codEmpresa, query, new { Comite = codComite });
        }

        /// <summary>
        /// Obtiene la lista de liquidaciones pendientes por comite
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codComite"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdCuentaData>> AfCdLiquidacionesPendientes_Obtener(int codEmpresa, int codComite)
        {
            string query = @"
            select 
                A.noperacion as Noperacion,
                C.notas as Notas,
                sum(A.monto) as Monto,
                C.estado as Estado,
                C.tesoreria_nsolicitud as TesoreriaNSolicitud,
                C.liquida_fecha as LiquidaFecha
            from afi_cd_cuentas C
            inner join afi_cd_cuentas_actividades A 
                on C.noperacion = A.noperacion
            where C.cod_comite = @Comite
              and C.PROCESO = 'T'
            group by 
                A.noperacion,
                C.notas,
                C.estado,
                C.tesoreria_nsolicitud,
                C.liquida_fecha";

            return DbHelper.ExecuteListQuery<AfCdCuentaData>(
                _portalDb, codEmpresa, query, new { Comite = codComite });
        }

        /// <summary>
        /// Obtiene la lista de cargos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdCargoData>> AfCdCargos_Lista_Obtener(int codEmpresa)
        {
            string query = @"Select CODIGO, DESCRIPCION from AFI_CD_CARGOS where ESTADO = 1";
            return DbHelper.ExecuteListQuery<AfCdCargoData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la cantidad de asociados activos en un comite
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codComite"></param>
        /// <returns></returns>
        public ErrorDto<int> AfCdCantidadAsociados_Obtener(int codEmpresa, int codComite)
        {
            string query = @"select count(*) as 'Cantidad' from socios 
                where EstadoActual = 'S' and cod_departamento 
                in(select Codigo_UP from Afi_CD_Comites_Unidades where cod_comite = @codComite)";
            return DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0, new { codComite });
        }

        /// <summary>
        /// Descarta una cuenta
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AfCdCuenta_Descartar(int codEmpresa, int operacion, string usuario)
        {
            const string query = @"exec spAFI_CD_Cuenta_Descarta @operacion, @usuario";
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { operacion, usuario });
        }

        /// <summary>
        /// Guarda una nueva cuenta 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AfCdCuenta_Guardar(int codEmpresa, string usuario, AfCdCuentaRequest request)
        {
            if (request?.cuenta == null)
            {
                return CreateError(-2, "La información especificada no es válida, verifíquela...");
            }

            NormalizeRequestCollections(request);

            try
            {
                var cuentaData = request.cuenta;
                var actividadesIds = request.actividades.Select(x => x.cod_actividad).ToList();

                if (!FxValidaCuenta(codEmpresa, cuentaData.noperacion, cuentaData.cod_comite, actividadesIds) &&
                    !request.confirmado)
                {
                    return CreateError(-3, "Existen actividades que ya fueron gestionadas anteriormente.");
                }

                int nuevoId = ObtenerSiguienteOperacion(codEmpresa);

                var insertResponse = InsertarCuenta(codEmpresa, usuario, cuentaData, nuevoId);
                if (HasError(insertResponse))
                {
                    return insertResponse;
                }

                var actividadesResponse = GuardarActividades(codEmpresa, nuevoId, request.actividades);
                if (HasError(actividadesResponse))
                {
                    return actividadesResponse;
                }

                var refundicionResponse = SbGuardaRefundicion(codEmpresa, nuevoId, request.refundiciones);
                if (HasError(refundicionResponse))
                {
                    return refundicionResponse;
                }

                var cargosResponse = SbGuardaCargos(codEmpresa, nuevoId, request.cargos);
                if (HasError(cargosResponse))
                {
                    return cargosResponse;
                }

                return new ErrorDto
                {
                    Code = nuevoId,
                    Description = "Solicitud Registrada: Proceda a la Aprobación!"
                };
            }
            catch (Exception ex)
            {
                return CreateError(-1, ex.Message);
            }
        }

        #region Helpers AfCdCuenta_Guardar
        private static void NormalizeRequestCollections(AfCdCuentaRequest request)
        {
            request.actividades ??= new List<AfCdActividadData>();
            request.refundiciones ??= new List<AfCdCuentaData>();
            request.cargos ??= new List<AfCdCargoData>();
        }

        private int ObtenerSiguienteOperacion(int codEmpresa)
        {
            const string sql = @"
                select isnull(max(noperacion),0) + 1
                from afi_cd_cuentas;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { }
            ).Result;
        }

        private ErrorDto? InsertarCuenta(int codEmpresa, string usuario, AfCdCuentaData cuentaData, int nuevoId)
        {
            const string sql = @"
                insert into afi_cd_cuentas
                (
                    noperacion, cod_comite, cedula, registro_fecha, registro_usuario,
                    estado, tipo, cuenta, id_banco, notas, aprueba, cod_director,
                    PROCESO, AJUSTE_ASOC, MONTO, MONTO_REFUNDE, MONTO_CARGOS,
                    SALDO, CANT_ASOCIADOS, GuidId
                )
                values
                (
                    @NOperacion, @CodComite, @Cedula, Getdate(), @Usuario,
                    'S', @Tipo, @Cuenta, @IdBanco, @Notas, @Aprueba, @CodDirector,
                    'T', @AjusteAsoc, @Monto, @MontoRefunde, @MontoCargos,
                    @Saldo, @CantAsociados, NEWID()
                );";

            int codDirector = cuentaData.cod_director == 0 ? 1 : cuentaData.cod_director;

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    NOperacion = nuevoId,
                    CodComite = cuentaData.cod_comite,
                    Cedula = Clean(cuentaData.cedula),
                    Usuario = usuario,
                    Tipo = Clean(cuentaData.tipo),
                    Cuenta = Clean(cuentaData.cuenta),
                    IdBanco = cuentaData.id_banco,
                    Notas = Clean(cuentaData.notas),
                    Aprueba = Clean(cuentaData.aprueba),
                    CodDirector = codDirector,
                    AjusteAsoc = cuentaData.ajuste_asoc,
                    Monto = cuentaData.monto,
                    MontoRefunde = cuentaData.monto_refunde,
                    MontoCargos = cuentaData.monto_cargos,
                    Saldo = cuentaData.monto,
                    CantAsociados = cuentaData.cant_asociados
                }
            );
        }

        private ErrorDto? GuardarActividades(int codEmpresa, int nuevoId, List<AfCdActividadData> actividades)
        {
            if (actividades.Count == 0)
            {
                return null;
            }

            const string sql = @"
                insert into afi_cd_cuentas_actividades (COD_ACTIVIDAD, NOPERACION, MONTO)
                values (@CodActividad, @NOperacion, @Monto);";

            foreach (var item in actividades)
            {
                var response = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        CodActividad = item.cod_actividad,
                        NOperacion = nuevoId,
                        Monto = item.monto
                    }
                );

                if (HasError(response))
                {
                    return response;
                }
            }

            return null;
        }

        private static bool HasError(ErrorDto? response)
        {
            return response is not null && response.Code < 0;
        }

        private static ErrorDto CreateError(int code, string description)
        {
            return new ErrorDto
            {
                Code = code,
                Description = description
            };
        }

        private static string Clean(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private bool FxValidaCuenta(int codEmpresa, int noperacion, int codComite, List<int> actividades)
        {
            string actividadesCsv = actividades != null && actividades.Any()
                ? string.Join(",", actividades)
                : string.Empty;

            const string sql = @"
            select dbo.fxAFI_CD_Cuenta_Valida(@NOperacion, @Comite, @Actividades) as Resultado;";

            var resultado = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                string.Empty,
                new
                {
                    NOperacion = noperacion,
                    Comite = codComite,
                    Actividades = actividadesCsv
                }
            ).Result;

            return string.IsNullOrWhiteSpace(resultado);
        }

        private ErrorDto SbGuardaRefundicion(int codEmpresa, int noperacion, List<AfCdCuentaData> refundiciones)
        {
            try
            {
                if (refundiciones == null || refundiciones.Count == 0)
                    return new ErrorDto { Code = 0, Description = "OK" };

                var item = refundiciones.LastOrDefault();
                if (item == null)
                    return new ErrorDto { Code = 0, Description = "OK" };

                const string sql = @"
                insert into AFI_CD_REFUNDICIONES (NOPERACIONR, NOPERACION, MONTO, FECHA)
                values (@NOperacionR, @NOperacion, @Monto, @Fecha);";

                var resp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        NOperacionR = item.noperacion,
                        NOperacion = noperacion,
                        Monto = item.monto,
                        Fecha = DateTime.Now.ToString("yyyyMMdd")
                    }
                );

                return resp ?? new ErrorDto { Code = 0, Description = "OK" };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        private ErrorDto SbGuardaCargos(int codEmpresa, int noperacion, List<AfCdCargoData> cargos)
        {
            try
            {
                if (cargos == null || cargos.Count == 0)
                    return new ErrorDto { Code = 0, Description = "OK" };

                var item = cargos.LastOrDefault();
                if (item == null)
                    return new ErrorDto { Code = 0, Description = "OK" };

                const string sql = @"
                insert into AFI_CD_CARGOS_CUENTAS (CODIGO, NOPERACION, MONTO, FECHA)
                values (@Codigo, @NOperacion, @Monto, @Fecha);";

                var resp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Codigo = item.codigo,
                        NOperacion = noperacion,
                        Monto = item.monto,
                        Fecha = DateTime.Now.ToString("yyyyMMdd")
                    }
                );

                return resp ?? new ErrorDto { Code = 0, Description = "OK" };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        #endregion  
    }
}
