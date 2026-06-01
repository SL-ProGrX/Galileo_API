using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCausasSeguimientoDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;

        private const int VModulo = 3;
        private const string GuardadoExitoso = "Informacion guardada satisfactoriamente...";
        private const string EliminadoExitoso = "Informacion eliminada satisfactoriamente...";

        public FrmCrCausasSeguimientoDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrCausasSeguimientoDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }

        /// <summary>
        /// Obtiene las causas de seguimiento por tipo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCausasSeguimientoData>> CrCausasSeguimiento_Causas_Obtener(
            int codEmpresa, string tipo)
        {
            const string sql = @"
                select
                    rtrim(isnull(cod_causas, '')) as cod_causas,
                    rtrim(isnull(descripcion, '')) as descripcion,
                    cast(isnull(estado, 0) as bit) as estado
                from OPERACION_CAUSAS
                where tipo = @Tipo
                order by cod_causas;";

            return DbHelper.ExecuteListQuery<CrCausasSeguimientoData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Tipo = tipo
                }
            );
        }

        /// <summary>
        /// Guarda una causa de seguimiento.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCausasSeguimiento_Causas_Guardar(
            int codEmpresa,
            CrCausasSeguimientoGuardarRequest request)
        {
            request.usuario = NormalizarTexto(request.usuario);
            request.tipo = NormalizarTexto(request.tipo);
            request.causa.cod_causas = NormalizarTexto(request.causa.cod_causas);
            request.causa.descripcion = (request.causa.descripcion ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(request.causa.cod_causas))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo de la causa."
                };
            }

            if (string.IsNullOrWhiteSpace(request.causa.descripcion))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar la descripcion de la causa."
                };
            }

            if (ExisteCausa(codEmpresa, request.tipo, request.causa.cod_causas))
            {
                return ActualizarCausa(codEmpresa, request);
            }

            return InsertarCausa(codEmpresa, request);
        }

        /// <summary>
        /// Elimina una causa de seguimiento.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCausasSeguimiento_Causas_Eliminar(
            int codEmpresa,
            CrCausasSeguimientoEliminarRequest request)
        {
            request.usuario = NormalizarTexto(request.usuario);
            request.tipo = NormalizarTexto(request.tipo);
            request.cod_causas = NormalizarTexto(request.cod_causas);

            if (string.IsNullOrWhiteSpace(request.cod_causas))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo de la causa."
                };
            }

            const string sql = @"
                delete from OPERACION_CAUSAS
                where cod_causas = @CodCausas
                  and tipo = @Tipo;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodCausas = request.cod_causas,
                    Tipo = request.tipo
                }
            );

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Elimina - WEB",
                $"Causas Seguimiento Tramite : {request.cod_causas} Tipo: {request.tipo}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = EliminadoExitoso
            };
        }

        private ErrorDto InsertarCausa(int codEmpresa, CrCausasSeguimientoGuardarRequest request)
        {
            const string sql = @"
                insert into OPERACION_CAUSAS
                (
                    tipo,
                    cod_causas,
                    descripcion,
                    estado
                )
                values
                (
                    @Tipo,
                    @CodCausas,
                    @Descripcion,
                    @Estado
                );";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                CrearParametrosGuardar(request)
            );

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Registra - WEB",
                $"Causa de Seguim.Tramite Cod : {request.causa.cod_causas} Tipo: {request.tipo}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = GuardadoExitoso
            };
        }

        private ErrorDto ActualizarCausa(int codEmpresa, CrCausasSeguimientoGuardarRequest request)
        {
            const string sql = @"
                update OPERACION_CAUSAS
                   set descripcion = @Descripcion,
                       estado = @Estado
                 where cod_causas = @CodCausas
                   and tipo = @Tipo;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                CrearParametrosGuardar(request)
            );

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Modifica - WEB",
                $"Causa de Seguim.Tramite Cod : {request.causa.cod_causas} Tipo: {request.tipo}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = GuardadoExitoso
            };
        }

        private bool ExisteCausa(int codEmpresa, string tipo, string codCausas)
        {
            const string sql = @"
                select isnull(count(*), 0)
                from OPERACION_CAUSAS
                where cod_causas = @CodCausas
                  and tipo = @Tipo;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new
                {
                    CodCausas = codCausas,
                    Tipo = tipo
                }
            );

            return resp.Result > 0;
        }

        private static object CrearParametrosGuardar(CrCausasSeguimientoGuardarRequest request)
        {
            return new
            {
                Tipo = request.tipo,
                CodCausas = request.causa.cod_causas,
                Descripcion = request.causa.descripcion,
                Estado = request.causa.estado ? 1 : 0
            };
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

        private static string NormalizarTexto(string? valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}