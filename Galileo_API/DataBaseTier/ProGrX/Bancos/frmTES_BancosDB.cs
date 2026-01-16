using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text.RegularExpressions;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesBancosDB
    {

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;
        private readonly MCntLinkDB mCntLink;
        private readonly string dirRDLC;
        private readonly int vModulo = 9; // Módulo de Tesorería
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);
        private readonly string vBanco = "Bancos";

        public FrmTesBancosDB(IConfiguration config)
        {
            DBBitacora = new MSecurityMainDb(config);
            mCntLink = new MCntLinkDB(config);
            _portalDB = new PortalDB(config);
            dirRDLC = config.GetSection("AppSettings").GetSection("RutaRDLC").Value!.ToString();
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtener informacion de un banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Contabilidad"></param>
        /// <param name="Banco"></param>
        /// <returns></returns>
        public ErrorDto<TesBancoDto> TES_Banco_Obtener(int CodEmpresa, int Contabilidad, int Banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select B.*,rtrim(G.Descripcion) as GrupoX
                    , dbo.fxTes_Formatos_Desc(B.Formato_Transferencia) as 'FormatoN1'
                    , dbo.fxTes_Formatos_Desc(B.Formato_Transferencias_N2) as 'FormatoN2'
                    , Dv.Descripcion as 'DivisaDesc'
                    , isnull(Cb.Cod_Cuenta_Mask,'') as 'COD_CUENTA', isnull(Cb.Descripcion,'') as 'COD_CUENTA_DESC'
                    , isnull(Cc.Cod_Cuenta_Mask,'') as 'COD_CUENTA_CON', isnull(Cc.Descripcion,'') as 'COD_CUENTA_CON_DESC'
                    , ISNULL(Ud.COD_UNIDAD,'') AS 'UNIDAD', ISNULL(Ud.DESCRIPCION,'') AS 'UNIDAD_DESC'
                    , ISNULL(Ccr.COD_CENTRO_COSTO,'') AS 'CENTRO', ISNULL(Ccr.DESCRIPCION,'') AS 'CENTRO_DESC'
                    , ISNULL(Cct.COD_CENTRO_COSTO,'') AS 'CENTRO_COM', ISNULL(Cct.DESCRIPCION,'') AS 'CENTRO_COM_DESC'
                    , ISNULL(Tc.COD_CONCEPTO,'') AS 'CONCEPTO', ISNULL(Tc.DESCRIPCION,'') AS 'CONCEPTO_DESC'
                     from Tes_Bancos B left join TES_BANCOS_GRUPOS G on B.cod_Grupo = G.cod_Grupo
                     left join CntX_Divisas Dv on B.cod_divisa = Dv.Cod_Divisa and Dv.cod_Contabilidad = @contabilidad
                     left join vCNTX_CUENTAS_LOCAL Cb on B.ctaConta = Cb.Cod_Cuenta
                     left join vCNTX_CUENTAS_LOCAL Cc on B.CONCILIA_AR_COMISION_CTA = Cc.Cod_Cuenta
                     left join CNTX_UNIDADES Ud on B.CONCILIA_AR_UNIDAD = Ud.COD_UNIDAD AND Ud.COD_CONTABILIDAD =  @contabilidad
                     left join CntX_Centro_Costos Ccr on B.CONCILIA_AR_CENTRO = Ccr.Cod_Centro_Costo AND Ccr.COD_CONTABILIDAD =  @contabilidad
                     left join CntX_Centro_Costos Cct on B.CONCILIA_AR_CENTRO_COM = Cct.Cod_Centro_Costo AND Cct.COD_CONTABILIDAD =  @contabilidad
                     left join TES_CONCEPTOS Tc on B.CONCILIA_AR_CONCEPTO = Tc.COD_CONCEPTO
                     where B.id_Banco = @banco";

                return conn.Query<TesBancoDto>(query,
                        new
                        {
                            contabilidad = Contabilidad,
                            banco = Banco
                        }).FirstOrDefault() ?? new TesBancoDto();
            });
        }

        /// <summary>
        /// Scroll bancos, navegar al siguiente o anterior id_banco mediante el scrollCode
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Contabilidad"></param>
        /// <param name="scrollCode"></param>
        /// <param name="Banco"></param>
        /// <returns></returns>
        public ErrorDto<TesBancoDto> TES_Bancos_Scroll_Obtener(int CodEmpresa, int Contabilidad, int scrollCode, int Banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                // scrollCode: 1 = siguiente (mayor), 2 = anterior (menor)

                const string query = @"
                    SELECT TOP 1 id_banco
                    FROM Tes_Bancos
                    WHERE
                          (@scroll = 1 AND id_banco > @banco)
                       OR (@scroll <> 1 AND id_banco < @banco)
                    ORDER BY
                        CASE WHEN @scroll = 1 THEN id_banco END ASC,
                        CASE WHEN @scroll <> 1 THEN id_banco END DESC;";
                var id_banco = conn.Query<int>(query, new { scroll = scrollCode, banco = Banco }).FirstOrDefault();
                // Si no hay siguiente/anterior, devuelvo el mismo banco (o podrías devolver vacío / error según tu regla)
                var bancoObjetivo = id_banco != 0 ? id_banco : Banco;

                return TES_Banco_Obtener(CodEmpresa, Contabilidad, bancoObjetivo);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesBancoDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtener lista de bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_Bancos_Lista_Obtener(int CodEmpresa, string filtro)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro) ?? new FiltrosLazyLoadData();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<TesBancoDto>()
                }
            };

            try
            {
                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                
                var offset = filtros!.pagina;
                var fetch = filtros!.paginacion;
                var usarPaginacion = fetch > 0;

                const string sqlCount = @"
                        SELECT COUNT(1)
                        FROM Tes_Bancos
                        WHERE
                            (@filtro IS NULL)
                         OR (CAST(id_banco AS NVARCHAR(50)) LIKE @like)
                         OR (descripcion LIKE @like)
                         OR (cta LIKE @like);";

                response.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    filtro = hasFiltro ? texto : null,
                    like
                });

                var sqlList = @"
                        SELECT id_banco, descripcion, cta
                        FROM Tes_Bancos
                        WHERE
                            (@filtro IS NULL)
                         OR (CAST(id_banco AS NVARCHAR(50)) LIKE @like)
                         OR (descripcion LIKE @like)
                         OR (cta LIKE @like)
                        ORDER BY id_banco ";

                if (usarPaginacion)
                {
                    sqlList += @"
                        OFFSET @offset ROWS
                        FETCH NEXT @fetch ROWS ONLY;";
                }

                response.Result.lista = conn.Query<TesBancoDto>(sqlList, new
                {
                    filtro = hasFiltro ? texto : null,
                    like,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Obtener grupos bancarios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Grupos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select rtrim(cod_grupo) as item,rtrim(Descripcion) as descripcion 
                        from TES_BANCOS_GRUPOS where Activo = 1";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Obtener Divisas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaDivisas>> TES_Bancos_Divisas_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"exec spSys_Divisas";

                return conn.Query<DropDownListaDivisas>(query).ToList();
            });
        }

        /// <summary>
        /// Obtener formatos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Formatos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select rtrim(cod_Formato) as item,rtrim(Descripcion) as descripcion
                        from vTes_Formatos
                        where Activo = 1";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Obtener unidades de negocio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Unidades_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select rtrim(COD_UNIDAD) AS item, rtrim(DESCRIPCION) AS descripcion
                        From CNTX_UNIDADES
                        where COD_CONTABILIDAD in(select COD_EMPRESA_ENLACE from SIF_EMPRESA)
                        and ACTIVA = 1
                        order by UNIDAD_OMISION desc, DESCRIPCION asc";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Obtener centros de costos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_CentrosCostos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select COD_CENTRO_COSTO AS item, RTRIM(DESCRIPCION) AS descripcion
                        From CNTX_CENTRO_COSTOS
                        Where Activo = 1 And COD_CONTABILIDAD = 1";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }


        /// <summary>
        /// Obtener conceptos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Conceptos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select rtrim(COD_CONCEPTO) AS item, rtrim(DESCRIPCION) AS descripcion
                        From TES_CONCEPTOS
                        where ESTADO = 'A'
                        order by DESCRIPCION";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Obtener cierres
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <returns></returns>
        public ErrorDto<List<TesBancosCierres>> TES_Bancos_Cierres_Obtener(int CodEmpresa, int Banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select Top 30 * from TES_BANCOS_CIERRES where id_banco = @banco order by corte desc";

                return conn.Query<TesBancosCierres>(query, new
                {
                    banco = Banco
                }).ToList();
            });
        }


        /// <summary>
        /// Agrega o actualiza la información de un banco según corresponda, 
        /// mediante vEdita se valida si corresponda a una actualizacion de datos o agregar un registro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vEdita"></param>
        /// <param name="Usuario"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ErrorDto TES_Bancos_Guardar(int CodEmpresa, bool vEdita, string Usuario, TesBancoDto param)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                // 1) Validación duplicado
                if (CuentaBancariaDuplicada(conn, param.cod_cuenta, vEdita ? param.id_banco : 0))
                {
                    return DbHelper.ErrorResponse("Existe ya un Banco registrado con la misma Cuenta Bancaria.");
                }

                // 2) Cuentas contables formateadas (una sola vez)
                string ctaContable = mCntLink.fxgCntCuentaFormato(CodEmpresa, false, param.cod_cuenta, 0);
                string ctaComisionSINPE = mCntLink.fxgCntCuentaFormato(CodEmpresa, false, param.cod_cuenta_con, 0);

                // 3) Parametrización única
                var sqlParams = BuildSqlParams(param, ctaContable, ctaComisionSINPE);

                // 4) Insert/Update
                int idBanco = vEdita
                    ? UpdateBanco(conn, param.id_banco, sqlParams)
                    : InsertBanco(conn, sqlParams);

                // 4) Insert/Update
                string msj = vEdita
                    ? "Información actualizada satisfactoriamente..."
                    : "Información guardada satisfactoriamente...";

                // 5) Bitácora
                RegistrarBitacora(CodEmpresa, Usuario, idBanco, vEdita);

                return DbHelper.OkResponse(msj);
            }
            catch (Exception ex)
            {
               return DbHelper.ErrorResponse(ex.Message);   
            }
            
        }

        private static bool CuentaBancariaDuplicada(SqlConnection conn, string cuentaBancaria, int idBancoActual)
        {
            string SqlExisteCuenta = @"SELECT ISNULL(COUNT(*),0) 
                                FROM Tes_Bancos 
                                WHERE cta = @cuentaBancaria 
                                  AND Id_Banco != @idBancoActual";
            var existe = conn.QueryFirst<int>(SqlExisteCuenta, new { cuentaBancaria, idBancoActual });
            return existe > 0;
        }

        private static object BuildSqlParams(
           TesBancoDto param,
            string ctaContable,
            string ctaComisionSinpe)
        {
            return new
            {
                nombre = param.descripcion.Trim(),
                cuentaBancariaPuente = param.puente,
                estado = param.estado,
                utilizaPlan = param.utiliza_plan,
                formato = param.formato_transferencia,
                formatoN2 = param.formato_transferencias_n2,
                cuentaBancaria = param.cod_cuenta,
                cuentaContable = ctaContable,
                descCorta = param.desc_corta,
                regional = param.cta_regional,
                monitoreo = param.monitoreo,
                grupo = param.cod_grupo,
                archivoEspecial = param.archivo_especial_ck,
                chequeEspecialFirma = param.archivo_cheques_firmas,
                chequeEspecialNoFirma = param.archivo_cheques_sin_firmas,
                formatoEspecial = param.utiliza_formato_especial,
                lugarEmision = param.lugar_emision,
                supervisa = param.supervision,
                dias = param.supervision_dias,
                SINPE_CtaInterna = param.sinpe_interna,
                SINPE_Codigo = param.sinpe_empresa,
                codigoCliente = param.codigo_cliente,
                divisa = param.cod_divisa,
                autoGestion = param.utiliza_autogestion,
                con_ComisionSINPEMnt = param.concilia_ar_comision,
                con_ComisionSINPECta = ctaComisionSinpe,
                con_Unidad = param.unidad,
                con_Centro = param.centro,
                con_Centro_Comision = param.centro_com,
                con_Concepto = param.concepto,
                banco = param.id_banco,
                ilocalizable = param.ilocalizable == true ? 1 : 0,
                int_grupos_asociados = param.int_grupos_asociados == true ? 1 : 0,
                int_requiere_cuenta_destino = param.int_requiere_cuenta_destino == true ? 1 : 0
            };
        }

        private static int UpdateBanco(SqlConnection conn, int idBanco, object sqlParams)
        {
            string SqlUpdateBanco =  @"update Tes_Bancos set Descripcion = @nombre, Puente = @cuentaBancariaPuente
                        ,estado = @estado, Utiliza_Plan = @utilizaPlan, formato_transferencia = @formato
                        ,formato_transferencias_N2 = @formatoN2, cta = @cuentaBancaria, CtaConta = @cuentaContable
                        ,Desc_Corta = @descCorta, cta_regional = @regional, monitoreo = @monitoreo, cod_grupo = @grupo
                        ,Archivo_Especial_CK = @archivoEspecial, archivo_cheques_firmas = @chequeEspecialFirma
                        ,archivo_cheques_sin_firmas = @chequeEspecialNoFirma, utiliza_formato_especial = @formatoEspecial
                        ,Lugar_Emision = @lugarEmision, SUPERVISION = @supervisa, SUPERVISION_DIAS = @dias
                        ,SINPE_INTERNA = @SINPE_CtaInterna, SINPE_EMPRESA = @SINPE_Codigo, CODIGO_CLIENTE = @codigoCliente
                        ,cod_divisa = @divisa, UTILIZA_AUTOGESTION = @autoGestion, CONCILIA_AR_COMISION = @con_ComisionSINPEMnt
                        , CONCILIA_AR_COMISION_CTA = @con_ComisionSINPECta, CONCILIA_AR_UNIDAD = @con_Unidad
                        , CONCILIA_AR_CENTRO = @con_Centro, CONCILIA_AR_CENTRO_COM = @con_Centro_Comision, CONCILIA_AR_CONCEPTO = @con_Concepto
                        , ILOCALIZABLE = @ilocalizable , INT_GRUPOS_ASOCIADOS = @int_grupos_asociados, INT_REQUIERE_CUENTA_DESTINO = @int_requiere_cuenta_destino
                        Where Id_Banco = @banco";
            conn.Execute(SqlUpdateBanco, sqlParams);
            return idBanco;
        }

        private static int InsertBanco(SqlConnection conn, object sqlParams)
        {
            string SqlInsertBanco = @"INSERT Tes_Bancos
                                        (
                                            descripcion,estado,Utiliza_Plan,formato_transferencia,formato_transferencias_N2,
                                            Cta,CtaConta,Desc_Corta,firmas_desde,firmas_hasta,saldo,fecha_envia,
                                            cta_regional,cod_grupo,monitoreo,ARCHIVO_ESPECIAL_CK,puente,
                                            archivo_cheques_firmas,archivo_cheques_sin_firmas,utiliza_formato_especial,lugar_emision,
                                            SUPERVISION,SUPERVISION_DIAS,SINPE_INTERNA,SINPE_EMPRESA,CODIGO_CLIENTE,cod_divisa,UTILIZA_AUTOGESTION,
                                            CONCILIA_AR_COMISION,CONCILIA_AR_COMISION_CTA,CONCILIA_AR_UNIDAD,CONCILIA_AR_CENTRO,CONCILIA_AR_CENTRO_COM,CONCILIA_AR_CONCEPTO,
                                            ILOCALIZABLE, INT_GRUPOS_ASOCIADOS, INT_REQUIERE_CUENTA_DESTINO
                                        )
                                        VALUES
                                        (
                                            @nombre, @estado, @utilizaPlan, @formato, @formatoN2,
                                            @cuentaBancaria, @cuentaContable, @descCorta, 0, 0, 0, dbo.MyGetdate(),
                                            @regional, @grupo, @monitoreo, @archivoEspecial, @cuentaBancariaPuente,
                                            @chequeEspecialFirma, @chequeEspecialNoFirma, @formatoEspecial, @lugarEmision,
                                            @supervisa, @dias, @SINPE_CtaInterna, @SINPE_Codigo, @codigoCliente, @divisa, @autoGestion,
                                            @con_ComisionSINPEMnt, @con_ComisionSINPECta, @con_Unidad, @con_Centro, @con_Centro_Comision, @con_Concepto,
                                            @ilocalizable, @int_grupos_asociados, @int_requiere_cuenta_destino
                                        );

                                        SELECT CAST(SCOPE_IDENTITY() AS int);";
            return conn.QueryFirst<int>(SqlInsertBanco, sqlParams);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, int idBanco, bool edita)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                DetalleMovimiento = $"Cuenta Bancaria: {idBanco}",
                Movimiento = edita ? "MODIFICA-WEB" : "REGISTRA-WEB",
                Modulo = 9
            });
        }

        /// <summary>
        /// Borra un banco mediante el id_banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto TES_Bancos_Borrar(int CodEmpresa, int Banco, string Usuario)
        {
            const string query = "delete Tes_Bancos where id_banco = @banco";
            var parametros = new
            {
                banco = Banco
            };
            var response = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, parametros);

            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = Usuario.ToUpper(),
                DetalleMovimiento = "Banco Cod: " + Banco,
                Movimiento = "ELIMINA - WEB",
                Modulo = vModulo
            });

            if(response.Code == -1)
            {
                return DbHelper.ErrorResponse(response.Description!);
            }

            return DbHelper.OkResponse("Banco eliminado correctamente");
        }


        /// <summary>
        /// Actualiza los rangos de firmas de un banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <param name="FirmaDesde"></param>
        /// <param name="FirmaHasta"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto TES_Bancos_RangoFirmas_Actualizar(int CodEmpresa, int Banco, int FirmaDesde, int FirmaHasta, string Usuario)
        {
            if (FirmaDesde > FirmaHasta)
            {
                return DbHelper.ErrorResponse("El valor de Firma Desde no puede ser mayor al de Firma Hasta");
            }

            const string query = "Update Tes_Bancos Set Firmas_Desde = @firmaDesde, Firmas_Hasta= @firmaHasta Where ID_Banco = @banco";
            var parametros = new
            {
                firmaDesde = FirmaDesde,
                firmaHasta = FirmaHasta,
                banco = Banco
            };
            var response = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, parametros);

            if (response.Code == -1)
            {
                return DbHelper.ErrorResponse(response.Description!);
            }

            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = Usuario.ToUpper(),
                DetalleMovimiento = "Firmas Banco = " + Banco + ", " + FirmaDesde + " a " + FirmaHasta,
                Movimiento = "MODIFICA - WEB",
                Modulo = vModulo
            });

           

            return DbHelper.OkResponse("Firmas Actualizadas!");
        }

        /// <summary>
        /// Actualiza el saldo y la fecha de envio de un banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Parametros"></param>
        /// <returns></returns>
        public ErrorDto TES_Bancos_SaldoFecha_Actualizar(int CodEmpresa, string Parametros)
        {
            ParametrosSaldoFecha param = JsonConvert.DeserializeObject<ParametrosSaldoFecha>(Parametros) ?? new ParametrosSaldoFecha();

            const string query = "Update Tes_Bancos Set Fecha_Envia = @fecha, Saldo = @saldo Where ID_Banco = @banco";

            var parametros = new
            {
                param.saldo,
                fecha = param.fecha.Date.AddDays(1).AddTicks(-1),
                banco = param.id_banco
            };

            var response = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, parametros);

            if (response.Code == -1)
            {
                return DbHelper.ErrorResponse(response.Description!);
            }

            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = param.usuario.ToUpper(),
                DetalleMovimiento = "Cta.Id [" + param.id_banco + "] Cta.Desc.: " + param.desc_corta.Trim() + ", Saldo: " + param.saldo,
                Movimiento = "MODIFICA - WEB",
                Modulo = vModulo
            });

            return DbHelper.OkResponse("Saldo y Fecha Corregidos!"); 
        }

        /// <summary>
        /// Actualiza la información de conciliacion de un banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Parametros"></param>
        /// <returns></returns>
        public ErrorDto TES_Bancos_Conciliacion_Actualizar(int CodEmpresa, string Parametros)
        {
            ParametrosConciliacion param = JsonConvert.DeserializeObject<ParametrosConciliacion>(Parametros) ?? new ParametrosConciliacion();

            const string query = @"Update tes_Bancos set CONCILIA_AR_COMISION = @comisionSINPEMnt
                        , CONCILIA_AR_COMISION_CTA = @comisionSINPECta
                        , CONCILIA_AR_UNIDAD = @unidad
                        , CONCILIA_AR_CENTRO = @centro
                        , CONCILIA_AR_CENTRO_COM = @centroComision
                        , CONCILIA_AR_CONCEPTO = @concepto
                        Where Id_Banco = @banco";

            string vSINPECta = mCntLink.fxgCntCuentaFormato(CodEmpresa, false, param.cod_cuenta_con, 0);

            var parametros = 
            new
            {
                comisionSINPEMnt = param.concilia_ar_comision,
                comisionSINPECta = vSINPECta,
                param.unidad,
                param.centro,
                centroComision = param.centro_com,
                param.concepto,
                banco = param.id_banco
            };


            var response = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, parametros);

            if (response.Code == -1)
            {
                return DbHelper.ErrorResponse(response.Description!);
            }

            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = param.usuario.ToUpper(),
                DetalleMovimiento = "Cta.Id [" + param.id_banco + "] Cta.Desc.: " + param.desc_corta.Trim() + ", Comisión: " + param.concilia_ar_comision,
                Movimiento = "MODIFICA - WEB",
                Modulo = 9
            });

            return DbHelper.OkResponse("Reglas de Conciliacion, Actualizadas!");
        }
    
    
        public ErrorDto<List<TesBancosGruposAsgDto>> TES_BancosGrupos_Lista(int CodEmpresa, int id_banco)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select B.ID_BANCO, G.cod_grupo, G.descripcion from Tes_Bancos_Grupos G 
                                 left join TES_BANCOS_GRUPOS_ASG B ON B.COD_GRUPO = G.COD_GRUPO AND B.ID_BANCO = @banco
                                 WHERE G.ACTIVO = 1
                                    order by B.ID_BANCO DESC";

                return conn.Query<TesBancosGruposAsgDto>(query, new { banco = id_banco }).ToList();
            });
        }
   
        public ErrorDto TES_BancosGrupos_Asignar(int CodEmpresa, int id_banco, bool asigna,TesBancosGruposAsgDto grupo )
        {
             using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                string msj = "";
                //limpio registro si existe
                var query = $@"delete TES_BANCOS_GRUPOS_ASG where ID_BANCO = @banco and COD_GRUPO = @cod_grupo";
                conn.Execute(query, new { banco = id_banco, grupo.cod_grupo });

                if (asigna)
                {
                    //inserto el registro
                    query = $@"insert into TES_BANCOS_GRUPOS_ASG (ID_BANCO, COD_GRUPO) values (@banco, @cod_grupo)";
                    conn.Execute(query, new { banco = id_banco, grupo.cod_grupo });
                    msj = "Grupo Asignado Correctamente!";
                }
                else
                {
                    msj = "Grupo Des-asignado Correctamente!";
                }

                return DbHelper.OkResponse(msj);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        // Change the return type of the method from ErrorDto to Task<ErrorDto>
        public async Task<ErrorDto> TES_BancosArchivos_Subir(
    int codEmpresa,
    int codBanco,
    string documento,
    IFormFile file)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            string ext = string.Empty;
            string col = string.Empty;
            var validacion = Validasiones(file, documento, ref ext, ref col);
            if(validacion.Code == -1)
                return validacion;

            // 1) Paths controlados
            var baseDir = Path.GetFullPath(Path.Combine(dirRDLC, codEmpresa.ToString(), vBanco));
            Directory.CreateDirectory(baseDir);

            var nuevoNombre = $"{codBanco}_{col}{ext}";
            var destino = Path.GetFullPath(Path.Combine(baseDir, nuevoNombre));

            if (!destino.StartsWith(baseDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return DbHelper.ErrorResponse("Ruta de destino inválida.");

            // 2) Guardar archivo nuevo primero
            try
            {
                await using (var stream = new FileStream(destino, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                // BD no tocada, así que solo reportas
                return DbHelper.ErrorResponse($"Error guardando archivo: {ex.Message}");
            }

            try
            {
                // 3) Actualizar BD (SP) + obtener anterior
                var p = new DynamicParameters();
                p.Add("@IdBanco", codBanco);
                p.Add("@DocumentoCol", col);
                p.Add("@NuevoNombre", nuevoNombre);
                p.Add("@NombreAnterior", dbType: DbType.String, size: 260, direction: ParameterDirection.Output);

                await conn.ExecuteAsync(
                    "dbo.TES_BancosArchivos_PrepararYActualizar",
                    p,
                    commandType: CommandType.StoredProcedure);

                var docNameOld = (p.Get<string>("@NombreAnterior") ?? string.Empty).Trim();

                // 4) Borrar anterior (best effort, sin tumbar el proceso)
                if (!string.IsNullOrWhiteSpace(docNameOld) &&
                    !string.Equals(docNameOld, nuevoNombre, StringComparison.OrdinalIgnoreCase))
                {
                    var oldNameOnly = Path.GetFileName(docNameOld);
                    var pathOld = Path.GetFullPath(Path.Combine(baseDir, oldNameOnly));

                    if (pathOld.StartsWith(baseDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                        File.Exists(pathOld))
                    {
                        File.Delete(pathOld);
                    }
                }

                return DbHelper.OkResponse("Archivo subido correctamente.");
            }
            catch (Exception ex)
            {
                // 5) Rollback manual: si BD falló, borra el nuevo para no dejar basura
                try
                {
                    if (File.Exists(destino))
                        File.Delete(destino);
                }
                catch
                {
                    // Aquí normalmente logueas, pero no tapes el error principal
                }

                return DbHelper.ErrorResponse($"Error actualizando BD: {ex.Message}");
            }
        }

        private static ErrorDto Validasiones(IFormFile file, string documento, ref string ext, ref string col)
        {
             // 0) Validaciones
            if (file == null || file.Length <= 0)
                return DbHelper.ErrorResponse("No se recibió un archivo válido.");

            ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".rdl" && ext != ".rdlc")
                return DbHelper.ErrorResponse("Extensión inválida. Solo .rdl/.rdlc.");

            col = (documento ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(col) ||
                !Regex.IsMatch(col, @"^[A-Za-z0-9_]+$", RegexOptions.None, RegexTimeout))
                return DbHelper.ErrorResponse("Nombre de columna inválido.");

            return DbHelper.CreateOkResponse();
        }


        // Resuelve qué archivo devolver (SIN exponerlo al cliente)
        public ErrorDto<ArchivoDto> ResolverDocumento(int codEmpresa, int codBanco, string documento)
        {
             using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            var error = new ErrorDto<ArchivoDto> { Code = 0, Description = "Ok" };



            string docNameOld = "";

            var Query = $@"SELECT ";
            Query += documento;
            Query += " FROM Tes_Bancos WHERE id_banco = @codBanco;";
            docNameOld = conn.QueryFirstOrDefault<string>(Query, new { codBanco }) ?? string.Empty;

            // 2) Armar ruta primaria o defaults
            string ruta = !string.IsNullOrWhiteSpace(docNameOld)
                ? Path.Combine(dirRDLC, codEmpresa.ToString(), vBanco, $"{codBanco}_{docNameOld}")
                : documento switch
                {
                    "archivo_especial_ck" => Path.Combine(dirRDLC, "Banking_DocFormat.rdl"),
                    "archivo_cheques_firmas" => Path.Combine(dirRDLC, "Banking_DocFormat01.rdl"),
                    _ => Path.Combine(dirRDLC, "Banking_DocFormat02.rdl"),
                };

            // 3) Fallback a carpeta de empresa si la ruta no existe
            if (!File.Exists(ruta))
            {
                ruta = documento switch
                {
                    "archivo_especial_ck" => Path.Combine(dirRDLC, codEmpresa.ToString(), vBanco, "Banking_DocFormat.rdl"),
                    "archivo_cheques_firmas" => Path.Combine(dirRDLC, codEmpresa.ToString(), vBanco, "Banking_DocFormat01.rdl"),
                    _ => Path.Combine(dirRDLC, codEmpresa.ToString(), vBanco, "Banking_DocFormat02.rdl"),
                };
                if (!File.Exists(ruta)) return new ErrorDto<ArchivoDto>();
            }

            // 4) Nombre “bonito” (sin prefijo CodBanco_)
            var fileName = Path.GetFileName(ruta);
            var prefix = codBanco + "_";
            if (fileName.StartsWith(prefix, StringComparison.Ordinal))
                fileName = fileName[(fileName.IndexOf('_') + 1)..];

            var bytes = File.ReadAllBytes(ruta);
            error.Result = new ArchivoDto
            {
                FileName = fileName,
                ContentType = "application/octet-stream",
                FileContentsBase64 = Convert.ToBase64String(bytes)
            };

            return error;
        }

    }
}