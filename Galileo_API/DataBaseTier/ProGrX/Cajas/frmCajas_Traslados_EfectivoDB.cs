using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;


namespace Galileo.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasTrasladosEfectivoDb
    {
        private readonly PortalDB _portalDb;

        public FrmCajasTrasladosEfectivoDb(IConfiguration? config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
        }

        /// <summary>
        /// Obtener los traslados de efectivo entre cajas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasTrasladosEfectivoDto>> Cajas_TrasladosEfectivo_Obtener(int CodEmpresa, CajasTrasladosEfectivoFiltros filtros)
        {
            var response = new ErrorDto<List<CajasTrasladosEfectivoDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasTrasladosEfectivoDto>()
            };

            try
            {
                filtros ??= new CajasTrasladosEfectivoFiltros { cod_caja = "0" };
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                string fuente = string.IsNullOrWhiteSpace(filtros.origen_destino) ? "D" : filtros.origen_destino.Trim().Substring(0, 1);
                string tipo = string.IsNullOrWhiteSpace(filtros.movimiento) ? "" : filtros.movimiento.Trim().Substring(0, 1);
                string estado = string.IsNullOrWhiteSpace(filtros.estado) ? "P" : filtros.estado.Trim().Substring(0, 1);

                DateTime? inicio = null;
                DateTime? corte = null;

                if (!filtros.sin_fechas)
                {
                    inicio = filtros.fecha_inicio.Date;
                    corte = filtros.fecha_corte.Date.AddDays(1).AddSeconds(-1);
                }

                response.Result = connection.Query<CajasTrasladosEfectivoDto>(
                    "spCajas_TE_Consulta",
                    new
                    {
                        Caja = filtros.cod_caja,
                        OrigenDestino = fuente,
                        Movimiento = tipo,
                        Estado = estado,
                        fInicio = inicio,
                        fCorte = corte
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CajasTrasladosEfectivoDto>();
            }

            return response;
        }

        /// <summary>
        /// Obtener el catálogo de traslados de efectivo
        /// 0 - Divisas
        /// 1 - Cajas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Index"></param>
        /// <param name="IdCaja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TrasladosEfectivo_Catalogo_Obtener(int CodEmpresa, int Index, string IdCaja)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                string query = Index switch
                {
                    0 => "select RTRIM(COD_DIVISA) as item, rtrim(DESCRIPCION) AS descripcion From vSys_Divisas ORDER BY DIVISA_LOCAL DESC",
                    1 => "Select cod_caja as item,descripcion from cajas_definicion where cod_caja <> @IdCaja and Activa = 1 and (PERMITE_TRASLADOS_EF = 1 or ROL_BOVEDA = 1)",
                    _ => string.Empty
                };

                if (string.IsNullOrWhiteSpace(query))
                {
                    response.Code = -1;
                    response.Description = "Opción inválida.";
                    return response;
                }

                response.Result = connection.Query<DropDownListaGenericaModel>(query, new { IdCaja }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DropDownListaGenericaModel>();
            }

            return response;
        }

        /// <summary>
        /// Obtener los movimientos de traslados de efectivo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="IdCaja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TrasladosEfectivo_Movimientos_Obtener(int CodEmpresa, string IdCaja)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                const string query = @"select descripcion,ACTIVA, PERMITE_TRASLADOS_EF, ROL_BOVEDA
                    from cajas_definicion where cod_caja = @IdCaja";

                var caja = connection.QueryFirstOrDefault(query, new { IdCaja });

                if (caja == null)
                {
                    response.Code = -1;
                    response.Description = "Caja no encontrada.";
                    response.Result = new List<DropDownListaGenericaModel>();
                    return response;
                }

                if (caja.ACTIVA == 0)
                {
                    response.Result.Add(new DropDownListaGenericaModel
                    {
                        item = "N",
                        descripcion = "Ninguno"
                    });

                    return response;
                }

                if (caja.PERMITE_TRASLADOS_EF == 0 && caja.ROL_BOVEDA == 0)
                {
                    response.Result.Add(new DropDownListaGenericaModel
                    {
                        item = "N",
                        descripcion = "Ninguno"
                    });
                }

                if (caja.PERMITE_TRASLADOS_EF == 1)
                {
                    response.Result.Add(new DropDownListaGenericaModel
                    {
                        item = "T",
                        descripcion = "Traslado de Efectivo"
                    });
                }

                if (caja.ROL_BOVEDA == 1)
                {
                    response.Result.Add(new DropDownListaGenericaModel
                    {
                        item = "A",
                        descripcion = "Aprovisionamiento"
                    });

                    response.Result.Add(new DropDownListaGenericaModel
                    {
                        item = "R",
                        descripcion = "Reintegro"
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DropDownListaGenericaModel>();
            }

            return response;
        }

        /// <summary>
        /// Obtener el tipo de cambio para el traslado de efectivo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Divisa"></param>
        /// <returns></returns>
        public ErrorDto<decimal> Cajas_TrasladosEfectivo_TipoCambio_Obtener(int CodEmpresa, string Divisa)
        {
            const string query = @"select dbo.fxCajas_TipoCambio (
                    (select TOP 1 cod_empresa_enlace from sif_empresa), 
                    @Divisa, GETDATE(), 'C') as TipoCambio";

            var result = DbHelper.ExecuteSingleQuery<decimal>(_portalDb, CodEmpresa, query, 0, new { Divisa });
            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse<decimal>(result.Description ?? string.Empty, result.Code ?? -1, 0);
        }

        /// <summary>
        /// Aplicar la resolución de un traslado de efectivo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Cajas_TrasladosEfectivo_Resolucion_Aplicar(int CodEmpresa, CajasTeResolucionRequest request)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                foreach (var tramiteId in request.lista.Distinct())
                {
                    connection.Execute(
                        "spCajas_TE_Resolucion",
                        new
                        {
                            TramiteId = tramiteId,
                            Resolucion = request.resolucion,
                            Caja = request.cod_caja,
                            CajaUsuario = request.caja_usuario,
                            CajaAperturaId = request.apertura_id,
                            Usuario = request.usuario
                        },
                        commandType: System.Data.CommandType.StoredProcedure
                    );
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
        /// Registrar un traslado de efectivo entre cajas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Movimiento"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Cajas_TrasladosEfectivo_Registrar(int CodEmpresa, string Movimiento, CajasTrasladosEfectivoDto request)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                connection.Execute(
                    "spCajas_TE_Registro",
                    new
                    {
                        CajaId = request.cod_caja,
                        CajaUsuario = request.registro_usuario,
                        CajaAperturaId = request.cod_apertura,
                        DCajaId = request.d_cod_caja,
                        Movimiento,
                        Divisa = request.cod_divisa,
                        TipoCambio = request.tipo_cambio,
                        Importe = request.importe,
                        Monto = request.monto,
                        Notas = request.notas,
                        Usuario = request.registro_usuario
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}
