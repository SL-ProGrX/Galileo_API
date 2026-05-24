using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrAdjuntosTiposDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private const int VModulo = 3;
        private const string guardadoExitoso = "Informacion guardada satisfactoriamente...";
        private const string eliminadoExitoso = "Informacion eliminada satisfactoriamente...";

        public FrmCrAdjuntosTiposDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrAdjuntosTiposDb(PortalDB portalDb, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDb;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene los tipos de adjuntos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrAdjuntoTipoData>> CrAdjuntosTipos_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    rtrim(COD_ADJUNTO) as cod_adjunto,
                    rtrim(isnull(DESCRIPCION, '')) as descripcion,
                    cast(isnull(ACTIVO, 0) as bit) as activo,
                    rtrim(isnull(REGISTRO_USUARIO, '')) as registro_usuario,
                    REGISTRO_FECHA as registro_fecha
                from CRD_ADJUNTOS_TIPOS
                order by COD_ADJUNTO;";

            return DbHelper.ExecuteListQuery<CrAdjuntoTipoData>(
                _portalDb,
                codEmpresa,
                sqlQuery
            );
        }

        /// <summary>
        /// Guarda un tipo de adjunto.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrAdjuntosTipos_Guardar(
            int codEmpresa,
            CrAdjuntoTipoGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.tipo.cod_adjunto = Limpiar(request.tipo.cod_adjunto);
            request.tipo.descripcion = (request.tipo.descripcion ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.tipo.cod_adjunto))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo del tipo adjunto."
                };
            }

            if (ExisteTipo(codEmpresa, request.tipo.cod_adjunto))
            {
                return ActualizarTipo(codEmpresa, request);
            }

            return InsertarTipo(codEmpresa, request);
        }

        /// <summary>
        /// Elimina un tipo de adjunto.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrAdjuntosTipos_Eliminar(
            int codEmpresa,
            CrAdjuntoTipoEliminarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_adjunto = Limpiar(request.cod_adjunto);

            if (string.IsNullOrWhiteSpace(request.cod_adjunto))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo del tipo adjunto."
                };
            }

            const string sqlDelete = @"
                delete from CRD_ADJUNTOS_TIPOS
                where COD_ADJUNTO = @CodAdjunto;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodAdjunto = request.cod_adjunto
                }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Elimina - WEB",
                $"Credito Tipo Adjunto: {request.cod_adjunto}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = eliminadoExitoso
            };
        }

        private ErrorDto InsertarTipo(int codEmpresa, CrAdjuntoTipoGuardarRequest request)
        {
            const string sqlInsert = @"
                insert into CRD_ADJUNTOS_TIPOS
                (
                    COD_ADJUNTO,
                    DESCRIPCION,
                    ACTIVO,
                    REGISTRO_USUARIO,
                    REGISTRO_FECHA
                )
                values
                (
                    @CodAdjunto,
                    @Descripcion,
                    @Activo,
                    @Usuario,
                    Getdate()
                );";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                CrearParametros(request)
            );

            return FinalizarGuardado(
                codEmpresa,
                request.usuario,
                request.tipo.cod_adjunto,
                "Registra - WEB",
                resp
            );
        }

        private ErrorDto ActualizarTipo(int codEmpresa, CrAdjuntoTipoGuardarRequest request)
        {
            const string sqlUpdate = @"
                update CRD_ADJUNTOS_TIPOS
                set DESCRIPCION = @Descripcion,
                    ACTIVO = @Activo
                where COD_ADJUNTO = @CodAdjunto;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                CrearParametros(request)
            );

            return FinalizarGuardado(
                codEmpresa,
                request.usuario,
                request.tipo.cod_adjunto,
                "Modifica - WEB",
                resp
            );
        }

        private static object CrearParametros(CrAdjuntoTipoGuardarRequest request)
        {
            return new
            {
                CodAdjunto = request.tipo.cod_adjunto,
                Descripcion = request.tipo.descripcion,
                Activo = request.tipo.activo ? 1 : 0,
                Usuario = request.usuario
            };
        }

        private ErrorDto FinalizarGuardado(
            int codEmpresa,
            string usuario,
            string codAdjunto,
            string movimiento,
            ErrorDto resp)
        {
            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento,
                $"Credito Tipo Adjunto: {codAdjunto}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = guardadoExitoso
            };
        }

        private bool ExisteTipo(int codEmpresa, string codAdjunto)
        {
            const string sqlExiste = @"
                select coalesce(count(*), 0)
                from CRD_ADJUNTOS_TIPOS
                where COD_ADJUNTO = @CodAdjunto;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodAdjunto = codAdjunto
                }
            );

            return resp.Result > 0;
        }

        private static string Limpiar(string valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = VModulo
            });
        }
    }
}