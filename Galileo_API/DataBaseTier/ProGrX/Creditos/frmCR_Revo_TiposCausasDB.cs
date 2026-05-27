using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrRevoTiposCausasDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private const int VModulo = 3;
        private const string GuardadoExitoso = "Informacion guardada satisfactoriamente...";

        public FrmCrRevoTiposCausasDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrRevoTiposCausasDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }

        /// <summary>
        /// Obtiene los tipos de causas de credito revolutivo.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se consulta CRD_REV_CAUSAS.</param>
        /// <returns>Lista de causas ordenada por codigo.</returns>
        public ErrorDto<List<CrRevoTiposCausasData>> CR_Revo_TiposCausas_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    cod_causa,
                    descripcion,
                    case
                        when tipo = 'A' then 'Activacion'
                        when tipo = 'R' then 'Reactivacion'
                        when tipo = 'C' then 'Cierre'
                        else 'Activacion'
                    end as tipo,
                    cast(isnull(activo, 0) as bit) as activo
                from CRD_REV_CAUSAS
                order by cod_causa";

            return DbHelper.ExecuteListQuery<CrRevoTiposCausasData>(
                _portalDb,
                codEmpresa,
                sqlQuery);
        }

        /// <summary>
        /// Guarda una causa de credito revolutivo, insertando o actualizando segun exista.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se guarda CRD_REV_CAUSAS.</param>
        /// <param name="usuario">Usuario que ejecuta el mantenimiento para bitacora.</param>
        /// <param name="request">Datos de la causa: cod_causa, descripcion, tipo y activo.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ErrorDto CR_Revo_TiposCausas_Guardar(
            int codEmpresa,
            string usuario,
            CrRevoTiposCausasData request)
        {
            request.cod_causa = (request.cod_causa ?? string.Empty).Trim().ToUpperInvariant();
            request.descripcion = (request.descripcion ?? string.Empty).Trim();
            usuario = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.cod_causa))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo de la causa."
                };
            }

            if (string.IsNullOrWhiteSpace(request.descripcion))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar la descripcion de la causa."
                };
            }

            string tipo = NormalizarTipo(request.tipo);
            if (string.IsNullOrWhiteSpace(tipo))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar un tipo valido: Activacion, Reactivacion o Cierre."
                };
            }

            request.tipo = tipo;

            var existe = ExisteCausa(codEmpresa, request.cod_causa);
            var resp = existe
                ? ActualizarCausa(codEmpresa, request)
                : InsertarCausa(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                existe ? "Modifica - WEB" : "Registra - WEB",
                $"Crd.Rev. Tipo Causa: {request.cod_causa}");

            return new ErrorDto
            {
                Code = 0,
                Description = GuardadoExitoso
            };
        }

        /// <summary>
        /// Elimina una causa de credito revolutivo.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa donde se elimina CRD_REV_CAUSAS.</param>
        /// <param name="usuario">Usuario que ejecuta la eliminacion para bitacora.</param>
        /// <param name="codCausa">Codigo de causa a eliminar.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ErrorDto CR_Revo_TiposCausas_Eliminar(
            int codEmpresa,
            string usuario,
            string codCausa)
        {
            codCausa = (codCausa ?? string.Empty).Trim().ToUpperInvariant();
            usuario = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(codCausa))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo de la causa."
                };
            }

            if (!ExisteCausa(codEmpresa, codCausa))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "La causa indicada no existe."
                };
            }

            const string sqlDelete = @"
                delete from CRD_REV_CAUSAS
                where cod_causa = @CodCausa";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodCausa = codCausa
                });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                "Elimina - WEB",
                $"Crd.Rev. Tipo Causa: {codCausa}");

            return new ErrorDto
            {
                Code = 0,
                Description = "Informacion eliminada satisfactoriamente..."
            };
        }

        private bool ExisteCausa(int codEmpresa, string codCausa)
        {
            const string sqlExiste = @"
                select isnull(count(*), 0)
                from CRD_REV_CAUSAS
                where cod_causa = @CodCausa";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodCausa = codCausa
                });

            return resp.Result > 0;
        }

        private ErrorDto InsertarCausa(
            int codEmpresa,
            string usuario,
            CrRevoTiposCausasData request)
        {
            const string sqlInsert = @"
                insert into CRD_REV_CAUSAS(
                    cod_causa,
                    descripcion,
                    tipo,
                    activo,
                    registro_fecha,
                    registro_usuario)
                values(
                    @CodCausa,
                    @Descripcion,
                    @Tipo,
                    @Activo,
                    dbo.MyGetdate(),
                    @Usuario)";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodCausa = request.cod_causa,
                    Descripcion = request.descripcion,
                    Tipo = request.tipo,
                    Activo = request.activo ? 1 : 0,
                    Usuario = usuario
                });
        }

        private ErrorDto ActualizarCausa(
            int codEmpresa,
            CrRevoTiposCausasData request)
        {
            const string sqlUpdate = @"
                update CRD_REV_CAUSAS
                set descripcion = @Descripcion,
                    tipo = @Tipo,
                    activo = @Activo
                where cod_causa = @CodCausa";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodCausa = request.cod_causa,
                    Descripcion = request.descripcion,
                    Tipo = request.tipo,
                    Activo = request.activo ? 1 : 0
                });
        }

        private static string NormalizarTipo(string tipo)
        {
            tipo = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            if (tipo.StartsWith("A", StringComparison.Ordinal))
                return "A";

            if (tipo.StartsWith("R", StringComparison.Ordinal))
                return "R";

            if (tipo.StartsWith("C", StringComparison.Ordinal))
                return "C";

            return string.Empty;
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
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
