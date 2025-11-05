using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_BancosDB
    {
        private readonly IConfiguration? _config;
        mSecurityMainDb DBBitacora;
        mCntLinkDB mCntLink;
        private string dirRDLC = "";

        public frmTES_BancosDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
            mCntLink = new mCntLinkDB(_config);
            dirRDLC = _config.GetSection("AppSettings").GetSection("RutaRDLC").Value.ToString();
        }

        public ErrorDTO Bitacora(BitacoraInsertarDTO data)
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
        public ErrorDTO<TES_BancoDTO> TES_Banco_Obtener(int CodEmpresa, int Contabilidad, int Banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TES_BancoDTO>
            {
                Code = 0,
                Result = new TES_BancoDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select B.*,rtrim(G.Descripcion) as GrupoX
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
                    response.Result = connection.Query<TES_BancoDTO>(query,
                        new
                        {
                            contabilidad = Contabilidad,
                            banco = Banco
                        }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Scroll bancos, navegar al siguiente o anterior id_banco mediante el scrollCode
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Contabilidad"></param>
        /// <param name="scrollCode"></param>
        /// <param name="Banco"></param>
        /// <returns></returns>
        public ErrorDTO<TES_BancoDTO> TES_Bancos_Scroll_Obtener(int CodEmpresa, int Contabilidad, int scrollCode, int Banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TES_BancoDTO>
            {
                Code = 0,
                Result = new TES_BancoDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Top 1 id_banco from Tes_Bancos";

                    if (scrollCode == 1)
                    {
                        query += " where id_banco > @banco order by id_banco asc";
                    }
                    else
                    {
                        query += " where id_banco < @banco order by id_banco desc";
                    }
                    var id_banco = connection.Query<int>(query, new { banco = Banco }).FirstOrDefault();
                    response = TES_Banco_Obtener(CodEmpresa, Contabilidad, id_banco);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener lista de bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<TablasListaGenericaModel> TES_Bancos_Lista_Obtener(int CodEmpresa, string filtro)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro) ?? new FiltrosLazyLoadData();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TablasListaGenericaModel>
            {
                Code = 0,
                Result = new TablasListaGenericaModel()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {


                    var query = $@"select count(id_banco) from Tes_Bancos";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null && filtros.filtro != "")
                    {
                        filtros.filtro = $@" WHERE id_banco like '%{filtros.filtro}%' 
                            OR descripcion like '%{filtros.filtro}%' 
                            OR cta like '%{filtros.filtro}%' ";
                    }

                    if (filtros.pagina != null)
                    {
                        query = $@"select id_banco,descripcion,cta from Tes_Bancos 
                            {filtros.filtro} 
                            ORDER BY id_banco  
                            OFFSET {filtros.pagina} ROWS
                            FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                        response.Result.lista = connection.Query<TES_BancoDTO>(query).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener grupos bancarios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> TES_Bancos_Grupos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select rtrim(cod_grupo) as item,rtrim(Descripcion) as descripcion 
                        from TES_BANCOS_GRUPOS where Activo = 1";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener Divisas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaDivisas>> TES_Bancos_Divisas_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaDivisas>>
            {
                Code = 0,
                Result = new List<DropDownListaDivisas>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spSys_Divisas";
                    response.Result = connection.Query<DropDownListaDivisas>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener formatos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> TES_Bancos_Formatos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select rtrim(cod_Formato) as item,rtrim(Descripcion) as descripcion
                        from vTes_Formatos
                        where Activo = 1";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener unidades de negocio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> TES_Bancos_Unidades_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select rtrim(COD_UNIDAD) AS item, rtrim(DESCRIPCION) AS descripcion
                        From CNTX_UNIDADES
                        where COD_CONTABILIDAD in(select COD_EMPRESA_ENLACE from SIF_EMPRESA)
                        and ACTIVA = 1
                        order by UNIDAD_OMISION desc, DESCRIPCION asc";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener centros de costos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> TES_Bancos_CentrosCostos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select COD_CENTRO_COSTO AS item, RTRIM(DESCRIPCION) AS descripcion
                        From CNTX_CENTRO_COSTOS
                        Where Activo = 1 And COD_CONTABILIDAD = 1";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        /// <summary>
        /// Obtener conceptos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> TES_Bancos_Conceptos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select rtrim(COD_CONCEPTO) AS item, rtrim(DESCRIPCION) AS descripcion
                        From TES_CONCEPTOS
                        where ESTADO = 'A'
                        order by DESCRIPCION";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener cierres
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <returns></returns>
        public ErrorDTO<List<TES_Bancos_Cierres>> TES_Bancos_Cierres_Obtener(int CodEmpresa, int Banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TES_Bancos_Cierres>>
            {
                Code = 0,
                Result = new List<TES_Bancos_Cierres>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Top 30 * from TES_BANCOS_CIERRES where id_banco = @banco order by corte desc";
                    response.Result = connection.Query<TES_Bancos_Cierres>(query,
                        new
                        {
                            banco = Banco
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
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
        public ErrorDTO TES_Bancos_Guardar(int CodEmpresa, bool vEdita, string Usuario, TES_BancoDTO param)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = ""
            };
            var query = "";
            try
            {
                string ctaContable = mCntLink.fxgCntCuentaFormato(CodEmpresa, false, param.cod_cuenta, 0);
                string ctaComisionSINPE = mCntLink.fxgCntCuentaFormato(CodEmpresa, false, param.cod_cuenta_con, 0);
                using var connection = new SqlConnection(stringConn);
                {
                    // --- Validación cuenta bancaria duplicada ---
                    var cuentaBancaria = param.cta?.Trim() ?? "";
                    var idBancoActual = vEdita ? param.id_banco : 0;

                    var queryExiste = @"SELECT ISNULL(COUNT(*),0) 
                                FROM Tes_Bancos 
                                WHERE cta = @cuentaBancaria 
                                  AND Id_Banco != @idBancoActual";

                    int existe = connection.QueryFirst<int>(queryExiste,
                        new { cuentaBancaria, idBancoActual });

                    if (existe > 0)
                    {
                        response.Code = -1;
                        response.Description = "Existe ya un Banco registrado con la misma Cuenta Bancaria.";
                        return response;
                    }

                    if (vEdita)
                    {
                        query = @"update Tes_Bancos set Descripcion = @nombre, Puente = @cuentaBancariaPuente
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
                        connection.Execute(query,
                            new
                            {
                                nombre = param.descripcion.Trim(),
                                cuentaBancariaPuente = param.puente,
                                estado = param.estado,
                                utilizaPlan = param.utiliza_plan,
                                formato = param.formato_transferencia,
                                formatoN2 = param.formato_transferencias_n2,
                                cuentaBancaria = cuentaBancaria,
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
                                con_ComisionSINPECta = ctaComisionSINPE,
                                con_Unidad = param.unidad,
                                con_Centro = param.centro,
                                con_Centro_Comision = param.centro_com,
                                con_Concepto = param.concepto,
                                banco = param.id_banco,
                                ilocalizable = (param.ilocalizable == true) ? 1 : 0,
                                int_grupos_asociados = (param.int_grupos_asociados == true) ? 1 : 0,
                                int_requiere_cuenta_destino = (param.int_requiere_cuenta_destino == true) ? 1 : 0
                            });

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Cuenta Bancaria: " + param.id_banco,
                            Movimiento = "MODIFICA - WEB",
                            Modulo = 9
                        });

                        response.Code = param.id_banco;
                        response.Description = "Información actualizada satisfactoriamente...";
                    }
                    else
                    {
                        query = @"insert Tes_Bancos(descripcion,estado,Utiliza_Plan,formato_transferencia,formato_transferencias_N2,Cta,CtaConta,Desc_Corta
                        ,firmas_desde,firmas_hasta,saldo,fecha_envia,cta_regional,cod_grupo,monitoreo,ARCHIVO_ESPECIAL_CK,puente
                        ,archivo_cheques_firmas,archivo_cheques_sin_firmas,utiliza_formato_especial,lugar_emision
                        ,SUPERVISION,SUPERVISION_DIAS,SINPE_INTERNA,SINPE_EMPRESA, CODIGO_CLIENTE, cod_divisa, UTILIZA_AUTOGESTION
                        ,CONCILIA_AR_COMISION, CONCILIA_AR_COMISION_CTA, CONCILIA_AR_UNIDAD, CONCILIA_AR_CENTRO, CONCILIA_AR_CENTRO_COM, CONCILIA_AR_CONCEPTO, ILOCALIZABLE, 
                        INT_GRUPOS_ASOCIADOS, INT_REQUIERE_CUENTA_DESTINO) 
                        values(@nombre, @estado, @utilizaPlan, @formato, @formatoN2, @cuentaBancaria, @cuentaContable, @descCorta,0,0,0,dbo.MyGetdate()
                        , @regional, @grupo, @monitoreo, @archivoEspecial, @cuentaBancariaPuente, @chequeEspecialFirma, @chequeEspecialNoFirma, @formatoEspecial
                        , @lugarEmision, @supervisa, @dias, @SINPE_CtaInterna, @SINPE_Codigo, @CodigoCliente, @divisa, @autoGestion, @con_ComisionSINPEMnt
                        , @con_ComisionSINPECta, @con_Unidad, @con_Centro, @con_Centro_Comision, @con_Concepto, @ilocalizable, 
                          @int_grupos_asociados, @int_requiere_cuenta_destino)";
                        connection.Execute(query,
                            new
                            {
                                nombre = param.descripcion,
                                estado = param.estado,
                                utilizaPlan = (param.utiliza_plan == true) ? 1 : 0,
                                formato = param.formato_transferencia,
                                formatoN2 = param.formato_transferencias_n2,
                                cuentaBancaria = cuentaBancaria,
                                cuentaContable = ctaContable,
                                descCorta = param.desc_corta.Trim(),
                                regional = (param.cta_regional == true) ? 1 : 0,
                                monitoreo = (param.monitoreo == true) ? 1 : 0,
                                grupo = param.cod_grupo,
                                archivoEspecial = param.archivo_especial_ck ?? "",
                                cuentaBancariaPuente = (param.puente == true) ? 1 : 0,
                                chequeEspecialFirma = param.archivo_cheques_firmas ?? "",
                                chequeEspecialNoFirma = param.archivo_cheques_sin_firmas ?? "",
                                formatoEspecial = (param.utiliza_formato_especial == true) ? 1 : 0,
                                lugarEmision = param.lugar_emision ?? "",
                                supervisa = (param.supervision == true) ? 1 : 0,
                                dias = param.supervision_dias,
                                SINPE_CtaInterna = (param.sinpe_interna == true) ? 1 : 0,
                                SINPE_Codigo = param.sinpe_empresa ?? "",
                                CodigoCliente = param.codigo_cliente ?? "",
                                divisa = param.cod_divisa,
                                autoGestion = (param.utiliza_autogestion == true) ? 1 : 0,
                                con_ComisionSINPEMnt = param.concilia_ar_comision ?? 0,
                                con_ComisionSINPECta = ctaComisionSINPE ?? "",
                                con_Unidad = param.unidad ?? "",
                                con_Centro = param.centro ?? "",
                                con_Centro_Comision = param.centro_com ?? "",
                                con_Concepto = param.concepto ?? "",
                                ilocalizable = (param.ilocalizable == true) ? 1 : 0,
                                int_grupos_asociados = (param.int_grupos_asociados == true) ? 1 : 0,
                                int_requiere_cuenta_destino = (param.int_requiere_cuenta_destino == true) ? 1 : 0
                            });

                        var queryId = "select isnull(max(id_Banco),0) as ultimo from Tes_Bancos";
                        int idBanco = connection.QueryFirst<int>(queryId);

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Cuenta Bancaria: " + idBanco,
                            Movimiento = "REGISTRA - WEB",
                            Modulo = 9
                        });

                        response.Code = idBanco;
                        response.Description = "Información guardada satisfactoriamente...";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }


        /// <summary>
        /// Borra un banco mediante el id_banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDTO TES_Bancos_Borrar(int CodEmpresa, int Banco, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = ""
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "delete Tes_Bancos where id_banco = @banco";
                    connection.Execute(query, new { banco = Banco });

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Banco Cod: " + Banco,
                        Movimiento = "ELIMINA - WEB",
                        Modulo = 9
                    });

                    response.Description = "Banco eliminado correctamente";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
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
        public ErrorDTO TES_Bancos_RangoFirmas_Actualizar(int CodEmpresa, int Banco, int FirmaDesde, int FirmaHasta, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = ""
            };
            try
            {
                if (FirmaDesde > FirmaHasta)
                {
                    response.Code = -1;
                    response.Description = "Verifique el Rango de Firmas";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = "Update Tes_Bancos Set Firmas_Desde = @firmaDesde, Firmas_Hasta= @firmaHasta Where ID_Banco = @banco";
                    connection.Execute(query,
                        new
                        {
                            firmaDesde = FirmaDesde,
                            firmaHasta = FirmaHasta,
                            banco = Banco
                        });

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Firmas Banco = " + Banco + ", " + FirmaDesde + " a " + FirmaHasta,
                        Movimiento = "MODIFICA - WEB",
                        Modulo = 9
                    });

                    response.Description = "Firmas Actualizadas!";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Actualiza el saldo y la fecha de envio de un banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Parametros"></param>
        /// <returns></returns>
        public ErrorDTO TES_Bancos_SaldoFecha_Actualizar(int CodEmpresa, string Parametros)
        {
            Parametros_SaldoFecha param = JsonConvert.DeserializeObject<Parametros_SaldoFecha>(Parametros) ?? new Parametros_SaldoFecha();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = ""
            };
            try
            {
                var FechaEnvia = param.fecha.Date.AddDays(1).AddTicks(-1);
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "Update Tes_Bancos Set Fecha_Envia = @fecha, Saldo = @saldo Where ID_Banco = @banco";
                    connection.Execute(query,
                        new
                        {
                            saldo = param.saldo,
                            fecha = FechaEnvia,
                            banco = param.id_banco
                        });

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = param.usuario.ToUpper(),
                        DetalleMovimiento = "Cta.Id [" + param.id_banco + "] Cta.Desc.: " + param.desc_corta.Trim() + ", Saldo: " + param.saldo,
                        Movimiento = "MODIFICA - WEB",
                        Modulo = 9
                    });

                    response.Description = "Saldo y Fecha Corregidos!";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Actualiza la información de conciliacion de un banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Parametros"></param>
        /// <returns></returns>
        public ErrorDTO TES_Bancos_Conciliacion_Actualizar(int CodEmpresa, string Parametros)
        {
            Parametros_Conciliacion param = JsonConvert.DeserializeObject<Parametros_Conciliacion>(Parametros) ?? new Parametros_Conciliacion();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = ""
            };
            try
            {
                string vSINPECta = mCntLink.fxgCntCuentaFormato(CodEmpresa, false, param.cod_cuenta_con, 0);
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"Update tes_Bancos set CONCILIA_AR_COMISION = @comisionSINPEMnt
                        , CONCILIA_AR_COMISION_CTA = @comisionSINPECta
                        , CONCILIA_AR_UNIDAD = @unidad
                        , CONCILIA_AR_CENTRO = @centro
                        , CONCILIA_AR_CENTRO_COM = @centroComision
                        , CONCILIA_AR_CONCEPTO = @concepto
                        Where Id_Banco = @banco";
                    connection.Execute(query,
                        new
                        {
                            comisionSINPEMnt = param.concilia_ar_comision,
                            comisionSINPECta = vSINPECta,
                            unidad = param.unidad,
                            centro = param.centro,
                            centroComision = param.centro_com,
                            concepto = param.concepto,
                            banco = param.id_banco
                        });
                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = param.usuario.ToUpper(),
                        DetalleMovimiento = "Cta.Id [" + param.id_banco + "] Cta.Desc.: " + param.desc_corta.Trim() + ", Comisión: " + param.concilia_ar_comision,
                        Movimiento = "MODIFICA - WEB",
                        Modulo = 9
                    });

                    response.Description = "Reglas de Conciliacion, Actualizadas!";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }
    
    
        public ErrorDTO<List<TES_Bancos_GruposAsgDTO>> TES_BancosGrupos_Lista(int CodEmpresa, int id_banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TES_Bancos_GruposAsgDTO>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TES_Bancos_GruposAsgDTO>()

            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select B.ID_BANCO, G.cod_grupo, G.descripcion from Tes_Bancos_Grupos G 
                                 left join TES_BANCOS_GRUPOS_ASG B ON B.COD_GRUPO = G.COD_GRUPO AND B.ID_BANCO = @banco
                                 WHERE G.ACTIVO = 1
                                    order by B.ID_BANCO DESC";
                    response.Result = connection.Query<TES_Bancos_GruposAsgDTO>(query, new { banco = id_banco }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }
   
        public ErrorDTO TES_BancosGrupos_Asignar(int CodEmpresa, int id_banco, bool asigna,TES_Bancos_GruposAsgDTO grupo )
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    //limpio registro si existe
                    var query = $@"delete TES_BANCOS_GRUPOS_ASG where ID_BANCO = @banco and COD_GRUPO = @cod_grupo";
                    connection.Execute(query, new { banco = id_banco, cod_grupo = grupo.cod_grupo });

                    if(asigna)
                    {
                        //inserto el registro
                        query = $@"insert into TES_BANCOS_GRUPOS_ASG (ID_BANCO, COD_GRUPO) values (@banco, @cod_grupo)";
                        connection.Execute(query, new { banco = id_banco, cod_grupo = grupo.cod_grupo });
                        response.Description = "Grupo Asignado Correctamente!";
                    }
                    else
                    {
                        response.Description = "Grupo Des-asignado Correctamente!";
                    }
                   
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        // Change the return type of the method from ErrorDTO to Task<ErrorDTO>
        public async Task<ErrorDTO> TES_BancosArchivos_Subir(
          int codEmpresa,
          int codBanco,
          string documento,
          IFormFile file)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            ErrorDTO error = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {

                string docNameOld = "";
                using (var connection = new SqlConnection(stringConn))
                {
                    // Obtener el nombre anterior
                    var query = $@"SELECT {documento} FROM Tes_Bancos WHERE id_banco = {codBanco}";
                    docNameOld = connection.QueryFirstOrDefault<string>(query);
                }

                // Eliminar archivo anterior
                if (!string.IsNullOrEmpty(docNameOld))
                {
                    var pathOld = Path.Combine(dirRDLC, codEmpresa.ToString(), "Bancos", $"{codBanco}_{docNameOld}");
                    if (File.Exists(pathOld))
                    {
                        File.Delete(pathOld);
                    }
                }

                // 🔹 Definir el nuevo nombre
                var nuevoNombre = $"{codBanco}_{file.FileName}";

                // Ruta destino
                var basePath = Path.Combine(dirRDLC, codEmpresa.ToString(), "Bancos");
                Directory.CreateDirectory(basePath);

                var destino = Path.Combine(basePath, nuevoNombre);

                // Guardar archivo
                using (var stream = System.IO.File.Create(destino))
                {
                    await file.CopyToAsync(stream); // ✅ usar await
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;
        }

        // Resuelve qué archivo devolver (SIN exponerlo al cliente)
        public ErrorDTO<ArchivoDTO> ResolverDocumento(int codEmpresa, int codBanco, string documento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var error = new ErrorDTO<ArchivoDTO> { Code = 0, Description = "Ok" };

            // 1) Validar columna para evitar inyección
            var columna = documento switch
            {
                "archivo_especial_ck" => "archivo_especial_ck",
                "archivo_cheques_firmas" => "archivo_cheques_firmas",
                "archivo_cheques_sin_firmas" => "archivo_cheques_sin_firmas",
                _ => null
            };
            if (columna is null) return null;

            string docNameOld = "";
            // 2) leer nombre guardado en BD con Dapper (parametriza el id)
            using var connection = new SqlConnection(stringConn);
            {
                //Elimina registro anterior:
                var Query = $@"SELECT {documento} FROM Tes_Bancos WHERE id_banco = {codBanco}";
                docNameOld = connection.QueryFirstOrDefault<string>(Query);
            }

            // 2) Armar ruta primaria o defaults
            string ruta = !string.IsNullOrWhiteSpace(docNameOld)
                ? Path.Combine(dirRDLC, codEmpresa.ToString(), "Bancos", $"{codBanco}_{docNameOld}")
                : columna switch
                {
                    "archivo_especial_ck" => Path.Combine(dirRDLC, "Banking_DocFormat.rdl"),
                    "archivo_cheques_firmas" => Path.Combine(dirRDLC, "Banking_DocFormat01.rdl"),
                    _ => Path.Combine(dirRDLC, "Banking_DocFormat02.rdl"),
                };

            // 3) Fallback a carpeta de empresa si la ruta no existe
            if (!File.Exists(ruta))
            {
                ruta = columna switch
                {
                    "archivo_especial_ck" => Path.Combine(dirRDLC, codEmpresa.ToString(), "Bancos", "Banking_DocFormat.rdl"),
                    "archivo_cheques_firmas" => Path.Combine(dirRDLC, codEmpresa.ToString(), "Bancos", "Banking_DocFormat01.rdl"),
                    _ => Path.Combine(dirRDLC, codEmpresa.ToString(), "Bancos", "Banking_DocFormat02.rdl"),
                };
                if (!File.Exists(ruta)) return null;
            }

            // 4) Nombre “bonito” (sin prefijo CodBanco_)
            var fileName = Path.GetFileName(ruta);
            var prefix = codBanco + "_";
            if (fileName.StartsWith(prefix, StringComparison.Ordinal))
                fileName = fileName[(fileName.IndexOf('_') + 1)..];

            var bytes = System.IO.File.ReadAllBytes(ruta);
            error.Result = new ArchivoDTO
            {
                FileName = fileName,
                ContentType = "application/octet-stream",
                FileContentsBase64 = Convert.ToBase64String(bytes)
            };

            return error;
        }

    }
}