using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSeguimientoAutorizacionesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MTesoreria _mTesoreria;
        private const int VModulo = 3;

        public FrmCrSeguimientoAutorizacionesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mTesoreria = new MTesoreria(config);
        }

        /// <summary>
        /// Obtiene el detalle de una solicitud pendiente de autorizacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoAutorizacionesDetalleData?> Cr_SeguimientoAutorizaciones_Detalle_Obtener(
            int codEmpresa,
            int operacion)
        {
            const string sql = @"
                select top 1
                    R.id_solicitud as operacion,
                    rtrim(R.codigo) as codigo,
                    rtrim(C.descripcion) as linea_desc,
                    rtrim(R.cod_destino) as cod_destino,
                    rtrim(D.descripcion) as destino_desc,
                    rtrim(R.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    isnull(R.montoapr, 0) as montoapr,
                    isnull(R.plazo, 0) as plazo,
                    isnull(R.[int], 0) as tasa,
                    isnull(R.cuota, 0) as cuota,
                    rtrim(isnull(R.garantia, '')) as garantia,
                    R.fechasol as fecha_sol,
                    rtrim(isnull(R.userrec, '')) as user_rec,
                    rtrim(isnull(R.observacion, '')) as observacion,
                    cast(1 as bit) as puede_autorizar
                from reg_creditos R
                inner join socios S
                    on R.cedula = S.cedula
                inner join catalogo C
                    on R.codigo = C.codigo
                   and C.retencion = 'N'
                   and C.poliza = 'N'
                inner join catalogo_destinos D
                    on R.cod_destino = D.cod_destino
                where R.autoriza_user is null
                  and R.estadosol = 'R'
                  and R.id_solicitud = @Operacion;";

            var response = DbHelper.ExecuteSingleQuery<CrSeguimientoAutorizacionesDetalleData>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { Operacion = operacion });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoAutorizacionesDetalleData?>(
                    response.Description ?? "No fue posible obtener la solicitud.",
                    response.Code.GetValueOrDefault(-1),
                    null);
            }

            if (response.Result is null)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoAutorizacionesDetalleData?>(
                    "La solicitud no cumple con alguno(s) de los siguientes par&aacute;metros: no se encuentra recibida, no existe la solicitud o ya se encuentra autorizada.",
                    -2,
                    null);
            }

            return DbHelper.CreateOkResponse<CrSeguimientoAutorizacionesDetalleData?>(response.Result);
        }

        /// <summary>
        /// Autoriza la solicitud indicada y registra bitacora y tag de seguimiento.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Cr_SeguimientoAutorizaciones_Autorizar(
            int codEmpresa,
            CrSeguimientoAutorizacionesAutorizarRequest request)
        {
            request.usuario = NormalizarTexto(request.usuario);
            request.notas = (request.notas ?? string.Empty).Trim();

            if (request.operacion <= 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar una operacion valida."
                };
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el usuario."
                };
            }

            if (string.IsNullOrWhiteSpace(request.notas))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar las notas de autorizacion."
                };
            }

            var detalleResp = Cr_SeguimientoAutorizaciones_Detalle_Obtener(codEmpresa, request.operacion);
            if (detalleResp.Code != 0 || detalleResp.Result is null || !detalleResp.Result.puede_autorizar)
            {
                return new ErrorDto
                {
                    Code = detalleResp.Code.GetValueOrDefault(-1),
                    Description = detalleResp.Description ?? "La solicitud no esta disponible para autorizacion."
                };
            }

            const string sql = @"
                update reg_creditos
                set autoriza_user = @Usuario,
                    autoriza_fecha = dbo.MyGetdate(),
                    Autoriza_nota = @Notas
                where id_solicitud = @Operacion
                  and autoriza_user is null
                  and estadosol = 'R';

                select @@ROWCOUNT;";

            var updateResp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new
                {
                    Usuario = request.usuario,
                    Notas = TruncarNotas(request.notas),
                    Operacion = request.operacion
                });

            if (updateResp.Code != 0)
            {
                return new ErrorDto
                {
                    Code = updateResp.Code.GetValueOrDefault(-1),
                    Description = updateResp.Description ?? "No fue posible autorizar la solicitud."
                };
            }

            if (updateResp.Result <= 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "La solicitud ya no esta disponible para autorizacion."
                };
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                $"Autorizacion Solicitud : {request.operacion}",
                "Aplica - WEB");

            _mTesoreria.sbCrdOperacionTags(
                codEmpresa,
                request.operacion,
                detalleResp.Result.codigo,
                "S09",
                request.usuario,
                string.Empty,
                request.notas);

            return new ErrorDto
            {
                Code = 0,
                Description = "Solicitud Autorizada Satisfactoriamente..."
            };
        }

        private static string TruncarNotas(string notas)
        {
            var valor = (notas ?? string.Empty).Trim();

            if (valor.Length > 500)
            {
                valor = valor[..500];
            }

            return valor;
        }

        private static string NormalizarTexto(string valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string detalleMovimiento,
            string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Modulo = VModulo,
                Movimiento = movimiento,
                DetalleMovimiento = detalleMovimiento
            });
        }
    }
}