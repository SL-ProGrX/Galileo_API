using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_CambiosFechasDB
    {
        private readonly IConfiguration? _config;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;
        private readonly mSecurityMainDb mSecurity;
        private readonly mTesoreria mTesoreria;
        private readonly int vModulo = 9;

        public frmTES_CambiosFechasDB(IConfiguration config)
        {
            _config = config;
            _AuxiliarDB = new mProGrX_AuxiliarDB(config);
            mSecurity = new mSecurityMainDb(config);
            mTesoreria = new mTesoreria(config);
        }

        /// <summary>
        /// Obtengo la solicitud para cambio de fechas de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO<TES_CambioFechasData> TES_CambioFechas_Obtener(int CodEmpresa, int solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TES_CambioFechasData>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.Nsolicitud,C.tipo,C.estado,C.ndocumento,C.id_banco,B.descripcion as BancoX
                                   ,T.descripcion as TipoDocX,C.detalle_Anulacion,C.Estado_Asiento
                                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_banco = B.id_Banco
                                    inner join  tes_tipos_doc T on C.tipo = T.tipo
                                    where C.nsolicitud = @solicitud ";

                        response.Result = connection.Query<TES_CambioFechasData>(query,
                        new
                        {
                            solicitud = solicitud
                        }).FirstOrDefault();

                    if (response.Result != null)
                    {
                        query = $@"select estado,fecha_emision,fecha_solicitud,fecha_anula from Tes_Transacciones where nsolicitud = @solicitud ";
                        var fechas = connection.Query<TES_CambioFechasData>(query,
                            new
                            {
                                solicitud = solicitud
                            }).FirstOrDefault();

                        if (fechas != null)
                        {
                            response.Result.estado = fechas.estado;
                            response.Result.fecha_emision = fechas.fecha_emision;
                            response.Result.fecha_solicitud = fechas.fecha_solicitud;
                            response.Result.fecha_anula = fechas.fecha_anula;
                        }
                        else
                        {
                            response.Result = new TES_CambioFechasData();
                            response.Result.fecha_emision = null;
                            response.Result.fecha_solicitud = null;
                            response.Result.fecha_anula = null;
                        }
                    }
                    else
                    {
                        response.Result = null;
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
        /// Metodo para cambiar la fecha de un documento 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fechas"></param>
        /// <returns></returns>
        public ErrorDTO TES_CambioFecha_Cambiar(int CodEmpresa, TES_CambioFechasModel fechas)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";

                    string fechaActual = _AuxiliarDB.validaFechaGlobal(fechas.fechaActual);
                    string fechaNueva = _AuxiliarDB.validaFechaGlobal(fechas.fechaNueva);

                    string bitacoara = "";

                    switch (fechas.fecha)
                    {
                        case "S": // Solicitud
                            query = $@"Update Tes_Transacciones Set Fecha_Solicitud = '{fechaNueva}' 
                                        Where NSolicitud = @solicitud";

                            connection.Execute(query,
                                    new
                                    {
                                        solicitud = fechas.nsolicitud
                                    });

                            bitacoara = $@"Cambia Fecha Solicitud de {fechaActual} a {fechaNueva} /Nota: {fechas.detalle_Anulacion}";

                            mTesoreria.sbTesBitacoraEspecial(CodEmpresa, fechas.nsolicitud, "08", bitacoara, fechas.usuario);
                            //Insertar en la bitacora
                            mSecurity.Bitacora(new BitacoraInsertarDTO
                            {
                                EmpresaId = CodEmpresa,
                                Usuario = fechas.usuario,
                                Modulo = vModulo, // Tesoreria
                                Movimiento = "Modifica",
                                DetalleMovimiento = bitacoara,
                            });

                            break;
                        case "E": // Emision

                            query = $@"Update Tes_Transacciones Set Fecha_Emision = '{fechaNueva}' 
                                        Where NSolicitud = @solicitud";

                            connection.Execute(query,
                                    new
                                    {
                                        solicitud = fechas.nsolicitud
                                    });

                            bitacoara = $@"Cambia Fecha Emisión de {fechaActual} a {fechaNueva} /Nota: {fechas.detalle_Anulacion}";

                            //Insertar en la bitacora
                            mTesoreria.sbTesBitacoraEspecial(CodEmpresa, fechas.nsolicitud, "08", bitacoara, fechas.usuario);
                            //Insertar en la bitacora
                            mSecurity.Bitacora(new BitacoraInsertarDTO
                            {
                                EmpresaId = CodEmpresa,
                                Usuario = fechas.usuario,
                                Modulo = vModulo, // Tesoreria
                                Movimiento = "Modifica",
                                DetalleMovimiento = bitacoara,
                            });

                            break;
                        case "A": // Anulacion

                            query = $@"Update Tes_Transacciones Set Fecha_Anula = '{fechaNueva}' 
                                        Where NSolicitud = @solicitud";

                            connection.Execute(query,
                                    new
                                    {
                                        solicitud = fechas.nsolicitud
                                    });

                            bitacoara = $@"Cambia Fecha Anulación de {fechaActual} a {fechaNueva} /Nota: {fechas.detalle_Anulacion}";


                            //Insertar en la bitacora
                            mTesoreria.sbTesBitacoraEspecial(CodEmpresa, fechas.nsolicitud, "08", bitacoara, fechas.usuario);

                            mSecurity.Bitacora(new BitacoraInsertarDTO
                            {
                                EmpresaId = CodEmpresa,
                                Usuario = fechas.usuario,
                                Modulo = vModulo, // Tesoreria
                                Movimiento = "Modifica",
                                DetalleMovimiento = bitacoara,
                            });


                            break;
                        default:
                            break;

                    }

                    response.Code = connection.Execute(query,
                    new
                    {
                        solicitud = fechas.nsolicitud
                    });
                }
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
