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
                if (fechas is null)
                    return DbHelper.ErrorResponse("Parámetros inválidos.");

                var fechaActual = MProGrXAuxiliarDB.validaFechaGlobal(fechas.fechaActual, "yyyy-MM-dd HH:mm:ss")
                    ?? throw new ArgumentException("Fecha actual inválida");

                var fechaNueva = MProGrXAuxiliarDB.validaFechaGlobal(fechas.fechaNueva, "yyyy-MM-dd HH:mm:ss")
                    ?? throw new ArgumentException("Fecha nueva inválida");

                const string sqlSolicitud = @"
UPDATE Tes_Transacciones
SET Fecha_Solicitud = @fechaNueva
WHERE NSolicitud = @solicitud;";

                const string sqlEmision = @"
UPDATE Tes_Transacciones
SET Fecha_Emision = @fechaNueva
WHERE NSolicitud = @solicitud;";

                const string sqlAnulacion = @"
UPDATE Tes_Transacciones
SET Fecha_Anula = @fechaNueva
WHERE NSolicitud = @solicitud;";

                string query;
                string etiqueta;

                switch (fechas.fecha)
                {
                    case "S":
                        query = sqlSolicitud;
                        etiqueta = "Solicitud";
                        break;
                    case "E":
                        query = sqlEmision;
                        etiqueta = "Emisión";
                        break;
                    case "A":
                        query = sqlAnulacion;
                        etiqueta = "Anulación";
                        break;
                    default:
                        return DbHelper.ErrorResponse("Tipo de fecha inválido.");
                }

                conn.Execute(query, new { fechaNueva, solicitud = fechas.nsolicitud });

                var nota = fechas.detalle_Anulacion ?? string.Empty;
                var bitacora = $"Cambia Fecha {etiqueta} de {fechaActual} a {fechaNueva} /Nota: {nota}";

                mTesoreria.sbTesBitacoraEspecial(CodEmpresa, fechas.nsolicitud, "08", bitacora, fechas.usuario);

                mSecurity.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = fechas.usuario,
                    Modulo = vModulo,
                    Movimiento = "Modifica",
                    DetalleMovimiento = bitacora
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
