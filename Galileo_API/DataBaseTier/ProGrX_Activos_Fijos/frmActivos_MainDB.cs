using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;


namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosMainDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MActivosFijos _mActivos;
        private readonly PortalDB _portalDB;

        public FrmActivosMainDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _mActivos = new MActivosFijos(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para consultar los departamentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Departamentos_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select rtrim(cod_departamento) as 'item',rtrim(descripcion) as 'descripcion' from Activos_departamentos order by cod_departamento";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método para consultar secciones segun el departament
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="departamento"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Secciones_Obtener(int CodEmpresa, string departamento)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select rtrim(cod_Seccion) as 'item',rtrim(descripcion) as 'descripcion' FROM  Activos_Secciones where cod_departamento = @departamento order by cod_Seccion";
                result.Result = connection.Query<DropDownListaGenericaModel>(query, new { departamento }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método  para consultar los responsables segun departamento y seccion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="departamento"></param>
        /// <param name="seccion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Responsable_Obtener(int CodEmpresa, string departamento, string seccion)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select rtrim(Identificacion) as 'item',rtrim(Nombre) as 'descripcion' FROM Activos_Personas where cod_departamento = @departamento and cod_Seccion = @seccion order by identificacion";
                result.Result = connection.Query<DropDownListaGenericaModel>(query, new { departamento, seccion }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método  para consultar las localizaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Localizacion_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select rtrim(COD_LOCALIZA) as 'item',rtrim(descripcion) as 'descripcion' from ACTIVOS_LOCALIZACIONES Where Activa = 1 order by descripcion";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método  para consultar los tipos de activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_TipoActivo_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select rtrim(tipo_activo) as 'item',rtrim(descripcion) as 'descripcion' from Activos_tipo_activo order by tipo_activo";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método  para validar si permite el registro de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<int> Activos_Main_PermiteRegistros_Validar(int CodEmpresa)
        {
            var result = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select isnull(REGISTRO_PERIODO_CERRADO,0) as 'Permite' from ACTIVOS_PARAMETROS";
                result.Result = connection.Query<int>(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = 0;
            }
            return result;
        }


        /// <summary>
        /// Método para forzar un tipo de activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<int> Activos_Main_ForzarTipoActivo_Validar(int CodEmpresa)
        {
            var result = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select forzar_tipoActivo from Activos_parametros";
                result.Result = connection.Query<int>(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = 0;
            }
            return result;
        }


        /// <summary>
        /// Método  para consultar un numero de placa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="orden"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_Main_NumeroPlaca_Consultar(int CodEmpresa, int orden, string placa)
        {
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select Top 1 num_placa from Activos_Principal";

                if (orden == 1)
                {
                    query += $@" where num_placa > @placa order by num_placa asc";
                }
                else
                {
                    query += $@" where num_placa  <  @placa order by num_placa asc";
                }

                result.Result = connection.Query<string>(query, new { placa }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método  para consultar el historico de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="estadoHistorico"></param>
        /// <returns></returns>
        public ErrorDto<List<MainHistoricoData>> Activos_Main_Historico_Consultar(int CodEmpresa, string codigo, string estadoHistorico)
        {
            var result = new ErrorDto<List<MainHistoricoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<MainHistoricoData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"exec spActivos_HistoricoConsolidado @codigo, @estadoHistorico";
                result.Result = connection.Query<MainHistoricoData>(query, new
                { codigo, estadoHistorico }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método para consultar el detalle de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<List<MainDetalleResponsablesData>> Activos_Main_DetalleResponsables_Consultar(int CodEmpresa, string placa)
        {
            var result = new ErrorDto<List<MainDetalleResponsablesData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<MainDetalleResponsablesData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select R.Identificacion,R.nombre,A.Registro_Fecha
                                    from Activos_Personas R inner join Activos_Responsables A
                                    on R.Identificacion = A.Identificacion  
                                     Where A.num_placa =@placa  order by A.registro_fecha desc";
                result.Result = connection.Query<MainDetalleResponsablesData>(query, new
                { placa }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método para consultar datos de modificacion de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<List<MainModificacionesData>> Activos_Main_Modificaciones_Consultar(int CodEmpresa, string placa)
        {
            var result = new ErrorDto<List<MainModificacionesData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<MainModificacionesData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select X.id_AddRet, x.fecha,x.monto,x.Descripcion,rtrim(J.cod_justificacion) + ' - ' + J.descripcion as Justifica
                                    ,A.nombre, case  when X.Tipo = 'A' then 'Adicion/Mejora' when X.Tipo = 'M' then 'Mantenimiento' when X.Tipo = 'R' then 'Retiro'
                                    		         when X.Tipo = 'V' then 'Revaluación'    when X.Tipo = 'D' then 'Deterioro'  else '' end as 'TipoMov'
                                    from Activos_retiro_adicion X inner join Activos_Principal A on X.num_placa = A.num_placa
                                    inner join Activos_justificaciones J on X.cod_justificacion = J.cod_justificacion
                                    left join Activos_proveedores P on X.compra_proveedor = P.cod_proveedor  
                                    where X.num_placa = @placa order by X.id_AddRet";
                result.Result = connection.Query<MainModificacionesData>(query, new
                { placa }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método  para consultar el listado de composicion de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<List<MainComposicionData>> Activos_Main_Composicion_Consultar(int CodEmpresa, string placa)
        {
            var result = new ErrorDto<List<MainComposicionData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<MainComposicionData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select num_placa,'X' as Tipo,nombre as descripcion,depreciacion_periodo,depreciacion_acum
                                   ,depreciacion_mes,fecha_adquisicion as Fecha, Valor_historico as Libros
                                    From Activos_Principal where num_placa =  @placa 
                                    Union
                                   select num_placa + '-' + CONVERT(char(3), id_AddRet) as num_Placa,'A' as Tipo,descripcion,depreciacion_periodo,depreciacion_acum
                                    ,depreciacion_mes,fecha as Fecha, Monto as Libros
                                    From Activos_retiro_Adicion where tipo = 'A' and num_placa  = @placa 
                                    order by fecha asc ";
                result.Result = connection.Query<MainComposicionData>(query, new
                { placa }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método  para consultar el listado de polizas de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<List<MainPolizasData>> Activos_Main_Polizas_Consultar(int CodEmpresa, string placa)
        {
            var result = new ErrorDto<List<MainPolizasData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<MainPolizasData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select T.descripcion as DescTipo,P.num_poliza,p.Documento,p.fecha_Inicio,p.fecha_vence,p.Descripcion,p.cod_poliza
                                    from Activos_polizas_tipos T 
                                    inner join Activos_polizas P  on T.tipo_poliza = P.tipo_poliza
                                    inner join Activos_polizas_asg A on P.cod_poliza = A.cod_poliza
                                    and A.num_placa = @placa 
                                   order by P.fecha_vence desc";
                result.Result = connection.Query<MainPolizasData>(query, new
                { placa }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método para consultar los datos generales de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<MainGeneralData> Activos_Main_DatosGenerales_Consultar(int CodEmpresa, string placa)
        {
            var result = new ErrorDto<MainGeneralData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new MainGeneralData()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@" select  A.num_placa, A.PLACA_ALTERNA,A.Nombre,A.Tipo_activo,A.met_depreciacion,A.Vida_Util,A.ud_produccion,A.ud_anio,
                                     A.Estado,A.vida_util_en,A.valor_historico,A.valor_desecho,A.fecha_adquisicion,A.fecha_instalacion,A.Descripcion,A.Cod_Departamento,
                                     A.Cod_Seccion,A.Identificacion, A.compra_documento,A.COD_PROVEEDOR,A.NUM_SERIE,A.modelo,A.marca,A.otras_senas,A.Registro_Usuario,A.Registro_fecha,
                                     A.depreciacion_periodo,A.depreciacion_acum,A.DEPRECIACION_MES
                                     ,Rtrim(D.descripcion) as 'Departamento_Desc'
                                     ,Rtrim(S.descripcion) as 'Seccion_Desc'
                                     ,Rtrim(R.Nombre) as 'Responsable_Desc'
                                     ,isnull(P.descripcion,'N/A') as 'Proveedor',T.descripcion as 'Tipo_Activo_Desc'
                                     ,isnull(A.cod_Localiza,'00') as 'Localiza_Id',isnull(La.Descripcion,'No Indica') as 'Localiza_Desc'
                                     from Activos_Principal A
                                     inner join Activos_departamentos D on A.cod_departamento = D.cod_departamento
                                     inner join Activos_Secciones S on A.cod_departamento = S.cod_departamento and A.cod_seccion = S.cod_seccion
                                     inner join Activos_Personas R on A.identificacion = R.Identificacion
                                     inner join Activos_proveedores P on A.cod_proveedor = P.cod_proveedor
                                     inner join Activos_tipo_activo T on A.tipo_activo = T.tipo_activo
                                     left join Activos_Localizaciones La on A.cod_localiza = La.cod_localiza
                                     where A.num_placa = @placa  ";
                result.Result = connection.Query<MainGeneralData>(query, new
                { placa }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método de validaciones para guardar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="placaAlternativa"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_Main_Validaciones_Consultar(int CodEmpresa, string placa, string placaAlternativa)
        {
            var result = new ErrorDto<string>
            {
                Code = -2,
                Description = "Ok",
                Result = string.Empty
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = $@"select count(*) as 'Existe' from Activos_Principal Where Num_Placa  = @placa";
                int resultado = connection.Query<int>(query, new { placa }).FirstOrDefault();

                if (resultado > 0)
                {
                    result.Code = resultado;
                    result.Result = "El número de Placa para este activo ya Existe! ...";
                }
                else
                {
                    if (placaAlternativa.Trim().Length > 0)
                    {
                        var query2 = $@" select dbo.fxActivos_Registro_Valida_Placa_Alterna(@placa,@placaAlternativa )  as Resultado";
                        int resultado2 = connection.Query<int>(query2, new { placa, placaAlternativa }).FirstOrDefault();

                        if (resultado2 == 0)
                            result.Code = resultado;
                        result.Result = "El número de Placa Alterna ya está siendo utilizada por otro activo...";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método para asignar responsables de un cambio  ingreso de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="responsable"></param>
        /// <param name="usuario"></param>
        private void Activos_Main_Responsable_Registrar(int CodEmpresa, string placa, string responsable, string usuario)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);
            var query = $@"exec spActivos_RegistroResponsable @placa, @responsable,@usuario";
            connection.Query(query, new { placa, responsable, usuario });
        }


        /// <summary>
        /// Método para registrar depreciasion de un cambio  ingreso de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="usuario"></param>
        /// <param name="limpia"></param>
        private void Activos_Main_Depreciacion_Registrar(int CodEmpresa, string placa, string usuario, int limpia)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);
            var query = $@" exec spActivos_DepreciacionTabla @placa, @usuario,@limpia";
            connection.Query(query, new { placa, usuario, limpia });
        }


        /// <summary>
        /// Método para asignar asientos de un ingreso de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="usuario"></param>
        private void Activos_Main_Asiento_Registrar(int CodEmpresa, string placa, string usuario)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);
            var query = $@" exec spActivos_AsientoRegistroInicial @placa, @usuario ";
            connection.Query(query, new { placa, usuario });
        }


        /// <summary>
        /// Método para modificar un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <param name="aplicacionTotal"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Activos_Main_Modificar(int CodEmpresa, MainGeneralData data, int aplicacionTotal, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                DateTime mFechaUltCierre = _mActivos.fxActivos_FechaUltimoCierre(CodEmpresa);
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"update Activos_Principal set nombre = @nombre , Placa_Alterna =@placa_alterna, descripcion = @descripcion,compra_documento = @compra_documento
                                    ,cod_proveedor =@cod_proveedor, num_serie =@num_serie ,marca = @marca, modelo =@modelo , otras_senas = @otras_senas";

                if (aplicacionTotal == 0)
                {
                    query += $@",tipo_activo = @tipo_activo ,met_depreciacion = @met_depreciacion,Vida_Util_en =@vida_util_en ,Vida_Util =@vida_util ,UD_ANIO =@ud_anio,
                                   UD_PRODUCCION =@ud_produccion ,  valor_historico =@valor_historico,valor_desecho =@valor_desecho ,fecha_adquisicion =@fecha_adquisicion, fecha_instalacion =@fecha_instalacion,
                                    cod_departamento =@cod_departamento,cod_seccion = @cod_seccion, identificacion =@identificacion, cod_Localiza = @localiza_id,
                                    Localiza_Fecha = dbo.myGetdate(), Modifica_Fecha = getdate(), Modifica_Usuario = @usuario";

                }
                query += $@" where num_placa = @num_placa  ";

                connection.Execute(query, new
                {
                    data.nombre,
                    data.placa_alterna,
                    data.descripcion,
                    data.compra_documento,
                    data.cod_proveedor,
                    data.num_serie,
                    data.marca,
                    data.modelo,
                    data.otras_senas,
                    data.tipo_activo,
                    data.met_depreciacion,
                    data.vida_util_en,
                    data.vida_util,
                    data.ud_anio,
                    data.ud_produccion,
                    data.valor_historico,
                    data.valor_desecho,
                    data.fecha_adquisicion,
                    data.fecha_instalacion,
                    data.cod_departamento,
                    data.cod_seccion,
                    data.identificacion,
                    data.localiza_id,
                    usuario,
                    data.num_placa
                });

                if (data.fecha_adquisicion > mFechaUltCierre)
                {
                    Activos_Main_Depreciacion_Registrar(CodEmpresa, data.num_placa, usuario, 1);
                }
                if (data.depreciacion_acum == 0)
                {
                    Activos_Main_Responsable_Registrar(CodEmpresa, data.num_placa, data.identificacion, usuario);
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $" Activo : {data.num_placa}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Método para insetar un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Activos_Main_Guardar(int CodEmpresa, MainGeneralData data, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"insert into Activos_Principal(num_placa, Placa_Alterna, nombre, tipo_activo, descripcion, met_depreciacion
                                , vida_util_en, vida_util, valor_historico, valor_desecho, fecha_adquisicion, fecha_instalacion
                                , cod_departamento, cod_seccion, identificacion, cod_localiza, localiza_fecha, cod_proveedor, compra_documento, num_serie, marca, modelo
                                , otras_senas, estado, depreciacion_acum, depreciacion_mes, depreciacion_periodo, ud_produccion
                                ,ud_anio, registro_fecha, registro_usuario)
                                values(@num_placa,@placa_alterna,@nombre,@tipo_activo,@descripcion,@met_depreciacion,@vida_util_en,@vida_util, @valor_historico, @valor_desecho, @fecha_adquisicion, @fecha_instalacion
                                , @cod_departamento, @cod_seccion, @identificacion, @localiza_id, dbo.myGetdate(), @cod_proveedor, @compra_documento, @num_serie, @marca, @modelo
                                ,@otras_senas,'A',0,0,0, @ud_produccion
                                ,@ud_anio, dbo.myGetdate(), @usuario)";

                connection.Execute(query, new
                {
                    data.num_placa,
                    data.placa_alterna,
                    data.nombre,
                    data.tipo_activo,
                    data.descripcion,
                    data.met_depreciacion,
                    data.vida_util_en,
                    data.vida_util,
                    data.valor_historico,
                    data.valor_desecho,
                    data.fecha_adquisicion,
                    data.fecha_instalacion,
                    data.cod_departamento,
                    data.cod_seccion,
                    data.identificacion,
                    data.localiza_id,
                    data.cod_proveedor,
                    data.compra_documento,
                    data.num_serie,
                    data.marca,
                    data.modelo,
                    data.otras_senas,
                    data.ud_produccion,
                    data.ud_anio,
                    usuario,

                });

                Activos_Main_Responsable_Registrar(CodEmpresa, data.num_placa, data.identificacion, usuario);
                Activos_Main_Depreciacion_Registrar(CodEmpresa, data.num_placa, usuario, 0);
                Activos_Main_Asiento_Registrar(CodEmpresa, data.num_placa, usuario);

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $" Activo : {data.num_placa}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Método  para eliminar un numero de placa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Activos_Main_Eliminar(int CodEmpresa, string codigo, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"exec spActivos_EliminaActivo  @codigo,@usuario";
                connection.Execute(query, new { codigo, usuario });
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $" Activo : {codigo}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Método para consultar listado de activos disponibles para Adición o Retiro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosData>> Activos_Main_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<ActivosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select num_placa, Placa_Alterna, Nombre from Activos_Principal ";
                result.Result = connection.Query<ActivosData>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método para consultar activos por tipo de activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_activo"></param>
        /// <returns></returns>
        public ErrorDto<MainActivosTiposData> Activos_Main_TipoActivo_Consultar(int CodEmpresa, string tipo_activo)
        {
            var result = new ErrorDto<MainActivosTiposData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new MainActivosTiposData()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select MET_DEPRECIACION,VIDA_UTIL,TIPO_VIDA_UTIL from Activos_tipo_activo where tipo_activo = @tipo_activo ";
                result.Result = connection.Query<MainActivosTiposData>(query, new { tipo_activo }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método para consultar la fecha del ultimo periodo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<DateTime> Activos_Main_FechaUltimoCierre(int CodEmpresa)
        {
            var result = new ErrorDto<DateTime>
            {
                Code = 0,
                Description = "Ok",
                Result = DateTime.Now,
            };
            try
            {
                result.Result = _mActivos.fxActivos_FechaUltimoCierre(CodEmpresa);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;

            }
            return result;
        }


        /// <summary>
        /// Método para obtener el listado de proveedores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Proveedores_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select cod_proveedor as 'item',descripcion as 'descripcion' from Activos_proveedores";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método para consultar el Id de placa inicial para un registro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_Main_PlacaId_Consultar(int CodEmpresa)
        {
            var result = new ErrorDto<string>
            {
                Code = -2,
                Description = "Ok",
                Result = string.Empty
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select dbo.fxActivos_Placa_Id() as 'PLACA_ID'";
                result.Result = connection.Query<string>(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método para consultar el listado de documentos de compra
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="proveedor"></param>
        /// <param name="adquisicion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_DocCompas_Obtener(int CodEmpresa, string proveedor, DateTime adquisicion)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select COD_FACTURA, COD_PROVEEDOR, CANTIDAD , REGISTRADOS , PRODUCTO From vCxP_Compras_Activos where
                              COD_PROVEEDOR = @proveedor AND YEAR(FECHA) =@anno  AND MONTH(FECHA) =@mes";
                result.Result = connection.Query<DropDownListaGenericaModel>(query, new { proveedor = proveedor, anno = adquisicion.Year, mes = adquisicion.Month }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
    }
}