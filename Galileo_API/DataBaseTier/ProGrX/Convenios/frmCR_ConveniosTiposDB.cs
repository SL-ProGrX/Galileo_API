using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    public class FrmCrConveniosTiposDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private const int VModulo = 16; // Módulo de Convenios (vModulo del formulario VB6)
        private const string guardadoExitoso = "Informacion guardada satisfactoriamente...";
        private const string eliminadoExitoso = "Informacion eliminada satisfactoriamente...";

        public FrmCrConveniosTiposDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrConveniosTiposDb(PortalDB portalDb, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDb;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene los tipos de convenios.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConvenioTipoData>> CrConveniosTipos_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    rtrim(Tipo_Convenio) as tipo_convenio,
                    rtrim(isnull(descripcion, '')) as descripcion,
                    cast(isnull(Activo, 0) as bit) as activo
                from CRD_CONVENIO_TIPO
                order by Tipo_Convenio;";

            return DbHelper.ExecuteListQuery<CrConvenioTipoData>(
                _portalDb,
                codEmpresa,
                sqlQuery
            );
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un tipo de convenio.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrConveniosTipos_Guardar(
            int codEmpresa,
            CrConvenioTipoGuardarRequest request)
        {
            request.usuario = (request.usuario ?? string.Empty).Trim();
            request.tipo.tipo_convenio = (request.tipo.tipo_convenio ?? string.Empty).Trim().ToUpperInvariant();
            request.tipo.descripcion = (request.tipo.descripcion ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.tipo.tipo_convenio))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el código del tipo de convenio."
                };
            }

            var existe = ExisteTipo(codEmpresa, request.tipo.tipo_convenio);

            if (existe.Code < 0)
            {
                return new ErrorDto
                {
                    Code = existe.Code,
                    Description = existe.Description
                };
            }

            return existe.Result > 0
                ? ActualizarTipo(codEmpresa, request)
                : InsertarTipo(codEmpresa, request);
        }

        /// <summary>
        /// Elimina un tipo de convenio.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrConveniosTipos_Eliminar(
            int codEmpresa,
            CrConvenioTipoEliminarRequest request)
        {
            var usuario = (request.usuario ?? string.Empty).Trim();
            var tipoConvenio = (request.tipo_convenio ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(tipoConvenio))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el código del tipo de convenio."
                };
            }

            const string sqlDelete = @"
                delete from CRD_CONVENIO_TIPO
                where Tipo_Convenio = @TipoConvenio;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new { TipoConvenio = tipoConvenio }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", tipoConvenio);

            return new ErrorDto
            {
                Code = 0,
                Description = eliminadoExitoso
            };
        }

        private ErrorDto InsertarTipo(int codEmpresa, CrConvenioTipoGuardarRequest request)
        {
            const string sqlInsert = @"
                insert into CRD_CONVENIO_TIPO
                (
                    Tipo_Convenio,
                    descripcion,
                    Activo,
                    registro_fecha,
                    registro_usuario
                )
                values
                (
                    @TipoConvenio,
                    @Descripcion,
                    @Activo,
                    dbo.MyGetdate(),
                    @Usuario
                );";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                CrearParametros(request)
            );

            return FinalizarGuardado(codEmpresa, request, "Registra - WEB", resp);
        }

        private ErrorDto ActualizarTipo(int codEmpresa, CrConvenioTipoGuardarRequest request)
        {
            const string sqlUpdate = @"
                update CRD_CONVENIO_TIPO
                set descripcion = @Descripcion,
                    Activo = @Activo
                where Tipo_Convenio = @TipoConvenio;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                CrearParametros(request)
            );

            return FinalizarGuardado(codEmpresa, request, "Modifica - WEB", resp);
        }

        private static object CrearParametros(CrConvenioTipoGuardarRequest request)
        {
            return new
            {
                TipoConvenio = request.tipo.tipo_convenio,
                Descripcion = request.tipo.descripcion,
                Activo = request.tipo.activo ? 1 : 0,
                Usuario = request.usuario
            };
        }

        private ErrorDto FinalizarGuardado(
            int codEmpresa,
            CrConvenioTipoGuardarRequest request,
            string movimiento,
            ErrorDto resp)
        {
            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, request.usuario, movimiento, request.tipo.tipo_convenio);

            return new ErrorDto
            {
                Code = 0,
                Description = guardadoExitoso
            };
        }

        private ErrorDto<int> ExisteTipo(int codEmpresa, string tipoConvenio)
        {
            const string sqlExiste = @"
                select isnull(count(*), 0) as Existe
                from CRD_CONVENIO_TIPO
                where Tipo_Convenio = @TipoConvenio;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new { TipoConvenio = tipoConvenio }
            );
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string tipoConvenio)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Tipo de Convenio : {tipoConvenio}",
                Movimiento = movimiento,
                Modulo = VModulo
            });
        }
    }
}
