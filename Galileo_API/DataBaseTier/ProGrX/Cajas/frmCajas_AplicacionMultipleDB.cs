using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Galileo_API.Models.ProGrX.Cajas;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasAplicacionMultipleDb
    {
        private readonly IConfiguration _config;

        public FrmCajasAplicacionMultipleDb(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Validar Caja AM
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="codApertura"></param>
        /// <param name="sesionId"></param>
        /// <param name="usuario"></param>
        /// <param name="monto"></param>
        /// <param name="tiquete"></param>
        /// <returns></returns>
        public ErrorDto<CajasAmValidacionDto> Cajas_AM_Validar(int codEmpresa,string codCaja,int codApertura,int sesionId,
            string usuario,decimal monto,string tiquete)
        {
            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<CajasAmValidacionDto>
            {
                Code = 0,
                Result = new CajasAmValidacionDto()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                response.Result = cn.QueryFirst<CajasAmValidacionDto>(
                    "spCajas_Transac_Validacion",
                    new
                    {
                        codCaja,
                        usuario,
                        codApertura,
                        sesionId,
                        Tipo = "Crd",
                        Codigo = "-AM-",
                        monto,
                        tiquete
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene creditos Pendientes
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
       public ErrorDto<List<CajasCreditoPendienteDto>> Cajas_AM_Creditos_Pendientes(int codEmpresa,CajasAMCreditosPendientesRequestDto request)
        {
            string connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                    var response = new ErrorDto<List<CajasCreditoPendienteDto>>
                    {
                        Code = 0,
                        Result = new List<CajasCreditoPendienteDto>()
                    };

                    try
                    {
                        using var cn = new SqlConnection(connString);

                        response.Result = cn.Query<CajasCreditoPendienteDto>(
                            "spCajas_Crd_Persona_Creditos_Pendientes_Lista",
                            new
                            {
                                cedula = request.cedula,
                                codCaja = request.codcaja,
                                codApertura = request.codapertura,
                                tiquete = request.tiquete,
                                fechaCorte = request.fechacorte,
                                tipoMovimiento = request.tipomovimiento,
                                fechaPago = request.fechapago
                            },
                            commandType: CommandType.StoredProcedure
                        ).ToList();
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
        /// Agrega creditos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_AM_Creditos_Agregar(
            int codEmpresa,
            List<CajasAmAgregarRequestDto> items)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Operación realizada correctamente",
                Result = true
            };

            try
            {
                using var cn = new SqlConnection(connString);

                foreach (var item in items)
                {
                    cn.Execute(
                        "spCajas_AM_Creditos_Add",
                        item,
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }

       /// <summary>
       /// Elimina el lote
       /// </summary>
       /// <param name="codEmpresa"></param>
       /// <param name="ids"></param>
       /// <returns></returns>
        public ErrorDto<bool> Cajas_AM_Eliminar(
            int codEmpresa,
            List<long> ids)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Operación realizada correctamente",
                Result = true
            };

            try
            {
                using var cn = new SqlConnection(connString);

                foreach (var id in ids)
                {
                    cn.Execute(
                        "spCajas_AM_Selected_Del",
                        new { id },
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }

        /// <summary>
        /// Aplica cajas AM
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<long> Cajas_AM_Aplicar(
            int codEmpresa,
            CajasAmAplicarRequestDto request)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<long>
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var cn = new SqlConnection(connString);

                long amId = cn.QuerySingle<long>(
                    "spCajas_AM_Registro_Control",
                    request,
                    commandType: CommandType.StoredProcedure
                );

                cn.Execute(
                    "spCajas_AM_Procesa",
                    new { pAM_Id = amId },
                    commandType: CommandType.StoredProcedure
                );

                response.Result = amId;
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

