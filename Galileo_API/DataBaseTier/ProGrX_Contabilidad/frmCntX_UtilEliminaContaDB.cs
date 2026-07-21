using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXUtilEliminaContaDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;

        public FrmCntXUtilEliminaContaDb(IConfiguration config)
           : this(new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmCntXUtilEliminaContaDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mProGrxMain;
        }

        /// <summary>
        /// Obtiene las contabilidades disponibles para eliminación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa cuya base de datos se consultará.</param>
        /// <returns>Lista de códigos y nombres de contabilidades.</returns>
        public ErrorDto<List<CntxContabilidadListaDto>> CntxUtil_Contabilidades_Obtener(int codEmpresa)
        {
            const string sql = @"SELECT COD_CONTABILIDAD AS cod_contabilidad,
                                        NOMBRE AS nombre
                                 FROM CntX_Contabilidades
                                 ORDER BY COD_CONTABILIDAD";

            return DbHelper.ExecuteListQuery<CntxContabilidadListaDto>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Elimina las contabilidades solicitadas y registra cada movimiento en la bitácora.
        /// </summary>
        /// <param name="request">Empresa, usuario y códigos de contabilidad que se eliminarán.</param>
        /// <returns>Resultado que indica si el proceso concluyó correctamente.</returns>
        public ErrorDto<bool> CntxUtil_Contabilidades_Eliminar(CntxUtilEliminaContabilidadesRequestDto request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "El parámetro 'request' no puede ser nulo.");
            }

            if (!request.cod_empresa.HasValue)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "El campo 'cod_empresa' es obligatorio.");
            }

            if (request.contabilidades == null || !request.contabilidades.Any())
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "Debe enviar al menos una contabilidad a eliminar.");
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "El campo 'usuario' es obligatorio.");
            }

            return DbHelper.WithConn(_portalDb, request.cod_empresa.Value, cn =>
            {
                foreach (var codContabilidad in request.contabilidades)
                {
                    cn.Execute(
                        "EXEC spCntX_Util_Contabilidad_Elimina @codContabilidad, @usuario, @token",
                        new
                        {
                            codContabilidad,
                            usuario = request.usuario,
                            token = "*xHM1tOk3n$"
                        });

                    _mSecurityMainDb.Bitacora(
                        new Galileo.Models.Security.BitacoraInsertarDto
                        {
                            EmpresaId = request.cod_empresa.Value,
                            Usuario = request.usuario,
                            DetalleMovimiento = $"Elimina: Contabilidad [{codContabilidad}]",
                            Movimiento = "Elimina - WEB",
                            Modulo = 20
                        });
                }

                return true;
            });
        }
    }
}
