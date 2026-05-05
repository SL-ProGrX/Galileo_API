using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXCatalogoCuentasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int VModulo = 20;

        public FrmCntXCatalogoCuentasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _dbBitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = VModulo
            });
        }

        /// <summary>
        /// Obtiene las divisas del catálogo de cuentas.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXCatalogoDivisas(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                select rtrim(cod_divisa) as item, rtrim(descripcion) as descripcion
                from CntX_Divisas
                where cod_contabilidad = @codContabilidad
                order by divisa_local desc";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Obtiene las unidades de negocio.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXCatalogoUnidades(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                select rtrim(cod_unidad) as item, rtrim(descripcion) as descripcion
                from CntX_Unidades
                where cod_contabilidad = @codContabilidad
                order by descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Obtiene los centros de costo activos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXCatalogoCentrosCosto(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                select rtrim(cod_centro_costo) as item, rtrim(descripcion) as descripcion
                from CntX_Centro_Costos
                where activo = 1
                  and cod_contabilidad = @codContabilidad
                order by descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Obtiene los tipos de cuenta del catÃ¡logo contable.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXCatalogoTiposCuenta(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                select rtrim(tipo_cuenta) as item,
                       rtrim(tipo_cuenta) + ' - ' + descripcion as descripcion
                from CntX_Tipos_Cuentas
                where cod_contabilidad = @codContabilidad
                order by tipo_cuenta";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Consulta el catálogo principal de cuentas.
        /// </summary>
        public ErrorDto<List<CntXCatalogoCuentaDto>> CntXCatalogoConsulta(int codEmpresa, CntXCatalogoCuentasFiltroRequest filtro)
        {
            string sql = filtro.MostrarBalance ? @"
                select C.cod_cuenta,
                       C.cod_cuenta_mask,
                       space(C.nivel * 2) + ltrim(C.descripcion) as descripcion,
                       C.cod_divisa,
                       C.tipo_cuenta,
                       rtrim(T.tipo_cuenta) + ' - ' + T.descripcion as tipo,
                       C.acepta_movimientos,
                       C.presupuesto,
                       C.bloqueada,
                       C.cuenta_auxiliar,
                       isnull(M.saldo_inicial, 0) as saldo_inicial,
                       isnull(M.total_debitos, 0) as total_debitos,
                       isnull(M.total_creditos, 0) as total_creditos,
                       cast(0 as bit) as isNew
                from CntX_Cuentas C
                inner join CntX_Tipos_Cuentas T
                  on T.tipo_cuenta = C.tipo_cuenta
                 and C.cod_contabilidad = T.cod_contabilidad
                left join vCntX_Mov_Cuentas_General M
                  on C.cod_cuenta = M.cod_cuenta
                 and C.cod_contabilidad = M.cod_contabilidad
                 and M.anio = @PeriodoAnio
                 and M.mes = @PeriodoMes
                where C.cod_contabilidad = @CodContabilidad
                  and C.nivel <= @Nivel"
            : @"
                select C.cod_cuenta,
                       C.cod_cuenta_mask,
                       space(C.nivel * 2) + ltrim(C.descripcion) as descripcion,
                       C.cod_divisa,
                       C.tipo_cuenta,
                       rtrim(T.tipo_cuenta) + ' - ' + T.descripcion as tipo,
                       C.acepta_movimientos,
                       C.presupuesto,
                       C.bloqueada,
                       C.cuenta_auxiliar,
                       cast(0 as decimal(18,2)) as saldo_inicial,
                       cast(0 as decimal(18,2)) as total_debitos,
                       cast(0 as decimal(18,2)) as total_creditos,
                       cast(0 as bit) as isNew
                from CntX_Cuentas C
                inner join CntX_Tipos_Cuentas T
                  on T.tipo_cuenta = C.tipo_cuenta
                 and C.cod_contabilidad = T.cod_contabilidad
                where C.cod_contabilidad = @CodContabilidad
                  and C.nivel <= @Nivel";

            if (!string.Equals(filtro.CodDivisa, "TODOS", StringComparison.OrdinalIgnoreCase))
            {
                sql += " and C.cod_divisa = @CodDivisa";
            }

            if (!string.IsNullOrWhiteSpace(filtro.Cuenta))
            {
                sql += " and C.cod_cuenta_mask like @CuentaLike";
            }

            if (!string.IsNullOrWhiteSpace(filtro.Descripcion))
            {
                sql += " and C.descripcion like @DescripcionLike";
            }

            sql += " order by C.cod_cuenta";

            return DbHelper.ExecuteListQuery<CntXCatalogoCuentaDto>(_portalDb, codEmpresa, sql, new
            {
                filtro.CodContabilidad,
                filtro.PeriodoAnio,
                filtro.PeriodoMes,
                filtro.CodDivisa,
                filtro.Nivel,
                CuentaLike = $"{filtro.Cuenta.Trim()}%",
                DescripcionLike = $"%{filtro.Descripcion.Trim()}%"
            });
        }

        /// <summary>
        /// Obtiene el detalle adicional de una cuenta.
        /// </summary>
        public ErrorDto<CntXCatalogoCuentaDetalleResponse> CntXCatalogoDetalle(int codEmpresa, int codContabilidad, string cuenta)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var param = new DynamicParameters();
                param.Add("@Contabilidad", codContabilidad);
                param.Add("@Cuenta", cuenta);

                var detalle = conn.QueryFirstOrDefault<CntXCatalogoCuentaDetalleDto>(
                    "spCntX_Cuenta_Detalle",
                    param,
                    commandType: CommandType.StoredProcedure) ?? new CntXCatalogoCuentaDetalleDto();

                const string sqlTraducciones = @"
                    select cod_idioma, descripcion
                    from CNTX_CUENTAS_TRADUCCION
                    where cod_contabilidad = @codContabilidad
                      and cod_cuenta = @cuenta";

                const string sqlProrrateos = @"
                    select P.cod_unidad,
                           isnull(U.descripcion, '') as unidad_desc,
                           P.cod_centro_costo,
                           isnull(CC.descripcion, '') as centro_desc,
                           P.porcentaje
                    from CNTX_CUENTAS_PRORRATA P
                    left join CNTX_UNIDADES U
                      on P.cod_contabilidad = U.cod_contabilidad
                     and P.cod_unidad = U.cod_unidad
                    left join CNTX_CENTRO_COSTOS CC
                      on P.cod_contabilidad = CC.cod_contabilidad
                     and P.cod_centro_costo = CC.cod_centro_costo
                    where P.cod_contabilidad = @codContabilidad
                      and P.cod_cuenta = @cuenta";

                return new CntXCatalogoCuentaDetalleResponse
                {
                    Detalle = detalle,
                    Traducciones = conn.Query<CntXCuentaTraduccionDto>(sqlTraducciones, new { codContabilidad, cuenta }).ToList(),
                    Prorrateos = conn.Query<CntXCuentaProrrataDto>(sqlProrrateos, new { codContabilidad, cuenta }).ToList()
                };
            });
        }

        /// <summary>
        /// Guarda la información adicional de una cuenta.
        /// </summary>
        public ErrorDto<bool> CntXCatalogoDetalleGuardar(int codEmpresa, CntXCatalogoCuentaDetalleGuardarRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var param = new DynamicParameters();
                param.Add("@Contabilidad", request.CodContabilidad);
                param.Add("@Cuenta", request.Cuenta);
                param.Add("@Desc_Alter", request.DescripcionAlterna);
                param.Add("@Ex_Ind", request.ExclusivaIndica ? 1 : 0);
                param.Add("@Ex_Unidad", request.ExclusivaUnidad);
                param.Add("@Ex_Centro", request.ExclusivaCentro);
                param.Add("@Pr_Ind", request.ProrrateaIndica ? 1 : 0);
                param.Add("@Pr_Unidad", request.ProrrateaUnidad);
                param.Add("@Pr_Centro", request.ProrrateaCentro);
                param.Add("@Pr_Total", request.ProrrateaTotal);
                param.Add("@Dc_Ind", request.DcIndica ? 1 : 0);
                param.Add("@Dc_Unidad", request.DcUnidad);
                param.Add("@Dc_Centro", request.DcCentro);
                param.Add("@Dc_Cta_Ing", request.DcCuentaIngreso);
                param.Add("@Dc_Cta_Gst", request.DcCuentaGasto);
                param.Add("@Usuario", request.Usuario);

                conn.Execute("spCntX_Cuenta_Detalle_Guardar", param, commandType: CommandType.StoredProcedure);
                RegistrarBitacora(codEmpresa, request.Usuario, $"Cuentas Info Adicional: {request.Cuenta}", "Registra - WEB");
                return true;
            });
        }

        /// <summary>
        /// Actualiza un indicador de cuenta.
        /// </summary>
        public ErrorDto<bool> CntXCatalogoCuentaEstadoGuardar(int codEmpresa, CntXCatalogoCuentaEstadoRequest request)
        {
            const string sqlAceptaMovimientos = @"
                update CntX_Cuentas
                set acepta_movimientos = @valor
                where cod_contabilidad = @codContabilidad
                  and cod_cuenta = @cuenta";

            const string sqlPresupuesto = @"
                update CntX_Cuentas
                set presupuesto = @valor
                where cod_contabilidad = @codContabilidad
                  and cod_cuenta = @cuenta";

            const string sqlBloqueada = @"
                update CntX_Cuentas
                set bloqueada = @valor
                where cod_contabilidad = @codContabilidad
                  and cod_cuenta = @cuenta";

            const string sqlCuentaAuxiliar = @"
                update CntX_Cuentas
                set cuenta_auxiliar = @valor
                where cod_contabilidad = @codContabilidad
                  and cod_cuenta = @cuenta";

            string? sql = request.Campo?.Trim().ToLowerInvariant() switch
            {
                "acepta_movimientos" => sqlAceptaMovimientos,
                "presupuesto" => sqlPresupuesto,
                "bloqueada" => sqlBloqueada,
                "cuenta_auxiliar" => sqlCuentaAuxiliar,
                _ => null
            };

            if (sql == null)
            {
                return new ErrorDto<bool> { Code = -1, Description = "Campo no permitido.", Result = false };
            }

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                int rows = conn.Execute(sql, new
                {
                    request.CodContabilidad,
                    request.Cuenta,
                    valor = request.Valor ? 1 : 0
                });
                return rows > 0;
            });

            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, request.Usuario, $"Cuenta {request.Cuenta}, campo {request.Campo}", "Modifica - WEB");
            }

            return result;
        }

        /// <summary>
        /// Registra o modifica una cuenta del catÃ¡logo contable usando el procedimiento original.
        /// </summary>
        public ErrorDto<CntXCatalogoCuentaGuardarResponse> CntXCatalogoCuentaGuardar(int codEmpresa, CntXCatalogoCuentaGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<CntXCatalogoCuentaGuardarResponse>("La cuenta es requerida.");
            }

            request.Cuenta = (request.Cuenta ?? string.Empty).Trim();
            request.Descripcion = (request.Descripcion ?? string.Empty).Trim();
            request.CodDivisa = (request.CodDivisa ?? string.Empty).Trim();
            request.TipoCuenta = (request.TipoCuenta ?? string.Empty).Trim();
            request.Usuario = (request.Usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.Cuenta))
            {
                return DbHelper.CreateErrorResponse<CntXCatalogoCuentaGuardarResponse>("La cuenta es requerida.");
            }

            if (string.IsNullOrWhiteSpace(request.Descripcion))
            {
                return DbHelper.CreateErrorResponse<CntXCatalogoCuentaGuardarResponse>("La descripciÃ³n es requerida.");
            }

            if (string.IsNullOrWhiteSpace(request.CodDivisa))
            {
                return DbHelper.CreateErrorResponse<CntXCatalogoCuentaGuardarResponse>("La divisa es requerida.");
            }

            if (string.IsNullOrWhiteSpace(request.TipoCuenta))
            {
                return DbHelper.CreateErrorResponse<CntXCatalogoCuentaGuardarResponse>("El tipo de cuenta es requerido.");
            }

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sql = @"
                    exec spCntX_Cuentas_Registro
                        @CodContabilidad,
                        @Cuenta,
                        @Descripcion,
                        @CodDivisa,
                        @TipoCuenta,
                        @AceptaMovimientos,
                        @Presupuesto,
                        @CuentaAuxiliar,
                        @Bloqueada,
                        'A',
                        @Usuario";

                var result = conn.QueryFirstOrDefault<CntXCatalogoCuentaGuardarResponse>(sql, new
                {
                    request.CodContabilidad,
                    request.Cuenta,
                    request.Descripcion,
                    request.CodDivisa,
                    request.TipoCuenta,
                    AceptaMovimientos = request.AceptaMovimientos ? 1 : 0,
                    Presupuesto = request.Presupuesto ? 1 : 0,
                    CuentaAuxiliar = request.CuentaAuxiliar ? 1 : 0,
                    Bloqueada = request.Bloqueada ? 1 : 0,
                    request.Usuario
                }) ?? new CntXCatalogoCuentaGuardarResponse();

                string cuentaBitacora = string.IsNullOrWhiteSpace(result.Cod_Cuenta_Mask)
                    ? request.Cuenta
                    : result.Cod_Cuenta_Mask;
                string movimiento = string.IsNullOrWhiteSpace(result.Movimiento)
                    ? (request.IsNew ? "Registra - WEB" : "Modifica - WEB")
                    : $"{result.Movimiento} - WEB";

                RegistrarBitacora(codEmpresa, request.Usuario, $"Cuenta en el Catalogo: {cuentaBitacora}", movimiento);

                return result;
            });
        }

        /// <summary>
        /// Ejecuta el mapeo de cuentas.
        /// </summary>
        public ErrorDto<bool> CntXCatalogoMapeo(int codEmpresa, CntXCatalogoMapeoRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var param = new DynamicParameters();
                param.Add("@CntActual", request.CodContabilidad);
                param.Add("@CtaActual", request.CuentaActual);
                param.Add("@CntNew", request.CodContabilidad);
                param.Add("@CtaNew", request.CuentaNueva);
                param.Add("@Usuario", request.Usuario);
                param.Add("@CambioCnf", 1);
                param.Add("@CambioTrn", request.CambiarTransacciones ? 1 : 0);

                conn.Execute("spCntX_Mapeo_Cuentas", param, commandType: CommandType.StoredProcedure);
                RegistrarBitacora(codEmpresa, request.Usuario, $"Mapeo de Cuentas: {request.CuentaActual} -> {request.CuentaNueva}", "Aplicar - WEB");
                return true;
            });
        }

        /// <summary>
        /// Ejecuta la baja de nivel de una cuenta.
        /// </summary>
        public ErrorDto<CntXCatalogoBajaNivelDto> CntXCatalogoBajaNivel(int codEmpresa, CntXCatalogoBajaNivelRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var param = new DynamicParameters();
                param.Add("@Contabilidad", request.CodContabilidad);
                param.Add("@Cuenta", request.Cuenta);
                param.Add("@Usuario", request.Usuario);

                var result = conn.QueryFirstOrDefault<CntXCatalogoBajaNivelDto>(
                    "spCntX_Cuentas_Baja_Nivel",
                    param,
                    commandType: CommandType.StoredProcedure) ?? new CntXCatalogoBajaNivelDto();

                RegistrarBitacora(codEmpresa, request.Usuario, $"Baja Nivel: {request.Cuenta} -> {result.Cuenta}", "Aplicar - WEB");
                return result;
            });
        }

        /// <summary>
        /// Guarda una traducción de cuenta.
        /// </summary>
        public ErrorDto<bool> CntXCatalogoTraduccionGuardar(int codEmpresa, CntXCatalogoTraduccionGuardarRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sqlExiste = @"
                    select count(1)
                    from CNTX_CUENTAS_TRADUCCION
                    where cod_contabilidad = @CodContabilidad
                      and cod_cuenta = @Cuenta
                      and cod_idioma = @CodIdioma";

                int existe = conn.ExecuteScalar<int>(sqlExiste, request);

                if (existe == 0)
                {
                    const string sqlInsert = @"
                        insert into CNTX_CUENTAS_TRADUCCION
                            (cod_idioma, cod_contabilidad, cod_cuenta, descripcion, registro_usuario, registro_fecha)
                        values
                            (@CodIdioma, @CodContabilidad, @Cuenta, @Descripcion, @Usuario, dbo.MyGetdate())";
                    conn.Execute(sqlInsert, request);
                    RegistrarBitacora(codEmpresa, request.Usuario, $"Cta. Traducción: {request.CodIdioma} Conta.{request.CodContabilidad}, Cta: {request.Cuenta}", "Registra - WEB");
                }
                else
                {
                    const string sqlUpdate = @"
                        update CNTX_CUENTAS_TRADUCCION
                        set descripcion = @Descripcion
                        where cod_contabilidad = @CodContabilidad
                          and cod_cuenta = @Cuenta
                          and cod_idioma = @CodIdioma";
                    conn.Execute(sqlUpdate, request);
                    RegistrarBitacora(codEmpresa, request.Usuario, $"Cta. Traducción: {request.CodIdioma} Conta.{request.CodContabilidad}, Cta: {request.Cuenta}", "Modifica - WEB");
                }

                return true;
            });
        }

        /// <summary>
        /// Elimina una traducción de cuenta.
        /// </summary>
        public ErrorDto<bool> CntXCatalogoTraduccionEliminar(int codEmpresa, CntXCatalogoTraduccionGuardarRequest request)
        {
            const string sql = @"
                delete CNTX_CUENTAS_TRADUCCION
                where cod_contabilidad = @CodContabilidad
                  and cod_cuenta = @Cuenta
                  and cod_idioma = @CodIdioma";

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn => conn.Execute(sql, request) > 0);

            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, request.Usuario, $"Cta. Traducción: {request.CodIdioma} Conta.{request.CodContabilidad}, Cta: {request.Cuenta}", "Elimina - WEB");
            }

            return result;
        }

        /// <summary>
        /// Guarda una prorrata de cuenta.
        /// </summary>
        public ErrorDto<bool> CntXCatalogoProrrataGuardar(int codEmpresa, CntXCatalogoProrrataGuardarRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sqlExiste = @"
                    select count(1)
                    from CNTX_CUENTAS_PRORRATA
                    where cod_contabilidad = @CodContabilidad
                      and cod_cuenta = @Cuenta
                      and cod_unidad = @CodUnidad
                      and cod_centro_costo = @CodCentroCosto";

                int existe = conn.ExecuteScalar<int>(sqlExiste, request);

                if (existe == 0)
                {
                    const string sqlInsert = @"
                        insert into CNTX_CUENTAS_PRORRATA
                            (cod_contabilidad, cod_cuenta, cod_unidad, cod_centro_costo, porcentaje, registro_usuario, registro_fecha)
                        values
                            (@CodContabilidad, @Cuenta, @CodUnidad, @CodCentroCosto, @Porcentaje, @Usuario, dbo.MyGetdate())";
                    conn.Execute(sqlInsert, request);
                    RegistrarBitacora(codEmpresa, request.Usuario, $"Cta. Prorrateo: Conta.{request.CodContabilidad}, Cta: {request.Cuenta}, Unidad: {request.CodUnidad}, Centro: {request.CodCentroCosto}", "Registra - WEB");
                }
                else
                {
                    const string sqlUpdate = @"
                        update CNTX_CUENTAS_PRORRATA
                        set porcentaje = @Porcentaje
                        where cod_contabilidad = @CodContabilidad
                          and cod_cuenta = @Cuenta
                          and cod_unidad = @CodUnidad
                          and cod_centro_costo = @CodCentroCosto";
                    conn.Execute(sqlUpdate, request);
                    RegistrarBitacora(codEmpresa, request.Usuario, $"Cta. Prorrateo: Conta.{request.CodContabilidad}, Cta: {request.Cuenta}, Unidad: {request.CodUnidad}, Centro: {request.CodCentroCosto}", "Modifica - WEB");
                }

                return true;
            });
        }

        /// <summary>
        /// Elimina una prorrata de cuenta.
        /// </summary>
        public ErrorDto<bool> CntXCatalogoProrrataEliminar(int codEmpresa, CntXCatalogoProrrataGuardarRequest request)
        {
            const string sql = @"
                delete CNTX_CUENTAS_PRORRATA
                where cod_contabilidad = @CodContabilidad
                  and cod_cuenta = @Cuenta
                  and cod_unidad = @CodUnidad
                  and cod_centro_costo = @CodCentroCosto";

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn => conn.Execute(sql, request) > 0);

            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, request.Usuario, $"Cta. Prorrateo: Conta.{request.CodContabilidad}, Cta: {request.Cuenta}, Unidad: {request.CodUnidad}, Centro: {request.CodCentroCosto}", "Elimina - WEB");
            }

            return result;
        }
    }
}
