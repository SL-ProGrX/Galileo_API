using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesCambiosFechasDB
    {

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb mSecurity;
        private readonly MTesoreria mTesoreria;
        private readonly int vModulo = 9;

        public FrmTesCambiosFechasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            mSecurity = new MSecurityMainDb(config);
            mTesoreria = new MTesoreria(config);
        }

        /// <summary>
        /// Obtengo la solicitud para cambio de fechas de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<TesCambioFechasData> TES_CambioFechas_Obtener(int CodEmpresa, int solicitud)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select C.Nsolicitud,C.tipo,C.estado,C.ndocumento,C.id_banco,B.descripcion as BancoX
                                   ,T.descripcion as TipoDocX,C.detalle_Anulacion,C.Estado_Asiento
                                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_banco = B.id_Banco
                                    inner join  tes_tipos_doc T on C.tipo = T.tipo
                                    where C.nsolicitud = @solicitud ";

                var result = conn.Query<TesCambioFechasData>(query,
                new
                { solicitud = solicitud }
                ).FirstOrDefault();

                if (result != null)
                {
                    query = $@"select estado,fecha_emision,fecha_solicitud,fecha_anula from Tes_Transacciones where nsolicitud = @solicitud ";
                    var fechas = conn.Query<TesCambioFechasData>(query,
                        new
                        {
                            solicitud = solicitud
                        }).FirstOrDefault();

                    if (fechas != null)
                    {
                        result.estado = fechas.estado;
                        result.fecha_emision = fechas.fecha_emision;
                        result.fecha_solicitud = fechas.fecha_solicitud;
                        result.fecha_anula = fechas.fecha_anula;
                    }
                   
                }
                else
                {
                    return DbHelper.CreateErrorResponse<TesCambioFechasData>("No se encontró la solicitud de tesorería.");
                }

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesCambioFechasData>($"Error al obtener la solicitud: {ex.Message}");
            }
            
        }

        /// <summary>
        /// Método para cambiar la fecha de un documento 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fechas"></param>
        /// <returns></returns>
        public ErrorDto TES_CambioFecha_Cambiar(int CodEmpresa, TesCambioFechasModel fechas)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = "";

                string fechaActual = MProGrXAuxiliarDB.validaFechaGlobal(fechas.fechaActual, "yyyy-MM-dd HH:mm:ss") ?? throw new ArgumentException("Fecha actual inválida");
                string fechaNueva = MProGrXAuxiliarDB.validaFechaGlobal(fechas.fechaNueva, "yyyy-MM-dd HH:mm:ss") ?? throw new ArgumentException("Fecha nueva inválida");

                string bitacoara = "";

                switch (fechas.fecha)
                {
                    case "S": // Solicitud
                        query = $@"Update Tes_Transacciones Set Fecha_Solicitud = @fechaNueva
                                        Where NSolicitud = @solicitud";

                        conn.Execute(query,
                                new
                                {
                                    fechaNueva = fechaNueva,
                                    solicitud = fechas.nsolicitud
                                });

                        bitacoara = $@"Cambia Fecha Solicitud de {fechaActual} a {fechaNueva} /Nota: {fechas.detalle_Anulacion}";

                        mTesoreria.sbTesBitacoraEspecial(CodEmpresa, fechas.nsolicitud, "08", bitacoara, fechas.usuario!);
                        //Insertar en la bitacora
                        mSecurity.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = fechas.usuario!,
                            Modulo = vModulo, // Tesoreria
                            Movimiento = "Modifica",
                            DetalleMovimiento = bitacoara,
                        });

                        break;
                    case "E": // Emision

                        query = $@"Update Tes_Transacciones Set Fecha_Emision = @fechaNueva
                                        Where NSolicitud = @solicitud";

                        conn.Execute(query,
                                new
                                {
                                    fechaNueva = fechaNueva,
                                    solicitud = fechas.nsolicitud
                                });

                        bitacoara = $@"Cambia Fecha Emisión de {fechaActual} a {fechaNueva} /Nota: {fechas.detalle_Anulacion}";

                        //Insertar en la bitacora
                        mTesoreria.sbTesBitacoraEspecial(CodEmpresa, fechas.nsolicitud, "08", bitacoara, fechas.usuario!);
                        //Insertar en la bitacora
                        mSecurity.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = fechas.usuario!,
                            Modulo = vModulo, // Tesoreria
                            Movimiento = "Modifica",
                            DetalleMovimiento = bitacoara,
                        });

                        break;
                    case "A": // Anulacion

                        query = $@"Update Tes_Transacciones Set Fecha_Anula = @fechaNueva
                                        Where NSolicitud = @solicitud";

                        conn.Execute(query,
                                new
                                {
                                    fechaNueva = fechaNueva,
                                    solicitud = fechas.nsolicitud
                                });

                        bitacoara = $@"Cambia Fecha Anulación de {fechaActual} a {fechaNueva} /Nota: {fechas.detalle_Anulacion}";


                        //Insertar en la bitacora
                        mTesoreria.sbTesBitacoraEspecial(CodEmpresa, fechas.nsolicitud, "08", bitacoara, fechas.usuario!);

                        mSecurity.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = fechas.usuario!,
                            Modulo = vModulo, // Tesoreria
                            Movimiento = "Modifica",
                            DetalleMovimiento = bitacoara,
                        });


                        break;
                    default:
                        break;

                }

                conn.Execute(query,
                new
                {
                    solicitud = fechas.nsolicitud
                });

                return DbHelper.OkResponse("Fecha cambiada con éxito.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al cambiar la fecha: {ex.Message}");
            }
        }

        

    }
}
