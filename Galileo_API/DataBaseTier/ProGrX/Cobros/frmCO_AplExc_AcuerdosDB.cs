using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplExcAcuerdosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 4;

        public FrmCoAplExcAcuerdosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCoAplExcAcuerdosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene informacion de un acuerdo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idAcuerdo"></param>
        /// <returns></returns>
        public ErrorDto<CoAplExcAcuerdosData?> CoAplExcAcuerdos_Obtener(int codEmpresa, int idAcuerdo)
        {
            const string query = @"exec spCbr_Excedente_Apl_Acuerdos_Consulta @IdAcuerdo";
            return DbHelper.ExecuteSingleQuery<CoAplExcAcuerdosData>(
                _portalDb, codEmpresa, query, null, new { IdAcuerdo = idAcuerdo }
            );
        }

        /// <summary>
        /// Obtiene la lista de acuerdos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAplExcAcuerdosData>> CoAplExcAcuerdos_Lista_Obtener(int codEmpresa, string filtro, string estado)
        {
            object activo;
            switch (estado)
            {
                case "Activos":
                    activo = 1;
                    break;
                case "Inactivos":
                    activo = 0;
                    break;
                default:
                    activo = null;
                    break;
            }
            if (filtro == "0")
            {
                filtro = "";
            }
            const string query = @"exec spCbr_Excedente_Apl_Acuerdos_List @Filtro, @Activo";
            return DbHelper.ExecuteListQuery<CoAplExcAcuerdosData>(
                _portalDb, codEmpresa, query,
                new { Filtro = filtro, Activo = activo }
            );
        }

        /// <summary>
        /// Guarda la informacion de un acuerdo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CoAplExcAcuerdos_Guardar(int codEmpresa, CoAplExcAcuerdosData request)
        {
            int activo = request.estado == "Activo" ? 1 : 0;
            int giro = request.giro_excedentes == "Sí" ? 1 : 0;

            const string sql = @"
            exec spCbr_Excedente_Apl_Acuerdos_Add
                @AcuerdoId,
                @Cedula,
                @FirmaBoleta,
                @FechaVencimiento,
                @Activo,
                @Giro,
                @Observaciones,
                @Usuario";

            var resp = DbHelper.ExecuteSingleQuery<CoAplExcAcuerdosGuardarResponse>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new
                {
                    AcuerdoId = request.id_acuerdo,
                    Cedula = request.cedula,
                    FirmaBoleta = request.firma_boleta,
                    FechaVencimiento = request.fecha_vencimiento,
                    Activo = activo,
                    Giro = giro,
                    Observaciones = request.observaciones,
                    Usuario = request.usuario_registra
                });

            if (resp.Code < 0 || resp.Result == null)
                return new ErrorDto { Code = -1, Description = resp.Description };

            string mensajeResp = resp.Result.mensaje;
            if (resp.Result.pass == 1)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario_registra,
                    movimiento: resp.Result.movimiento + " - WEB",
                    detalle: mensajeResp
                );
                mensajeResp = "Se ha procesado satisfactoriamente! Acción: " + resp.Result.movimiento 
                    + " de Acuerdo de Cobros: Aplicación Mora Excedentes Id: " + resp.Result.acuerdoId;
            } 

            return new ErrorDto { Code = resp.Result.acuerdoId, Description = mensajeResp };
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
