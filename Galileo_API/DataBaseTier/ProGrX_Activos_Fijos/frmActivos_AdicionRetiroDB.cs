using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosAdicionRetiroDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MActivosFijos _mActivos;
        private readonly PortalDB _portalDB;
        public FrmActivosAdicionRetiroDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _mActivos = new MActivosFijos(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para consultar lista de justificaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_AdicionRetiro_Justificaciones_Obtener(int CodEmpresa, string tipo)
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
                var query = $@"select rtrim(cod_justificacion) as 'item',rtrim(descripcion) as 'descripcion' FROM Activos_justificaciones where tipo = @tipo";
                result.Result = connection.Query<DropDownListaGenericaModel>(query, new { tipo }).ToList();
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
        /// Método para obtener los proveedores de un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_AdicionRetiro_Proveedores_Obtener(int CodEmpresa)
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
                var query = $@"select cod_proveedor as 'item',descripcion as 'descripcion' FROM Activos_proveedores ";
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
        /// Método para consultar listado de activos disponibles para Adición o Retiro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosData>> Activos_AdicionRetiro_Activos_Obtener(int CodEmpresa)
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
        /// Método para consultar  retiros, adiciones por número de placa y id
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Id_AddRet"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosRetiroAdicionData> Activos_AdicionRetiro_Consultar(int CodEmpresa, int Id_AddRet, string placa)
        {
            var result = new ErrorDto<ActivosRetiroAdicionData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosRetiroAdicionData()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = $@"select X.*,rtrim(J.cod_justificacion) as 'Motivo_Id', rtrim(J.descripcion) as 'Motivo_Desc' ,A.nombre,P.cod_proveedor,P.descripcion as Proveedor 
                                FROM Activos_retiro_adicion  X 
                                    inner join Activos_Principal A on X.num_placa = A.num_placa
                                    inner join Activos_justificaciones J on X.cod_justificacion = J.cod_justificacion
                                    left join Activos_proveedores P on X.compra_proveedor = P.cod_proveedor
                                where X.Id_AddRet = @Id_AddRet and X.num_placa = @placa ";


                result.Result = connection.Query<ActivosRetiroAdicionData>(query, new
                {
                    Id_AddRet,
                    placa
                }).FirstOrDefault();

                if (result.Result != null && result.Result.tipo_vidautil != "R")
                {
                    result.Result.tipo_vidautil = "S";
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
        /// Método para validar un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_AdicionRetiro_Validar(int CodEmpresa, string placa, DateTime fecha)
        {
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "",
                Result = string.Empty
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = $@"select fecha_adquisicion from Activos_Principal where num_placa = @placa and  estado <> 'R' ";
                result.Result = connection.Query<string>(query, new { placa }).FirstOrDefault();

                if (result.Result == null)
                {
                    result.Code = -2;
                    result.Description = "El Activo no existe, o ya fue retirado ...";
                }
                else
                {

                    DateTime fecha_adquisicion = DateTime.Parse(result.Result, System.Globalization.CultureInfo.InvariantCulture);


                    if ((fecha - fecha_adquisicion).Days < 1)
                    {
                        result.Code = -2;
                        result.Description = "La fecha del Movimiento no es válida, ya que es menor a la del activo ...";

                    }

                }
                var query2 = $@"select estado, dbo.fxActivos_PeriodoActual() as 'PeriodoActual' from Activos_periodos 
                                    where anio = @anno and mes =@mes ";
                var result2 = connection.Query<ActivosPeriodosData>(query2, new { anno = fecha.Year, mes = fecha.Month }).FirstOrDefault();

                if (result2 != null)
                {
                    if (result2.estado.Trim() != "P")
                    {
                        result.Code = -2;
                        result.Description = result.Description + " - El Periodo del Movimiento ya fue cerrado ... ";
                    }

                    if ((fecha.Year != result2.periodoactual.Year) || (fecha.Month != result2.periodoactual.Month))
                    {
                        result.Code = -2;
                        result.Description = result.Description + " - La fecha de aplicación del movimiento no corresponde al periodo abierto!";
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
        ///  Método para consultar los meses de un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="tipo"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public ErrorDto<int> Activos_AdicionRetiro_Meses_Consulta(int CodEmpresa, string placa, string tipo, DateTime fecha)
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
                var query = $@"select dbo.fxActivos_VidaUtilPendiente(@placa,@tipo,@fecha)";
                result.Result = connection.Query<int>(query, new { placa, tipo, fecha }).FirstOrDefault();
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
        /// Método para  Consulta los datos de Depreciación del Activo al Corte
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosPrincipalData> Activos_AdicionRetiro_DatosActivo_Consultar(int CodEmpresa, string placa)
        {
            var result = new ErrorDto<ActivosPrincipalData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosPrincipalData()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = $@"exec spActivos_InfoDepreciacion @placa ";
                result.Result = connection.Query<ActivosPrincipalData>(query, new { placa }).FirstOrDefault();

                if (result.Result == null)
                {
                    result.Result = new ActivosPrincipalData();
                    result.Result.depreciacionPeriodo = "????";
                    result.Result.depreciacion_acum = 0;
                    result.Result.valor_historico = 0;
                    result.Result.valor_desecho = 0;
                    result.Result.valor_libros = 0;
                }
                else
                {
                    result.Result.depreciacionPeriodo = result.Result.depreciacion_periodo.ToString("dd/MM/yyyy");
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
        /// Método para guardar un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Activos_AdicionRetiro_Guardar(int CodEmpresa, string usuario, ActivosRetiroAdicionData data)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"exec spActivos_AdicionRetiro @Placa,@Tipo,@Justificacion,@Descripcion,@Fecha,@Monto,@Meses,@Usuario, @CompraDoc,@CompraProv,@VentaDoc,@VentaCliente";
                int Linea = connection.Query<int>(query, new
                {
                    Placa = data.num_placa,
                    Tipo = data.tipo,
                    Justificacion = data.cod_justificacion,
                    Descripcion = data.descripcion,
                    Fecha = data.fecha,
                    Monto = data.monto,
                    Meses = data.meses_calculo,
                    Usuario = usuario,
                    CompraDoc = data.compra_documento,
                    CompraProv = data.proveedor,
                    VentaDoc = data.venta_documento,
                    VentaCliente = data.venta_cliente
                }).FirstOrDefault();

                result.Code = Linea;

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"{data.tipoDescripcion} (Placa: {data.num_placa}) Id: {data.tipoDescripcion}:{data.id_addret}_ {data.justificacion} ",
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
        ///  Método para consultar el histórico de retiros y adiciones de un activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosHistoricoData>> Activos_AdicionRetiro_Historico_Consultar(int CodEmpresa, string placa)
        {
            var result = new ErrorDto<List<ActivosHistoricoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosHistoricoData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select X.id_AddRet,x.fecha,x.MONTO,x.DESCRIPCION, rtrim(J.cod_justificacion) + ' - ' + J.descripcion as Justifica                                  
                                  , case when X.Tipo = 'A' then 'Adicion/Mejora' when X.Tipo = 'M' then 'Mantenimiento' when X.Tipo = 'R' then 'Retiro' else '' end as 'TipoMov' 
                               from Activos_retiro_adicion X inner join Activos_Principal A on X.num_placa = A.num_placa
                                inner join Activos_justificaciones J on X.cod_justificacion = J.cod_justificacion
                                left join Activos_proveedores P on X.compra_proveedor = P.cod_proveedor
                                 where X.num_placa = @placa and X.Tipo in('A','R','M') ";
                result.Result = connection.Query<ActivosHistoricoData>(query, new
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
        /// Método para consultar los cierres de un registro de Adiciones, Mejoras o Retiros del Activ
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="Id_AddRet"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosRetiroAdicionCierreData>> Activos_AdicionRetiro_Cierres_Consultar(int CodEmpresa, string placa, int Id_AddRet)
        {
            var result = new ErrorDto<List<ActivosRetiroAdicionCierreData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosRetiroAdicionCierreData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = $@"select A.* 
                                    from Activos_Auxiliar_Adiciones A inner join Activos_Periodos P on A.anio = P.Anio and A.mes = P.mes
                                    where A.num_placa = @placa and A.ID_AddRet = @Id_AddRet 
                                            and P.Estado = 'C' order by A.anio desc,A.mes desc";
                result.Result = connection.Query<ActivosRetiroAdicionCierreData>(query, new
                { placa, Id_AddRet }).ToList();


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
        /// Método para obtener el nombre del activo por número de placa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_AdicionRetiro_ActivosNombre_Consultar(int CodEmpresa, string placa)
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
                var query = $@"select nombre from Activos_Principal where num_placa =@placa";
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
        /// Método para eliminar un registro de Adiciones, Mejoras o Retiros del Activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <param name="Id_AddRet"></param>
        /// <returns></returns>
        public ErrorDto Activos_AdicionRetiro_Eliminar(int CodEmpresa, string placa, int Id_AddRet)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"delete Activos_retiro_adicion where num_placa = @placa and Id_AddRet = @Id_AddRet";
                connection.Execute(query, new { placa, Id_AddRet });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Consulta el periodo pendiente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            var result = new ErrorDto<DateTime>
            {
                Code = 0,
                Description = "Ok",
                Result = DateTime.Now,
            };
            try
            {
                result.Result = _mActivos.fxCntX_PeriodoActual(CodEmpresa, contabilidad);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

    }
}
