using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;


namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneficiosTrasladoDB
    {
        private readonly IConfiguration _config;
        mSecurityMainDb DBBitacora;
        mBeneficiosDB mBeneficiosDB;
        mTESFuncionesDB mTESFuncionesDB;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private mProGrX_AuxiliarDB mAuxiliarDB;
        private readonly mTesoreria _mtes;
        public string sendEmail = "";
        public string TestMail = "";
        public string Notificaciones = "";
        public string CodComision = "";
        public string CtaComision = "";

        public frmAF_BeneficiosTrasladoDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
            mBeneficiosDB = new mBeneficiosDB(_config);
            mTESFuncionesDB = new mTESFuncionesDB(_config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            mAuxiliarDB = new mProGrX_AuxiliarDB(_config);
            _mtes = new mTesoreria(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            TestMail = _config.GetSection("AppSettings").GetSection("TestEmail").Value.ToString();
            Notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value.ToString();
            CodComision = _config.GetSection("AFI_Beneficios").GetSection("CodComision").Value.ToString();
            CtaComision = _config.GetSection("AFI_Beneficios").GetSection("CtaComisionBeneficios").Value.ToString();
        }

        public ErrorDTO Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        #region Remesas

        public ErrorDTO<AfiBeneficiosRemesasDTOLista> AfiRemesas_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<AfiBeneficiosRemesasDTOLista>
            {
                Result = new AfiBeneficiosRemesasDTOLista(),
                Code = 0
            };

            try
            {
                AfiRemesasFiltros? vfiltro = null;
                if (!string.IsNullOrWhiteSpace(filtros))
                {
                    try { vfiltro = JsonConvert.DeserializeObject<AfiRemesasFiltros>(filtros); }
                    catch { /* si el JSON viene mal, lo tratamos como sin filtros */ }
                }

                string where = "";
                string paginaActual = "";
                string paginacionActual = "";
                var p = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(vfiltro?.filtro))
                {
                    where = @"
                WHERE  cod_remesa LIKE '%' + @filtro + '%'
                   OR  usuario    LIKE '%' + @filtro + '%'
                   OR  estado     LIKE '%' + @filtro + '%'
                   OR  CONVERT(VARCHAR(19), fecha, 120) LIKE '%' + @filtro + '%'";
                    p.Add("@filtro", vfiltro!.filtro);
                }

                if (vfiltro?.pagina.HasValue == true && vfiltro?.paginacion.HasValue == true)
                {
                    paginaActual = " OFFSET @offset ROWS ";
                    paginacionActual = " FETCH NEXT @fetch ROWS ONLY ";
                    p.Add("@offset", vfiltro.pagina!.Value);
                    p.Add("@fetch", vfiltro.paginacion!.Value);
                }

                using var connection = new SqlConnection(clienteConnString);

                // Total con el mismo WHERE
                var countSql = $"SELECT COUNT(*) FROM AFI_BENEFICIOS_REMESAS {where}";
                response.Result.Total = connection.Query<int>(countSql, p).FirstOrDefault();

                // Datos paginados
                var dataSql = $@"
                            SELECT *
                            FROM AFI_BENEFICIOS_REMESAS
                            {where}
                            ORDER BY fecha DESC
                            {paginaActual} {paginacionActual}";

                response.Result.Beneficios = connection.Query<AfiBeneficiosRemesasDTO>(dataSql, p).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                if (response.Result == null) response.Result = new AfiBeneficiosRemesasDTOLista();
                response.Result.Total = 0;
                response.Result.Beneficios = null;
            }

            return response;
        }

        // public ErrorDTO<AfiBeneficiosRemesasDTOLista> AfiRemesas_Obtener(int CodCliente, string? filtro, int? pagina, int? paginacion)
        // {
        //     var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
        //     var response = new ErrorDTO<AfiBeneficiosRemesasDTOLista>
        //     {
        //         Result = new AfiBeneficiosRemesasDTOLista()
        //     };
        //     string paginaActual = " ", paginacionActual = " ", vfiltro = " ";
        //     try
        //     {
        //         var query = "";
        //         using var connection = new SqlConnection(clienteConnString);
        //         {
        //             //Busco Total
        //             query = "SELECT COUNT(*) FROM AFI_BENEFICIOS_REMESAS";
        //             response.Result.Total = connection.Query<int>(query).FirstOrDefault();

        //             if (filtro != null)
        //             {
        //                 vfiltro = " WHERE cod_remesa LIKE '%" + filtro + "%' OR usuario LIKE '%" + filtro + "%' " +
        //                     "OR fecha LIKE '%" + filtro + "%' OR estado LIKE '%" + filtro + "%' ";
        //             }

        //             if (pagina != null)
        //             {
        //                 paginaActual = " OFFSET " + pagina + " ROWS ";
        //                 paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
        //             }

        //             query = $@"select * from AFI_BENEFICIOS_REMESAS {vfiltro} order by fecha desc {paginaActual} {paginacionActual}";

        //             response.Result.Beneficios = connection.Query<AfiBeneficiosRemesasDTO>(query).ToList();

        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         response.Code = -1;
        //         response.Description = ex.Message;
        //         response.Result.Total = 0;
        //     }
        //     return response;
        // }

        /// <summary>
        /// Obtiene la remesa por codigo
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_remesa"></param>
        /// <returns></returns>
        public ErrorDTO<AfiBeneficiosRemesasDTO> AfiRemesa_Obtener(int CodCliente, int cod_remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<AfiBeneficiosRemesasDTO>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select * from AFI_BENEFICIOS_REMESAS where Cod_Remesa = {cod_remesa}";

                    response.Result = connection.Query<AfiBeneficiosRemesasDTO>(query).FirstOrDefault();
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
        /// Inserta una nueva remesa
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDTO AfiRemesa_Insertar(int CodCliente, AfiBeneficiosRemesasDTO remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select isnull(max(cod_remesa),0) + 1 as Ultimo from AFI_BENEFICIOS_REMESAS";

                    var ultimo = connection.Query<int>(query).FirstOrDefault();

                    string fecha_inicio = mAuxiliarDB.validaFechaGlobal(remesa.fecha_inicio);
                    string fecha_corte = mAuxiliarDB.validaFechaGlobal(remesa.fecha_corte);

                    query = $@"insert into AFI_BENEFICIOS_REMESAS (cod_remesa,usuario,fecha,estado,fecha_inicio,fecha_corte,notas)
                                values ({ultimo}, '{remesa.usuario}', Getdate(),'A', '{fecha_inicio}', '{fecha_corte}', '{remesa.notas}' )";

                    var resp = connection.Execute(query);

                    if (resp > 0)
                    {
                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodCliente,
                            Usuario = remesa.usuario.ToUpper(),
                            DetalleMovimiento = $"Registra, Remesa de Beneficios Traslado a Tesoreria: {remesa.cod_remesa} ",
                            Movimiento = "REGISTRA - WEB",
                            Modulo = 7
                        });
                    }
                    else
                    {
                        info.Code = -1;
                        info.Description = "Error al actualizar el registro";
                    }
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Actualiza una remesa
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="remesa"></param>
        /// <returns></returns>
        public ErrorDTO AfiRemesa_Actualizar(int CodCliente, AfiBeneficiosRemesasDTO remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    DateTime fecha_inicioActual = (DateTime)remesa.fecha_inicio;
                    string fecha_inicio = mAuxiliarDB.validaFechaGlobal(remesa.fecha_inicio);

                    DateTime fecha_corteActual = (DateTime)remesa.fecha_corte;
                    string fecha_corte = mAuxiliarDB.validaFechaGlobal(remesa.fecha_corte);

                    query = $@"update AFI_BENEFICIOS_REMESAS set usuario = '{remesa.usuario}', 
                                fecha_inicio = '{fecha_inicio}', 
                                fecha_corte = '{fecha_corte}', notas = '{remesa.notas}' where cod_remesa = {remesa.cod_remesa}";

                    var resp = connection.Execute(query);

                    if (resp > 0)
                    {
                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodCliente,
                            Usuario = remesa.usuario.ToUpper(),
                            DetalleMovimiento = $"Modifica, Remesa de Beneficios Traslado a Tesoreria: {remesa.cod_remesa} ",
                            Movimiento = "Modifica - WEB",
                            Modulo = 7
                        });
                    }
                    else
                    {
                        info.Code = -1;
                        info.Description = "Error al actualizar el registro";
                    }
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Elimina una remesa
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_remesa"></param>
        /// <returns></returns>
        public ErrorDTO AfiRemesa_Eliminar(int CodCliente, long cod_remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = "update  afi_bene_otorga set COD_REMESA = NULL where Cod_Remesa = " + cod_remesa;
                    var resp = connection.Execute(query);

                    query = "delete from AFI_BENEFICIOS_REMESAS where Cod_Remesa = " + cod_remesa;
                    resp = connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDTO<List<AfiBeneTrasladoOpciones>> AfiRemesaOficinasFechas_Obtener(int CodCliente, string inicio, string corte)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneTrasladoOpciones>>
            {
                Result = []
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select rtrim(cod_oficina) as 'item', rtrim(descripcion) as 'descripcion'
		                            from SIF_Oficinas  where cod_oficina in(
		                            select R.cod_oficina_R
		                            from reg_creditos R inner join Catalogo C on R.codigo = C.codigo and C.retencion = 'N' and C.poliza = 'N'
		                            where R.estadosol='F' 
		                        	and R.fechaforp between '{inicio.Replace("-", "/")} 00:00:00' and '{corte.Replace("-", "/")} 23:59:59'
		                            and R.tesoreria is null and R.estado in('A','C')
			                        --and id_solicitud not in(select id_solicitud from AFI_BENEFICIOS_REMESAS_DETALLE)
		                            group by R.cod_oficina_R)
		                            order by cod_oficina";

                    response.Result = connection.Query<AfiBeneTrasladoOpciones>(query).ToList();

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

        public ErrorDTO<List<AfiBeneTrasladoOpciones>> AfiRemesaOficinas_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneTrasladoOpciones>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select rtrim(cod_oficina) as 'item', rtrim(descripcion) as 'descripcion'
                                 from SIF_Oficinas order by cod_oficina";

                    response.Result = connection.Query<AfiBeneTrasladoOpciones>(query).ToList();

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

        #endregion

        #region Cargas

        /// <summary>
        /// Carga lista de bancos
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="inicio"></param>
        /// <param name="corte"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfiBeneTrasladoOpciones>> CargarBancos_Obtener(int CodCliente, string inicio, string corte)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneTrasladoOpciones>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"Select B.cod_Banco as 'item',TB.descripcion as 'descripcion'
			                          from afi_bene_pago B
			                          inner join tes_bancos TB on B.cod_banco = TB.id_banco 
			                          inner join afi_bene_otorga O on B.cod_beneficio = O.cod_beneficio
			                          and B.consec = O.consec  
			                        -- and registra_fecha between '{inicio} 00:00:00' and '{corte} 23:59:59'
			                          where B.ESTADO = 'S' and B.tesoreria is null group by B.cod_Banco,TB.descripcion ";

                    response.Result = connection.Query<AfiBeneTrasladoOpciones>(query).ToList();

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
        /// Carga lista de usuarios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="inicio"></param>
        /// <param name="corte"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfiBeneTrasladoOpciones>> CargarUsuarios_Obtener(int CodCliente, string inicio, string corte)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneTrasladoOpciones>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"Select distinct O.Registra_User as 'item',O.Registra_User as 'descripcion'
			                              from afi_bene_pago B
			                              inner join tes_bancos TB on B.cod_banco = TB.id_banco 
			                              inner join afi_bene_otorga O on B.cod_beneficio = O.cod_beneficio
			                              and B.consec = O.consec 
			                        -- and registra_fecha between '{inicio} 00:00:00' and '{corte} 23:59:59'
			                          where B.ESTADO = 'S' and B.tesoreria is null ";

                    response.Result = connection.Query<AfiBeneTrasladoOpciones>(query).ToList();

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

        public ErrorDTO<List<AfiBeneTrasladoOpciones>> CargarBeneficios_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneTrasladoOpciones>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select B.cod_beneficio as 'item',TB.descripcion as 'descripcion'
			                          from afi_bene_pago B
			                          inner join afi_beneficios TB on B.cod_beneficio = TB.cod_beneficio 
			                          inner join afi_bene_otorga O on B.cod_beneficio = O.cod_beneficio
			                          and B.consec = O.consec  
			                          where B.ESTADO = 'S' and B.tesoreria is null group by B.cod_beneficio,TB.descripcion ";

                    response.Result = connection.Query<AfiBeneTrasladoOpciones>(query).ToList();

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
        /// Obtiene las cargas de los beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<AfiBeneficiosCargasDataLista> BusquedaCargas_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<AfiBeneficiosCargasDataLista>
            {
                Result = new AfiBeneficiosCargasDataLista()
            };
            AfiFiltrosCargas filtro = JsonConvert.DeserializeObject<AfiFiltrosCargas>(filtros) ?? new AfiFiltrosCargas();
            BeneficioGeneralDatos beneficio = new();
            var respon = new ErrorDTO();
            bool bSueprvisar = true;
            string paginaActual = " ", paginacionActual = " ";
            try
            {

                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    var queryCantidad = $@"SELECT COUNT(B.CEDULA) from afi_bene_pago B inner join socios S on B.cedula = S.cedula 
                        inner join afi_bene_otorga O on B.cod_beneficio = O.cod_beneficio and B.consec = O.consec 
                        inner join Afi_Estados_Persona E on S.EstadoActual = E.Cod_Estado 
                        inner join Tes_Bancos Ban on B.cod_Banco = Ban.id_Banco 
                        INNER JOIN AFI_BENEFICIOS A ON B.cod_beneficio = A.cod_beneficio
                        where O.cod_remesa is null and O.ESTADO in (select COD_ESTADO from AFI_BENE_ESTADOS where P_FINALIZA = 1 and PROCESO = 'A')";
                    response.Result.Total = connection.Query<int>(queryCantidad).FirstOrDefault();

                    if (filtro.cod_remesa > 0)
                    {
                        query = $@"select fecha_inicio,fecha_corte from AFI_BENEFICIOS_REMESAS where cod_remesa = {filtro.cod_remesa} ";
                    }
                    else
                    {
                        query = $@"select TOP 1 fecha_inicio,fecha_corte from AFI_BENEFICIOS_REMESAS ORDER BY FECHA_INICIO DESC ";
                    }
                    var fechas = connection.Query<AfiBeneficiosRemesasDTO>(query).FirstOrDefault();

                    if (filtro.cod_banco != "" && filtro.extraFiltros == true)
                    {
                        bSueprvisar = fxSupervisaBanco(CodCliente, filtro.cod_banco);
                    }

                    string fecha_inicio = mAuxiliarDB.validaFechaGlobal(fechas.fecha_inicio);

                    string fecha_corte = mAuxiliarDB.validaFechaGlobal(fechas.fecha_corte);
                    ;

                    if (bSueprvisar)
                    {
                        query = $@"Select B.*,S.Nombre,E.Descripcion as 'EstadoPersona',Ban.Descripcion as 'BancoDesc', O.id_beneficio,
				                 dbo.fxTesSupervisa(B.cedula,S.nombre,B.monto,0,'C') as 'Duplicado', O.REGISTRA_FECHA 
                                  ,(select DESCRIPCION from AFI_BENEFICIOS where COD_BENEFICIO = B.COD_BENEFICIO) AS BENEFICIO_DESC ,
                                    A.COD_CATEGORIA
				                  from afi_bene_pago B inner join socios S on B.cedula = S.cedula
				                  inner join afi_bene_otorga O on B.cod_beneficio = O.cod_beneficio and B.consec = O.consec
				                  inner join Afi_Estados_Persona E on S.EstadoActual = E.Cod_Estado
				                   inner join Tes_Bancos Ban on B.cod_Banco = Ban.id_Banco
                                    INNER JOIN AFI_BENEFICIOS A ON B.cod_beneficio = A.cod_beneficio
				                  where B.cod_remesa is null 
				                 and B.registro_fecha between '{fecha_inicio.Split(' ')[0]} 00:00:00' and '{fecha_corte.Split(' ')[0]} 23:59:59'
				                    and B.ESTADO = 'S' and B.tesoreria is null 
                                and O.ESTADO in (select COD_ESTADO from AFI_BENE_ESTADOS where P_FINALIZA = 1 and PROCESO = 'A')";
                    }
                    else
                    {
                        query = $@"Select B.*,S.Nombre,E.Descripcion as 'EstadoPersona',Ban.Descripcion as 'BancoDesc', O.id_beneficio, O.REGISTRA_FECHA 
                                    ,(select DESCRIPCION from AFI_BENEFICIOS where COD_BENEFICIO = B.COD_BENEFICIO) AS BENEFICIO_DESC,
                                           A.COD_CATEGORIA                                     
                                    from afi_bene_pago B inner join socios S on B.cedula = S.cedula 
                                     inner join afi_bene_otorga O on B.cod_beneficio = O.cod_beneficio and B.consec = O.consec
                                     inner join Afi_Estados_Persona E on S.EstadoActual = E.Cod_Estado
                                     inner join Tes_Bancos Ban on B.cod_Banco = Ban.id_Banco
                                        INNER JOIN AFI_BENEFICIOS A ON B.cod_beneficio = A.cod_beneficio
                                     where B.cod_remesa is null 
                                     and B.registro_fecha between '{fecha_inicio.Split(' ')[0]} 00:00:00' and '{fecha_corte.Split(' ')[0]} 23:59:59'
                                       and B.ESTADO = 'S' and B.tesoreria is null 
                                and O.ESTADO in (select COD_ESTADO from AFI_BENE_ESTADOS where P_FINALIZA = 1 and PROCESO = 'A')";

                    }

                    if (mBeneficiosDB.fxSIFParametros(CodCliente, "16") == "S")
                    {
                        query += " and O.Analista_Revision = 'S' ";
                    }

                    if (filtro.cod_oficina != "")
                    {
                        query = query + " and O.cod_oficina = '" + filtro.cod_oficina + "' ";
                    }

                    if (filtro.extraFiltros)
                    {
                        if (filtro.cod_banco != "")
                        {
                            query = query + " and B.cod_banco = '" + filtro.cod_banco + "' ";
                        }

                        if (filtro.usuario != "")
                        {
                            query = query + " and B.Registro_usuario = '" + filtro.usuario + "' ";
                        }

                        if (filtro.cod_beneficio != "")
                        {
                            query = query + " and B.cod_beneficio = '" + filtro.cod_beneficio + "' ";
                        }
                    }

                    if (filtro.vfiltro != null && filtro.vfiltro != "")
                    {
                        query = query + $@" AND ( B.cedula LIKE '%{filtro.vfiltro} %' OR 
                                            B.cta_Bancaria LIKE '%{filtro.vfiltro}%' OR 
                                            S.Nombre LIKE '%{filtro.vfiltro}%' OR 
                                            Ban.Descripcion LIKE '%{filtro.vfiltro}%' OR 
                                            B.cod_Banco LIKE '%{filtro.vfiltro}%' OR 
                                            O.cod_beneficio like '%{filtro.vfiltro}%' OR 
                                            O.id_beneficio like '%{filtro.vfiltro}%' OR 
                                            O.consec like '%{filtro.vfiltro}%' ) AND 
                                        dbo.fxTesSupervisa(B.cedula,S.nombre,B.monto,0,'C') != 1";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = query + " order by B.cedula" + paginaActual + paginacionActual;

                    response.Result.Beneficios = connection.Query<AfiBeneficiosCargasData>(query).ToList();

                    foreach (var item in response.Result.Beneficios)
                    {
                        AfBeneficioIntegralDropsLista estado = new();
                        AfBeneficioIntegralDropsLista cod_beneficio = new();
                        estado.item = item.Estado;
                        cod_beneficio.item = item.Cod_Beneficio;
                        beneficio.estado = estado;
                        beneficio.cedula = item.Cedula;
                        beneficio.monto_aplicado = item.Monto;
                        beneficio.registra_user = filtro.registro_usuario;
                        beneficio.cod_beneficio = cod_beneficio;
                        //respon = mBeneficiosDB.ValidarPersonaPago(CodCliente, item.Cedula, beneficio.cod_beneficio.item);
                        //if (respon.Description.Trim() != "" && respon.Description != null)
                        //{
                        //    item.Valida_Beneficio = respon.Description;
                        //}

                        respon = mBeneficiosDB.ValidaCargaPagos(CodCliente, beneficio);
                        if (respon.Description.Trim() != "" && respon.Description != null)
                        {
                            item.Valida_Beneficio = respon.Description;
                        }

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

        private bool fxSupervisaBanco(int CodCliente, string cod_banco)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            bool resp = false;
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@" Select isnull(SUPERVISION,0) as 'SUPERVISION' from tes_bancos where id_banco = '{cod_banco}' ";
                    resp = connection.Query<bool>(query).FirstOrDefault();
                }

            }
            catch (Exception)
            {
                resp = false;
            }
            return resp;
        }

        /// <summary>
        /// Aplicar remesa a los beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="carga"></param>
        /// <returns></returns>
        public ErrorDTO CargaCarga_Aplicar(int CodCliente, string carga)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            int count = 0;
            try
            {
                string query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    AfiCargasAplicar infoCarga = JsonConvert.DeserializeObject<AfiCargasAplicar>(carga);
                    //Valida el Estado de la Remesa
                    query = $@"select count(*) as Existe from AFI_BENEFICIOS_REMESAS
		                            where cod_remesa =  {infoCarga.cod_remesa}
		                            and estado = 'A'";
                    var existe = connection.Query<int>(query).FirstOrDefault();
                    if (existe == 0)
                    {
                        info.Code = -1;
                        info.Description = "La Remesa actual; ya se encuentra cerrada...";
                        return info;
                    }


                    //Calcula los casos a procesar

                    if (infoCarga.casos.Count == 0)
                    {
                        info.Code = 1;
                        info.Description = "No se han seleccionado casos para procesar";
                        return info;
                    }

                    //Valida duplicacion de pago
                    foreach (var item in infoCarga.casos)
                    {
                        query = $@"SELECT COUNT(*) 
                        FROM afi_bene_pago 
                        WHERE consec = {item.consec} 
                          AND cod_beneficio = '{item.cod_beneficio}' 
                          AND estado = 'E' 
                          AND tesoreria IS NOT NULL 
                          AND ENVIO_FECHA BETWEEN 
                              (SELECT FECHA_INICIO FROM AFI_BENEFICIOS_REMESAS WHERE COD_REMESA = {infoCarga.cod_remesa}) 
                              AND 
                              (SELECT FECHA_CORTE FROM AFI_BENEFICIOS_REMESAS WHERE COD_REMESA = {infoCarga.cod_remesa})";
                        existe = connection.Query<int>(query).FirstOrDefault();
                        if (existe > 0)
                        {
                            query = $@"SELECT 
                            CONCAT(RIGHT(CONCAT('00000', ID_BENEFICIO), 5),TRIM(COD_BENEFICIO) ,RIGHT(CONCAT('00000',CONSEC), 5))  AS Expediente 
                            FROM AFI_BENE_OTORGA 
                            WHERE consec = {item.consec} 
                              AND cod_beneficio = '{item.cod_beneficio}' ";
                            string expediente = connection.Query<string>(query).FirstOrDefault();

                            info.Code = -1;
                            info.Description = "Ya se realiz� el pago del beneficio con el expediente: " + expediente;
                            return info;
                        }
                    }

                    foreach (var item in infoCarga.casos)
                    {
                        query = $@"update afi_bene_otorga set cod_remesa = {infoCarga.cod_remesa}
				                       where consec = {item.consec} 
				                       and cod_beneficio = '{item.cod_beneficio}'";

                        var resp = connection.Execute(query);

                        query = $@"update afi_bene_pago set cod_remesa = {infoCarga.cod_remesa}
				                       where consec = {item.consec} 
				                       and cod_beneficio = '{item.cod_beneficio}'";
                        connection.Execute(query);

                        if (item.justificacion != null)
                        {
                            query = $@"update afi_bene_pago set justificacion = '{item.justificacion}' 
				                       where consec = {item.consec} 
				                       and cod_beneficio = '{item.cod_beneficio}'";
                            connection.Execute(query);
                        }

                        if (resp > 0)
                        {
                            //todo correcto
                        }
                        else
                        {
                            count++;
                            info.Code = -1;
                            info.Description = "Error al actualizar el registro";
                        }
                    }

                    if (count == 0)
                    {
                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodCliente,
                            Usuario = infoCarga.usuario.ToUpper(),
                            DetalleMovimiento = $"Carga Remesa Traslado a Tesoreria: {infoCarga.cod_remesa} ",
                            Movimiento = "Aplica - WEB",
                            Modulo = 7
                        });
                    }

                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Cerrar remesa de los beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_remesa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO CargasCarga_Cerrar(int CodCliente, string cod_remesa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Valida el Estado de la Remesa
                    query = $@"select count(*) as Existe from AFI_BENEFICIOS_REMESAS
                                    where cod_remesa =  {cod_remesa}
                                    and estado = 'A'";

                    var existe = connection.Query<int>(query).FirstOrDefault();
                    if (existe == 0)
                    {
                        info.Code = -1;
                        info.Description = "La Remesa actual; ya se encuentra cerrada...";
                        return info;
                    }

                    //Actualiza el Estado de la Remesa como cerrada
                    query = $@"update AFI_BENEFICIOS_REMESAS set estado = 'C' where cod_remesa = {cod_remesa}";

                    var resp = connection.Execute(query);

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodCliente,
                        Usuario = usuario.ToUpper(),
                        DetalleMovimiento = $"Cierra Remesa Traslado a Tesoreria: {cod_remesa} ",
                        Movimiento = "Aplica - WEB",
                        Modulo = 7
                    });

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDTO<List<AfiBeneficiosRemesasDTO>> AfiCargasRemesas_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneficiosRemesasDTO>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select *, CONCAT(COD_REMESA, USUARIO, FECHA, FECHA_INICIO, FECHA_CORTE) AS DESCRIPCION
                        from AFI_BENEFICIOS_REMESAS where estado = 'A' order by fecha desc";

                    response.Result = connection.Query<AfiBeneficiosRemesasDTO>(query).ToList();
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

        #endregion

        #region Trasladar
        /// <summary>
        /// Obtener lista de traslados
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfiBeneficiosRemesasDTO>> AfiTraslados_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneficiosRemesasDTO>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select *, CONCAT(COD_REMESA, USUARIO, FECHA, FECHA_INICIO, FECHA_CORTE) AS DESCRIPCION
                        from AFI_BENEFICIOS_REMESAS where estado = 'C' order by fecha desc";

                    response.Result = connection.Query<AfiBeneficiosRemesasDTO>(query).ToList();
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
        /// Obtener el traslado con el beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<AfiBeneficiosCargasDataLista> AfiTraslado_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<AfiBeneficiosCargasDataLista>();
            response.Result = new AfiBeneficiosCargasDataLista();
            AfiBeneficiosTrasladoDTO filtro = JsonConvert.DeserializeObject<AfiBeneficiosTrasladoDTO>(filtros);
            string paginaActual = " ", paginacionActual = " ";
            try
            {
                var query = "";
                var vfiltro = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    var queryCantidad = $@"Select COUNT(B.Cedula) 
                        from afi_bene_pago B inner join socios S on B.cedula = S.cedula
                        inner join afi_bene_otorga O on B.cod_beneficio = O.cod_beneficio and B.consec = O.consec
                        inner join Afi_Estados_Persona E on S.EstadoActual = E.Cod_Estado 
                        inner join Tes_Bancos Ban on B.cod_Banco = Ban.id_Banco 
                        Where O.cod_remesa = {filtro.cod_remesa}
                        and O.registra_fecha between '{filtro.fecha_inicio}' and '{filtro.fecha_corte}'
			            and O.ESTADO in (select COD_ESTADO from AFI_BENE_ESTADOS where P_FINALIZA = 1 and PROCESO = 'A') 
                        and B.tesoreria is null";
                    response.Result.Total = connection.Query<int>(queryCantidad).FirstOrDefault();

                    //string fInicio = filtro.fecha_inicio.Replace("-", "/").Replace("T00:00:00", "");
                    //string fCorte = filtro.fecha_corte.Replace("-", "/").Replace("T00:00:00", "");

                    if (filtro.vfiltro != null && filtro.vfiltro != "")
                    {
                        vfiltro = " AND B.cedula LIKE '" + filtro.vfiltro + "%' OR B.cta_Bancaria LIKE '" + filtro.vfiltro + "%' " +
                            "OR O.Nombre LIKE '" + filtro.vfiltro + "%' OR Ban.Descripcion LIKE '" + filtro.vfiltro + "%' ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = "ORDER BY B.cod_Beneficio OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"Select B.*,S.Nombre,E.Descripcion as 'EstadoPersona',Ban.Descripcion as 'BancoDesc', O.cod_remesa, O.registra_fecha, O.ID_BENEFICIO 
                                  ,(select DESCRIPCION from AFI_BENEFICIOS where COD_BENEFICIO = B.COD_BENEFICIO) AS BENEFICIO_DESC, B.id_pago 
			                      from afi_bene_pago B inner join socios S on B.cedula = S.cedula
			                      inner join afi_bene_otorga O on B.cod_beneficio = O.cod_beneficio and B.consec = O.consec
			                      inner join Afi_Estados_Persona E on S.EstadoActual = E.Cod_Estado
			                      inner join Tes_Bancos Ban on B.cod_Banco = Ban.id_Banco
			                      Where O.cod_remesa = {filtro.cod_remesa}
			                        and B.registro_fecha between '{filtro.fecha_inicio}' and '{filtro.fecha_corte}'
			                        and O.ESTADO in (select COD_ESTADO from AFI_BENE_ESTADOS where P_FINALIZA = 1 and PROCESO = 'A') 
                                    and B.tesoreria is null
                                {vfiltro}
                                {paginaActual} {paginacionActual}
                            ";

                    response.Result.Beneficios = connection.Query<AfiBeneficiosCargasData>(query).ToList();

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
        /// Aplicar traslado a los beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="traslado"></param>
        /// <returns></returns>
        public async Task<ErrorDTO> AfiTraslado_Aplicar(int CodCliente, string traslado)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    AfiTrasladoAplicar infoCarga = JsonConvert.DeserializeObject<AfiTrasladoAplicar>(traslado);
                    //Valida el Estado de la Remesa
                    string vToken;
                    if (infoCarga.token == null)
                    {
                        query = $@"select top 1 id_token from tes_tokens where estado = 'A' order by registro_fecha";
                        var existe = connection.Query<string>(query).FirstOrDefault();
                       
                        if (existe == null)
                        {
                            vToken = mTESFuncionesDB.fxTesToken(CodCliente, infoCarga.usuario);
                        }
                        else
                        {
                            vToken = existe;
                        }
                    }
                    else
                    {
                        vToken = infoCarga.token;
                    }



                    query = $@"select VALOR from SIF_PARAMETROS where COD_PARAMETRO = '{CodComision}'";
                    float valorComision = float.Parse(connection.Query<string>(query).FirstOrDefault());

                    query = $@"select VALOR from SIF_PARAMETROS where COD_PARAMETRO = 'AFIBENE'";
                    string valorConcepto = connection.Query<string>(query).FirstOrDefault();



                    query = $@"Select VALOR from SIF_PARAMETROS where COD_PARAMETRO = '{CtaComision}'";
                    string vCtaComision = connection.Query<string>(query).FirstOrDefault();


                    foreach (AfiBeneTrasladoAplciar item in infoCarga.casos)
                    {
                        query = $@"select CTACONTA from TES_BANCOS where ID_BANCO  = {item.cod_banco}";
                        string vCtaConcepto = connection.Query<string>(query).FirstOrDefault();

                        query = $@"select descripcion,cod_cuenta  from afi_beneficios where cod_beneficio =  '{item.cod_beneficio}' ";

                        AfiBeneficiosTraslado beneficio = connection.Query<AfiBeneficiosTraslado>(query).FirstOrDefault();
                        string vCtaBene = beneficio.cod_cuenta;
                        string vDetalle = item.cod_beneficio;
                        string vDetalle2 = beneficio.descripcion;

                        var vTesoreria = mTESFuncionesDB.fxgTesoreriaMaestro(CodCliente, infoCarga.usuario, new TesoreriaMaestroModel
                        {

                            vTipoDocumento = item.tipo_emision,
                            vBanco = item.cod_banco,
                            vMonto = item.monto,
                            vBeneficiario = item.nombre,
                            vCodigo = item.cedula,
                            vOP = 0,
                            vDetalle1 = vDetalle,
                            vReferencia = 0,
                            vDetalle2 = vDetalle2,
                            vCuenta = item.cta_bancaria,
                            vConcepto = valorConcepto,
                            vUnidad = "OC",
                            vFecha = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day,
                            vRemesa = infoCarga.cod_remesa,
                            vRemesaTipo = "BEN",
                            vCodApp = "ProGrX-Web"

                        });

                        //Actualiza estado en afi_bene_pago
                        query = $@"Update afi_bene_pago set estado = 'E',tesoreria = {vTesoreria},
					                  envio_user = '{infoCarga.usuario}',envio_fecha = Getdate()
					                  ,ID_TOKEN = '{vToken}', cod_remesa = {infoCarga.cod_remesa} 
					                   where cedula = '{item.cedula}' and id_pago = {item.id_pago} 
					                   and cod_beneficio = '{item.cod_beneficio}' and consec = '{item.consec}'";

                        var resp = connection.Execute(query);

                        //Actualiza el estado en tabla afi_bene_otorga
                        query = $@"SELECT COALESCE((
                                SELECT COD_REMESA
                                FROM afi_bene_otorga
                                WHERE cedula = '{item.cedula}'
					               and cod_beneficio = '{item.cod_beneficio}' 
                                   and consec = '{item.consec}'
                            ), 0) AS COD_REMESA;";
                        int existeRemesa = connection.Query<int>(query).FirstOrDefault();
                        if (existeRemesa == 0)
                        {
                            query = $@"Update afi_bene_otorga set estado = 'A',autoriza_user = '{infoCarga.usuario}',
					                      autoriza_fecha = Getdate(),cod_remesa = {infoCarga.cod_remesa}  where cedula = '{item.cedula}'
					                       and cod_beneficio = '{item.cod_beneficio}' and consec = '{item.consec}'";

                            resp = connection.Execute(query);
                        }

                        //Detalle de tesoreria
                        if (item.cod_banco != 58 && infoCarga.aplicaComision == true)
                        {
                            mTESFuncionesDB.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
                            {
                                vSolicitud = vTesoreria,
                                vCtaConta = vCtaConcepto,
                                vMonto = item.monto - valorComision,
                                vDH = "H",
                                vLinea = 1
                            });

                            mTESFuncionesDB.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
                            {
                                vSolicitud = vTesoreria,
                                vCtaConta = vCtaComision,
                                vMonto = valorComision,
                                vDH = "H",
                                vLinea = 2
                            });

                            mTESFuncionesDB.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
                            {
                                vSolicitud = vTesoreria,
                                vCtaConta = vCtaBene,
                                vMonto = item.monto,
                                vDH = "D",
                                vLinea = 3
                            });
                        }
                        else
                        {
                            mTESFuncionesDB.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
                            {
                                vSolicitud = vTesoreria,
                                vCtaConta = vCtaConcepto,
                                vMonto = item.monto,
                                vDH = "H",
                                vLinea = 1
                            });

                            mTESFuncionesDB.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
                            {
                                vSolicitud = vTesoreria,
                                vCtaConta = vCtaBene,
                                vMonto = item.monto,
                                vDH = "D",
                                vLinea = 2
                            });
                        }

                        mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                        {
                            EmpresaId = CodCliente,
                            cod_beneficio = item.cod_beneficio,
                            consec = item.consec,
                            movimiento = "Actualiza",
                            detalle = $@"Env�o pago a tesorer�a Cod.Remesa: [{infoCarga.cod_remesa}]",
                            registro_usuario = infoCarga.usuario
                        });
                    }

                    //Actualiza y Carga Remesa
                    query = $@"update AFI_BENEFICIOS_REMESAS SET Estado = 'T'
		                             Where cod_remesa = {infoCarga.cod_remesa}";

                    var resp2 = connection.Execute(query);

                    foreach (var item in infoCarga.casos)
                    {
                        //Envio Correo
                        //await CorreoNotificacionPago_Enviar(CodCliente, item);
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        /// <summary>
        /// METODO: Obtiene los tokens disponibles para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO<List<TokenConsultaModel>> Afi_LiqAsientosToken_Obtener(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_Consulta(CodEmpresa, usuario);
        }

        /// <summary>
        /// METODO: Genera un nuevo token para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO Afi_LiqAsientoToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_New(CodEmpresa, usuario);
        }

        #endregion

        #region Informe


        /// <summary>
        /// Obtener informe top de remesas
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<AfiBeneficiosRemesasDTOLista> AfiInformesTop_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var vfiltro = JsonConvert.DeserializeObject<AfiInformesTopFiltros>(filtros);
            var response = new ErrorDTO<AfiBeneficiosRemesasDTOLista>
            {
                Result = new AfiBeneficiosRemesasDTOLista(),
                Code = 0
            };

            string where = "";
            string paginaActual = "";
            string paginacionActual = "";

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var p = new DynamicParameters();

                // WHERE (similar al método CoreUsuariosLista_Obtener)
                if (!string.IsNullOrWhiteSpace(vfiltro?.filtro))
                {
                    // Nota: fecha se convierte a varchar para permitir LIKE
                    where = @"
                WHERE  cod_remesa LIKE '%' + @filtro + '%'
                   OR  usuario    LIKE '%' + @filtro + '%'
                   OR  CONVERT(VARCHAR(19), fecha, 120) LIKE '%' + @filtro + '%'";
                    p.Add("@filtro", vfiltro.filtro);
                }


                if (vfiltro.pagina != null)
                {
                    paginaActual = " OFFSET " + vfiltro.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + vfiltro.paginacion + " ROWS ONLY ";
                }


                // Total con el mismo WHERE
                var countSql = $"SELECT COUNT(*) FROM AFI_BENEFICIOS_REMESAS {where}";
                response.Result.Total = connection.Query<int>(countSql, p).FirstOrDefault();

                // Datos paginados
                var dataSql = $@"
                                SELECT *
                                FROM AFI_BENEFICIOS_REMESAS
                                {where}
                                ORDER BY fecha DESC
                                {paginaActual} {paginacionActual}";

                response.Result.Beneficios = connection.Query<AfiBeneficiosRemesasDTO>(dataSql, p).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                if (response.Result == null) response.Result = new AfiBeneficiosRemesasDTOLista();
                response.Result.Total = 0;
                response.Result.Beneficios = null;
            }

            return response;
        }

        // public ErrorDTO<AfiBeneficiosRemesasDTOLista> AfiInformesTop_Obtener(int CodCliente, string? filtro, int? pagina, int? paginacion)
        // {
        //     var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

        //     var response = new ErrorDTO<AfiBeneficiosRemesasDTOLista>
        //     {
        //         Result = new AfiBeneficiosRemesasDTOLista()
        //     };
        //     string paginaActual = " ", paginacionActual = " ", where = " ";
        //     try
        //     {
        //         var query = "";
        //         using var connection = new SqlConnection(clienteConnString);
        //         {
        //             //Busco Total
        //             query = "SELECT COUNT(*) FROM AFI_BENEFICIOS_REMESAS";
        //             response.Result.Total = connection.Query<int>(query).FirstOrDefault();

        //             if (filtro != null && filtro != "")
        //             {
        //                 where = " WHERE cod_remesa LIKE '%" + filtro + "%' OR usuario LIKE '%" + filtro + "%' OR fecha LIKE '%" + filtro + "%' ";
        //             }

        //             if (pagina != null)
        //             {
        //                 paginaActual = " OFFSET " + pagina + " ROWS ";
        //                 paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
        //             }

        //             query = $@"select * from AFI_BENEFICIOS_REMESAS {where} order by fecha desc {paginaActual} {paginacionActual}";

        //             response.Result.Beneficios = connection.Query<AfiBeneficiosRemesasDTO>(query).ToList();
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         response.Code = -1;
        //         response.Description = ex.Message;
        //         response.Result = null;
        //     }
        //     return response;
        // }

        public ErrorDTO<List<CuboBeneficiosData>> Cubo_Beneficios_Obtener(int CodCliente, CuboParametros remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<CuboBeneficiosData>>();
            var filtro = "";
            var filtroProd = "";
            var filtroMora = "";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //if (remesa.cod_remesa != 0)
                    //{
                    //    filtro = $@" AND R.COD_REMESA = {remesa.cod_remesa} ";
                    //}

                    string fechaInicio = mAuxiliarDB.validaFechaGlobal(remesa.fecha_inicio);
                    string fechaCorte = mAuxiliarDB.validaFechaGlobal(remesa.fecha_corte);

                    string fechaInicioSinHora = fechaInicio.Substring(0, 10);
                    string fechaCorteSinHora = fechaCorte.Substring(0, 10);

                    var query = $@"exec spAFI_Bene_Cubo_Consulta '{fechaInicioSinHora} 00:00:00', '{fechaCorteSinHora} 23:59:59', '{remesa.detalle}'";

                    response.Result = connection.Query<CuboBeneficiosData>(query).ToList();

                    //foreach (var item in response.Result)
                    //{
                    //    switch (item.provincia)
                    //    {
                    //        case "1":
                    //            item.provincia = "San Jos�";
                    //            break;
                    //        case "2":
                    //            item.provincia = "Alajuela";
                    //            break;
                    //        case "3":
                    //            item.provincia = "Heredia";
                    //            break;
                    //        case "4":
                    //            item.provincia = "Cartago";
                    //            break;
                    //        case "5":
                    //            item.provincia = "Puntarenas";
                    //            break;
                    //        case "6":
                    //            item.provincia = "Guanacaste";
                    //            break;
                    //        case "7":
                    //            item.provincia = "Lim�n";
                    //            break;
                    //        default:
                    //            break;
                    //    }

                    //    item.tipo = item.tipo switch
                    //    {
                    //        "M" => "Monetario",
                    //        "P" => "Producto",
                    //        _ => "Otro",
                    //    };
                    //}
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

        #endregion

        #region Notificaciones 

        private async Task CorreoNotificacionPago_Enviar(int CodCliente, AfiBeneTrasladoAplciar traslado)
        {
            EnvioCorreoModels eConfig = new();
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            AfiBeneDatosCorreoPago socio = new();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var queryCodBene = @$"SELECT C.COD_CATEGORIA, C.COD_SMTP FROM AFI_BENE_CATEGORIAS C
                                            WHERE C.COD_CATEGORIA IN (
                                            SELECT B.COD_CATEGORIA FROM AFI_BENEFICIOS B
                                            WHERE B.COD_BENEFICIO IN (
                                            SELECT DISTINCT H.COD_BENEFICIO FROM AFI_BENE_OTORGA H
                                            WHERE H.COD_BENEFICIO = '{traslado.cod_beneficio}'
                                            )
                                            )";
                    string codCategoria = connection.Query<string>(queryCodBene).FirstOrDefault();

                    eConfig = _envioCorreoDB.CorreoConfig(CodCliente, Notificaciones);

                    var query = $@"select 
                        P.cedula,
                        RTRIM(P.COD_BENEFICIO) + RIGHT('00000' + CAST(P.CONSEC AS VARCHAR(5)), 5) AS expediente,
                        B.descripcion as beneficio,
                        P.T_EMAIL AS email, 
                        P.monto,
                        C.cod_divisa 
                        from  AFI_BENE_PAGO P 
                        inner join SYS_CUENTAS_BANCARIAS C ON P.CEDULA = C.IDENTIFICACION
                        inner join AFI_BENEFICIOS B ON P.COD_BENEFICIO = B.COD_BENEFICIO
                        WHERE P.CEDULA = '{traslado.cedula}' AND P.COD_BENEFICIO = '{traslado.cod_beneficio}' AND P.CONSEC = {traslado.consec}";

                    socio = connection.Query<AfiBeneDatosCorreoPago>(query).FirstOrDefault();

                    switch (socio.cod_divisa)
                    {
                        case "COL":
                            socio.cod_divisa = "Colones";
                            break;
                        case "DOL":
                            socio.cod_divisa = "D�lares";
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            switch (traslado.tipo_emision)
            {
                case "CK":
                    traslado.tipo_emision = "Cheque";
                    break;
                case "TE":
                    traslado.tipo_emision = "Transferencia";
                    break;
                case "RE":
                    traslado.tipo_emision = "Efectivo";
                    break;
                case "ND":
                    traslado.tipo_emision = "Nota D�bito";
                    break;
                case "NC":
                    traslado.tipo_emision = "Nota Cr�dito";
                    break;
                case "OT":
                    traslado.tipo_emision = "Otro...";
                    break;
                case "CD":
                    traslado.tipo_emision = "Ctrl Desembolsos";
                    break;
                default:
                    break;
            }

            string body = @$"<html lang=""es"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Notificaci�n de Dep�sito</title>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                    }}
                                    .container {{
                                        width: 600px;
                                        margin: 0 auto;
                                        border: 1px solid #eaeaea;
                                        padding: 20px;
                                    }}
                                    .header {{
                                        background-color: #e8f3ff;
                                        padding: 10px;
                                    }}
                                    .header img {{
                                        width: auto;
                                        height: 50px;
                                    }}
                                    .content {{
                                        margin-top: 20px;
                                    }}
                                    .content h2 {{
                                        font-size: 16px;
                                        color: #0072ce;
                                    }}
                                    .table {{
                                        width: 100%;
                                        margin-top: 20px;
                                        border-collapse: collapse;
                                    }}
                                    .table th, .table td {{
                                        padding: 10px;
                                        border-bottom: 1px solid #dcdcdc;
                                        text-align: left;
                                    }}
                                    .table th {{
                                        background-color: #0072ce;
                                        color: white;
                                    }}
                                    .footer {{
                                        margin-top: 20px;
                                        font-size: 12px;
                                    }}
                                    .footer ul {{
                                        list-style: none;
                                        padding: 0;
                                    }}
                                    .footer li {{
                                        margin: 5px 0;
                                    }}
                                    .footer a {{
                                        color: #0072ce;
                                        text-decoration: none;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class=""container"">
                                    <div class=""header"">
                                        <img src=""https://www.aseccssenlinea.com/Content/Login/ASECCSSLogo.png"" alt=""Logo"">
                                    </div>
                                    <div class=""content"">
                                        <h2>Estimado asociado: <strong>{traslado.nombre}</strong>, c�dula <strong>{socio.cedula}</strong> </h2>
                                        <p>Este es un mensaje de confirmaci�n de la transferencia generada a su cuenta de ASECCSS {traslado.cta_bancaria}.</p>
                                        <table class=""table"">
                                            <tr>
                                                <th> Fecha Transacci�n </th>
                                                <td>{DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year}</td>
                                            </tr>
                                            <tr>
                                                <th> Tipo de Movimiento </th>
                                                <td>{traslado.tipo_emision}</td>
                                            </tr>
                                            <tr>
                                                <th> Monto de la Transacci�n </th>
                                                <td>{traslado.monto:N2}</td>
                                            </tr>
                                            <tr>
                                                <th> Moneda </th>
                                                <td> {socio.cod_divisa}</td>
                                            </tr>
                                            <tr>
                                                <th> Concepto </th>
                                                <td>Desembolso a su cuenta de ASECCSS </td>
                                            </tr>
                                            <tr>
                                                <th> N�mero de Solicitud </th>
                                                <td>{socio.expediente}</td>
                                            </tr>
                                            <tr>
                                                <th> Detalle de la Solicitud </th>
                                                <td>Pago {socio.beneficio}</td>
                                            </tr>
                                        </table>
                                    </div>
                                    <div class=""footer"">
                                        <p>Para obtener m�s informaci�n sobre la transacci�n realizada, puede comunicarse a trav�s de nuestros medios:</p>
                                        <ul>
                                            <li>Plataforma de Servicio en L�nea (PSL): <a href=""https://www.aseccssenlinea.com"">www.aseccssenlinea.com</a></li>
                                            <li>L�nea gratuita 800 ASECCSS (2732277)</li>
                                            <li>Chat en l�nea: <a href=""https://www.aseccss.com"">www.aseccss.com</a></li>
                                            <li>Correo: <a href=""mailto:servicioasociado@aseccss.com"">servicioasociado@aseccss.com</a></li>
                                            <li>Promotores</li>
                                            <li>Oficinas Centrales y Regionales</li>
                                        </ul>
                                    </div>
                                </div>
                            </body>
                            </html>";

            List<IFormFile> Attachments = [];

            if (sendEmail == "Y")
            {
                EmailRequest emailRequest = new()
                {
                    To = socio.email,
                    From = eConfig.User,
                    Subject = "Notificaci�n de Dep�sito",
                    Body = body,
                    Attachments = Attachments
                };

                if (eConfig != null)
                {
                    ErrorDTO response = new ErrorDTO();
                    await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, response);
                }
            }


        }

        #endregion
    }
}